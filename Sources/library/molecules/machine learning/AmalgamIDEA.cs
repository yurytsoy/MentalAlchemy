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
using MathNet.Numerics.LinearAlgebra;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// [molecule]
	/// 
	/// AMaLGaM IDEA implementation with anticipated mean shift (AMS) using description from:
	///		Bosman, P.; Grahl, J. & Thierens, D. (2007), 
	///		'Adapted Maximum-Likelihood Gaussian Models for Numerical Optimization with Continuous EDAs'
	///		Technical report, CWI
	/// see p. 14. The lines numbers are set according to that algorithm's pseudocode.
	/// </summary>
	public class AmalgamIDEA : EvolutionaryAlgorithm
	{
		protected const float tau = 0.3f;	// truncation selection threshold.
		protected float thetaSdr = 1;	// threshold for normalized improvements.
		protected const float nDecrease = 0.9f;	// mean shift multiplier
		protected const float nIncrease = 1 / nDecrease;	// mean shift multiplier
		protected float alpha = tau / (2 - 2*tau);
		protected float delta = 2;
		protected float cMult;
		protected int t;
		float[] mean, muPrev;
		float[,] cov;

		protected List<Individual> offsp;

		public bool UseCovariance = true;	// if [false] then correlation matrix is used.
		public bool SwitchToCorrelation = false;	// if [true] then covariance is changed to correlation matrix when fitness improves for less than 10% over last generation.
		public bool EnableDistrDegradation = false;	// if [true] then covariance/correlation matrix is multiplied on the additional factor which degrades with the number of generations (1/Generation).

		public int SwitchGeneration;

		public new Individual BestIndividual
		{
			get
			{
				return (Individual)EAElements.GetBestIndividual(popul);
			}
		}

		/// <summary>
		/// Population of offspring individuals.
		/// </summary>
		public List<Individual> Offspring
		{
			get { return offsp; }
			set { offsp = value; }
		}

		public override void Init(EAParameters parameters)
		{
			//parameters.MinGeneValue = -10;
			//parameters.GeneValueRange = 15;
			base.Init(parameters);
			cMult = 1;

			SwitchGeneration = 0;
			if (SwitchToCorrelation)
			{	// force starting from the covariance matrix.
				UseCovariance = true;
			}
		}

		public override void Continue(EAParameters parameters)
		{
			//
			// validate [parameters].
			if (FitnessFunction != null && !EAElements.ValidateParameters(parameters)) throw new Exception("[AmalgamIDEA.Continue]: Invalid parameters setting or fitness function is undefined.");

			Evaluate();

			//
			// Main cycle.
			//var muPrev = new float[1];
			//int selSize = (int)(tau*popul.Count);
				//,
				//offspSize = popul.Count - selSize,
				//shiftSize = (int)(alpha * selSize);
			for (CurrentGeneration = 1; CurrentGeneration <= parameters.GenerationsNumber; ++CurrentGeneration)
			{
				Select(parameters);
				Cross(parameters);
				Mutate(parameters);
				NextGeneration(parameters);
			}
		}

		/// <summary>
		/// Selection.
		/// </summary>
		/// <param name="parameters"></param>
		public override void Select(EAParameters parameters)
		{
			int selSize = (int)(tau * popul.Count);
			selPopul = EAElements.TruncationSelection(popul, selSize);	// line 11.01
		}

		/// <summary>
		/// Crossing
		/// </summary>
		/// <param name="parameters"></param>
		public override void Cross(EAParameters parameters)
		{
			var offspSize = popul.Count - selPopul.Count;

			// compute ML estimates.
			if (UseCovariance)
			{
				EAElements.ComputeMLEstimates(selPopul, out mean, out cov);	// line 11.02
			}
			else
			{
				mean = EAElements.ComputeMean(popul);
				cov = EAElements.ComputeCorrelationMatrixBiased(popul);
			}
			cov = !EnableDistrDegradation ? MatrixMath.Mul(cov, cMult) : MatrixMath.Mul(cov, cMult / CurrentGeneration);
			var gaussDistr = new GaussianDistribution(mean, cov);

			//
			// create offsprings.
			offsp = new List<Individual>();
			for (int i = 0; i < offspSize; i++)	// 11.04
			{
				var tempOffsp = new Individual();
				tempOffsp.Genes = new List<float>(gaussDistr.Next());
				offsp.Add(tempOffsp);	// 11.04.1
			}
		}

		/// <summary>
		/// Mutation and update of multipliers.
		/// </summary>
		/// <param name="parameters"></param>
		public override void Mutate(EAParameters parameters)
		{
			var shiftSize = (int)(alpha * selPopul.Count);

			//
			// apply anticipated mean shift.
			if (CurrentGeneration > 1)
			{
				var muShift = VectorMath.Sub(mean, muPrev);	//	11.05.1
				var shift = VectorMath.Mul(muShift, cMult * delta);
				for (int i = 0; i < shiftSize; i++)
				{
					offsp[i].Genes = new List<float>(VectorMath.Add(offsp[i].Genes.ToArray(), shift));	// 11.05.2.1
				}
			}

			//UpdateAlgorithmParameters(parameters);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Updates inner parameters of the algorithm.
		/// Note: If FitnessFunction is empty, the fitness should be assessed elsewhere
		///		before calling this method.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void UpdateAlgorithmParameters(EAParameters parameters)
		{
			var offspSize = popul.Count - selPopul.Count;

			//
			// find current best.
			var bestFit = EAElements.GetBestIndividual(selPopul).Fitness;	// 11.06

			//
			// Find out about improvements by offspring.
			var nImprovement = 0;	// 11.07. Number of cases when offspring is better than parents.
			var xImprovement = VectorMath.Zeros(offsp[0].Genes.Count);	// 11.08	// average direction of improvement by offsprings.
			for (int i = 0; i < offspSize; i++)
			{
				if (FitnessFunction != null)
				{
					offsp[i].Fitness = FitnessFunction.Compute(offsp[i].Genes.ToArray());	// 11.09.1
				}
				if (FitnessComparator.IsWorse(offsp[i].Fitness, bestFit)) continue;
                
				nImprovement++;
				VectorMath.Accumulate(ref xImprovement, offsp[i].Genes.ToArray());	// what if not offsp[i], but (offsp[i] - bestInd) should be used?
			}

			//
			// apply offpsring improvement.
			if (nImprovement > 0)	// 11.10
			{
				VectorMath.Mul(ref xImprovement, 1f / nImprovement);	// 11.10.1

				//// get Cholesky decomposition matrix.
				//var m = Matrix.Create(MatrixMath.ConvertToDoubles(cov));
				//var cholTri = m.CholeskyDecomposition.TriangularFactor;	// todo: check this!
				//var inv = cholTri.Inverse();	// invert Cholesky triangular factor.

				// compute rho
				var rho = 0f;
				for (int i = 0; i < xImprovement.Length; i++)	// 11.10.2
				{
					if (cov[i, i] == 0f) continue;

					var temp = Math.Abs(xImprovement[i] - mean[i]) / cov[i, i];
					if (temp > rho) { rho = temp; }
				}

				if (rho > thetaSdr)
				{
					cMult *= nIncrease;	// 11.10.3.1
				}
			}
			else
			{
				cMult *= nDecrease;	// 11.10.1
			}

			//if (cMult < 1){cMult = 1;} // 11.11 & 11.11.2
			if (cMult < parameters.MutationStep) { cMult = parameters.MutationStep; } // 11.11 & 11.11.2
		}

		/// <summary>
		/// Advance to the next generation.
		/// </summary>
		/// <param name="parameters"></param>
		public override void NextGeneration(EAParameters parameters)
		{
			UpdateAlgorithmParameters(parameters);

			popul.Clear();
			popul.AddRange(selPopul);
			for (int i = 0; i < offsp.Count; i++) { popul.Add(offsp[i]); }	// 11.12 & 11.13 because fitness is copied with genes.

			muPrev = mean;	// 11.14

			CollectStats();

			if (SwitchToCorrelation && UseCovariance && (CurrentGeneration > 1))
			{	// switch between matrices.
				var fitT = 0.02f;
				var fitDiff = Math.Abs(FitnessStats[CurrentGeneration - 2].Mean - FitnessStats[CurrentGeneration-1].Mean);
				if (fitDiff / FitnessStats[CurrentGeneration - 1].Mean < fitT)
				{
					UseCovariance = false;
					SwitchGeneration = CurrentGeneration;
				}
			}
		}

		///// <summary>
		///// 
		///// This is a C# version of Peter Bosman's funciton:
		/////		double *generateNewSolution( int population_index )
		///// from the file "AMaLGaM-Bayesian.c"
		///// </summary>
		///// <returns></returns>
		//public static float[] GenerateNewSolution (int paramsCount, int popIndex)
		//{
		//    int i, j, var_index, times_not_in_bounds;
		//    float[] result = new float[paramsCount];
		//    float mean;

		//    //(double *) Malloc( number_of_parameters*sizeof( double ) );

		//    for( i = 0; i < paramsCount; i++ )
		//    {
		//        var_index = sampling_ordering[popIndex][i];

		//        mean = mean_vectors[population_index][var_index]*precision_matrices_first_col[population_index][var_index][0];
		//    for( j = 0; j < parent_vectors_lengths[population_index][var_index]; j++ )
		//      mean -= (result[parent_vectors[population_index][var_index][j]] - mean_vectors[population_index][parent_vectors[population_index][var_index][j]])*precision_matrices_first_col[population_index][var_index][j+1];

		//    if( precision_matrices_first_col[population_index][var_index][0] == 0 )
		//      mean = mean_vectors[population_index][var_index];
		//    else
		//      mean /= precision_matrices_first_col[population_index][var_index][0];

		//    if( variance_vectors_sampling[population_index][var_index] <= 0.0 )
		//    {
		//      result[var_index] = mean;
		//      if( !isParameterInRangeBounds( result[var_index], var_index ) )
		//        result[var_index] = lower_init_ranges[var_index] + (upper_init_ranges[var_index] - lower_init_ranges[var_index])*randomRealUniform01();
		//    }
		//    else
		//    {
		//      times_not_in_bounds = -1;
		//      out_of_bounds_draws[population_index]--;
			  
		//      do
		//      {
		//        times_not_in_bounds++;
		//        samples_drawn_from_normal[population_index]++;
		//        out_of_bounds_draws[population_index]++;
			    
		//        if( times_not_in_bounds >= 100 )
		//          result[var_index] = lower_init_ranges[var_index] + (upper_init_ranges[var_index] - lower_init_ranges[var_index])*randomRealUniform01();
		//        else
		//          result[var_index] = random1DNormalParameterized( mean, variance_vectors_sampling[population_index][var_index] );
		//      }
		//      while( !isParameterInRangeBounds( result[var_index], var_index ) );
		//    }
		//    }

		//    return( result );
		//}
	}
}
