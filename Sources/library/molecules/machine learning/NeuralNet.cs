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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	public class NeuralNetProperties
	{
		public bool UseBias;	// defines whether bias value is used or not.
		public int[] nodesNumber;	// number of nodes at each layer.
		public ActivationFunction[] actFunctions;	// activation functions for each layer.

		public NeuralNetProperties (){}

		public NeuralNetProperties (LayeredNeuralNetwork net)
		{
			UseBias = net.UseBias;
			
			nodesNumber = new int[net.Layers.Count];
			for (int i = 0; i < nodesNumber.Length; i++)
			{
				nodesNumber[i] = net.Layers[i].Nodes.Count;
			}

			actFunctions = new ActivationFunction[net.Layers.Count];
			for (int i = 0; i < actFunctions.Length; i++)
			{
				actFunctions[i] = net.Layers[i].Nodes[0].ActivationFunction;
			}
		}

		public List<string> ToStrings ()
		{
			var res = new List<string>();
			res.Add("Use bias:\t" + UseBias);
			res.Add("Nodes number:\t" + VectorMath.ConvertToString(nodesNumber, '\t'));

			// form list of activations.
			var acts = new List<string>();
			foreach (var function in actFunctions)
			{
				acts.Add(function.Method.Name);
			}
			res.Add("Activations:\t" + StringUtils.Concat(acts.ToArray()));
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Class for a nueral node.
	/// </summary>
	[Serializable]
	public class Node
	{
		private float _a = 1.0f;
		private float[] _weights;

		#region - Public properties. -
		/// <summary>
		/// Node's weights.
		/// </summary>
		public float[] Weights
		{
			get { return _weights; }
			set { _weights = value; }
		}
		/// <summary>
		/// Output signal value.
		/// </summary>
		public float Output { get; set; }
		
		/// <summary>
		/// Node's bias.
		/// </summary>
		public float Bias { get; set; }

		/// <summary>
		/// Node's index.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Activation function delegate.
		/// </summary>
		public ActivationFunction ActivationFunction { get; set; }
		#endregion

		/// <summary>
		/// Creates and initializes node's weights.
		/// </summary>
		/// <param name="count">Number of weights.</param>
		/// <param name="rand">RNG.</param>
		public void InitWeights (int count, Random rand) {
			Bias = (float)(0.2 * rand.NextDouble() - 0.1);

			_weights = new float[count];
			for (int i=0; i<count; ++i) {
				_weights[i] = (float)(0.2 * rand.NextDouble() - 0.1);
			}
			//a = (float)(1.0 + rand.NextDouble());
		}

		/// <summary>
		/// Calculate [Output] value.
		/// </summary>
		/// <param name="x">Input signals.</param>
		public void Calculate (float[] x)
		{
			if (ActivationFunction == null) throw new Exception("[Node.Calculate]: Undefined activation function.");

			Output = ActivationFunction(_weights, x, Bias, _a);
			//Output = ActivationFunctions.Linear(weights, x, Bias, a);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Normalizes weights of the node (including biases!) using L2 norm.
		/// </summary>
		public void NormalizeWeights()
		{
			if (_weights == null) return;

			var w = new List<float>(_weights);
			w.Add(Bias);
			var normw = VectorMath.NormalizeL2(w.ToArray());
			Bias = normw[_weights.Length];
			_weights = VectorMath.Subvector(normw, 0, _weights.Length-1);
		}

		/// <summary>
		/// Converts a node to string.
		/// </summary>
		/// <returns></returns>
		public override string ToString ()
		{
			var res = "";
			res += "Param (a):\t" + _a;
			res += "\nBias:\t" + _a;
			res += "\nWeights:\t" + VectorMath.ConvertToString(_weights, '\t');
			return res;
		}

		/// <summary>
		/// Converts a node to string using the normalized connection weights.
		/// </summary>
		/// <returns></returns>
		public string ToStringNorm()
		{
			var res = "";
			res += "Param (a):\t" + _a;
			res += "\nBias:\t" + Bias;
			var normw = VectorMath.NormalizeL2(_weights);
			res += "\nWeights:\t" + VectorMath.ConvertToString(normw, '\t');
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Class for the neural layer.
	/// </summary>
	[Serializable]
	public class NeuralLayer : ICloneable
	{
		private List<Node> nodes = new List<Node>();

		public List<Node> Nodes
		{
			get { return nodes; }
			set { nodes = value; }
		}

		#region - Construction. -
		/// <summary>
		/// [molecule]
		/// 
		/// Default constructor.
		/// </summary>
		public NeuralLayer(){}

		/// <summary>
		/// [molecule]
		/// 
		/// Copies constructor.
		/// </summary>
		public NeuralLayer(NeuralLayer layer)
		{
			foreach (var node in layer.Nodes)
			{
				nodes.Add(node);
			}
		} 
		#endregion

		#region - Public methods. -
		public void InitNodesWeights(int count, Random rand)
		{
			for (int i = 0; i < nodes.Count; ++i)
			{
				if (nodes[i] == null)
				{
					nodes[i] = new Node();
				}
				nodes[i].InitWeights(count, rand);
			}
		}

		public void Calculate(float[] inputs)
		{
			foreach (var node in nodes)
			{
				node.Calculate(inputs);
			}
			//for (int i = 0; i < nodes.Count; ++i)
			//{
			//    nodes[i].Calculate(inputs);
			//}
		}

		public void GetOutputs(out float[] outputs)
		{
			outputs = new float[nodes.Count];
			for (int i = 0; i < nodes.Count; ++i)
			{
				outputs[i] = nodes[i].Output;
			}
		}

		public void SetOutputs(float[] outputs)
		{
			int i = 0;
			foreach (var node in nodes)
			{
				node.Output = outputs[i];
				++i;
			}
			//for (int i = 0; i < nodes.Count; ++i)
			//{
			//    nodes[i].Output = outputs[i];
			//}
		}

		public List<float[]> GetWeights ()
		{
			var res = new List<float[]>();
			foreach (var node in nodes)
			{
				var w = new List<float>();
				w.Add(node.Bias);
				w.AddRange(node.Weights);
				res.Add(w.ToArray());
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns total number of weights in the layer.
		/// </summary>
		/// <returns></returns>
		public int GetWeightsCount()
		{
			int res = 0;
			foreach (var node in nodes)
			{
				res += node.Weights.Length + 1;	// to take into account the bias.
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Normalizes weights of each node (including biases!) using L2 norm.
		/// </summary>
		public void NormalizeWeights()
		{
			foreach (var node in nodes)
			{
				node.NormalizeWeights();
			}
		}

		#region - Output. -
		public List<string> ToStrings()
		{
			var res = new List<string>();
			foreach (var node in nodes)
			{
				res.Add(node.ToString());
			}
			return res;
		}

		/// <summary>
		/// Returns layer info with normalized node weights.
		/// </summary>
		/// <returns></returns>
		public List<string> ToStringsNorm()
		{
			var res = new List<string>();
			foreach (var node in nodes)
			{
				res.Add(node.ToStringNorm());
			}
			return res;
		} 
		#endregion

		#region - ICloneable implementation. -
		public object Clone()
		{
			return new NeuralLayer(this);
		}  
		#endregion
		#endregion
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Multi-layered neural network class.
	/// </summary>
	[Serializable]
	public class LayeredNeuralNetwork : ICloneable, IVectorFunction
	{
		private List<NeuralLayer> layers = new List<NeuralLayer>();

		#region - Properties. -
		public bool UseBias { get; set; }

		public List<NeuralLayer> Layers
		{
			get { return layers; }
			set { layers = value; }
		}

		public int OutputsNumber
		{
			get { return layers[layers.Count - 1].Nodes.Count; }
		}

		public int InputsNumber
		{
			get { return layers[0].Nodes.Count; }
		}

		public List<TrainingSample> RefData { get; set; }
		#endregion

		#region - Construction. -
		/// <summary>
		/// [molecule]
		/// 
		/// Default constructor.
		/// </summary>
		public LayeredNeuralNetwork()
		{
			UseBias = true;
			RefData  = new List<TrainingSample>();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Copies constructor.
		/// </summary>
		/// <param name="net">Network to copy from.</param>
		public LayeredNeuralNetwork (LayeredNeuralNetwork net)
		{
			foreach (var layer in net.Layers)
			{
				layers.Add((NeuralLayer)layer.Clone());
			}

			UseBias = net.UseBias;

			if(net.RefData == null) return;
			foreach (var sample in net.RefData)
			{
				RefData.Add(sample);
			}
		}
		#endregion

		#region - Functionality methods. -
		// this function should be called after all layers are added.
		public void Construct(Random rand)
		{
			for (int i = 0; i < layers[0].Nodes.Count; ++i)
			{
				if (layers[0].Nodes[i] == null)
				{
					layers[0].Nodes[i] = new Node();
				}
			}
			for (int i = 1; i < layers.Count; ++i)
			{
				layers[i].InitNodesWeights(layers[i - 1].Nodes.Count, rand);

				if (!UseBias)
				{
					// reset biases.
					foreach (var node in layers[i].Nodes)
					{
						node.Bias = 0;
					}
				}
			}
		}

		/// <summary>
		/// Calculates network output.
		/// </summary>
		/// <param name="inputs">Input signals.</param>
		public void Calculate(float[] inputs)
		{
			layers[0].SetOutputs(inputs);

			for (int i = 1; i < layers.Count; ++i)
			{
				float[] outputs;
				layers[i - 1].GetOutputs(out outputs);
				layers[i].Calculate(outputs);
			}
		}

		/// <summary>
		/// Returns vector of output signals for ANN.
		/// </summary>
		/// <param name="outputs"></param>
		public void GetOutputs(out float[] outputs)
		{
			layers[layers.Count - 1].GetOutputs(out outputs);
		}
		#endregion

		#region - Utility methods. -
		/// <summary>
		/// Create new neural network using provided properties.
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static LayeredNeuralNetwork CreateNetwork(NeuralNetProperties props) 
		{
			var net = new LayeredNeuralNetwork();

			// create layers.
			// create input layer.
			net.Layers.Add(new NeuralLayer());
			net.Layers[0].Nodes = new List<Node>(new Node[props.nodesNumber[0]]);

			// create hidden and output layers.
			for (var i = 1; i < props.nodesNumber.Length; ++i)
			{	// create i-th layer.
				net.Layers.Add(new NeuralLayer());
				net.Layers[i].Nodes = new List<Node>();

				// create nodes and set activation functions
				for (var j = 0; j < props.nodesNumber[i]; j++)
				{
					var tempNode = new Node();
					tempNode.ActivationFunction = props.actFunctions[i];
					net.Layers[i].Nodes.Add(tempNode);
				}
			}

			net.UseBias = props.UseBias;
            net.Construct(ContextRandom.rand);
			return net;
		}

		public NeuralNetProperties GetNetworkProperties ()
		{
			var res = new NeuralNetProperties();
			res.nodesNumber = new int[layers.Count];
			res.actFunctions = new ActivationFunction[layers.Count];
			for (int i = 0; i < layers.Count; i++)
			{
				res.nodesNumber[i] = layers[i].Nodes.Count;
				res.actFunctions[i] = layers[i].Nodes[0].ActivationFunction;
			}

			res.UseBias = UseBias;

			return res;
		}

		/// <summary>
		/// Returns total number of ANN connections.
		/// </summary>
		/// <returns></returns>
		public int GetTotalConnectionsNumber()
		{
			int res = 0;
			for (int i = 1; i < layers.Count; ++i)
			{
				var nodesCount = UseBias ? layers[i - 1].Nodes.Count + 1 : layers[i - 1].Nodes.Count;
				res += layers[i].Nodes.Count * nodesCount;
			}
			return res;
		}

		/// <summary>
		/// Returns weights and bias values for each node.
		/// </summary>
		public List<float> GetConnectionWeights()
		{
			var res = new List<float>(GetTotalConnectionsNumber());

			for (int i = 1; i < Layers.Count; ++i)
			{
				var layer = Layers[i];
				for (int j = 0; j < layer.Nodes.Count; ++j)
				{
					var node = layer.Nodes[j];
					if (UseBias)
					{
						res.Add(node.Bias);
					}
					for (int k = 0; k < node.Weights.Length; ++k)
					{
						res.Add(node.Weights[k]);
					}
				}
			}

			return res;
		}

		/// <summary>
		/// Set weights and bias values for each nodes from the given array.
		/// </summary>
		/// <param name="w"></param>
		public void SetConnectionWeights (IList<float> w)
		{
			int count = 0;
			for (int i = 1; i < Layers.Count; ++i)
			{
				var layer = Layers[i];
				for (int j = 0; j < layer.Nodes.Count; ++j)
				{
					var node = layer.Nodes[j];
					if (UseBias)
					{
						node.Bias = w[count];
						++count;
					}
					for (int k = 0; k < node.Weights.Length; ++k)
					{
						node.Weights[k] = w[count];
						++count;
					}
				}
			}
		}

		/// <summary>
		/// Saves neural network in the given file.
		/// </summary>
		/// <param name="filename"></param>
		public void Save(string filename)
		{
			FileIO.Serialize(filename, this);
		}

		/// <summary>
		/// Loads neural network from the given file.
		/// </summary>
		/// <param name="filename"></param>
		public void Load(string filename)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
			var net = (LayeredNeuralNetwork)formatter.Deserialize(stream);
			stream.Close();

			Layers = new List<NeuralLayer>();
			foreach (NeuralLayer layer in net.Layers)
			{
				Layers.Add(layer);
			}

			RefData = new List<TrainingSample>();
			if (net.RefData != null)
			{
				foreach (TrainingSample sample in net.RefData)
				{
					RefData.Add(sample);
				}
			}
		} 

		/// <summary>
		/// [molecule]
		/// 
		/// Normalizes weights of each node (including biases!) using L2 norm.
		/// </summary>
		public void NormalizeWeights ()
		{
			foreach (var layer in layers)
			{
				layer.NormalizeWeights();
			}
		}
		#endregion

		public object Clone()
		{
			return new LayeredNeuralNetwork(this);
		}
	}
}
