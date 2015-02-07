/*************************************************************************
    This file is part of the MentalAlchemy library.

    MentalAlchemy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentalAlchemy.Atoms;
using GdpPolicyParameters = MentalAlchemy.Molecules.NevaParameters;
using StructuredObject = MentalAlchemy.Molecules.FlexibleNeuralNetwork2;

namespace MentalAlchemy.Molecules
{
	public class GdpElements
	{
		#region - Adapted operators from NEvA. -
		/// <summary>
		/// Mutation, which adds a connection to the network of the given individual.
		/// </summary>
		/// <param name="ind">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static StructuredObject MutateAddConnection(StructuredObject net, GdpPolicyParameters pars)
		{
			var res = (FlexibleNeuralNetwork2)net.Clone();

			//var tempEdge = new Edge();
			var conns = res.ConnectionsCount;	// overall number of connections for individual with given index
			int finish;

			//
			// start and finish are GLOBAL neural indices
			//int i;
			//float weight;

			// define new connection parameters
			// Feedback is disabled
			var nonOuts = res.NonOutputNodes;
			var node = pars.UseNodeDegrees ? NevaElements.SelectByActivity(nonOuts) : VectorMath.GetRandomElement(nonOuts.ToArray());
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
				node = pars.UseNodeDegrees? NevaElements.SelectByActivity(nonIns) : VectorMath.GetRandomElement(nonIns.ToArray());
				finish = node.Index;

				if (res.OutputIds.Contains(finish))
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
		/// Mutation which deletes a random edge from the network of the given individual.
		/// </summary>
		/// <param name="net">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static StructuredObject MutateDeleteConnection(StructuredObject net, GdpPolicyParameters pars)
		{
			var res = (StructuredObject)net.Clone();
			var mutIndex = (int)(ContextRandom.NextDouble() * res.ConnectionsCount);

			if (mutIndex < res.Edges.Count)
			{
				res.Edges.RemoveAt(mutIndex);
				res.BuildNetwork();
				//res.Network.RebuildNetwork(res.Edges);
			}
			// else
			// trying to remove connection from empty ANN.
			// simply ignore.

			// remove possibly emerged isolated nodes.
			var oldIds = new List<int> (net.NonInputIndices);
			var newIds = res.NonInputIndices;
			if (oldIds.Count != newIds.Count)
			{
				// compare old and new IDs and find out which node was deleted.
				foreach (var id in newIds)
				{
					if (oldIds.Contains(id))
					{
						oldIds.Remove(id);
					}
				}

				// the indices, that are left, correspond to the isolated nodes
				// emerged after removal of the connection.
				// technically, there should be only 1 such node, but just in case:
				if (oldIds.Count > 1) { throw new Exception("MutateDeleteConnection"); }
				res.Activations.Remove(oldIds[0]);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which adds a random node to the network of the given individual.
		/// The node's activation is set by default.
		/// </summary>
		/// <param name="net">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static StructuredObject MutateAddNode(StructuredObject net, GdpPolicyParameters pars)
		{
			StructuredObject res = new StructuredObject(net);
			int nodeId = net.GetMaxNodeIndex() + 1;	// index of a newly added node.

			if (pars.UseAddSingleNode)
			{
				res.Activations.Add(nodeId, net.DefaultActivationFunction);	// set default activation.
				return res;
			}

			//
			// otherwise add node and two connections.
			res = (StructuredObject)net.Clone();

			int start, finish;

			//
			// Feedback is disabled

			// Add input connection for the new neuron
			var nonOuts = res.NonOutputNodes;
			var node = pars.UseNodeDegrees ? NevaElements.SelectByActivity(nonOuts) : VectorMath.GetRandomElement(nonOuts.ToArray());
			start = node.Index;

			finish = nodeId;
			var weight = (float)(ContextRandom.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);

			var tempEdge = new Edge(start, finish, weight);
			res.Edges.Add(tempEdge);

			//
			// Add output connection for the new neuron
			var nonIns = res.NonInputNodes;
			start = finish;	// the 2nd connection should start from the newly added node.
			do
			{	// connection can not finish in the layer preceeding or equal to start neuron layer
				node = pars.UseNodeDegrees ? NevaElements.SelectByActivity(nonIns) : VectorMath.GetRandomElement(nonIns.ToArray());
				finish = node.Index;
			} while (finish == start);

			weight = (float)(ContextRandom.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);

			tempEdge = new Edge(start, finish, weight);
			res.Edges.Add(tempEdge);
			res.Activations.Add(nodeId, res.DefaultActivationFunction);

			//
			// Rebuild network.
			res.BuildNetwork();

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Mutation which deletes a node from the network of the given individual.
		/// </summary>
		/// <param name="net">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static StructuredObject MutateDeleteNode(StructuredObject net, GdpPolicyParameters pars)
		{
			var res = (StructuredObject)net.Clone();

			if (res.HiddenNodesCount > 0)
			{
				int conns = res.ConnectionsCount;	// overall number of connections for individual with given index.
				int i;

				//
				// select neuron to delete
				var nonOuts = res.NonOutputNodes;
				var node = pars.UseNodeDegrees ? NevaElements.ReverseSelectByActivity(nonOuts) : VectorMath.GetRandomElement(nonOuts.ToArray());
				int mutIndex = node.Index;

				//
				// delete connections associated with the deleted neuron.
				for (i = 0; i < res.ConnectionsCount; i++)
				{
					if ((res.Edges[i].BeginIdx == mutIndex) || (res.Edges[i].EndIdx == mutIndex))
					{
						res.Edges.RemoveAt(i);
						i--;
					}
				}	// end of for (i=0; i<tempInd->Genes.GetSize(); i++)

				//
				// correct indices
				for (i = 0; i < res.ConnectionsCount; i++)
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
		/// Mutation which modifies a weight of the network of the given individual.
		/// </summary>
		/// <param name="net">Individual to mutate.</param>
		/// <param name="pars">Algorithm parameters.</param>
		/// <returns>Resulting individual.</returns>
		public static StructuredObject MutateWeight(StructuredObject net, GdpPolicyParameters pars)
		{
			var res = (StructuredObject)net.Clone();

			if (res.ConnectionsCount > 0)
			{
				var temp = (int)(ContextRandom.NextDouble() * res.ConnectionsCount);
				res.Edges[temp].Weight += (float)(ContextRandom.NextDouble() * pars.GeneValueRange + pars.MinGeneValue);
				res.BuildNetwork();
				//res.Network.RebuildNetwork(res.Edges);
			}

			return res;
		}
		#endregion

		#region - Adapted operators from NEvA-2. -
		// Note: No need to adapt:
		//	- MutateAddConnection.
		//	- MutateDeleteConnection.


		#endregion

		#region - Network stats. -
		public static float CalculateConnFactor(FlexibleNeuralNetwork2 net)
		{
			int conns = net.ConnectionsCount, inputs = net.InputIds.Count, outputs = net.OutputIds.Count,
				neurons = inputs + outputs + net.HiddenNodesCount;

			// maximum number of connections
			var maxConns = (int)((neurons * (neurons - 1) - inputs * (inputs - 1) - outputs * (outputs - 1)) * 0.5);
			var cfactor = (float)conns / maxConns;

			return cfactor * cfactor;
		}

		public static float CalculateNodeFactor(FlexibleNeuralNetwork2 net)
		{
			int inputs = net.InputIds.Count, outputs = net.OutputIds.Count,
				neurons = inputs + outputs + net.HiddenNodesCount;

			var nfactor = ((float)(inputs + outputs)) / neurons;
			nfactor *= nfactor;
			return nfactor * CalculateConnFactor(net);
		} 
		#endregion
	}
}
