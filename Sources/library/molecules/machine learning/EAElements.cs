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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	#region - Delegates. -
	public delegate Individual[] Operator(Individual[] ind);
	public delegate bool FitnessComparison(float f1, float f2);
	#endregion

	#region - Interfaces. -
	/// <summary>
	/// [molecule]
	/// 
	/// Base class for individuals.
	/// </summary>
	public abstract class AbstractIndividual
	{
		public abstract int Size { get; }
		public int Age { get; set; }
		public Fitness Fitness { get; set; }
		public abstract AbstractIndividual Clone();
		protected AbstractIndividual()
		{
			Fitness = new Fitness();
		}
	}
	#endregion

	#region - Types. -
	#region - Types for fitness representation and operations. -
	///// <summary>
	///// [molecule]
	///// 
	///// Class to store fitness value and some optional additional information.
	///// </summary>
	//[Serializable]
	//public class Fitness
	//{
	//    #region - Public properties. -
	//    public float Value { get; set; }
	//    public List<float> Extra { get; set; }
	//    #endregion

	//    #region - Construction. -
	//    public Fitness()
	//    {
	//        Extra = new List<float>();
	//    }
	//    public Fitness(Fitness argFitness)
	//    {
	//        Assign(argFitness);
	//    }
	//    public Fitness(float val)
	//    {
	//        Value = val;
	//        Extra = new List<float>();
	//    }
	//    #endregion

	//    #region - Public methods. -
	//    public Fitness Clone() { return new Fitness(this); }

	//    #region - Assignment. -
	//    public Fitness Assign(Fitness argFitness)
	//    {
	//        Value = argFitness.Value;
	//        Extra = new List<float>(argFitness.Extra);
	//        return this;
	//    }

	//    public Fitness Assign(float argD)
	//    {
	//        Value = argD;
	//        Extra.Clear();
	//        return this;
	//    } 
	//    #endregion

	//    #region - Arithmetics. -
	//    public static Fitness operator- (Fitness arg1, Fitness arg2)
	//    {
	//        var res = new Fitness();
	//        res.Value = arg1.Value - arg2.Value;
	//        res.Extra = new List<float>(VectorMath.Sub(arg1.Extra.ToArray(), arg2.Extra.ToArray()));
	//        return res;
	//    }
	//    #endregion

	//    #region - Comparison. -
	//    public bool Equals (Fitness fit)
	//    {
	//        return Value == fit.Value && VectorMath.Equal(Extra.ToArray(), fit.Extra.ToArray(), float.Epsilon);
	//    }
	//    #endregion

	//    #region - Utility methods. -
	//    public void Clear()
	//    {
	//        Value = 0f;
	//        Extra.Clear();
	//    }

	//    public void Print(TextWriter writer)
	//    {
	//        var res = Value.ToString();
	//        if (Extra.Count > 0)
	//        {
	//            res += " ";
	//            foreach (var extra in Extra)
	//            {
	//                res += "[" + extra + "]";
	//            }
	//        }
	//        writer.Write(res);
	//    }

	//    public override string ToString()
	//    {
	//        var res = Value.ToString();
	//        if (Extra.Count > 0)
	//        {
	//            res += " ";
	//            foreach (var extra in Extra)
	//            {
	//                res += "[" + extra + "]";
	//            }
	//        }
	//        return res;
	//    }
	//    #endregion
	//    #endregion
	//}

	/// <summary>
	/// [molecule]
	/// 
	/// Class to compare fitness values depending on whether minimization of maximization problem is considered.
	/// </summary>
	public class FitnessComparator
	{
		private static bool multiObj = false;

		/// <summary>
		/// Indicates whether minimization of maximization problem is considered.
		/// </summary>
		public static bool MinimizeFitness { get; set; }

		/// <summary>
		/// Indicates whether fitness comparison is single or multi-objective.
		/// </summary>
		public static bool Multiobjective { get { return multiObj; } set { multiObj = value; } }

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness.</param>
		/// <returns>[True] if 1st fitness value is worse than the 2nd one and [False] otherwise.</returns>
		public static bool IsWorse(Fitness f1, Fitness f2)
		{
			if (!multiObj)
			{
				return MinimizeFitness ? f1.Value > f2.Value : f1.Value < f2.Value;
			}
			else
			{
				return IsWorseMultiobjective(f1, f2);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses using multiple objective functions written as fitness extras and ordered by priority descend.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness.</param>
		/// <returns>[True] if 1st fitness value is worse than the 2nd one and [False] otherwise.</returns>
		public static bool IsWorseMultiobjective(Fitness f1, Fitness f2)
		{
			var size = f1.Extra.Count;
			
			// look for the first unequal extras.
			int idx = 0;
			while (idx < size && f1.Extra[idx] == f2.Extra[idx]) { idx++; }

			if (idx < size)
			{
				return MinimizeFitness
							? f1.Extra[idx] > f2.Extra[idx]
							: f1.Extra[idx] < f2.Extra[idx];
			}
			return false;
			//}

			//for (int i = 0; i < size; i++)
			//{
			//    var worse = MinimizeFitness 
			//                ? f1.Extra[i] > f2.Extra[i]
			//                : f1.Extra[i] < f2.Extra[i];
			//    if (!worse && i == 0) {return false;}
			//}
			//return true;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness.</param>
		/// <returns>[True] if 1st fitness value is better than the 2nd one and [False] otherwise.</returns>
		public static bool IsBetter(Fitness f1, Fitness f2)
		{
			if (!multiObj)
			{
				return MinimizeFitness ? f1.Value < f2.Value : f1.Value > f2.Value;
			}
			else
			{
				return IsBetterMultiobjective(f1, f2);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses using multiple objective functions written as fitness extras and ordered by priority descend.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness.</param>
		/// <returns>[True] if 1st fitness value is better than the 2nd one and [False] otherwise.</returns>
		public static bool IsBetterMultiobjective(Fitness f1, Fitness f2)
		{
			var size = f1.Extra.Count;

			// look for the first unequal extras.
			int idx = 0;
			while (idx < size && f1.Extra[idx] == f2.Extra[idx]) { idx++; }

			if (idx < size)
			{
				return MinimizeFitness
					? f1.Extra[idx] < f2.Extra[idx]
					: f1.Extra[idx] > f2.Extra[idx];
			}
			return false;

			//var size = f1.Extra.Count;
			//for (int i = 0; i < size; i++)
			//{
			//    var better = MinimizeFitness
			//                ? f1.Extra[i] < f2.Extra[i]
			//                : f1.Extra[i] > f2.Extra[i];
			//    if (!better) { return false; }
			//}
			//return true;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness value.</param>
		/// <returns>[True] if 1st fitness value is worse than the 2nd one and [False] otherwise.</returns>
		public static bool IsWorse(Fitness f1, float f2)
		{
			return MinimizeFitness ? f1.Value > f2 : f1.Value < f2;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses.
		/// </summary>
		/// <param name="f1">1st fitness value.</param>
		/// <param name="f2">2nd fitness value.</param>
		/// <returns>[True] if 1st fitness value is worse than the 2nd one and [False] otherwise.</returns>
		public static bool IsWorse(float f1, float f2)
		{
			return MinimizeFitness ? f1 > f2 : f1 < f2;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness value.</param>
		/// <returns>[True] if 1st fitness value is better than the 2nd one and [False] otherwise.</returns>
		public static bool IsBetter(Fitness f1, float f2)
		{
			return MinimizeFitness ? f1.Value < f2 : f1.Value > f2;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Compares two given fitnesses.
		/// </summary>
		/// <param name="f1">1st fitness.</param>
		/// <param name="f2">2nd fitness value.</param>
		/// <returns>[True] if 1st fitness value is better than the 2nd one and [False] otherwise.</returns>
		public static bool IsBetter(float f1, float f2)
		{
			return MinimizeFitness ? f1 < f2 : f1 > f2;
		}
	}
	#endregion

	[Serializable]
	public class EAParameters
	{
		public int GenerationsNumber { get; set; }
		public int IndividualSize { get; set; }
		public int PopulationSize { get; set; }
		public int TournamentSize { get; set; }
		public float MinGeneValue { get; set; }
		public float GeneValueRange { get; set; }
		public float MRate { get; set; }
		public float MutationStep { get; set; }
		public float XRate { get; set; }
		public float StopFitness { get; set; }
		public Random RNG { get; set; }
		public int AgeLimit { get; set; }
		public int ParentsNumber { get; set; }
		public int OffspringNumber { get; set; }

		public List<string> ToStrings ()
		{
			var res = new List<string>();

			res.Add("Generations number:\t" + GenerationsNumber);
			res.Add("Genes count:\t" + IndividualSize);
			res.Add("Population size:\t" + PopulationSize);
			res.Add("Tournament size:\t" + TournamentSize);
			res.Add("Min gene value:\t" + MinGeneValue);
			res.Add("Gene value range:\t" + GeneValueRange);
			res.Add("Mutation rate:\t" + MRate);
			res.Add("Mutation step:\t" + MutationStep);
			res.Add("Crossover rate:\t" + XRate);
			res.Add("Stop fitness:\t" + StopFitness);
			res.Add("Age limit:\t" + AgeLimit);
			res.Add("Parents number:\t" + ParentsNumber);
			res.Add("Offsprings number:\t" + OffspringNumber);

			return res;
		}
	}

	/// <summary>
	/// Class for individual containing genome of real values.
	/// </summary>
	[Serializable]
	public class Individual : AbstractIndividual
	{
		private List<float> genes = new List<float>();
		public object Tag;

		#region - Properties. -
		public List<float> Genes
		{
			get { return genes; }
			set { genes = value; }
		}

		public override int Size
		{
			get { return genes.Count; }
		}
		#endregion

		#region - Construction. -
		public Individual() { }

		public Individual(Individual ind)
		{
			Assign(ind);
		}
		#endregion

		#region - Public methods. -
		/// <summary>
		/// Initializes randomly individual's genome using RNG and values range.
		/// </summary>
		/// <param name="size">Number of genes.</param>
		/// <param name="rand">RNG.</param>
		/// <param name="range">Range of possible genes values.</param>
		/// <param name="min">Minmal gene value.</param>
		public virtual void Init(int size, Random rand, float range, float min)
		{
			genes.Clear();
			for (int i = 0; i < size; ++i)
			{
				genes.Add((float)(range * rand.NextDouble() + min));
			}
		}

		/// <summary>
		/// Checks whether given [Individual] object equals to the current one.
		/// </summary>
		/// <param name="ind">Individual to compare.</param>
		/// <returns>[True] if to individuals aer equal and [False] otherwise.</returns>
		public virtual bool Equals(Individual ind)
		{
			if (Genes.Count != ind.Genes.Count) return false;

			for (int i = 0; i < Genes.Count; ++i)
			{
				if (Genes[i] != ind.Genes[i])
				{
					return false;
				}
			}

			return true;
		}

		public override AbstractIndividual Clone()
		{
			var res = new Individual();
			res.Fitness = new Fitness(Fitness);

			res.genes = new List<float>();
			foreach (float gene in genes)
			{
				res.genes.Add(gene);
			}

			return res;
		}

		public virtual Individual Assign(Individual ind)
		{
			Fitness = ind.Fitness;

			genes.Clear();
			foreach (var gene in ind.genes)
			{
				genes.Add(gene);
			}

			return this;
		}

		public override string ToString()
		{
			var res = VectorMath.ConvertToString(genes.ToArray(), '\t');
			return res;
		}
		#endregion
	}
	#endregion

	public class EAElements
	{
		#region - Evolutionary operators. -
		#region - Initialization. -
		/// <summary>
		/// [molecule]
		/// 
		/// Initializes real-coded population.
		/// </summary>
		/// <param name="parameters">EA parameters.</param>
		/// <returns>List of initialized individuals.</returns>
		public static List<AbstractIndividual> InitPopulationRealCoded(EAParameters parameters)
		{
			var res = new List<AbstractIndividual>();

			for (int i = 0; i < parameters.PopulationSize; i++)
			{
				var ind = new Individual();
				ind.Init(parameters.IndividualSize, parameters.RNG, parameters.GeneValueRange, parameters.MinGeneValue);
				res.Add(ind);
			}

			return res;
		}
		#endregion

		#region - Validation. -
		/// <summary>
		/// Validates given fitness function and EA parameters.
		/// </summary>
		/// <param name="parameters">EA parameters.</param>
		/// <returns>[True] if validation is successful and [False] otherwise.</returns>
		public static bool ValidateParameters(EAParameters parameters)
		{
			if (parameters.RNG == null) return false;
			if (parameters.IndividualSize <= 0) return false;
			if (parameters.PopulationSize <= 0) return false;

			return true;
		}
		#endregion

		#region - Evaluation. -
		///// <summary>
		///// [molecule]
		///// 
		///// Evaluates given population using the specified fitness function.
		///// </summary>
		///// <param name="popul">Population to evaluate.</param>
		///// <param name="objFunc">Fitness function.</param>
		///// <param name="bestInd">Best found indiviual.</param>
		///// <returns>Min and max fitness values.</returns>
		//public static void Evaluate(List<AbstractIndividual> popul, FitnessFunction objFunc, out Individual bestInd)
		//{
		//    float minF = popul[0].Fitness.Value;
		//    bestInd = (Individual)popul[0];
		//    for (var i = 0; i < popul.Count; i++)
		//    {
		//        var ind = popul[i];
		//        ind.Fitness.Value = objFunc(((Individual)ind).Genes.ToArray());

		//        if (FitnessComparator.IsBetter(ind.Fitness, minF))
		//        {
		//            minF = ind.Fitness.Value;
		//            bestInd = (Individual)ind;
		//        }
		//    }
		//    bestInd = (Individual)bestInd.Clone();	// to remove double pointing to the same object and occasional change of the best found individual.
		//}

		/// <summary>
		/// [molecule]
		/// 
		/// Evaluates given population using the specified fitness function.
		/// </summary>
		/// <param name="popul">Population to evaluate.</param>
		/// <param name="fitFunc">Fitness function.</param>
		/// <param name="bestInd">Best found indiviual.</param>
		/// <returns>Min and max fitness values.</returns>
		public static void Evaluate(List<AbstractIndividual> popul, FitnessFunction fitFunc, out Individual bestInd)
		{
			var minF = float.MaxValue;
			bestInd = (Individual)popul[0];
			for (var i = 0; i < popul.Count; i++)
			{
				var ind = popul[i];
				ind.Fitness = fitFunc.Compute(((Individual)ind).Genes.ToArray());

				if (FitnessComparator.IsBetter(ind.Fitness, minF))
				{
					minF = ind.Fitness.Value;
					bestInd = (Individual)ind;
				}
			}
			bestInd = (Individual)bestInd.Clone();	// to remove double pointing to the same object and occasional change of the best found individual.
		}

		///// <summary>
		///// [molecule]
		///// 
		///// Evaluates given population using the specified fitness function.
		///// </summary>
		///// <param name="popul">Population to evaluate.</param>
		///// <param name="objFunc">Fitness function.</param>
		///// <returns>Min and max fitness values.</returns>
		//public static void Evaluate(List<AbstractIndividual> popul, FitnessFunction objFunc)
		//{
		//    for (var i = 0; i < popul.Count; i++)
		//    {
		//        var ind = popul[i];
		//        ind.Fitness = objFunc(((Individual)ind).Genes.ToArray());
		//    }
		//}

		/// <summary>
		/// [molecule]
		/// 
		/// Evaluates given population using the specified fitness function.
		/// </summary>
		/// <param name="popul">Population to evaluate.</param>
		/// <param name="objFunc">Fitness function.</param>
		/// <returns>Min and max fitness values.</returns>
		public static void Evaluate(List<AbstractIndividual> popul, FitnessFunction objFunc)
		{
			for (var i = 0; i < popul.Count; i++)
			{
				var ind = popul[i];
				ind.Fitness = objFunc.Compute(((Individual)ind).Genes.ToArray());
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns best individual without evaluation of population.
		/// </summary>
		/// <param name="popul">Input population.</param>
		/// <returns>Best individual according to the [FitnessComparator] settings.</returns>
		public static AbstractIndividual GetBestIndividual(List<AbstractIndividual> popul)
		{
			var temp = Sort(popul);
			return temp[0];
		}
		#endregion

		#region - Selection. -
		/// <summary>
		/// Performs tournament selection over [popul] population and return selection results.
		/// </summary>
		/// <param name="popul">Population to select from.</param>
		/// <param name="tourSize">Tournament size.</param>
		/// <param name="rand">RNG.</param>
		public static List<AbstractIndividual> TournamentSelection(List<AbstractIndividual> popul, int tourSize, Random rand)
		{
			var selPopul = new List<AbstractIndividual>();
			selPopul.Clear();
			int populSize = popul.Count;
			for (int i = 0; i < populSize; ++i)
			{
				var winner = popul[(int)(rand.NextDouble() * populSize)];
				for (int j = 1; j < tourSize; ++j)
				{
					var tempInd = popul[(int)(rand.NextDouble() * populSize)];
					if (FitnessComparator.IsWorse(winner.Fitness, tempInd.Fitness))
					{
						winner = tempInd;
					}
				}

				selPopul.Add(winner.Clone());
			}

			return selPopul;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs truncation selection over the given population using [targetSize]
		/// to define resulting population size.
		/// </summary>
		/// <param name="popul"></param>
		/// <param name="targetSize"></param>
		/// <returns></returns>
		public static List<AbstractIndividual> TruncationSelection(List<AbstractIndividual> popul, int targetSize)
		{
			var res = Sort (popul);	// sort population by fitness decrease.
			res.RemoveRange(targetSize, popul.Count - targetSize);
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Sorts given population in fitness descending order according to the [FitnessComparator] settings.
		/// Insertion sort is used.
		/// </summary>
		/// <param name="popul">Population to sort.</param>
		/// <returns>Sorted population.</returns>
		public static List<AbstractIndividual> Sort(List<AbstractIndividual> popul)
		{
			var res = new List<AbstractIndividual>(popul);
			var size = popul.Count;
			for (int i = 1; i < size; ++i)
			{
				var key = popul[i];

				int j = i - 1;
				for (; j >= 0 && FitnessComparator.IsWorse(res[j].Fitness, key.Fitness); --j)
				{
					res[j + 1] = res[j];
				}

				res[j + 1] = key;
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates stats for the given population of pre-evaluated individuals.
		/// </summary>
		/// <param name="popul">Input population.</param>
		/// <returns>Stats data.</returns>
		public static Stats GetFitnessStats (List<AbstractIndividual> popul)
		{
			var fits = new Fitness[popul.Count];
			for (int i = 0; i < popul.Count; i++)
			{
				fits[i] = popul[i].Fitness;
			}
			return GetFitnessStats(fits);

			//var fits = new float[popul.Count];
			//for (int i = 0; i < popul.Count; i++)
			//{
			//    fits[i] = popul[i].Fitness.Value;
			//}
			//var stat = VectorMath.CalculateStats(fits);

			////
			//// calculate extras if any.
			//if (popul[0].Fitness.Extra.Count > 0)
			//{
			//    var extras = (float[])popul[0].Fitness.Extra.ToArray().Clone();
			//    for (int i = 1; i < popul.Count; i++)
			//    {
			//        VectorMath.Accumulate(ref extras, popul[i].Fitness.Extra.ToArray());
			//    }
			//    // perform averaging.
			//    VectorMath.Mul(ref extras, 1.0f/popul.Count);

			//    var count = 0;
			//    var str = "extra{0}";
			//    foreach (var extra in extras)
			//    {
			//        stat.AppendData(extra, string.Format(str, count));
			//        count++;
			//    }
			//}
	
			//return stat;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates stats for the given population of pre-evaluated individuals.
		/// </summary>
		/// <param name="fitVals">Input fitnesses.</param>
		/// <returns>Stats data.</returns>
		public static Stats GetFitnessStats (Fitness[] fitVals)
		{
			var fits = new float[fitVals.Length];
			for (int i = 0; i < fitVals.Length; i++)
			{
				fits[i] = fitVals[i].Value;
			}
			var stat = VectorMath.CalculateStats(fits);

			//
			// calculate extras if any.
			if (fitVals[0].Extra.Count > 0)
			{
				var extras = (float[])fitVals[0].Extra.ToArray().Clone();
				for (int i = 1; i < fitVals.Length; i++)
				{
					VectorMath.Accumulate(ref extras, fitVals[i].Extra.ToArray());
				}
				// perform averaging.
				VectorMath.Mul(ref extras, 1.0f / fitVals.Length);

				var count = 0;
				var str = "extra{0}";
				foreach (var extra in extras)
				{
					stat.AppendData(extra, string.Format(str, count));
					count++;
				}
			}
	
			return stat;
		}
		#endregion

		#region - Crossing. -
		#region - Weighted crossing. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs batch crossing, which produces 1 individual as updated variant of the [selPopul[0]]
		///  using information from all other individuals.
		/// </summary>
		/// <param name="selPopul"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static List<AbstractIndividual> WeightedCrossBatch (List<AbstractIndividual> selPopul, EAParameters parameters)
		{
			var resList = new List<AbstractIndividual>();
			var size = selPopul.Count;
			var partsCount = selPopul[0].Fitness.Extra.Count;
			var partSize = ((Individual) selPopul[0]).Genes.Count/partsCount;
			var parentParts = VectorMath.Split(((Individual)selPopul[0]).Genes.ToArray(), partsCount);

			for (int i1 = 0; i1 < parameters.OffspringNumber; i1++)
			{
				var res = (Individual)selPopul[0].Clone();
				var diffs = new List<float[]>();
				for (int i = 0; i < partsCount; i++)
				{
					diffs.Add(new float[partSize]);
				}

				for (int i = 1; i < size; i++)
				{
					var par2Parts = VectorMath.Split(((Individual)selPopul[i]).Genes.ToArray(), partsCount);

					for (int j = 0; j < partsCount; j++)
					{
						var w1 = res.Fitness.Extra[j];	// weight.
						var w2 = selPopul[i].Fitness.Extra[j];	// weight.

						// 2. compute offset vectors.
						var tempDiff = WeightedCross(parentParts[j], w1, par2Parts[j], w2);
						var tempDiffLen = VectorMath.L2Norm(tempDiff);

						// 2.5. compute random weight proportional to the offset.
						var w = parameters.RNG.NextDouble() * tempDiffLen;
						VectorMath.Mul(ref tempDiff, (float)w);

						// 3. update batch update vector for selPopul[0].
						diffs[j] = VectorMath.Add(diffs[j], tempDiff);	// accumulate offsets.
					}
				}

				// 4. apply batch update.
				var diffM = MatrixMath.CreateFromRowsList(diffs);
				var batchUpd = MatrixMath.ConvertToVector(diffM);
				var newGenes = VectorMath.Add(res.Genes.ToArray(), batchUpd);
				res.Genes = new List<float>(newGenes);

				resList.Add(res);
			}

			return resList;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Method for weighted crossing of individuals with weights defined using fitness extras.
		/// </summary>
		/// <param name="selPopul"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static List<AbstractIndividual> WeightedCross(List<AbstractIndividual> selPopul, EAParameters parameters)
		{
			var res = new List<AbstractIndividual>();

			var populSize = parameters.PopulationSize;
			var selSize = selPopul.Count;
			var xRate = parameters.XRate;
			var rand = parameters.RNG;

			while (res.Count < populSize)
			{

				#region - Select parents. -
				//
				// select randomly [parentsNum] parents.
				var parsv = new List<AbstractIndividual>();
				while (parsv.Count < 2)
				{
					var idx = ContextRandom.Next(selSize);
					parsv.Add(selPopul[idx]);
				}
				#endregion

				if (xRate < rand.NextDouble() || parsv[0] == parsv[1]) { continue; }

				#region - Cross selected individuals and produce 1 offspring. -
				var partsCount = parsv[0].Fitness.Extra.Count;
				var g1 = VectorMath.Split(((Individual)parsv[0]).Genes.ToArray(), partsCount);
				var g2 = VectorMath.Split(((Individual)parsv[1]).Genes.ToArray(), partsCount);

				var cGenes = new List<float>();
				for (int i = 0; i < partsCount; i++)
				{
					var w1 = parsv[0].Fitness.Extra[i];	// weight.
					var w2 = parsv[1].Fitness.Extra[i];	// weight.

					// p1 is the better parent part.
					// p2 is the worse parent part.
					var p1 = FitnessComparator.IsBetter(w1, w2) ? g1[i] : g2[i];
					var p2 = FitnessComparator.IsBetter(w1, w2) ? g2[i] : g1[i];

					//var child = WeightedCrossNormW(p1, w1, p2, w2, rand);
					var child = WeightedCross(p1, w1, p2, w2);
					cGenes.AddRange(child);
				}

				//
				// create offspring.
				var offsp = new Individual();
				offsp.Genes = cGenes;
				res.Add(offsp);
				#endregion
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Crossing of two parents with normalization of weights.
		/// </summary>
		/// <param name="p1">1st parent.</param>
		/// <param name="v1">Weight of the first parent.</param>
		/// <param name="p2">2nd parent.</param>
		/// <param name="v2">Weight of the second parent.</param>
		/// <param name="rand">RNG.</param>
		/// <returns></returns>
		public static float[] WeightedCrossNormW(float[] p1, float v1, float[] p2, float v2, Random rand)
		{
			var w1 = v1 / (v1 + v2);	// normalized weight1.
			var w2 = 1 - w1;	// normalized weight2.

			// gradW -- analog of the gradient of the weights.
			var gradW = Math.Abs(w1 - w2);	// absolute value of the 'gradient'.
			var size = p1.Length;
			var res = new float[size];

			for (int j = 0; j < size; j++)
			{
				// todo: think about the case, when x1 and x2 are of different signs and have close weights.
				// variant 2 (smth like Euler's method).
				var x1 = p1[j];
				var x2 = p2[j];
				var delta = x1 - x2;	// sign of delta shows direction of the movement.
				delta = (float)(delta * Math.Exp(-delta));	// update delta value with exponential scaling (when delta is large the gradient estimate is unreliable).

				var c = (float)(x1 + gradW * rand.NextDouble() * delta);
				res[j] = c;
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Crossing of two parents using Euler's weights.
		/// </summary>
		/// <param name="p1">1st parent (the better one).</param>
		/// <param name="v1">Weight of the first parent.</param>
		/// <param name="p2">2nd parent.</param>
		/// <param name="v2">Weight of the second parent.</param>
		/// <returns></returns>
		public static float[] WeightedCross(float[] p1, float v1, float[] p2, float v2)
		{
			var sub = VectorMath.Sub(p1, p2);
			var dist = VectorMath.EuclidianDistance(p1, p2);

			if (dist != 0)
			{
				// gradW -- analog of the gradient of the weights.
				//var gradW = Math.Abs(v1 - v2) / dist * Math.Exp(-dist);	// absolute normalized value of the 'gradient', exponentially weighted by the distance.
				var gradW = Math.Abs(v1 - v2) / dist;	// absolute normalized value of the 'gradient', exponentially weighted by the distance.
				VectorMath.Mul(ref sub, gradW);

				var res = VectorMath.Add(p1, sub);
				return res;
			}

			// if two parents are equal then return copy of the 1st parent.
			return (float[])p1.Clone();
		}
		#endregion

		/// <summary>
		/// [molecule]
		/// 
		/// Performs crossing of the given the population using BLX-alpha crossover and creates [populSize] individuals.
		/// </summary>
		/// <param name="selPopul">Population to cross.</param>
		/// <param name="parameters">EA parameters.</param>
		/// <returns>List of offsprings.</returns>
		public static List<AbstractIndividual> CrossBlx(List<AbstractIndividual> selPopul, EAParameters parameters)
		{
			var children = new List<AbstractIndividual>();

			var populSize = parameters.PopulationSize;
			var rand = parameters.RNG;
			var xRate = parameters.XRate;
			var populSize_2 = parameters.PopulationSize / 2;
			var genesCount = ((Individual)selPopul[0]).Genes.Count;
			for (var i = 0; i < populSize_2; ++i)
			{
				var i1 = (int)(rand.NextDouble() * populSize);
				int i2;
				var p1 = selPopul[i1];
				do
				{
					i2 = (int)(rand.NextDouble() * populSize);
				} while (i2 == i1);
				var p2 = selPopul[i2];
				Individual c1 = (Individual)p1.Clone(), c2 = (Individual)p2.Clone();

				if (xRate > rand.NextDouble() && p1 != p2)
				{	// make crossing using BLX-alpha crossover.
					var alpha = 0.5f;
					for (var j = 0; j < genesCount; ++j)
					{
						var pw1 = ((Individual)p1).Genes[j];
						var pw2 = ((Individual)p2).Genes[j];
						float cw1, cw2;
						CrossBlx(pw1, pw2, out cw1, out cw2, alpha, rand);

						c1.Genes[j] = cw1;
						c2.Genes[j] = cw2;
					}
				}

				children.Add(c1);
				children.Add(c2);
			}

			// shrink population size if necessary.
			if (children.Count > populSize)
			{
				children.RemoveAt(children.Count - 1);
			}

			return children;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates new values for variables according to the BLX-alpha crossover formulae.
		/// </summary>
		/// <param name="pw1">Parent1 gene.</param>
		/// <param name="pw2">Parent2 gene.</param>
		/// <param name="cw1">Child1 gene.</param>
		/// <param name="cw2">Child2 gene.</param>
		/// <param name="alpha">Parameter.</param>
		/// <param name="rand">RNG.</param>
		public static void CrossBlx(float pw1, float pw2, out float cw1, out float cw2, float alpha, Random rand)
		{
			var g1 = Math.Min(pw1, pw2);
			var g2 = Math.Max(pw1, pw2);
			var delta = g2 - g1;

			g1 -= delta * alpha;
			g2 += delta * alpha;
			delta = g2 - g1;

			cw1 = (float)(rand.NextDouble() * delta + g1);
			cw2 = (float)(rand.NextDouble() * delta + g1);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs PCX-crossover operator.
		/// For all formulae see:
		///		1. K. Deb, A. Anand, and D. Joshi. A computationally efficient evolutionary algorithm for real-parameter optimization. Technical report, Indian Institute of Technology, April 2002.
		///		2. K. Deb. A population-based algorithm-generator for real-parameter optimization. Soft Computing, 9(4):236–253, April 2005.
		/// </summary>
		/// <param name="selPopul">Population to cross.</param>
		/// <param name="parameters">EA parameters.</param>
		/// <returns>List of offsprings.</returns>
		public static List<AbstractIndividual> CrossPcx(List<AbstractIndividual> selPopul, EAParameters parameters)
		{
			var res = new List<AbstractIndividual>();

			var populSize = parameters.PopulationSize;
			var parentsNum = parameters.ParentsNumber;
			var offspringNum = parameters.OffspringNumber;
			var selSize = selPopul.Count;

			while (res.Count < populSize)
			{
				#region - Select parents. -
				//
				// select randomly [parentsNum] parents.
				var parsv = new List<float[]>();
				while (parsv.Count < parentsNum)
				{
					var idx = ContextRandom.Next(selSize);
					var selInd = (Individual)selPopul[idx];
					parsv.Add(selInd.Genes.ToArray());
				}
				#endregion

				var offsp = CrossPcx(parsv, offspringNum);
				for (int i = 0; i < offsp.Count && res.Count < populSize; i++)
				{
					var child = new Individual();
					child.Genes = new List<float>(offsp[i]);
					res.Add(child);
				}
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Produces offspring using PCX crossover from the given set of parent vectors.
		/// </summary>
		/// <param name="parents"></param>
		/// <param name="offspringNum"></param>
		/// <returns></returns>
		public static List<float[]> CrossPcx(List<float[]> parents, int offspringNum)
		{
			var res = new List<float[]>();

			var indSize = parents[0].Length;
			var parentsNum = parents.Count;

			#region - RNGs. -
			var w1 = new NormalDistribution(0, 0.1);	// sigma = 0.1 as from Deb et al., 2002.
			var w2 = new NormalDistribution(0, 0.1);	// sigma = 0.1 as from Deb et al., 2002.
			#endregion

			//
			// Calculate mean vector for the selected parent individuals. -
			var g = VectorMath.MeanVector(parents);

			#region - Create offsprings. -
			//
			// Create offsprings.
			for (int j = 0; j < offspringNum; j++)
			{
				// select random parent.
				var idx = ContextRandom.Next(parentsNum);
				var selParent = parents[idx];

				//
				// define offspring vector.
				var offspring = (float[])selParent.Clone();

				if (!VectorMath.Equal(offspring, g, 0f))
				{	// if offspring is not at the parents center, e.g. all parents are not equal.
					// calculate average distance of all parents to the [dp].
					//var dist = 0f;
					for (int k = 0; k < parentsNum; k++)
					{
						if (k == idx) continue;
						//dist += Numerics.CalculateDistance(g, selParent, parents[k]);
					}
					//dist /= (parentsNum - 1);

					// calculate offset to the parental center.
					var dp = VectorMath.Sub(selParent, g);

					// Calculate orthonormal basis which spans subspace perpendicular to [dp].
					#region - Remove zeroes from dp. -
					// this is a small hack in order to ease the orthonormal basis search.
					for (int i = 0; i < indSize; i++)
					{
						if (dp[i] == 0)
						{
							dp[i] = (float)w1.NextDouble();
						}
					}
					#endregion
					var basis = Numerics.GetOrthonormalBasisPerp(dp);

					// 1. calculate basis combination.
					//w2.Sigma = dist;
					foreach (var ort in basis)
					{
						//var rand2 = w2.NextDouble() * dist;
						var rand2 = w2.NextDouble();
						var tempV = VectorMath.Mul(ort, (float)rand2);
						VectorMath.Accumulate(ref offspring, tempV);
					}

					// 2. calculate selected parent-defined offset.
					//w1.Sigma = VectorMath.L2Norm(dp);
					var rand1 = w1.NextDouble();
					VectorMath.Accumulate(ref offspring, VectorMath.Mul(dp, (float)rand1));
				}
				else
				{	// all parents are equal.
					var tempV = new float[indSize];
					//var oldSigma = w1.Sigma;
					//w1.Sigma = ContextRandom.NextDouble();
					for (int i = 0; i < indSize; i++)
					{
						tempV[i] = (float)w1.NextDouble();
					}
					//w1.Sigma = oldSigma;

					VectorMath.Accumulate(ref offspring, tempV);
				}


				res.Add(offspring);
			}
			#endregion

			return res;
		}
		#endregion

		#region - Mutation. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs mutation of the given population. The result is stored in the input population.
		/// </summary>
		/// <param name="popul">Population to mutate.</param>
		/// <param name="parameters">EA parameters.</param>
		public static void Mutate(List<AbstractIndividual> popul, EAParameters parameters)
		{
			foreach (var individual in popul)
			{
				Mutate(individual, parameters);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs mutation for real-coded individual.
		/// </summary>
		/// <param name="ind"></param>
		/// <param name="parameters"></param>
		public static void Mutate(AbstractIndividual ind, EAParameters parameters)
		{
			var genesCount = ((Individual)ind).Genes.Count;
			for (var i = 0; i < genesCount; ++i)
			{
				if (parameters.MRate <= parameters.RNG.NextDouble()) continue;

				// the mutation changes up to (parameters.MutationStep * 100) % of gene's value.
				//var gene = parameters.MutationStep * ((Individual)ind).Genes[i];
				//((Individual)ind).Genes[i] += (float)((2 * parameters.RNG.NextDouble() - 1.0) * gene);
				var delta = (float)((2 * parameters.RNG.NextDouble() - 1.0) * parameters.MutationStep);
				((Individual)ind).Genes[i] += delta;
			}
		}
		#endregion
		#endregion

		#region - Coevolutionary operators. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs coevolutionary evaluation of individuals in the given EAs.
		/// The number of evaluations equals to [popSize] argument.
		/// </summary>
		/// <param name="eas"></param>
		/// <param name="fitFunc"></param>
		/// <param name="popSize"></param>
		/// <returns></returns>
		public static Stats Evaluate(EvolutionaryAlgorithm[] eas, FitnessFunction fitFunc, int popSize)
		{
			// reset usage counts.
			foreach (var ea in eas)
			{
				foreach (var ind in ea.Population)
				{
					((Individual)ind).Tag = 0;
					ind.Fitness.Value = 0;
				}
			}

			// compute fitness.
			var fits = new List<Fitness>();
			for (int i = 0; i < popSize; i++)
			{
				// get sub-solutions from each sub-EA.
				var inds = new Individual[eas.Length];
				var w = new List<float>();
				for (int i1 = 0; i1 < eas.Length; i1++)
				{
					var ea = eas[i1];
					inds[i1] = (Individual) ea.GetRandomIndividual();
					w.AddRange(inds[i1].Genes);
				}

				var f = fitFunc.Compute(w.ToArray());
				fits.Add(f);	// for stats.

				// add resulting fitness to all individuals.
				foreach (var ind in inds)
				{
					ind.Fitness.Value += f.Value;
					ind.Tag = (int)ind.Tag + 1;	// increment number of usages.
				}
			}
			var fitStats = GetFitnessStats(fits.ToArray());

			// compute averaged fitness values.
			// reset usage counts.
			foreach (var ea in eas)
			{
				foreach (var ind in ea.Population)
				{
					var tagVal = (int)((Individual) ind).Tag;
					if (tagVal != 0)
					{
						ind.Fitness.Value /= tagVal;
					}
					else
					{	// we know nothing about this individual so assign them mean fitness to enable the probability of their selection.
						ind.Fitness.Value = fitStats.Mean;
					}
				}
			}

			//
			// compute fitness stats.
			return fitStats;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs coevolutionary evaluation of individuals in the given populations.
		/// The number of evaluations equals to [popSize] argument.
		/// </summary>
		/// <param name="populs"></param>
		/// <param name="fitFunc"></param>
		/// <param name="popSize"></param>
		/// <returns></returns>
		public static Stats Evaluate(List<Individual>[] populs, FitnessFunction fitFunc, int popSize)
		{
			// reset usage counts.
			foreach (var popul in populs)
			{
				foreach (var ind in popul)
				{
					ind.Tag = 0;
					ind.Fitness.Value = 0;
				}
			}

			// compute fitness.
			var fits = new List<Fitness>();
			for (int i = 0; i < popSize; i++)
			{
				// get sub-solutions from each sub-EA.
				var inds = new Individual[populs.Length];
				var w = new List<float>();
				for (int i1 = 0; i1 < populs.Length; i1++)
				{
					var popul = populs[i1];
					inds[i1] = popul[ContextRandom.Next(popul.Count)];
					w.AddRange(inds[i1].Genes);
				}

				var f = fitFunc.Compute(w.ToArray());
				fits.Add(f);	// for stats.

				// add resulting fitness to all individuals.
				foreach (var ind in inds)
				{
					ind.Fitness.Value += f.Value;
					ind.Tag = (int)ind.Tag + 1;	// increment number of usages.
				}
			}
			var fitStats = GetFitnessStats(fits.ToArray());

			// compute averaged fitness values.
			// reset usage counts.
			foreach (var popul in populs)
			{
				foreach (var ind in popul)
				{
					var tagVal = (int)ind.Tag;
					if (tagVal != 0)
					{
						ind.Fitness.Value /= tagVal;
					}
					else
					{	// we know nothing about this individual so assign them mean fitness to enable the probability of their selection.
						ind.Fitness.Value = fitStats.Mean;
					}
				}
			}

			//
			// compute fitness stats.
			return fitStats;
		}

		/// <summary>
		/// Per-algorithm selection.
		/// </summary>
		/// <param name="eas"></param>
		/// <param name="parameters"></param>
		public static void Select(EvolutionaryAlgorithm[] eas, EAParameters parameters)
		{
			foreach (var ea in eas)
			{
				ea.Select(parameters);
			}
		}

		/// <summary>
		/// Per-algorithm crossing.
		/// </summary>
		/// <param name="eas"></param>
		/// <param name="parameters"></param>
		public static void Cross(EvolutionaryAlgorithm[] eas, EAParameters parameters)
		{
			foreach (var ea in eas)
			{
				ea.Cross(parameters);
			}
		}

		/// <summary>
		/// Per-algorithm mutation.
		/// </summary>
		/// <param name="eas"></param>
		/// <param name="parameters"></param>
		public static void Mutate(EvolutionaryAlgorithm[] eas, EAParameters parameters)
		{
			foreach (var ea in eas)
			{
				ea.Mutate(parameters);
			}
		}

		/// <summary>
		/// Per-algorithm next generation.
		/// </summary>
		/// <param name="eas"></param>
		/// <param name="parameters"></param>
		public static void NextGeneration(EvolutionaryAlgorithm[] eas, EAParameters parameters)
		{
			foreach (var ea in eas)
			{
				ea.NextGeneration(parameters);
			}
		}
		#endregion

		#region - Statistics. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes ML estimates for the given population if each individual can be converted to the fixed length array of floats.
		/// </summary>
		/// <param name="popul"></param>
		/// <param name="mean"></param>
		/// <param name="cov"></param>
		public static void ComputeMLEstimates (List<AbstractIndividual> popul, out float[] mean, out float[,] cov)
		{
			var inds = DecodeChromosomes(popul);
			mean = VectorMath.MeanVector(inds);

			var data = MatrixMath.ConvertToDoubles(MatrixMath.CreateFromRowsList(inds));
			double[,] tempCov;
			alglib.covm(data, out tempCov);
			cov = MatrixMath.ConvertToFloats(tempCov);
			//cov = MatrixMath.ComputeCovarianceMatrix(MatrixMath.Transpose(inds));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes ML estimates for the given population if each individual can be converted to the fixed length array of floats.
		/// </summary>
		/// <param name="popul"></param>
		/// <param name="mean"></param>
		/// <param name="cov"></param>
		public static void ComputeMLEstimatesBiased(List<AbstractIndividual> popul, out float[] mean, out float[,] cov)
		{
			var inds = DecodeChromosomes(popul);
			mean = VectorMath.MeanVector(inds);
			cov = MatrixMath.ComputeCovarianceMatrixBiased(MatrixMath.Transpose(inds));

			//var data = MatrixMath.ConvertToDoubles(MatrixMath.CreateFromRowsList(inds));
			//double[,] tempCov;
			//alglib.covm(data, out tempCov);
			//cov = MatrixMath.ConvertToFloats(tempCov);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes mean vector for population.
		/// </summary>
		/// <param name="popul"></param>
		public static float[] ComputeMean(List<AbstractIndividual> popul)
		{
			var inds = DecodeChromosomes(popul);
			return VectorMath.MeanVector(inds);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes ML estimate for correlation matrix for the given population if each individual can be converted to the fixed length array of floats.
		/// </summary>
		/// <param name="popul"></param>
		public static float[,] ComputeCorrelationMatrixBiased(List<AbstractIndividual> popul)
		{
			var inds = DecodeChromosomes(popul);
			return MatrixMath.ComputeCorrelationMatrix(MatrixMath.Transpose(inds));
		}
		#endregion

		#region - Logging. -
		/// <summary>
		/// [molecule]
		/// 
		/// Sub-class for operations with log files.
		/// </summary>
		public class Logging
		{
			public const string SECTION_START = "> ";
			public const string SUBSECTION_START = ">> ";
			public const string SUBSUBSECTION_START = ">>> ";
			public const string EXPDESCRIPTION_SECTION = SECTION_START + "Experiments description:";
			public const string ALGORITHM_SECTION = SECTION_START + "Algorithm name:";
			public const string PARAMETERS_SECTION = SECTION_START + "Algorithm parameters:";
			public const string OBJFUNCTION_SECTION = SECTION_START + "Objective function:";
			public const string TIMESTATS_SECTION = SECTION_START + "Time stats (ms):";
			public const string RESULTS_SECTION = SECTION_START + "Main results:";

			public const string CLASSIFIER_PERF_SECTION = SECTION_START + "Classification on training/validation/test data sets:";
			public const string ANN_STRUCTURE_SECTION = SECTION_START + "ANN structure:";
			public const string NE_INTRAINING_SECTION = SECTION_START + "NE performance during training:";
			public const string DECOMPOSITION_TYPE_SECTION = SECTION_START + "Decomposition type:";

			/// <summary>
			/// [molecule]
			/// 
			/// Writes EA parameters into the specified stream.
			/// </summary>
			/// <param name="writer">Output stream.</param>
			/// <param name="props">EA parameters.</param>
			public static void Write(StreamWriter writer, EAParameters props)
			{
				writer.WriteLine();
				writer.WriteLine(string.Format("Generations number:\t{0}", props.GenerationsNumber));
				writer.WriteLine(string.Format("Population size:\t{0}", props.PopulationSize));
				writer.WriteLine(string.Format("Tournament size:\t{0}", props.TournamentSize));
				writer.WriteLine(string.Format("Crossover rate:\t{0}", props.XRate));
				writer.WriteLine(string.Format("Mutation rate:\t{0}", props.MRate));
				writer.WriteLine();
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Returns information about individuals in a list of arrays.
			/// </summary>
			/// <param name="inds"></param>
			/// <returns></returns>
			public static List<string> ToStrings(List<AbstractIndividual> inds)
			{
				var res = new List<string>();
				foreach (var ind in inds)
				{
					res.Add(ind.ToString());
				}
				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Generates log-file lines with the following structure:
			/// > date and time
			/// > aim of the study
			/// > parameters settings
			/// > problem name (if any)
			/// > main numerical results & stats
			/// </summary>
			/// <param name="expDescr"></param>
			/// <param name="eaparams"></param>
			/// <param name="fitFunc"></param>
			/// <param name="stats"></param>
			/// <returns></returns>
			public static List<string> GetLogLines(ExperimentDescription expDescr, EAParameters eaparams, string algName, FitnessFunction fitFunc, Stats timeStats, List<Stats> stats)
			{
				var res = new List<string>();

				res.Add(EXPDESCRIPTION_SECTION);
				res.AddRange(expDescr.ToStrings());
				res.Add("");
				res.Add(PARAMETERS_SECTION);
				res.AddRange(eaparams.ToStrings());
				res.Add("");
				res.Add(ALGORITHM_SECTION);
				res.Add(algName);
				res.Add("");
				res.Add(OBJFUNCTION_SECTION);
				if (fitFunc is NEObjFunction)
				{
					res.Add(fitFunc + "(" + ((NEObjFunction)fitFunc).Regularizer + ")");
				}
				else
				{
					res.Add(fitFunc.ToString());
				}
				res.Add("");
				res.Add(TIMESTATS_SECTION);
				res.Add(timeStats.GetStatsString());
				res.Add("");
				res.Add(RESULTS_SECTION);
				res.AddRange(StructMath.ConvertToStringsList(stats, true));

				return res;
			}
		}
		#endregion

		#region - Conversion methods. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns real-coded representation of chromosomes.
		/// </summary>
		/// <param name="popul"></param>
		/// <returns></returns>
		public static List<float[]> DecodeChromosomes (List<AbstractIndividual> popul)
		{
			var res = new List<float[]>();
			for (int i = 0; i < popul.Count; i++)
			{
				var ind = (Individual)popul[i];
				res.Add(ind.Genes.ToArray());
			}
			return res;
		}
		#endregion

		#region - Misc. -
		/// <summary>
		/// [molecule]
		/// 
		/// Inserts given individual [ind] into the population so that it is placed before the first individual, which is worse than [ind].
		/// </summary>
		/// <param name="popul">Population.</param>
		/// <param name="ind">Individual to insert.</param>
		public static void InsertViaFitness(List<AbstractIndividual> popul, AbstractIndividual ind)
		{
			for (int i = 0; i < popul.Count; i++)
			{
				if (FitnessComparator.IsWorse(popul[i].Fitness, ind.Fitness))
				{
					// insert ind before popul[i].
					popul.Insert(i, ind);
					return;
				}
			}
			// add ind to the tail.
			popul.Add(ind);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Makes given population 'older' by 1 unit.
		/// </summary>
		/// <param name="popul">Population to get older.</param>
		public static int TimeFliesLikeArrow(List<AbstractIndividual> popul)
		{
			int maxAge = 0;
			foreach (var individual in popul)
			{
				individual.Age++;
				if (maxAge < individual.Age) maxAge = individual.Age;
			}
			return maxAge;
		}
		#endregion
	}

	#region - Algorithms. -
	/// <summary>
	/// Class for getting available EAs.
	/// </summary>
	public class EvolutionaryAlgorithms
	{
		public const string RC_EVOLUTIONARY_ALGORITHM = "Real-coded evolutionary algorithm";
		public const string AMALGAM_IDEA = "Amalgam IDEA";
		public const string G3PCX = "G3PCX algorithm";

		/// <summary>
		/// [molecule]
		/// 
		/// Returns list of available evolutionary algorithms.
		/// </summary>
		/// <returns>List of supported EA names.</returns>
		public static string[] Algorithms ()
		{
			var res = new List<string>();
			res.Add(RC_EVOLUTIONARY_ALGORITHM);
			res.Add(AMALGAM_IDEA);
			res.Add(G3PCX);
			return res.ToArray();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns EA by its name.
		/// </summary>
		/// <param name="name">EA name.</param>
		/// <returns>Algorithm instance.</returns>
		public static EvolutionaryAlgorithm GetAlgorithm (string name)
		{
			if (name == RC_EVOLUTIONARY_ALGORITHM) return new EvolutionaryAlgorithm();
			if (name == AMALGAM_IDEA) return new AmalgamIDEA();
			if (name == G3PCX) return new G3Pcx();

			throw new Exception(string.Format("Unknown EA name: {0}", name));
		}
	}
	#endregion
}
