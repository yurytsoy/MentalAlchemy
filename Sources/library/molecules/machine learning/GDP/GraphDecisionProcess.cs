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
