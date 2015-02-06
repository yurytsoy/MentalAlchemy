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

using System;
using System.Collections.Generic;
//using MentalAlchemy.Molecules.MachineLearning;

using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Class to represent slot for a single signal.
	/// </summary>
	[Serializable]
	public class SignalSlot
	{
		/// <summary>
		/// Signal's ID.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Signal's weight.
		/// </summary>
		public float Weight { get; set; }

		/// <summary>
		/// Signal's value.
		/// </summary>
		public float Value { get; set; }

		public SignalSlot (int id, float weight)
		{
			Id = id;
			Weight = weight;
		}
	}

	/// <summary>
	/// Class for flexible node.
	/// </summary>
	[Serializable]
	public class FlexNode : Node
	{
		#region - Protected members. -
		protected List<SignalSlot> inputs = new List<SignalSlot>();
		protected List<SignalSlot> outputs = new List<SignalSlot>(); 
		#endregion
		
		#region - Public properties. -
		/// <summary>
		/// List of input signals slots.
		/// </summary>
		public List<SignalSlot> Inputs
		{
			get { return inputs; }
			set { inputs = value; }
		}

		/// <summary>
		/// List of output signals slots.
		/// </summary>
		public List<SignalSlot> Outputs
		{
			get { return outputs; }
			set { outputs = value; }
		}

		/// <summary>
		/// Total node activity obtained after the last reset.
		/// </summary>
		public float TotalActivity{get; set;}
		#endregion

		#region - Construction. -
		public FlexNode (int idx, float bias, ActivationFunction actFunc)
		{
			Index = idx;
			Bias = bias;
			TotalActivity = 0;
			ActivationFunction = actFunc;
			//Weights = new float[1];
		}

		public FlexNode (FlexNode node)
		{
			Index = node.Index;
			Bias = node.Bias;
			TotalActivity = node.TotalActivity;
			ActivationFunction = node.ActivationFunction;
			//Weights = new List<float>(node.Weights);
			Weights = node.Weights != null? (float[])node.Weights.Clone() : null;
			Inputs = new List<SignalSlot>(node.Inputs);
			Outputs = new List<SignalSlot>(node.Outputs);
		}
		#endregion

		#region - Public methods. -
		/// <summary>
		/// Resets all output signals to zero.
		/// </summary>
		public void Reset ()
		{
			Output = 0;
			//TotalActivity = 0;
			foreach (var slot in outputs)
			{
				slot.Value = 0;
			}
		}

		/// <summary>
		/// Calculates node output using the specified environment [env] state and updates state [newEnv] signals.
		/// </summary>
		/// <param name="env">Current environment.</param>
		/// <param name="newEnv">Updated environment.</param>
		public void Calculate(Dictionary<int, float> env, Dictionary<int, float> newEnv)
		{
			if (inputs.Count == 0)
			{	// no calculation is performed.
				// simply copy [env] to [newEnv].
				foreach (var pair in env)
				{
					if (newEnv.ContainsKey(pair.Key)) { newEnv[pair.Key] = pair.Value; }
					else { newEnv.Add(pair.Key, pair.Value); }
				}
				return;
			}

			//
			// read input signals from the [env].
			foreach (var slot in inputs)
			{
				slot.Value = env.ContainsKey(slot.Id) ? env[slot.Id] : 0;
			}

			//
			// set inputs.
			var input = new float[inputs.Count];
			for (int i = 0; i < input.Length; i++) { input[i] = inputs[i].Value; }

			//
			// set weights.
			Weights = new float[input.Length];
			for (int i = 0; i < input.Length; i++) { Weights[i] = inputs[i].Weight; }

			//
			// calculate output signal.
			Calculate(input);
			TotalActivity += Math.Abs(Output);

			//
			// set outputs.
			foreach (var slot in outputs)
			{
				slot.Value = Output * slot.Weight;

				if (newEnv.ContainsKey(slot.Id))
				{
					newEnv[slot.Id] += slot.Value;
				}
				else
				{
					newEnv.Add(slot.Id, slot.Value);
				}
			}
		}

		/// <summary>
		/// Indicates whether node has input signal with the given ID.
		/// </summary>
		/// <param name="id">Requested input signal ID.</param>
		/// <returns>[True] if there is a signal with such [id] and [False] otherwise.</returns>
		public bool HasInput (int id)
		{
			foreach (var input in inputs)
			{
				if (input.Id == id) return true;
			}
			return false;
		}

		/// <summary>
		/// Returns input signal slot with the given [id].
		/// </summary>
		/// <param name="id">Requested input signal ID.</param>
		/// <returns>Input signal slot or null if search for the specified input had failed.</returns>
		public SignalSlot GetInput (int id)
		{
			foreach (var input in inputs)
			{
				if (input.Id == id) return input;
			}
			return null;
		}

		/// <summary>
		/// Returns output signal slot with the given [id].
		/// </summary>
		/// <param name="id">Requested output signal ID.</param>
		/// <returns>Output signal slot or null if search for the specified output had failed.</returns>
		public SignalSlot GetOutput(int id)
		{
			foreach (var output in outputs)
			{
				if (output.Id == id) return output;
			}
			return null;
		}
		#endregion
	}
}