using System;
using System.Collections.Generic;
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	public class Neva
	{
		protected float avgNodesPrev;
		protected float[] avgNodes;
		protected NevaContents contents = new NevaContents();
		
		#region - Public properties. -
		public List<Stats> FitnessStats { get { return contents.FitnessStats; } }
		public NevaInd BestIndividual { get { return (NevaInd)contents.BestIndividual; } }
		public NevaContents Contents { get { return contents; } }
		#endregion

		#region - Protected methods. -
		protected void Init(NevaParameters parameters)
		{
			//FitnessComparator.MinimizeFitness = true;
			contents.Init(parameters);
			parameters.Algorithm = this;

			avgNodes = new float[parameters.NodesWindowSize];
			avgNodesPrev = 0f;
		}
		#endregion

		#region - Public methods. -
		/// <summary>
		/// Runs algorithm with the specified parameters setting.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void Run(NevaParameters parameters)
		{
			//
			// validate [parameters].
			if (!EAElements.ValidateParameters(parameters)) throw new Exception("[Neva.Run]: Invalid parameters setting or fitness function is undefined.");

			Init(parameters);

			for (contents.GenerationNumber = 0; contents.GenerationNumber < parameters.GenerationsNumber; ++contents.GenerationNumber)
			{
				Evaluate(parameters);
				contents.SelPopul = EAElements.TournamentSelection(contents.Popul, parameters.TournamentSize, parameters.RNG);
				//contents.Popul = NevaElements.Cross(contents.SelPopul, parameters);
				contents.Popul = contents.SelPopul;
				contents.Popul = NevaElements.Mutate(contents.Popul, parameters);

				//
				// implement elitism via substitution of the 1st child with the best individual found.
				if (parameters.UseElitism)
				{
					contents.Popul[0] = contents.BestIndividual.Clone();
				}
			}

			Evaluate(parameters);	// final population evaluation.
		}

		/// <summary>
		///  Evaluates population and updates [stats].
		/// </summary>
		public void Evaluate(NevaParameters parameters)
		{
			//
			// perform evaluation and (if required) replace current best individual.
			NevaInd tempBest;
			NevaElements.Evaluate(contents.Popul, contents.FitnessFunction, out tempBest);
			if (contents.BestIndividual == null || FitnessComparator.IsBetter(tempBest.Fitness, contents.BestIndividual.Fitness))
				contents.BestIndividual = tempBest.Clone();

			CollectStats();

			#region - Track average number of hidden nodes. -
			if (parameters.NodesWindowSize > 0 && contents.NodesMutationLockCount <= 0)
			{
				var idx = contents.GenerationNumber % parameters.NodesWindowSize;
				avgNodes[idx] = contents.GetMeanHiddenNodes();

				if (avgNodesPrev != 0f)
				{
					var curMean = VectorMath.Mean(avgNodes);
					var diff = Math.Abs(curMean - avgNodesPrev) / avgNodesPrev;
					if (diff < parameters.NodesMutationLockThreshold)
					{
						// lock nodes mutation and clear avg nodes history.
						contents.NodesMutationLockCount = parameters.NodesMutationLockTime;
						avgNodes = new float[parameters.NodesWindowSize];
						avgNodesPrev = 0f;
					}
					else
					{
						avgNodesPrev = curMean;
					}
				}
				else
				{
					avgNodesPrev = VectorMath.Mean(avgNodes);
				}
			}
			else
			{
				//
				// reduce locking time.
				contents.NodesMutationLockCount--;
			}
			#endregion
		}

		/// <summary>
		/// Collects statistics on current population and updates stats array.
		/// </summary>
		public virtual void CollectStats()
		{
			#region - Fitness and ANN structure stats. -
			//
			// collect statistics and update [stats].
			var fits = new float[contents.Popul.Count];
			var hiddenNodes = new int[contents.Popul.Count];
			var edges = new int[contents.Popul.Count];
			for (int i = 0; i < fits.Length; i++)
			{
				var ind = (NevaInd)contents.Popul[i];
				fits[i] = ind.Fitness.Value;
				hiddenNodes[i] = ind.Network.HiddenNodesCount;
				edges[i] = ind.Size;
			}
			var stat = VectorMath.CalculateStats(fits);

			var nodesStats = VectorMath.CalculateStats(hiddenNodes);
			stat.AppendData(nodesStats.Mean, "Avg hidden nodes");
			stat.AppendData(nodesStats.Variance, "Var hidden nodes");

			var edgeStats = VectorMath.CalculateStats(edges);
			stat.AppendData(edgeStats.Mean, "Avg edges");
			stat.AppendData(edgeStats.Variance, "Var edges");

			#endregion

			contents.Stats.Add(stat);
		}
		#endregion
	}
}