﻿/*************************************************************************
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentalAlchemy.Atoms;
using System.IO;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// 
	/// 
	/// This class should substitute the [FlexibleNeuralNetwork] class, after some time.
	/// </summary>
	[Serializable]
	public class FlexibleNeuralNetwork2 : IVectorFunction
	{
		#region - Protected properties. -
		public const float DEFAULT_WEIGHT_RANGE = 0.2f;
		public const float DEFAULT_WEIGHT_MIN = -0.1f;

		protected List<FlexNode> nodes = new List<FlexNode>();
		#endregion

		#region - Public properties. -
		public List<Edge> Edges = new List<Edge>();

		/// <summary>
		/// List of activation functions for all the non-input nodes indexed by the node ID.
		/// </summary>
		public Dictionary<int, ActivationFunction> Activations { get; set; }

		/// <summary>
		/// List of IDs for input signals.
		/// </summary>
		public List<int> InputIds { get; set; }
		
		/// <summary>
		/// List of IDs for output signals.
		/// </summary>
		public List<int> OutputIds { get; set; }

		/// <summary>
		/// Returns all nodes' indices that correspond to the non-input nodes.
		/// </summary>
		public List<int> NonInputIndices
		{
			get
			{
				var res = new List<int>(Activations.Keys.ToArray());
				return res;
			}
		}

		/// <summary>
		/// Returns all non-input nodes.
		/// </summary>
		public List<FlexNode> NonInputNodes
		{
			get
			{
				var res = new List<FlexNode>();
				foreach (var node in nodes)
				{
					if (!InputIds.Contains(node.Index))
					{
						res.Add(node);
					}
				}
				return res;
			}
		}

		/// <summary>
		/// Returns all nodes' indices that correspond to the non-output nodes.
		/// </summary>
		public List<int> NonOutputIndices
		{
			get
			{
				var res = new List<int>();
				foreach (var node in nodes)
				{
					if (!OutputIds.Contains(node.Index))
					{
						res.Add(node.Index);
					}
				}
				return res;
			}
		}

		/// <summary>
		/// Returns all non-output nodes.
		/// </summary>
		public List<FlexNode> NonOutputNodes
		{
			get
			{
				var res = new List<FlexNode>();
				foreach (var node in nodes)
				{
					if (!OutputIds.Contains(node.Index))
					{
						res.Add(node);
					}
				}
				return res;
			}
		}

		/// <summary>
		/// Returns number of the hidden nodes.
		/// </summary>
		public int HiddenNodesCount { get; set; }

		/// <summary>
		/// Returns number of connections in the network.
		/// 
		/// NOTE: Currently the number is computed as sum of inputs
		///		of non-input nodes. But in general case this is not
		///		very feasible, because some nodes can have the same
		///		output signals.
		/// </summary>
		public int ConnectionsCount
		{
			get 
			{
				return Edges.Count;
			}
		}

		/// <summary>
		/// Activation function which is assigned to the hidden nodes by default.
		/// </summary>
		public ActivationFunction DefaultActivationFunction { get; set; }

		/// <summary>
		/// Activation function which is assigned to the output nodes by default.
		/// </summary>
		public ActivationFunction DefaultOutputActivation { get; set; }
		#endregion

		#region - [INeuralNetwork] interface implementation. -
		public void GetOutputs(out float[] outputs)
		{
			outputs = new float[OutputIds.Count];
			for (int i = 0; i < OutputIds.Count; i++)
			{
				var id = OutputIds[i];
				outputs[i] = GetNode(id).Output;
			}
		}

		public void Calculate(float[] inputs)
		{
			var input = new Dictionary<int, float>();
			for (int i = 0; i < InputIds.Count; i++)
			{
				input.Add(InputIds[i], inputs[i]);
			}
			CalculateOutput(input, false);
		}

		public object Clone() { return new FlexibleNeuralNetwork2(this); }
		#endregion

		#region - Construction. -
		public FlexibleNeuralNetwork2()
		{
			DefaultOutputActivation = ActivationFunctions.Linear;
		}

		public FlexibleNeuralNetwork2(List<int> inIds, List<int> outIds)
		{
			InputIds = inIds;
			OutputIds = outIds;

			// Create edges.
			Edges = new List<Edge>();
			for (int i = 0; i < InputIds.Count; i++)
			{
				for (int j = 0; j < OutputIds.Count; j++)
				{
					var edge = new Edge();
					edge.BeginIdx = InputIds[i];
					edge.EndIdx = OutputIds[j];
					edge.Weight = (float)(ContextRandom.NextDouble() * DEFAULT_WEIGHT_RANGE - DEFAULT_WEIGHT_MIN);
					edge.Enabled = true;

					Edges.Add(edge);
				}
			}

			// Create activations.
			DefaultOutputActivation = ActivationFunctions.Linear;
			DefaultActivationFunction = ActivationFunctions.Sigmoid;
			Activations = new Dictionary<int, ActivationFunction>();
			for (int i = 0; i < OutputIds.Count; ++i )
			{
				Activations.Add(OutputIds[i], DefaultOutputActivation);
			}

			BuildNetwork();
		}

		public FlexibleNeuralNetwork2(FlexibleNeuralNetwork2 net)
		{
			foreach (var node in net.nodes)
			{
				nodes.Add(new FlexNode(node));
			}
			InputIds = new List<int>(net.InputIds);
			OutputIds = new List<int>(net.OutputIds);
			DefaultActivationFunction = net.DefaultActivationFunction;
			DefaultOutputActivation = net.DefaultOutputActivation;
			HiddenNodesCount = net.HiddenNodesCount;

			Edges = new List<Edge>();
			foreach (var edge in net.Edges)
			{
				Edges.Add(new Edge(edge));
			}

			Activations = new Dictionary<int,ActivationFunction> ();
			foreach (var act in net.Activations)
			{
				Activations.Add(act.Key, act.Value);
			}
		}
		#endregion

		#region - Public methods. -
		#region - Network state calculation. -
		/// <summary>
		/// [molecule]
		/// 
		/// Calculates output signals of the network.
		/// </summary>
		/// <param name="env">Environmental state.</param>
		/// <param name="updateInputs">Defines whether inputs signals can be updated by the network.</param>
		/// <returns>New environmental state.</returns>
		public Dictionary<int, float> CalculateOutput(Dictionary<int, float> env, bool updateInputs)
		{
			//
			// todo:
			// 1. Reset all nodes outputs (set to 0).
			// 2. Introduce input signals.
			// ---- k times. ----
			// 3. For each node: 
			//	3.0. [sum] <-- 0
			//	3.1. For each input:
			//		3.1.1. Update [sum]
			//	3.2. Calculate node's output.
			// ---- k times. ----
			//
			// k -- number of iterations for the network update.
			// ways of calculation of k:
			//	- max path length from input to output.
			//	- min path length from input to output.
			//	- k is defined dynamically by the stabilization of output signals (optimal for feedforward networks, but may be unpredictable for recurrent networks).

			//
			// 1. Reset all nodes outputs (set to 0).
			ResetOutputs();

			var newEnv = GetCurrentState();
			StructMath.PartialCopy(env, ref newEnv, InputIds);

			//
			// Note that number of iterations equals to the number of nodes. This is an upper bound for the feedforward ANN and thus ineffective.
			for (int i = 0; i < nodes.Count; i++)
			{
				// todo: test.
				var tempEnv = UpdateOneStep (newEnv);

				//
				// copy input signals from [env] into [newEnv].
				if (!updateInputs)
				{
					StructMath.PartialCopy(env, ref tempEnv, InputIds);
				}

				var dist = StructMath.DistanceEuclidianNoKeyMatch(tempEnv, newEnv);
				if (dist == 0f) break;	// the network output is stabilized.
				if (dist == float.PositiveInfinity) throw new Exception("[CalculateOutput] exception: Infinite distance between environment states!");

				//
				// update environment state.
				newEnv = new Dictionary<int, float>(tempEnv);

			}
			return newEnv;
		}
		
		/// <summary>
		/// Updates outputs of all nodes using current environment state.
		/// The method does not reset the nodes states and does not make any assumptions about
		/// the context of the method's usage.
		/// <remarks>In fact this summary is larger than the method itself.</remarks>
		/// </summary>
		/// <param name="env"></param>
		/// <returns></returns>
		public Dictionary<int, float> UpdateOneStep (Dictionary<int, float> env)
		{
			var tempEnv = new Dictionary<int, float>();
			foreach (var node in nodes)
			{
				node.Calculate(env, tempEnv);
			}
			return tempEnv;
		}

		/// <summary>
		/// Sets output signals to 0.
		/// </summary>
		public void ResetOutputs()
		{
			foreach (var node in nodes)
			{
				node.Reset();
			}
		}

		/// <summary>
		/// Returns values of outputs of all nodes, including input and output ones.
		/// </summary>
		public Dictionary<int, float> GetCurrentState()
		{
			var res = new Dictionary<int, float>();
			foreach (var node in nodes)
			{
				if (!res.ContainsKey(node.Index))
				{
					res.Add(node.Index, node.Output);
				}
				else
				{
					res[node.Index] = node.Output;
				}
			}
			return res;
		} 
		#endregion

		#region - Networks structure parameters. -
		/// <summary>
		/// [molecule]
		/// 
		/// Sets up general ANN structure parameters required for building and rebuilding the network.
		/// </summary>
		/// <param name="net"></param>
		public void SetStructureParameters (FlexibleNeuralNetwork net)
		{
			DefaultActivationFunction = net.DefaultActivationFunction;
			DefaultOutputActivation = net.DefaultOutputActivation;
			InputIds = net.InputIds;
			OutputIds = net.OutputIds;
		}
		#endregion

		#region - Network structure modification. -
		public void BuildNetwork()
		{
			BuildNetwork(Edges, Activations);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Builds network using given list of connections and existing set of activation functions.
		/// Note: the input and output indices should be set prior to this function call.
		/// </summary>
		/// <param name="edgeList">List of weighted edges.</param>
		public void BuildNetwork(List<Edge> edgeList)
		{
			BuildNetwork(edgeList, Activations);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Builds network using given list of connections.
		/// Note: the input and output indices should be set prior to this function call.
		/// </summary>
		/// <param name="edgeList">List of weighted edges.</param>
		public void BuildNetwork(List<Edge> edgeList, Dictionary<int, ActivationFunction> activations)
		{
			// clear network.
			nodes.Clear();

			#region - Create input and output nodes. -
			foreach (var id in InputIds)
			{
				var node = new FlexNode(id, 0f, ActivationFunctions.Identity);
				node.Inputs.Add(new SignalSlot(id, 1f));
				nodes.Add(node);
			}

			foreach (var id in OutputIds)
			{
				//var node = new FlexNode(id, 0f, ActivationFunctions.Linear);
				var node = new FlexNode(id, 0f, DefaultOutputActivation);
				node.Outputs.Add(new SignalSlot(id, 1f));
				nodes.Add(node);
			}
			#endregion

			#region - Create hidden nodes. -
			RebuildNetwork(edgeList, activations);
			#endregion
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Rebuilds inner network structure and reassignes connection weights.
		/// Uses existing set of activation functions.
		/// If there are isolated nodes the method removes them.
		/// </summary>
		/// <param name="edgeList"></param>
		public void RebuildNetwork(List<Edge> edgeList)
		{
			RebuildNetwork(edgeList, Activations);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Rebuilds inner network structure and reassignes connection weights.
		/// If there are isolated nodes the method removes them.
		/// </summary>
		/// <param name="edgeList"></param>
		public void RebuildNetwork(List<Edge> edgeList, Dictionary<int, ActivationFunction> activations)
		{
			foreach (var node in nodes)
			{
				// prevent removal of the inputs from input nodes and outputs from output nodes.
				if (!InputIds.Contains(node.Index)) { node.Inputs.Clear(); }
				if (!OutputIds.Contains(node.Index)) { node.Outputs.Clear(); }
			}

			#region - Assign connections. -
			Edges = edgeList;
			foreach (var edge in Edges)
			{
				if (!edge.Enabled) continue;

				#region - Process starting node. -
				var startNode = GetNode(edge.BeginIdx);
				if (startNode == null)
				{
					startNode = new FlexNode(edge.BeginIdx, 0f, DefaultActivationFunction);
					startNode.Outputs.Add(new SignalSlot(edge.BeginIdx, 1f));
					nodes.Add(startNode);
				}
				else
				{
					var output = startNode.GetOutput(edge.BeginIdx);
					if (output == null)
					{
						startNode.Outputs.Add(new SignalSlot(edge.BeginIdx, 1f));
					}
				}
				#endregion

				#region - Process finishing node. -
				var finishNode = GetNode(edge.EndIdx);
				if (finishNode == null)
				{
					finishNode = new FlexNode(edge.EndIdx, 0f, DefaultActivationFunction);
					finishNode.Inputs.Add(new SignalSlot(edge.BeginIdx, edge.Weight));
					nodes.Add(finishNode);
				}
				else
				{
					var input = finishNode.GetInput(edge.BeginIdx);
					if (input != null)
					{
						input.Weight += edge.Weight;
					}
					else
					{
						finishNode.Inputs.Add(new SignalSlot(edge.BeginIdx, edge.Weight));
					}
				}
				#endregion
			}
			#endregion

			#region - Remove isolated nodes. -
			for (int i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];
				if (node.Inputs.Count == 0 && node.Outputs.Count == 0)
				{
					nodes.RemoveAt(i);
					i--;
				}
			}
			#endregion

			AssignActivations(activations);
			RecalculateHiddenNodesCount();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Recalculates the number of hidden nodes.
		/// </summary>
		public void RecalculateHiddenNodesCount()
		{
			HiddenNodesCount = 0;
			foreach (var node in nodes)
			{
				if (!OutputIds.Contains(node.Index) && !(InputIds.Contains(node.Index)))
				{
					++HiddenNodesCount;
				}
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Deletes all the nodes from the network.
		/// </summary>
		public void ClearNetwork()
		{
			nodes.Clear();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Adds a node and corrects HiddenNodesCount property if necessary.
		/// </summary>
		/// <param name="node">Node to remove.</param>
		public void AddNode(FlexNode node)
		{
			nodes.Add(node);
			if (!InputIds.Contains(node.Index) && !OutputIds.Contains(node.Index)) { HiddenNodesCount++; }
		} 

		/// <summary>
		/// [molecule]
		/// 
		/// Removes given node and corrects HiddenNodesCount property if necessary.
		/// </summary>
		/// <param name="node">Node to remove.</param>
		public void RemoveNode(FlexNode node)
		{
			nodes.Remove(node);
			if (!InputIds.Contains(node.Index) && !OutputIds.Contains(node.Index)) { HiddenNodesCount--; }
		} 

		/// <summary>
		/// [molecule]
		/// 
		/// Assigns given activation functions to the network nodes.
		/// </summary>
		/// <param name="acts"></param>
		public void AssignActivations (Dictionary<int, ActivationFunction> acts)
		{
			foreach (var node in nodes)
			{
				if (acts.ContainsKey(node.Index))
				{
					node.ActivationFunction = acts[node.Index];
				}
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Changes activation for the node with given index.
		/// </summary>
		/// <param name="idx"></param>
		/// <param name="act"></param>
		public void ChangeActivation(int idx, ActivationFunction act)
		{
			var node = GetNode(idx);
			if (node != null)
			{
				node.ActivationFunction = act;
			}
			Activations[idx] = act;
		}
		#endregion

		#region - Get/set methods. -
		/// <summary>
		/// Returns node by its index.
		/// If the node is absent then [null] is returned.
		/// </summary>
		/// <param name="idx"></param>
		/// <returns></returns>
		public FlexNode GetNode(int idx)
		{
			foreach (var node in nodes)
			{
				if (node.Index == idx) return node;
			}
			return null;
		}

		/// <summary>
		/// Returns max index of the node.
		/// </summary>
		/// <returns>Max index of the node.</returns>
		public int GetMaxNodeIndex ()
		{
			if (Activations != null && Activations.Count > 0)
			{
				var ids = Activations.Keys.ToArray();
				return VectorMath.Max(ids);
			}

			// otherwise there are no hidden nodes.
			// look through input and output nodes.
			var res = nodes[0].Index;
			for (int i = 1; i < nodes.Count; i++)
			{
				if (res < nodes[i].Index) res = nodes[i].Index;
			}

			return res;
		}
		#endregion

		#region - Utility methods. -
		/// <summary>
		/// Returns matrix of network weights. Each non-zero element at position (i,j) indicates that
		/// there's a connection from node i to node j with a weight equal to the matrix element.
		/// </summary>
		/// <returns>Weights matrix.</returns>
		public float[,] ToWeightsMatrix ()
		{
			var size = GetMaxNodeIndex() + 1;
			var res = new float[size, size];

			foreach (var node in nodes)
			{
				foreach (var input in node.Inputs)
				{
					res[input.Id, node.Index] = input.Weight;
				}
			}

			return res;
		}

		/// <summary>
		/// Prints matrix of weights into the specified stream.
		/// </summary>
		/// <param name="writer"></param>
		public void Print (TextWriter writer)
		{
			//
			// write individual's info.
			writer.WriteLine("\n> Connections count:\t{0}", ConnectionsCount);
			writer.WriteLine("Connections info:\tstart\tfinish\tweight\tis enabled");
			foreach (var gene in Edges)
			{
				var line = String.Format("{0}\t{1}\t{2}\t{3}", gene.BeginIdx, gene.EndIdx, gene.Weight, gene.Enabled);
				writer.WriteLine(line);
			}

			writer.WriteLine("\n> Network info:");
			writer.WriteLine("\n> Hidden nodes count:\t{0}", HiddenNodesCount);

			var wMatrix = ToWeightsMatrix();
			var rows = MatrixMath.ConvertToRowsList(wMatrix);
			var lines = MatrixMath.ConvertToStringsList(rows);

			writer.WriteLine("\n> Network's weights matrix:");
			FileIO.WriteColumns(writer, lines, null);

			//var cfactor = NevaElements.CalculateConnFactor(this);
			//var nfactor = NevaElements.CalculateNodeFactor(this);
			//writer.WriteLine(String.Format("\nConn factor:\t{0}", cfactor));
			//writer.WriteLine(String.Format("Node factor:\t{0}", nfactor));
		}
		#endregion

		#region - Export. -
		/// <summary>
		/// [molecule]
		/// 
		/// Converts ANN to the DOT-format for graphs representation.
		/// </summary>
		/// <param name="net"></param>
		/// <returns></returns>
		public static string[] ConvertToDotFormat (FlexibleNeuralNetwork2 net)
		{
			const string CONN_STR = "{0} -> {1};";
			var res = new List<string>();

			// add standard heading for directed graph.
			res.Add("digraph graphname {");

			var ins = net.InputIds;
			var outs = net.OutputIds;
			foreach (var node in net.nodes)
			{
				if (outs.Contains(node.Index)) continue;	// skip output nodes.

				var beginNodeId = node.Index;
				foreach (var signalSlot in node.Outputs)
				{
					var inputId = signalSlot.Id;
					foreach (var flexNode in net.nodes)
					{
						if (ins.Contains(flexNode.Index)) continue;	// skip input nodes.

						if (flexNode.HasInput(inputId))
						{
							res.Add(string.Format(CONN_STR, beginNodeId, flexNode.Index));
						}
					}	// foreach (var flexNode in net.nodes)
				}	// foreach (var signalSlot in node.Outputs)
			}	// foreach (var node in net.nodes)

			// add closing bracket.
			res.Add("}");

			return res.ToArray();
		}
		#endregion
		#endregion

		//#region - Export. -
		///// <summary>
		///// [molecule]
		///// 
		///// Converts ANN to the DOT-format for graphs representation.
		///// </summary>
		///// <param name="net"></param>
		///// <returns></returns>
		//public static string[] ConvertToDotFormat(FlexibleNeuralNetwork2 net)
		//{
		//	const string CONN_STR = "{0} -> {1};";
		//	var res = new List<string>();

		//	// add standard heading for directed graph.
		//	res.Add("digraph graphname {");

		//	var ins = net.InputIds;
		//	var outs = net.OutputIds;
		//	foreach (var node in net.nodes)
		//	{
		//		if (outs.Contains(node.Index)) continue;	// skip output nodes.

		//		var beginNodeId = node.Index;
		//		foreach (var signalSlot in node.Outputs)
		//		{
		//			var inputId = signalSlot.Id;
		//			foreach (var flexNode in net.nodes)
		//			{
		//				if (ins.Contains(flexNode.Index)) continue;	// skip input nodes.

		//				if (flexNode.HasInput(inputId))
		//				{
		//					res.Add(string.Format(CONN_STR, beginNodeId, flexNode.Index));
		//				}
		//			}	// foreach (var flexNode in net.nodes)
		//		}	// foreach (var signalSlot in node.Outputs)
		//	}	// foreach (var node in net.nodes)

		//	// add closing bracket.
		//	res.Add("}");

		//	return res.ToArray();
		//}
		//#endregion
	}
}
