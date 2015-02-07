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
