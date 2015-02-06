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
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Generalized Generation Gap (G3) GA model with PCX crossover described in
	///	 K. Deb, A. Anand, and D. Joshi. A computationally efficient evolutionary algorithm for real-parameter optimization. Technical report, Indian Institute of Technology, April 2002.
	/// </summary>
	[Serializable]
	public class G3Pcx : EvolutionaryAlgorithm
	{
		#region - Public methods. -
		/// <summary>
		/// Runs Generalized Generation Gap (G3) EA with PCX crossover.
		/// </summary>
		/// <param name="parameters"></param>
		public override void Run(EAParameters parameters)
		{
			//
			// validate [parameters].
			if (FitnessFunction != null && !EAElements.ValidateParameters(parameters)) throw new Exception("[EvolutionaryAlgorithm.Run]: Invalid parameters setting or fitness function is undefined.");

			Init(parameters);
			Evaluate();

			// sort [popul].
			Population = EAElements.Sort(Population);

			for (var i = 0; i < parameters.GenerationsNumber; ++i)
			{
				var popSize = Population.Count;

				// selPopul contains [parameters.OffspringNumber] offspring individuals.
				selPopul = EAElements.CrossPcx(Population, parameters);
				EAElements.Evaluate(selPopul, FitnessFunction);

				// select 2 random parent individuals for removal.
				var remParent1 = ContextRandom.Next(popSize);
				var remParent2 = ContextRandom.Next(popSize);

				// select 2 best individuals from the combined population (popul+selPopul)
				// as two first entries in the [combPopul] list.
				var combPopul = new List<AbstractIndividual>();
				combPopul.Add(selPopul[0]);
				for (int j = 1; j < parameters.OffspringNumber; j++) { EAElements.InsertViaFitness(combPopul, selPopul[j]); }

				EAElements.InsertViaFitness(combPopul, Population[0]);
				EAElements.InsertViaFitness(combPopul, Population[1]);

				// substitute selected parents with 2 best individuals from the combined population.
				Population[remParent1] = combPopul[0].Clone();
				Population[remParent2] = combPopul[1].Clone();
				Population = EAElements.Sort(Population);

				//
				// update fitness stats.
				var stat = EAElements.GetFitnessStats(Population);
				stats.Add(stat);
			}

			bestInd = (Individual)Population[0];
		}
		#endregion

	}
}
