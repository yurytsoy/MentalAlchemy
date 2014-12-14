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
