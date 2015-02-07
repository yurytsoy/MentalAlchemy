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
using System.Diagnostics;
using System.IO;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	public delegate void Evaluation (List<AbstractIndividual> popul, FitnessFunction fitFunc, out Individual bestInd);
	public delegate void Mutation (List<AbstractIndividual> popul, EAParameters pars);
	public delegate List<AbstractIndividual> PopulationCreation(EAParameters pars);
	public delegate List<AbstractIndividual> Crossing(List<AbstractIndividual> popul, EAParameters pars);

	/// <summary>
	/// Class for a baseline real-coded elitist evolutionary algorithm.
	/// </summary>
	[Serializable]
	public class EvolutionaryAlgorithm
	{
		#region - Variables. -

		protected List<AbstractIndividual> popul = new List<AbstractIndividual>();
		protected List<AbstractIndividual> selPopul = new List<AbstractIndividual>();
		protected List<Stats> stats = new List<Stats>();	// run statistics.
		protected Individual bestInd;
		#endregion

		#region - Properties. -
		#region - EA customization properties. -
		/// <summary>
		/// Defines evaluation procedure and can be overloaded to implement additional
		///	operations before or after evaluation of population/individuals.
		/// </summary>
		public Evaluation Evaluation { get; set; }
		/// <summary>
		/// Defines mutation procedure and can be overloaded to implement additional
		///	operations before or after mutation of population/individuals.
		/// </summary>
		public Mutation Mutation { get; set; }
	
		/// <summary>
		/// Defines mutation procedure and can be overloaded to implement additional
		///	operations before or after mutation of population/individuals.
		/// </summary>
		public Crossing Crossing{ get; set; }

		/// <summary>
		/// Defines method to create a population.
		/// Default is [EAElements.InitPopulationRealCoded].
		/// </summary>
		public PopulationCreation PopulationCreation { get; set; }
		#endregion

		/// <summary>
		/// Number of the current generation.
		/// </summary>
		public int CurrentGeneration { get; set; }

		/// <summary>
		/// Time in milliseconds, required to perform a run.
		/// </summary>
		public long ElapsedMilliseconds { get; set; }

		/// <summary>
		/// Objective function to be used to evaluate individuals.
		/// </summary>
		public FitnessFunction FitnessFunction { get; set; }
		
		/// <summary>
		/// Best individual ever found during the algorithm run.
		/// </summary>
		public Individual BestIndividual 
		{
			get { return bestInd; } 
			set { bestInd = value;}
		}

		/// <summary>
		/// Fitness statistics.
		/// </summary>
		public List<Stats> FitnessStats {get { return stats;}}

		/// <summary>
		/// Current population.
		/// </summary>
		public List<AbstractIndividual> Population
		{
			get { return popul; }
			set { popul = value; }
		}
		#endregion

		#region - Constructor. -
		public EvolutionaryAlgorithm()
		{
			Evaluation = EAElements.Evaluate;
			Mutation = EAElements.Mutate;
			Crossing = EAElements.CrossBlx;
			PopulationCreation = EAElements.InitPopulationRealCoded;
		} 
		#endregion

		#region - Public methods. -
		/// <summary>
		/// [molecule]
		/// 
		/// Runs EA for the prescribed number of generations.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void Run(EAParameters parameters)
		{
			//
			// validate [parameters].
			if (FitnessFunction != null && !EAElements.ValidateParameters(parameters)) throw new Exception("[EvolutionaryAlgorithm.Run]: Invalid parameters setting or fitness function is undefined.");

			Init(parameters);

			var timer = new Stopwatch();
			timer.Start();
			Continue(parameters);
			timer.Stop();
			ElapsedMilliseconds = timer.ElapsedMilliseconds;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Continues EA run for the prescribed number of generations.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void Continue(EAParameters parameters)
		{
			//
			// validate [parameters].
			if (FitnessFunction != null && !EAElements.ValidateParameters(parameters)) throw new Exception("[EvolutionaryAlgorithm.Run]: Invalid parameters setting or fitness function is undefined.");

			for (CurrentGeneration = 1; CurrentGeneration <= parameters.GenerationsNumber; ++CurrentGeneration)
			{
				Evaluate();
				Select(parameters);
				Cross(parameters);
				Mutate(parameters);
				NextGeneration(parameters);
			}

			Evaluate();	// final population evaluation.
		}

		/// <summary>
		/// Performs initialization and prepares data structures.
		/// </summary>
		/// <param name="parameters">EA parameters.</param>
		public virtual void Init(EAParameters parameters)
		{
			// create population.
			popul = PopulationCreation(parameters);
			bestInd = null;	// reset best individual ...
			stats.Clear();	// ... and stats
		}

		/// <summary>
		///  Evaluates population and updates [stats].
		/// </summary>
		public virtual void Evaluate ()
		{
			// perform evaluation and (if required) replace current best individual.
			Individual tempBest;
			Evaluation(popul, FitnessFunction, out tempBest);
			if (bestInd == null || FitnessComparator.IsBetter(tempBest.Fitness, bestInd.Fitness)) bestInd = (Individual)tempBest.Clone();

			CollectStats();
		}

		/// <summary>
		/// Performs selection.
		/// </summary>
		public virtual void Select(EAParameters parameters)
		{
			selPopul = EAElements.TournamentSelection(popul, parameters.TournamentSize, parameters.RNG);
		}

		/// <summary>
		/// Performs crossing to create new offspring.
		/// </summary>
		public virtual void Cross(EAParameters parameters)
		{
			popul = Crossing(selPopul, parameters);
		}

		/// <summary>
		/// Performs mutation.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void Mutate(EAParameters parameters)
		{
			Mutation(popul, parameters);
		}

		/// <summary>
		/// Advance to the next generation.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void NextGeneration(EAParameters parameters)
		{
			// implement elitism via substitution of the 1st child with the best individual found.
			popul[0] = bestInd.Clone();
		}


		/// <summary>
		/// [molecule]
		/// 
		/// Collect fitness stats across the population.
		/// </summary>
		public virtual void CollectStats ()
		{
			// collect statistics and update [stats].
			var stat = EAElements.GetFitnessStats(popul);
			stats.Add(stat);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns random individual from the current population.
		/// </summary>
		/// <returns></returns>
		public AbstractIndividual GetRandomIndividual()
		{
			return popul[ContextRandom.Next(popul.Count)];
		}
		#endregion
	}
}


