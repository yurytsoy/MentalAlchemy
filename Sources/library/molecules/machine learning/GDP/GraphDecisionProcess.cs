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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Basic implementation of graph decision process with one policy.
	/// </summary>
	public class GraphDecisionProcess
	{
		public SystemStateGraph Ssg;
		public GdpPolicy Policy;
		public FlexibleNeuralNetwork2 StrObject;
		public NeuralObjectiveFunction ObjFunction;

		#region - Variables for reporting. -
		public bool VerboseMode;
		public List<string> Report; 
		#endregion

		#region - Construction and init. -
		/// <summary>
		/// Substitute with initialization from a config.
		/// 
		/// Note: [ObjFunction] property should be set before calling this function!
		/// </summary>
		public void Init() 
		{
			Report = new List<string>();

			#region - Temporal initialization. To be substituted in the future. -
			// load SSG.
			Ssg = new SystemStateGraph();

			// load policy and init parameters.
			Policy = GdpPolicy.CreateRandomPolicy ();
			Policy.Parameters.FitnessFunction = ObjFunction;
			Policy.Parameters.GeneValueRange = FlexibleNeuralNetwork2.DEFAULT_WEIGHT_RANGE;
			Policy.Parameters.MinGeneValue = FlexibleNeuralNetwork2.DEFAULT_WEIGHT_MIN;

			// init and evaluate structured object.
			if (ObjFunction != null)
			{
				StrObject = new FlexibleNeuralNetwork2(ObjFunction.InputIds, ObjFunction.OutputIds);
				var q = ObjFunction.Calculate(StrObject);

				Ssg.CurNode.ConnCount = StrObject.ConnectionsCount;
				Ssg.CurNode.NodeCount = StrObject.HiddenNodesCount;
				Ssg.CurNode.Quality.Add(new Fitness(q));
			}
			#endregion
		}
		#endregion


		public void Run(int iterCount)
		{
			Init();
			Continue (iterCount);
		}

		public void Continue(int iterCount)
		{
			for (int i = 0; i < iterCount; ++i)
			{
				Iter();
				if (VerboseMode)
				{
					// Report current state of SSG.
					Report.Add(Ssg.CurNode.Id + "\t" + Ssg.CurNode.MeanQuality);
				}
			}
		}

		/// <summary>
		/// One iteration.
		/// </summary>
		public void Iter()
		{
			var action = Policy.SelectAction();
			var newObj = (FlexibleNeuralNetwork2)StrObject.Clone();
			action.Operate(newObj);
			
			// evaluate a new structured object and make a decision
			//	whether to keep or discard it.
			var newQuality = ObjFunction.Calculate(newObj);
			var curQuality = Ssg.CurNode.Quality[Ssg.CurNode.Quality.Count-1];
			if (!FitnessComparator.IsWorse(newQuality, curQuality))
			{
				StrObject = newObj;
				Ssg.StateTransition(StrObject.ConnectionsCount, StrObject.HiddenNodesCount, action.Name);
				Ssg.CurNode.Quality.Add(newQuality);
			}
			else
			{
				Ssg.RegisterAttempt(newObj.ConnectionsCount, newObj.HiddenNodesCount, action.Name, newQuality);
			}
		}
	}
}
