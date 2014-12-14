using System;
using System.Collections.Generic;
using System.IO;
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	public class NevaInd : AbstractIndividual
	{
		#region - Protected fields. -
		private FlexibleNeuralNetwork net = new FlexibleNeuralNetwork();
		private List<Edge> edges = new List<Edge>(); 
		#endregion

		#region - Public properties. -
		public FlexibleNeuralNetwork Network
		{
			get { return net; }
			set { net = value; }
		}

		/// <summary>
		/// All non-input nodes of the [Network] including isolated ones (from [Activations]).
		/// </summary>
		public List<FlexNode> NonInputNodes
		{
			get
			{
				var nonIns = Network.NonInputIndices;
				var res = Network.NonInputNodes;
				foreach (var pair in Activations)
				{	// if there are some isolated hidden nodes, then add them to the resulting list too.
					if (!nonIns.Contains(pair.Key))
					{
						res.Add(new FlexNode(pair.Key, 0f, pair.Value));
					}
					//res.Add(nonIns.Contains(pair.Key)
					//        ? Network.GetNode(pair.Key)
					//        : new FlexNode(pair.Key, 0f, pair.Value));
				}

				return res;
			}
		}

		/// <summary>
		/// All non-output nodes of the [Network] including isolated ones (from [Activations]).
		/// </summary>
		public List<FlexNode> NonOutputNodes
		{
			get
			{
				var nonOuts = Network.NonOutputIndices;
				var res = Network.NonOutputNodes;
				foreach (var pair in Activations)
				{
					if (Network.OutputIds.Contains(pair.Key)) continue;

					if (!nonOuts.Contains(pair.Key))
					{	// if there are some isolated hidden nodes, then add them to the resulting list too.
						res.Add(new FlexNode(pair.Key, 0f, pair.Value));
					}
					//res.Add(nonOuts.Contains(pair.Key)
					//        ? Network.GetNode(pair.Key)
					//        : new FlexNode(pair.Key, 0f, pair.Value));
				}

				return res;
			}
		}

		/// <summary>
		/// All non-input nodes' indices of the [Network] including isolated ones (from [Activations]).
		/// </summary>
		public List<int> NonInputIndices
		{
			get
			{
				var nonIns = Network.NonInputIndices;
				foreach (var key in Activations.Keys)
				{	// if there are some isolated hidden nodes, then add them to the resulting list too.
					if (!nonIns.Contains(key))
					{
						nonIns.Add(key);
					}
				}

				return nonIns;
			}
		}

		/// <summary>
		/// All non-output nodes' indices of the [Network] including isolated ones (from [Activations]).
		/// </summary>
		public List<int> NonOutputIndices
		{
			get
			{
				var nonOuts = Network.NonOutputIndices;
				foreach (var key in Activations.Keys)
				{
					if (Network.OutputIds.Contains(key)) continue;

					if (!nonOuts.Contains(key))
					{	// if there are some isolated hidden nodes, then add them to the resulting list too.
						nonOuts.Add(key);
					}
				}

				return nonOuts;
			}
		}

		/// <summary>
		/// Indices of hidden nodes.
		/// </summary>
		public List<int> HiddenNodesIndices
		{
			get
			{
				var nonIns = Network.NonInputIndices;
				var outs = Network.OutputIds;
				foreach (var outId in outs)
				{
					nonIns.Remove(outId);
				}
				return nonIns;
			}
		}

		/// <summary>
		/// List of activation functions for all non-input nodes indexed by node ID.
		/// </summary>
		public Dictionary<int, ActivationFunction> Activations { get; set; }

		public List<Edge> Edges
		{
			get { return edges; }
			set { edges = value; }
		}

		public override int Size
		{
			get { return edges.Count; }
		}
		#endregion

		#region - Construction. -
		public NevaInd()
		{
			Fitness = new Fitness();
		}
		public NevaInd(NevaInd ind)
		{
			Assign(ind);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates individual using specified input and output signals' IDs and NEvA parameters.
		/// </summary>
		/// <param name="inputs">Input signals IDs.</param>
		/// <param name="outputs">Output signals IDs.</param>
		/// <param name="weightRange">Range of weights values.</param>
		/// <param name="weightMin">Minimal weight value.</param>
		/// <param name="rng">RNG.</param>
		/// <returns>New individual without hidden nodes and with random connection weights.</returns>
		public NevaInd(List<int> inputs, List<int> outputs, float weightRange, float weightMin, Random rng)
		{
			for (int i = 0; i < inputs.Count; i++)
			{
				for (int j = 0; j < outputs.Count; j++)
				{
					var edge = new Edge();
					edge.BeginIdx = inputs[i];
					edge.EndIdx = outputs[j];
					edge.Weight = (float)(weightRange * rng.NextDouble() + weightMin);
					edge.Enabled = true;

					Edges.Add(edge);
				}
			}

			//
			// Create and build network.
			Network = new FlexibleNeuralNetwork();
			Network.InputIds = inputs;
			Network.OutputIds = outputs;
			Network.DefaultActivationFunction = ActivationFunctions.Sigmoid;
			Network.BuildNetwork(Edges, weightRange, weightMin, rng);
		}
		#endregion

		#region - Public methods. -
		public override AbstractIndividual Clone() { return new NevaInd(this); }

		public virtual void Assign(NevaInd ind)
		{
			// copy edges.
			edges.Clear();
			foreach (var edge in ind.edges)
			{
				edges.Add(edge.Clone());
			}

			// copy fitness.
			Fitness = ind.Fitness.Clone();

			if (ind.Network != null)
			{
				Network = new FlexibleNeuralNetwork(ind.Network);
			}
		}

		public bool EqualsTo (NevaInd argInd) 
		{
			if (edges.Count == argInd.Edges.Count) {
				int size1 = Edges.Count, size2 = argInd.Edges.Count;
				for (int i=0; i<size1; i++) 
				{
					int j = 0;
					for (; j<size2; j++) {
						if (	(edges[i].BeginIdx	== argInd.Edges[j].BeginIdx)
						    	&&(edges[i].EndIdx == argInd.Edges[j].EndIdx)
						    	&& (edges[i].Weight == argInd.Edges[j].Weight))
						{
							break;
						}
					}
					if (j == size2) 
					{	// if no equal edges are found.
						return false;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Writes information on individual into the specified stream.
		/// </summary>
		/// <param name="writer">Stream to write to.</param>
		public void Print (TextWriter writer)
		{
			//
			// write individual's info.
			writer.WriteLine("\n> Individual size:\t{0}", Size);
			writer.WriteLine("Gene output format:\tstart\tfinish\tweight\tis enabled");
			foreach (var gene in edges)
			{
				var line = String.Format("{0}\t{1}\t{2}\t{3}", gene.BeginIdx, gene.EndIdx, gene.Weight, gene.Enabled);
				writer.WriteLine(line);
			}
			

			if (Network != null)
			{
				writer.WriteLine("\n> Network info:");
				Network.Print(writer);

				var cfactor = NevaElements.CalculateConnFactor(this);
				var nfactor = NevaElements.CalculateNodeFactor(this);
				writer.WriteLine(String.Format("\nConn factor:\t{0}", cfactor));
				writer.WriteLine(String.Format("Node factor:\t{0}", nfactor));
			}
		}

		/// <summary>
		/// Builds neural network using individual's info.
		/// </summary>
		public virtual void BuildNetwork ()
		{
			net.BuildNetwork(edges);
			Network.AssignActivations(Activations);
		}

		/// <summary>
		/// Returns maximal existing node index.
		/// </summary>
		/// <returns></returns>
		public int GetMaxNodeIndex ()
		{
			var res = Network.GetMaxNodeIndex();
			foreach (var key in Activations.Keys)
			{	// also look through isolated nodes.
				if (key > res) { res = key;}
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Changes activation for the node with given index.
		/// </summary>
		/// <param name="idx"></param>
		/// <param name="act"></param>
		public void ChangeActivation (int idx, ActivationFunction act)
		{
			var node = Network.GetNode(idx);
			if (node != null)
			{
				node.ActivationFunction = act;
			}
			Activations[idx] = act;
		}
		#endregion

		#region - Public static methods. -
		//public static NevaInd Create (List<int> inputs, List<int> outputs, float weightRange, float weightMin, Random rng)
		//{
		//    var res = new NevaInd();
		//    for (int i = 0; i < inputs.Count; i++)
		//    {
		//        for (int j = 0; j < outputs.Count; j++)
		//        {
		//            var edge = new Edge();
		//            edge.BeginIdx = inputs[i];
		//            edge.EndIdx = outputs[j];
		//            edge.Weight = (float)(weightRange * rng.NextDouble() + weightMin);
		//            edge.Enabled = true;

		//            res.Edges.Add(edge);
		//        }
		//    }

		//    //
		//    // Create and build network.
		//    res.Network = new FlexibleNeuralNetwork();
		//    res.Network.InputIds = inputs;
		//    res.Network.OutputIds = outputs;
		//    res.Network.DefaultActivationFunction = ActivationFunctions.Sigmoid;
		//    res.Network.BuildNetwork(res.Edges, weightRange, weightMin, rng);

		//    return res;
		//}
		#endregion
	}
}