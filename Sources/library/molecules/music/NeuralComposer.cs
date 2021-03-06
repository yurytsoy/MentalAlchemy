﻿/*************************************************************************
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

using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;
using MathNet.Numerics.Distributions;

namespace MentalAlchemy.Molecules.Music
{
	/// <summary>
	/// Class to compose a music using neural networks.
	/// The notes are specified by the sequence of nodes activity.
	/// </summary>
	public class NeuralComposer : BaseComposer
	{
		FlexibleNeuralNetwork2 _ann;
		
		public NeuralComposer() {}
		
		public void ResetOutputs ()
		{
			if (_ann == null) return;
			_ann.ResetOutputs();
		}

		public override string[] ComposeMonotone (int length)
		{
			if (_ann == null) return null;

			var input = new [] {(float)ContextRandom.NextDouble ()};
			
			// iterate through states of the ANN environment.
			var state = _ann.GetCurrentState ();
			var res = new string[length];
			var inputsOutputsCount = _ann.InputIds.Count + _ann.OutputIds.Count;
			for (int i=0; i<length; ++i)
			{
				state = _ann.UpdateOneStep (state);

				// retreive nodes states.
				var stateValues = new float[state.Count];
				var stateKeys = new int[state.Count];
				state.Keys.CopyTo(stateKeys, 0);
				state.Values.CopyTo(stateValues, 0);
				var idx = VectorMath.IndexOfMax(stateValues, inputsOutputsCount);
				var keyIdx = stateKeys[ idx ];
				state[0] = state[_ann.InputIds.Count];	// introduce output to the input.
				state[keyIdx] = 0;	// reset the winner.
				
				// pick the note according to the index.
				var noteStr = MusicUtils.OffsetToNoteString(keyIdx - _ann.InputIds.Count);	// skip inputs.
				res[i] = noteStr;
			}
			return res;
		}
		
		public void CreateComposingNetwork (float connProb = 0.5f, ActivationFunction actFunc = null)
		{
			// Create random weighted graph with predefined number of nodes and edges probability.
			const int nodesCount = 7;	// = number of notes.
			const int inputsCount = 1;
			const int outputsCount = 1;
			Graph graph = GraphElements.RandomDirected (nodesCount + inputsCount + outputsCount, connProb);
			var rnd = new NormalDistribution(0, 1);
			foreach (var edge in graph.Edges)
			{
				edge.Weight = (float)rnd.NextDouble ();
			}
			
			// add cyclic edges.
			for (int i = 0; i < nodesCount; ++i)
			{
				if (ContextRandom.NextBoolean())
				{
					var edge = new Edge();
					edge.BeginIdx = inputsCount + i;
					edge.EndIdx = inputsCount + i;
					edge.Weight = -0.2f * (float)ContextRandom.NextDouble();
					graph.Edges.Add(edge);
				}
			}

			// convert the graph to the ANN.
			// create activations.
			var activations = new Dictionary <int, ActivationFunction> ();
			for (int i=inputsCount; i<inputsCount + nodesCount; ++i)
			{
				activations.Add(i, actFunc ?? ActivationFunctions.Sigmoid);
			}
			
			_ann = new FlexibleNeuralNetwork2 ();
			_ann.InputIds = new List<int> (new[] {0});
			_ann.OutputIds = new List<int> (new[] {nodesCount + inputsCount});
			_ann.BuildNetwork (graph.Edges, activations);
		}

		public string[] ToDotFormat()
		{
			if (_ann == null) return null;
			return FlexibleNeuralNetwork2.ConvertToDotFormat(_ann);
		}
	}
}
