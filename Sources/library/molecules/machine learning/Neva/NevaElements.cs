/*************************************************************************
The MIT License (MIT)

Copyright (c) 2014 Yury Tsoy

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*************************************************************************/

using System.Collections.Generic;
using System.IO;
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	public enum MutationFactor { MF_UNKNOWN = 0, MF_ORIGINAL, MF_SQUARE, MF_MULTIPLY, MF_SQUARE_MULTIPLY };

	public class NevaElements
	{
		public static int[,] ConfusionMatrix { get; set; }

		#region - [SignalSlot] operations. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns signal from the given array by the specified ID.
		/// </summary>
		/// <param name="signals">List of signals representing some environment state.</param>
		/// <param name="id">Signal's ID.</param>
		/// <returns>SignalSlot object or null.</returns>
		public static SignalSlot GetSignalById(List<SignalSlot> signals, int id)
		{
			foreach (var slot in signals)
			{
				if (slot.Id == id) return slot;
			}
			return null;
		}
		#endregion

		#region - Population generation. -
		/// <summary>
		/// Creates population using the given NEvA parameters.
		/// </summary>
		/// <param name="pars"></param>
		/// <returns></returns>
		public static List<AbstractIndividual> CreatePopulation(NevaParameters pars)
		{
			var res = new List<AbstractIndividual>();
			for (int i = 0; i < pars.PopulationSize; ++i)
			{
				res.Add(new NevaInd(pars.FitnessFunction.InputIds, pars.FitnessFunction.OutputIds,
									pars.GeneValueRange, pars.MinGeneValue, pars.RNG));
			}
			return res;
		}
		#endregion

		#region - Evaluation. -
		/// <summary>
		/// [Atomic]
		/// 
		/// Evaluates given population using the specified fitness function.
		/// </summary>
		/// <param name="popul">Population to evaluate.</param>
		/// <param name="objFunc">Fitness function.</param>
		/// <param name="bestInd">Best found indiviual.</param>
		/// <returns>Min and max fitness values.</returns>
		public static void Evaluate(List<AbstractIndividual> popul, NeuralObjectiveFunction objFunc, out NevaInd bestInd)
		{
			var minF = popul[0].Fitness;
			bestInd = (NevaInd)popul[0];
			for (var i = 0; i < popul.Count; i++)
			{
				var ind = (NevaInd)popul[i];
				ind.BuildNetwork();
				ind.Fitness = objFunc.Calculate(ind.Network);

				//if (ind.Fitness.Value < minF)
				//{
				//    minF = ind.Fitness.Value;
				//    bestInd = ind;
				//}
				if (FitnessComparator.IsBetter(ind.Fitness, minF))
				{
					minF = ind.Fitness;
					bestInd = ind;
				}
			}
			bestInd = (NevaInd)bestInd.Clone();	// to remove double pointing to the same object and occasional change of the best found individual.
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates fitness value using mean square-error criterion for the given neural network and data.
		/// </summary>
		/// <param name="net">Neural network.</param>
		/// <param name="data">Data to use for error calculation.</param>
		/// <returns>Fitness.</returns>
		public static Fitness CalculateFitnessMSE (IVectorFunction net, List<TrainingSample> data)
		{
			var res = new Fitness();
			//res.Value = Performance.MachineLearning.CalculateMSE(net, data);
			res.Value = NeuralObjectiveFunction.CalculateMSE(net, data);
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates sum distance error and Euclidian distance between actual and required network output.
		/// </summary>
		/// <param name="net">Neural network.</param>
		/// <param name="data">Data to use for error calculation.</param>
		/// <returns>Fitness.</returns>
		public static Fitness TestNetwork(IVectorFunction net, List<TrainingSample> data)
		{
			var er = 0f;
			ConfusionMatrix = new int[data[0].Response.Length, data[0].Response.Length];
			foreach (var sample in data)
			{
				var row = MatrixMath.GetRow(sample.Data, 0);
				net.Calculate(row);

				float[] netOut;
				net.GetOutputs(out netOut);
				var winner = VectorMath.IndexOfMax(netOut);

				var output = MatrixMath.GetRow(sample.Response, 0);
				var reqWinner = VectorMath.IndexOfMax(output);
				//var dist = VectorMath.EuclidianDistance(netOut.ToArray(), output);
				if (winner != reqWinner)
				{
					er++;
				}

				ConfusionMatrix[reqWinner, winner]++;	// fill-in confusion matrix.
			}

			var res = new Fitness();
			res.Value = 100f * er / data.Count;
			return res;
		}
		#endregion

		#region - Crossing. -
		public static List<AbstractIndividual> Cross(List<AbstractIndividual> selPopul, NevaParameters pars)
		{
			var res = new List<AbstractIndividual>();

			//
			// todo: implement dynamic sizing (do I really need it?).
			//

			var selSize = selPopul.Count / 2;
			for (int i = 0; i < selSize; i++)
			{
				if (pars.XRate > pars.RNG.NextDouble())
				{
					//
					// select parent individuals.
					int p1 = i;
					int p2;
					do
					{
						p2 = (int)(selSize * pars.RNG.NextDouble());
					} while (p2 == p1 || ((NevaInd)selPopul[p1]).EqualsTo((NevaInd)selPopul[p2]));

					//
					// create children.
					NevaInd parent1 = (NevaInd)selPopul[p1],
					        parent2 = (NevaInd) selPopul[p2];
					NevaInd child1, child2;
					Cross(parent1, parent2, out child1, out child2, pars);
					res.Add(child1);
					res.Add(child2);
				}	// end of if (CrossoverRate > QRandN())
			}	// end of for (i=0; i<SelectCount; i++)

			if (res.Count < selPopul.Count)
			{	// add mutated individuals.
				while (res.Count < selPopul.Count)
				{
					var i = (int)(selSize * pars.RNG.NextDouble());
					var tempInd = (NevaInd)selPopul[i];
					tempInd = MutateWeight(tempInd, pars);
					res.Add(tempInd);
				}
			}
			else
			{	// remove extra individuals.
				res.RemoveRange(selPopul.Count, selPopul.Count - res.Count);
			}

			if (EAElements.TimeFliesLikeArrow(res) > pars.AgeLimit)
			{	// restart: the population is too old.
				res = CreatePopulation(pars);
			}

			return res;
		}

		public static void Cross(NevaInd parent1, NevaInd parent2, out NevaInd c1, out NevaInd c2, NevaParameters pars)
		{
			const float DEFAULT_ALPHA = 0.5f;

			//
			// 0. Find common nodes.
			// Define parent with longer chromosome.
			NevaInd p1, p2;
			if (parent1.Edges.Count > parent2.Edges.Count)
			{
				p1 = parent1;
				p2 = parent2;
			}
			else
			{
				p1 = parent2;
				p2 = parent1;
			}

			int inputs = p1.Network.InputIds.Count, outputs = p1.Network.OutputIds.Count;

			//var stepSize_1 = 1.0f / stepSize;
			int neurons1 = p1.Network.HiddenNodesCount + inputs + outputs,
			    neurons2 = p2.Network.HiddenNodesCount + inputs + outputs;
			List<Edge> list1 = new List<Edge>(p1.Edges),
			           list2 = new List<Edge>(p2.Edges);

			c1 = new NevaInd();
			c2 = new NevaInd();

			//Edge tempEdge;

			//bool flag;
			//float pw1, pw2, cw1, cw2;

			//
			// hence parent1 has more genes than parent2
			// search for common genes
			//int coins = 0;
			for (int i=0; i<list1.Count; i++) 
			{
				for (int j = 0; j < list2.Count; j++)
				{
					if (list1[i].EqualDirection(list2[j])) {
						// a coincidence.
						//coins++;

						var pw1 = list1[i].Weight;
						var pw2 = list2[j].Weight;
						float cw1, cw2;
						EAElements.CrossBlx(pw1, pw2, out cw1, out cw2, DEFAULT_ALPHA, pars.RNG);
						list1[i].Weight = cw1;
						list2[j].Weight = cw2;

						// copy resulting genes to the childish chromosomes
						c1.Edges.Add(list1[i]);
						c2.Edges.Add(list2[j]);

						list1.RemoveAt(i);
						list2.RemoveAt(j);
						i--;	// one step back
						break;
					}	// end of if (list1[i].equals(list2[j]))
				}
			}	// end of for (i=0; i<list1.GetSize(); i++)

			//
			// Gamble list1 contents
			List<Edge> res1, res2;
			GambleEdgeList(list1, out res1, out res2, pars, neurons2);
			c1.Edges.AddRange(res1);
			c2.Edges.AddRange(res2);

			//
			// Gamble list2 contents
			GambleEdgeList(list2, out res1, out res2, pars, neurons1);
			c1.Edges.AddRange(res1);
			c2.Edges.AddRange(res2);

			// set general ANN parameters.
			c1.Network.SetStructureParameters(p1.Network);
			c2.Network.SetStructureParameters(p2.Network);
		}
		#endregion

		#region - Mutation. -
		public static List<AbstractIndividual> Mutate (List<AbstractIndividual> popul, NevaParameters pars)
		{
			var res = new List<AbstractIndividual>();
			var insouts = pars.FitnessFunction.InputIds.Count * pars.FitnessFunction.OutputIds.Count;
			for (int i = 0; i < popul.Count; i++)
			{
				if (popul[i].Size != 0)
				{
					var tempMRate = 1.0f / popul[i].Size;
					var nevaInd = (NevaInd) popul[i];
					for (int j = 0; j < insouts; j++)
					{
						if (tempMRate > pars.RNG.NextDouble())
						{
							nevaInd = Mutate(nevaInd, pars);
						}
						else if (pars.RNG.NextDouble() > 0.5f)
						{
							nevaInd = MutateWeight(nevaInd, pars);
						}
					}
					res.Add(nevaInd);
				}
			}	// for (i=1, count=0; i<size; i++)
			return res;
		}

		public static float CalculateConnFactor(NevaInd ind)
		{
			int conns = ind.Size, inputs = ind.Network.InputIds.Count, outputs = ind.Network.OutputIds.Count,
				neurons = inputs + outputs + ind.Network.HiddenNodesCount;

			// maximum number of connections
			var maxConns = (int)((neurons * (neurons - 1) - inputs * (inputs - 1) - outputs * (outputs - 1)) * 0.5);
			var cfactor = (float)conns / maxConns;

			return cfactor*cfactor;
		}

		public static float CalculateNodeFactor(NevaInd ind)
		{
			int inputs = ind.Network.InputIds.Count, outputs = ind.Network.OutputIds.Count,
				neurons = inputs + outputs + ind.Network.HiddenNodesCount;

			var nfactor = ((float)(inputs + outputs)) / neurons;
			nfactor *= nfactor;
			return nfactor*CalculateConnFactor(ind);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs single mutation over given individual using specified algorithm parameters settings.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Mutated individual.</returns>
		public static NevaInd Mutate (NevaInd ind, NevaParameters pars)
		{
			NevaInd res;	// = new NevaInd();

			var cfactor = CalculateConnFactor(ind);
			bool disableNodeMutation = pars.Algorithm.Contents.NodesMutationLockCount > 0;

			//	if (QRandN() > factor || factor < 0.1) {
			if (pars.RNG.NextDouble() > cfactor)
			{
				if (pars.RNG.NextDouble() > cfactor)
				{
					res = MutateAddConnection(ind, pars);
				}
				else
				{
					if (ind.Network.HiddenNodesCount > 0)
					{
						res = !disableNodeMutation ? MutateDeleteNode(ind, pars) : MutateWeight(ind, pars);
					}
					else
					{
						res = MutateAddConnection(ind, pars);
					}
				}
			}
			else
			{
				var nfactor = CalculateNodeFactor(ind);
				if (pars.RNG.NextDouble() > nfactor)
				{
					if ((pars.RNG.NextDouble() > cfactor) && ind.Network.HiddenNodesCount > 0)
					{
						res = !disableNodeMutation ? MutateDeleteNode(ind, pars) : MutateWeight(ind, pars);
					}
					else
					{
						res = MutateDeleteConnection(ind, pars);
					}
				}
				else
				{
					res = !disableNodeMutation ? MutateAddNode(ind, pars) : MutateWeight(ind, pars);

					/*		An alternative variant: less speed, simpler networks
								if (QRandBool()) {
									mutateAddNeuron ();
								} else {
									mutateDeleteConnection ();
								}/**/
				}
			}	// end of if (QRandN() > (double)conns/(double)maxConns)
			res.Age = 0;

			return res;
		}

		#region - Specific mutation operators. -
		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which adds a connection to the network of the given individual.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static NevaInd MutateAddConnection(NevaInd ind, NevaParameters pars)
		{
			var res = (NevaInd)ind.Clone();

			//var tempEdge = new Edge();
			var conns = res.Size;	// overall number of connections for individual with given index
			int finish;

			//
			// start and finish are GLOBAL neural indices
			//int i;
			//float weight;

			// define new connection parameters
			// Feedback is disabled
			var nonOuts = res.NonOutputNodes;
			var node = pars.UseNodeDegrees? SelectByActivity(nonOuts) : VectorMath.GetRandomElement(nonOuts.ToArray());
			int start = node.Index;
			//do
			//{	// connection can not start from output neuron
			//    start = (int)(pars.RNG.NextDouble() * nodes);
			//} while (res.Network.OutputIds.Contains(start));
			// todo: find out if it's critical not finding isolated nodes (e.g. the nodes which are not connected with the input)
			//} while ((res.Network.OutputIds.Contains(start)) || (mut->getNeuronTag(start) == ISOLATED_NEURON));

			//tempEdge.EndIdx = start;
			var nonIns = res.NonInputNodes;
			int i;
			do
			{
				// connection can not finish in the layer preceeding or equal to start neuron layer
				node = pars.UseNodeDegrees? SelectByActivity(nonIns) : VectorMath.GetRandomElement(nonIns.ToArray());
				finish = node.Index;

				if (res.Network.OutputIds.Contains(finish))
				{
					// any connection may end up on the output neuron
					break;
				}

				// todo: think whether search for loops is necessary.
				//// find out if a loop appears
				//tempEdge.EndIdx = start;
				//tempEdge.BeginIdx = finish;
				//for (i = 0; i < conns; i++)
				//{
				//    if (tempEdge.EqualDirection(res.Edges[i]))
				//    {
				//        break;
				//    }
				//}
				//if (i < conns)
				//{
				//    // if previous cycle was not performed till the end
				//    continue; // look for another finish
				//}
			} while (finish == start);
			//} while ((finish == start) || (res.Network.InputIds.Contains(finish)) || ((mut->getNeuronTag(start) >= mut->getNeuronTag(finish))));

			//var weight = (float)(pars.RNG.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);
			var weight = (float)(ContextRandom.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);
			var tempEdge = new Edge(start, finish, weight);
			//tempEdge.BeginIdx = start;
			//tempEdge.EndIdx = finish;
			//tempEdge.Weight = (float)(pars.RNG.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);

			for (i = 0; i < conns; i++)
			{
				if (tempEdge.EqualDirection(res.Edges[i]))
				{
					break;
				}
			}
			if (i < conns)
			{	// if such connection already exists.
				// note: then do nothing.
				// note: res.Edges[i].Weight = tempEdge.Weight;
			}
			else
			{	// no such connection in genome
				res.Edges.Add(tempEdge);
				res.BuildNetwork();
				//res.Network.RebuildNetwork(res.Edges);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which deletes a node from the network of the given individual.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static NevaInd MutateDeleteNode(NevaInd ind, NevaParameters pars)
		{
			var res = (NevaInd)ind.Clone();

			if (res.Network.HiddenNodesCount > 0)
			{
				int conns = res.Size;	// overall number of connections for individual with given index.
				//int inputs = res.Network.InputIds.Count, outputs = res.Network.OutputIds.Count;
				////	tempInd->HiddenNeuronsCount = Network.Neurons.GetSize() - inputs-outputs;
				//int hidden = res.Network.HiddenNodesCount;//,
				    //neurons = inputs + outputs + hidden;
				// start and finish are GLOBAL neural indices.
				int i;

				//
				// select neuron to delete
				var nonOuts = res.NonOutputNodes;
				var node = pars.UseNodeDegrees? ReverseSelectByActivity(nonOuts) : VectorMath.GetRandomElement(nonOuts.ToArray());
				int mutIndex = node.Index;
				//do
				//{
				//    mutIndex = (int)(neurons * pars.RNG.NextDouble());// + (inputs+outputs));
				//} while (pars.FitnessFunction.OutputIds.Contains(mutIndex));

				//
				// delete connections associated with the deleted neuron.
				for (i = 0; i < res.Size; i++)
				{
					if ((res.Edges[i].BeginIdx == mutIndex) || (res.Edges[i].EndIdx == mutIndex))
					{
						res.Edges.RemoveAt(i);
						i--;
					}
				}	// end of for (i=0; i<tempInd->Genes.GetSize(); i++)

				//
				// correct indices
				for (i = 0; i < res.Size; i++)
				{
					if (res.Edges[i].BeginIdx > mutIndex)
					{
						res.Edges[i].BeginIdx--;
					}
					if (res.Edges[i].EndIdx > mutIndex)
					{
						res.Edges[i].EndIdx--;
					}
				}

				//
				// rebuild network if its size is reduced.
				if (conns != res.Edges.Count) res.BuildNetwork();  //res.Network.RebuildNetwork(res.Edges);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which deletes a random edge from the network of the given individual.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static NevaInd MutateDeleteConnection(NevaInd ind, NevaParameters pars) 
		{
			var res = (NevaInd)ind.Clone();
			var mutIndex = (int)(pars.RNG.NextDouble() * res.Size);

			if (mutIndex < res.Edges.Count)
			{
				res.Edges.RemoveAt(mutIndex);
				res.BuildNetwork();
				//res.Network.RebuildNetwork(res.Edges);
			}
			// else
			// trying to remove connection from empty ANN.
			// simply ignore.

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which adds a random node to the network of the given individual.
		/// The node's activation is set by default.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static NevaInd MutateAddNode(NevaInd ind, NevaParameters pars)
		{
			var res = (NevaInd)ind.Clone();

			//int conns = res.Size;	// overall number of connections for individual with given index
			//int inputs = res.Network.InputIds.Count, outputs = res.Network.OutputIds.Count;
			//int hidden = res.Network.HiddenNodesCount,
			//    neurons=inputs+outputs+hidden;
			int start, finish;
			// start and finish are GLOBAL neural indices

			//
			// Feedback is disabled

			// Add input connection for the new neuron
			var nonOuts = res.NonOutputNodes;
			var node = pars.UseNodeDegrees? SelectByActivity(nonOuts) : VectorMath.GetRandomElement(nonOuts.ToArray());
			start = node.Index;

			//do {	// connection can not start from output neuron
			//    start = (int)(pars.RNG.NextDouble() * neurons);
			//} while (res.Network.OutputIds.Contains(start));
			//} while ((res.Network.OutputIds.Contains(start)) || (mut->getNeuronTag(start) == ISOLATED_NEURON));
			// todo: decide whether node tag should really be used.
			
			finish = res.GetMaxNodeIndex() + 1;
			var weight = (float)(pars.RNG.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);

			var tempEdge = new Edge(start, finish, weight);
			res.Edges.Add(tempEdge);
			//int layer = mut->getNeuronTag (start) + 1;	// layer of the new neuron

			//
			// Add output connection for the new neuron
			var nonIns = res.NonInputNodes;
			start = finish;	// the 2nd connection should start from the newly added node.
			do {	// connection can not finish in the layer preceeding or equal to start neuron layer
				node = pars.UseNodeDegrees? SelectByActivity(nonIns) : VectorMath.GetRandomElement(nonIns.ToArray());
				finish = node.Index;
			} while (finish == start);

			weight = (float)(pars.RNG.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);

			tempEdge = new Edge(start, finish, weight);
			res.Edges.Add(tempEdge);

			//
			// Rebuild network.
			res.BuildNetwork();
			//res.Network.RebuildNetwork(res.Edges);

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which modifies a weight of the network of the given individual.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static NevaInd MutateWeight(NevaInd ind, NevaParameters pars)
		{
			var res = (NevaInd)ind.Clone();

			if (res.Size > 0)
			{
				var temp = (int)(pars.RNG.NextDouble() * res.Size);
				res.Edges[temp].Weight += (float)(pars.RNG.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);
				res.BuildNetwork();
				//res.Network.RebuildNetwork(res.Edges);
			}

			return res;
		} 
		#endregion
		#endregion

		#region - Operations for collection of [Edge] objects. -
		/// <summary>
		/// Gambles edges from the given list between two graphs with respect to graph structure.
		/// </summary>
		/// <param name="list">Given list of edges.</param>
		/// <param name="res1">Edges won for the 1st graph.</param>
		/// <param name="res2">Edges won for the 2nd graph.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <param name="nodes2">Initial number of nodes in the 2nd graph.</param>
		public static void GambleEdgeList(List<Edge> list, out List<Edge> res1, out List<Edge> res2, NevaParameters pars, int nodes2)
		{
			res1 = new List<Edge>();
			res2 = new List<Edge>();
			var list1 = new List<Edge> (list);
			for (; list1.Count > 0; )
			{
				if (list1[0].BeginIdx >= (nodes2) || list1[0].EndIdx >= (nodes2))
				{
					var flag = false;

					// parent 2 has not such a neuron, so inherit
					// neuron with all its input/output connections
					if (list1[0].BeginIdx >= (nodes2))
					{
						flag = true;
						var temp = list1[0].BeginIdx;
						//				fout<<"Inherit neuron "<<temp<<"\n";
						// todo: check whether this operation really leads to the boolean 'choice'.
						if (pars.RNG.Next(0, 2) > 0)
						{
							// First child wins!
							res1.Add(list1[0]);
							for (int i = 1; i < list1.Count; i++)
							{
								if (list1[0].BeginIdx == temp || list1[0].EndIdx == temp)
								{
									res1.Add(list1[i]);
									list1.RemoveAt(i);
									i--;	// step back
								}
							}
						}
						else
						{
							// Second child wins!
							res2.Add(list1[0]);
							for (int i = 1; i < list1.Count; i++)
							{
								if (list1[0].BeginIdx == temp || list1[0].EndIdx == temp)
								{
									res2.Add(list1[i]);
									list1.RemoveAt(i);
									i--;	// step back
								}
							}
						}
					}	// end of if (list1[0].Start >= (neurons2))

					if (list1[0].EndIdx >= (nodes2))
					{
						var temp = list1[0].EndIdx;
						if (pars.RNG.Next(2) > 0)
						{	// if true.
							// First child wins
							if (!flag)
							{
								res1.Add(list1[0]);
							}
							for (int i = 1; i < list1.Count; i++)
							{
								if (list1[i].BeginIdx == temp || list1[i].EndIdx == temp)
								{
									res1.Add(list1[i]);
									list1.RemoveAt(i);
									i--;	// step back
								}
							}
						}
						else
						{
							// Second child wins
							if (!flag)
							{
								res2.Add(list1[0]);
							}
							for (int i = 1; i < list1.Count; i++)
							{
								if (list1[i].BeginIdx == temp || list1[i].EndIdx == temp)
								{
									res2.Add(list1[i]);
									list1.RemoveAt(i);
									i--;	// step back
								}
							}
						}
					}	// end of if (list1[0].Finish >= (neurons2))
				}
				else
				{
					// gamble single connection
					// inherit connection without loop arrival check
					if (pars.RNG.Next(2) > 0)
					{
						res1.Add(list[0]);
					}
					else
					{
						res2.Add(list[0]);
					}
				}	// end of if if (list1[0].Start >= (neurons2) || list1[0].Finish >= (neurons2))
				list1.RemoveAt(0);
			}
		}
		#endregion

		#region - Operations for collection of [FlexNode] objects. -
		/// <summary>
		/// Probabilistic selection of node from the given list according to the nodes' total activity values using roulette-like procedure.
		/// </summary>
		/// <param name="nodes">Input list of nodes.</param>
		/// <returns>Selected node.</returns>
		public static FlexNode SelectByActivity (List<FlexNode> nodes)
		{
			var offset = 0.1f;	// in order to make it possible for isolated and 'silent' nodes to be selected.
			var acts = new float [nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				acts[i] = nodes[i].TotalActivity + offset;
			}

			var selIdx = VectorMath.Roulette(acts);
			return nodes[selIdx];
		}

		/// <summary>
		/// Probabilistic selection of node from the given list contrary to the nodes' total activity values using roulette-like procedure.
		/// </summary>
		/// <param name="nodes">Input list of nodes.</param>
		/// <returns>Selected node.</returns>
		public static FlexNode ReverseSelectByActivity(List<FlexNode> nodes)
		{
			var acts = new float[nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				acts[i] = nodes[i].TotalActivity;
			}

			//
			// reverse the activity array to increase the probability of the selection of least active nodes.
			var max = VectorMath.Max(acts);
			var min = VectorMath.Min(acts);
			for (int i = 0; i < acts.Length; i++)
			{
				acts[i] = max - acts[i] + min;
			}

			var selIdx = VectorMath.Roulette(acts);
			return nodes[selIdx];
		}
		#endregion

		#region - Operations for network sampling. -
		/// <summary>
		/// [molecule]
		/// [not used as of 2011/01/12]
		/// 
		/// Performs probabilistic selection of the node index to define connection beginning using node activity information.
		/// </summary>
		/// <param name="ind">Individual, corresponding to ANN.</param>
		/// <returns>Node index.</returns>
		public static int SelectConnBegin(NevaInd ind)
		{
			var nonOuts = ind.NonOutputNodes;
			var selNode = SelectByActivity(nonOuts);
			return selNode.Index;
		}

		/// <summary>
		/// [molecule]
		/// [not used as of 2011/01/12]
		/// 
		/// Performs probabilistic selection of the node index to define connection end using node activity information.
		/// </summary>
		/// <param name="ind">Individual, corresponding to ANN.</param>
		/// <returns>Node index.</returns>
		public static int SelectConnEnd(NevaInd ind)
		{
			var nonIns = ind.NonInputNodes;
			var selNode = SelectByActivity(nonIns);
			return selNode.Index;
		}
		#endregion

		#region - Multiple runs. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs multiple runs of the NEvA algorithms with specified parameters setting.
		/// </summary>
		/// <param name="pars">Parameters setting.</param>
		/// <param name="runsCount">Number of runs.</param>
		/// <param name="bests">List of best individual found at each run.</param>
		/// <returns>Averaged runs statistics.</returns>
		public static List<Stats> Run (NevaParameters pars, int runsCount, out List<NevaInd> bests)
		{
			var stats = new List<List<Stats>>();
			bests = new List<NevaInd>();
			for (int i = 0; i < runsCount; i++)
			{
				var neva = new Neva();
				neva.Run(pars);
				stats.Add(neva.FitnessStats);
				bests.Add(neva.BestIndividual);
			}

			var avgStats = StructMath.Average(stats);
			return avgStats;
		}
		#endregion
	}

	public class NevaParameters : EAParameters
	{
		public NeuralObjectiveFunction FitnessFunction { get; set; }

		public Neva Algorithm { get; set; }

		/// <summary>
		/// Defines the size of the window (number of generations) to track average number of nodes.
		/// </summary>
		public int NodesWindowSize { get; set; }
		/// <summary>
		/// Defines number of generations to lock (disable) mutations to change number of hidden nodes.
		/// </summary>
		public int NodesMutationLockTime { get; set; }
		/// <summary>
		/// Defines threshold, which enables node mutation locking.
		/// </summary>
		public float NodesMutationLockThreshold { get; set; }
		/// <summary>
		/// Use competition between parent and child.
		/// </summary>
		public bool UseParentChildCompetition { get; set; }
		public bool UseElitism { get; set; }

		/// <summary>
		/// Indicates whether nodes' degrees information is used when new nodes and connections are added.
		/// </summary>
		public bool UseNodeDegrees { get; set; }

		/// <summary>
		/// Indicates whether new node is added without any connections.
		/// </summary>
		public bool UseAddSingleNode { get; set; }

		/// <summary>
		/// Indicates whether information on previous usage of operators to be used.
		/// </summary>
		public bool UseSmartOperators { get; set; }

		public void Print (TextWriter writer)
		{
			FileIO.WriteAllLines(writer, ToStrings());
			//FileIO.WriteAllLines(writer, ToStrings());
			//writer.WriteLine("FitnessFunction:\t{0}", FitnessFunction.Name);
			//writer.WriteLine("Generations number:\t{0}", GenerationsNumber);
			//writer.WriteLine("Population size:\t{0}", PopulationSize);
			//writer.WriteLine("TournamentSize:\t{0}", TournamentSize);
			//writer.WriteLine("NodesWindowSize:\t{0}", NodesWindowSize);
			//writer.WriteLine("NodesMutationLockTime:\t{0}", NodesMutationLockTime);
			//writer.WriteLine("NodesMutationLockThreshold:\t{0}", NodesMutationLockThreshold);
			//writer.WriteLine("UseElitism:\t{0}", UseElitism);
			//writer.WriteLine("UseAddSingleNode:\t{0}", UseAddSingleNode);
			//writer.WriteLine("UseParentChildCompetition:\t{0}", UseParentChildCompetition);
			//writer.WriteLine("UseNodeDegrees:\t{0}", UseNodeDegrees);
		}

		public new List<string> ToStrings ()
		{
			var lines = base.ToStrings();
			lines.Add(string.Format("FitnessFunction:\t{0}", FitnessFunction.Name));
			lines.Add(string.Format("Generations number:\t{0}", GenerationsNumber));
			lines.Add(string.Format("Population size:\t{0}", PopulationSize));
			lines.Add(string.Format("TournamentSize:\t{0}", TournamentSize));
			lines.Add(string.Format("NodesWindowSize:\t{0}", NodesWindowSize));
			lines.Add(string.Format("NodesMutationLockTime:\t{0}", NodesMutationLockTime));
			lines.Add(string.Format("NodesMutationLockThreshold:\t{0}", NodesMutationLockThreshold));
			lines.Add(string.Format("UseElitism:\t{0}", UseElitism));
			lines.Add(string.Format("UseAddSingleNode:\t{0}", UseAddSingleNode));
			lines.Add(string.Format("UseParentChildCompetition:\t{0}", UseParentChildCompetition));
			lines.Add(string.Format("UseNodeDegrees:\t{0}", UseNodeDegrees));
			return lines;
		}
	}

	public class NevaContents
	{
		protected List<AbstractIndividual> popul = new List<AbstractIndividual>();
		protected List<AbstractIndividual> selPopul = new List<AbstractIndividual>();
		protected List<Stats> stats = new List<Stats>();	// run statistics.
		protected AbstractIndividual bestInd;

		#region - Properties. -
		/// <summary>
		/// Objective function to be used to evaluate individuals.
		/// </summary>
		public NeuralObjectiveFunction FitnessFunction { get; set; }

		/// <summary>
		/// Best individual ever found during the algorithm run.
		/// </summary>
		public AbstractIndividual BestIndividual
		{
			get { return bestInd; }
			set { bestInd = value; }
		}

		/// <summary>
		/// Fitness statistics.
		/// </summary>
		public List<Stats> FitnessStats { get { return stats; } }

		/// <summary>
		/// Main population.
		/// </summary>
		public List<AbstractIndividual> Popul
		{
			get { return popul; }
			set { popul = value; }
		}

		/// <summary>
		/// Population after selection.
		/// </summary>
		public List<AbstractIndividual> SelPopul
		{
			get { return selPopul; }
			set { selPopul = value; }
		}

		/// <summary>
		/// Contents statistics.
		/// </summary>
		public List<Stats> Stats
		{
			get { return stats; }
			set { stats = value; }
		}

		/// <summary>
		/// Indicates the number of generations for nodes mutation lock.
		/// </summary>
		public int NodesMutationLockCount{get; set;}

		/// <summary>
		/// Current generation number.
		/// </summary>
		public int GenerationNumber { get; set; }
		#endregion

		#region - Initialization. -
		public virtual void Init (NevaParameters pars)
		{
			popul = NevaElements.CreatePopulation(pars);
			bestInd = default(AbstractIndividual);	// reset best individual ...
			stats.Clear();	// ... and stats

			NodesMutationLockCount = 0;
			GenerationNumber = 0;

			FitnessFunction = pars.FitnessFunction;
		}
		#endregion

		#region - Utility methods. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns mean number of hidden nodes in the [Popul] container.
		/// </summary>
		/// <returns></returns>
		public float GetMeanHiddenNodes ()
		{
			var hiddenNodes = new int[Popul.Count];
			for (int i = 0; i < Popul.Count; i++)
			{
				var ind = (NevaInd)Popul[i];
				hiddenNodes[i] = ind.Network.HiddenNodesCount;
			}
			var nodesStats = VectorMath.CalculateStats(hiddenNodes);
			return nodesStats.Mean;
		}
		#endregion
	}
}
