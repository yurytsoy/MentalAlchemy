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
using System.Globalization;
using System.IO;
using System.Threading;
using Encog.Neural.Activation;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Logic;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.NeuralData;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	public class MachineLearningElements
	{
		#region - Methods for voting. -
		/// <summary>
		/// [molecule]
		/// 
		/// Return Id of the class (key) with the maximum number of votes (value). If there is no winner then [failClassId] is returned.
		/// </summary>
		/// <param name="votes">Dictionary of number of votes for each class entry.</param>
		/// <param name="failClassId">Return value for the "no-winner" case.</param>
		/// <returns>ID of the winning class or [failClassId] otherwise.</returns>
		public static int GetMaxClassId (Dictionary<int, int> votes, int failClassId)
		{
			if (votes.Count == 0) return failClassId;

			var tempCounts = new int[votes.Count];
			var tempClassIds = new int[votes.Count];
			votes.Values.CopyTo(tempCounts, 0);
			votes.Keys.CopyTo(tempClassIds, 0);

			var max = VectorMath.Max(tempCounts);
			var count = VectorMath.Calculate(tempCounts, max);
			if (count == 1)
			{
				var idx = VectorMath.FirstIndexOf(tempCounts, max);
				return tempClassIds[idx];
			}

			return failClassId;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Return Id of the class (key) with the maximum probability. If there is no winner then [failClassId] is returned.
		/// </summary>
		/// <param name="probs">Dictionary of number of votes for each class entry.</param>
		/// <param name="failClassId">Return value for the "no-winner" case.</param>
		/// <returns>ID of the winning class or [failClassId] otherwise.</returns>
		public static int GetMaxClassId(Dictionary<int, float> probs, int failClassId)
		{
			if (probs.Count == 0) return failClassId;

			var tempProbs = new float[probs.Count];
			probs.Values.CopyTo(tempProbs, 0);

			var max = VectorMath.Max(tempProbs);
			var count = VectorMath.Calculate(tempProbs, max);
			if (count == 1)
			{
				var idx = VectorMath.FirstIndexOf(tempProbs, max);

				// return class ID using the found index [idx].
				var tempClassIds = new int[probs.Count];
				probs.Keys.CopyTo(tempClassIds, 0);
				return tempClassIds[idx];
			}

			return failClassId;
		}
		#endregion

		#region - Methods for [TrainingSample] -
		#region - Conversion. -
		/// <summary>
		/// [molecule]
		/// 
		/// Converts given training samples into list of [VectorProb] entries.
		/// </summary>
		/// <param name="data">List of training samples.</param>
		/// <returns>Training data entry if the search is successful or empty vector pair otherwise.</returns>
		public static List<VectorProb> ConvertToVectorProbs(List<TrainingSample> data)
		{
			if (data.Count == 0) return new List<VectorProb>();

			// look for vectors which are within the [eps] bounds.
			var res = new List<VectorProb>();
			foreach (var sample in data)
			{
				var rows = MatrixMath.ConvertToRowsList(sample.Data);

				foreach (var row in rows)
				{
					var pair = new VectorProb();
					pair.Init(row);
					pair.counts.Add(sample.ClassID, 1);
					res.Add(pair);
				}
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Converts input signals for each training sample into a vector of doubles.
		/// </summary>
		/// <param name="data">List of training samples.</param>
		/// <returns>List of vectors of doubles, each corresponding to a training sample input.</returns>
		public static List<double[]> ConvertInputsToVectorsDouble(List<TrainingSample> data)
		{
			var res = new List<double[]>();
			foreach (var sample in data)
			{
				var v = MatrixMath.ConvertToVector(sample.Data);
				var vd = VectorMath.ConvertToDoubles(v);
				res.Add(vd);
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Converts input signals for each training sample into a vector of floats.
		/// </summary>
		/// <param name="data">List of training samples.</param>
		/// <returns>List of vectors of floats, each corresponding to a training sample input.</returns>
		public static List<float[]> ConvertInputsToVectorsFloat(List<TrainingSample> data)
		{
			var res = new List<float[]>();
			foreach (var sample in data)
			{
				var v = MatrixMath.ConvertToVector(sample.Data);
				res.Add(v);
			}
			return res;
		} 

		/// <summary>
		/// [molecule]
		/// 
		/// Converts inputs from a given list of [TrainingSample] objects into a list of strings.
		/// Each input matrix is firstly converted into vector.
		/// </summary>
		/// <param name="data">List of training samples.</param>
		/// <returns>Resulting list of strings for input signals.</returns>
		public static List<string> ConvertInputsToStrings (List<TrainingSample> data)
		{
			var res = new List<string>();
			foreach (var sample in data)
			{
				var tempV = MatrixMath.ConvertToVector(sample.Data);
				res.Add(VectorMath.ConvertToString(tempV, '\t'));
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Converts given set of objects descriptions to the matrix, with rows corresponding to objects.
		/// </summary>
		/// <param name="data">List of objects descriptions.</param>
		/// <returns>Descriptions matrix.</returns>
		public static float[,] ConvertToMatrix(List<TrainingSample> data)
		{
			var vs = new List<float[]>();
			foreach (var sample in data)
			{
				vs.Add(MatrixMath.ConvertToVector(sample.Data));
			}

			return MatrixMath.CreateFromRowsList(vs);
		}
		#endregion

		#region - Utility methods. -

		/// <summary>
		/// [molecule]
		/// 
		/// Looks through the given [VectorProb] entries and searches for entries, containing the given vector with respect to the precision.
		/// </summary>
		/// <param name="data">Data to search from.</param>
		/// <param name="v">Vector to look for.</param>
		/// <param name="dist">Function to calculate distance between two vectors.</param>
		/// <param name="eps">Precision.</param>
		/// <returns>Training data entry if the search is successful or empty vector pair otherwise.</returns>
		public static List<VectorProb> GetSimilarVectorProbEntries(List<VectorProb> data, float[] v, DistanceMeasure1D dist, float eps)
		{
			if (data.Count == 0) return new List<VectorProb>();

			// look for vectors which are within the [eps] bounds.
			var res = new List<VectorProb>();
			foreach (var vp in data)
			{
				if (dist(vp.vector, v) >= eps) continue;

				res.Add(vp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates number of different classes in the given array of training samples.
		/// </summary>
		/// <param name="data">Array of training samples.</param>
		/// <returns>Number of different classes.</returns>
		public static int CalculateClasses(TrainingSample[] data)
		{
			var ids = GetDifferentClassesIds(data);
			return ids.Count;
		}

		/// <summary>
		/// Returns array of different IDs of classes for the given training samples.
		/// </summary>
		/// <param name="data">Array of training samples.</param>
		/// <returns>Classes' IDs.</returns>
		public static IList<int> GetDifferentClassesIds(TrainingSample[] data)
		{
			var clIds = new List<int>();
			foreach (var entry in data)
			{
				if (!clIds.Contains(entry.ClassID))
				{
					clIds.Add(entry.ClassID);
				}
			}
			return clIds;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns array of class IDs in their appearance order in the given array of training samples.
		/// </summary>
		/// <param name="data">Collection of training samples.</param>
		/// <returns>List of class IDs.</returns>
		public static List<int> GetClassIds(TrainingSample[] data)
		{
			var res = new List<int>();
			foreach (var sample in data)
			{
				res.Add(sample.ClassID);
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Fills responses for classification problem by predefined class IDs.
		/// Returns dictionary containing correspondence between response output index and class ID.
		/// </summary>
		/// <param name="data"></param>
		public static Dictionary<int, int> FillResponses(List<TrainingSample> data)
		{
			var cls = GetDifferentClassesIds(data.ToArray());
			var cor = new Dictionary<int, int>();	// dictionary to be returned.
			var corRev = new Dictionary<int, int>();	// local intermediate dictionary for fast access to response output index.
			for (int i = 0; i < cls.Count; i++)
			{
				cor.Add(i, cls[i]);
				corRev.Add(cls[i], i);
			}

			foreach (var sample in data)
			{
				var temp = VectorMath.Zeros(cls.Count);
				temp[corRev[sample.ClassID]] = 1.0f;
				sample.Response = MatrixMath.CreateFromVector(temp, temp.Length);
			}

			return cor;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates distance between two given data sets.
		/// </summary>
		/// <param name="data1">1st data set.</param>
		/// <param name="data2">2nd data set.</param>
		/// <returns>Distance.</returns>
		public static float CalculateDistance(List<TrainingSample> data1, List<TrainingSample> data2)
		{
			//
			// Form 2d array for the 1st data set.
			var table1 = new List<float[]>();
			foreach (var sample in data1)
			{
				var row = MatrixMath.ConvertToVector(sample.Data);
				table1.Add(row);
			}

			//
			// Form 2d array for the 1st data set.
			var table2 = new List<float[]>();
			foreach (var sample in data2)
			{
				var row = MatrixMath.ConvertToVector(sample.Data);
				table2.Add(row);
			}

			//
			// calculate all distances from the table1 to the table2.
			var normCoef = 1f / table1[0].Length;	// = 1 / max distance between two vectors.
			var dist12 = 0f;
			for (int i = 0; i < table1.Count; i++)
			{
				var row = table1[i];
				var idx = 0;
				var minDist = VectorMath.GetMinDistance(row, table2, out idx, VectorMath.EuclidianDistance);
				if (data1[i].ClassID != data2[idx].ClassID) { minDist += 1; }
				dist12 += minDist;
			}
			dist12 *= (normCoef / table1.Count);	// normalize dist12.

			//
			// calculate all distances from the table1 to the table2.
			var dist21 = 0f;
			for (int i = 0; i < table2.Count; i++)
			{
				var row = table2[i];
				var idx = 0;
				var minDist = VectorMath.GetMinDistance(row, table1, out idx, VectorMath.EuclidianDistance);
				if (data2[i].ClassID != data1[idx].ClassID) { minDist += 1; }
				dist21 += minDist;
			}
			dist21 *= (normCoef / table2.Count);	// normalize dist21.

			return 0.5f * (dist12 + dist21);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns training sample from a dataset closest to the reference sample in the sense of specified distance measure.
		/// </summary>
		/// <param name="refSample"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static TrainingSample GetClosestSample(TrainingSample refSample, List<TrainingSample> data, DistanceMeasure1D dist)
		{
			var dataM = ConvertToMatrix(data);
			var rows = MatrixMath.ConvertToRowsList(dataM);
			var refV = MatrixMath.ConvertToVector(refSample.Data);
			var idx = VectorMath.GetClosestVectorIndex(refV, rows, dist);

			return data[idx];
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Transforms data by substracting mean feature values. As a result all features has zero mean.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static List<TrainingSample> ConvertToZeroMean(List<TrainingSample> data)
		{
			var mData = ConvertToMatrix(data);
			var mean = MatrixMath.MeanRow(mData);

			var res = new List<TrainingSample>();
			var len = mean.Length;
			foreach (var sample in data)
			{
				var v = MatrixMath.ConvertToVector(sample.Data);
				var sub = VectorMath.Sub(v, mean);
				
				var temp = new TrainingSample();
				temp.ClassID = sample.ClassID;
				temp.Response = sample.Response;
				temp.Name = sample.Name;
				temp.Data = MatrixMath.CreateFromVector(sub, len);

				res.Add(temp);
			}

			return res;
		}
		#endregion

		#region - Generation. -

		/// <summary>
		/// [molecule]
		/// 
		/// Creates [TrainingSample] object from the given 1D vector using first [inputsNumber] elements as data and 
		///		the next [outputsNumber] elements as response. Also class ID is defined as index of the first 1.0 element
		///		of the response vector.
		/// </summary>
		/// <param name="v">Values vector.</param>
		/// <param name="inputsNumber">Number of inputs.</param>
		/// <param name="outputsNumber">Number of outputs.</param>
		/// <returns>Training sample.</returns>
		public static TrainingSample CreateTrainingSample(float[] v, int inputsNumber, int outputsNumber)
		{
			var tSample = new TrainingSample();
			tSample.Data = new float[1, inputsNumber];
			tSample.Response = new float[1, outputsNumber];

			// read inpus signals.
			int count = 0;
			for (int j = 0; j < inputsNumber; ++j, ++count)
			{
				tSample.Data[0, j] = v[count];
			}

			// read output signals
			for (int j = 0; j < outputsNumber; ++j, ++count)
			{
				tSample.Response[0, j] = v[count];
			}

			// define class ID.
			var tempOut = MatrixMath.GetRow(tSample.Response, 0);
			tSample.ClassID = VectorMath.FirstIndexOf(tempOut, 1.0f);

			return tSample;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates list of training samples from random Gaussians of specified dimensionality and
		/// using prescribed number of samples from each Gaussian.
		/// The centers of Gaussians are picked at random from the [-10, 10]^n cells.
		/// </summary>
		/// <param name="size">Number of samples from each random Gaussian.</param>
		/// <param name="dim">Dimensionality of Gaussians (and thus data).</param>
		/// <param name="distrCount">Number of distributions.</param>
		/// <returns></returns>
		public static List<TrainingSample> CreateGaussianSamples (int size, int dim, int distrCount)
		{
			const float MIN_MU = -3, MAX_MU = 3;
			//var distrs = new List<GaussianDistribution>();
			var res = new List<TrainingSample>();
			for (int i = 0; i < distrCount; i++)
			{
				var cov = MatrixMath.RandomCovariance(dim, ContextRandom.rand);
				var mu = VectorMath.CreateRandomVector(ContextRandom.rand, dim, MIN_MU, MAX_MU);
				var gauss = new GaussianDistribution(mu, cov);
				for (int j = 0; j < size; j++)
				{
					var tr = new TrainingSample();
					tr.Data = MatrixMath.CreateFromVector(gauss.Next(), dim);
					res.Add(tr);
				}
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates training samples from the given time-series and a specified window-size.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static List<TrainingSample> CreateFromTimeSeries (float[] data, int size)
		{
			var res = new List<TrainingSample>();

			var count = data.Length - size + 1;
			for (int i = 0; i < count; i++)
			{
				var temp = new float[size];
				for (int j = 0; j < size; j++)
				{
					temp[j] = data[j + i];
				}

				var tempSample = new TrainingSample();
				tempSample.Data = MatrixMath.CreateFromVector(temp, size);
				res.Add(tempSample);
			}

			return res;
		}
		#endregion

		#region - Computing features. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes Gram matrix for the features using given set of objects descriptions.
		/// Can be used to find out whether features are orthogonal or not.
		/// </summary>
		/// <param name="data">List of objects descriptions.</param>
		/// <returns>Gram matrix for features.</returns>
		public static float[,] ComputeGramMatrixFeatures(List<TrainingSample> data)
		{
			var m = ConvertToMatrix(data);
			var cols = MatrixMath.ConvertToColumnsList(m);
			return MatrixMath.ComputeGramMatrix(cols);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes autocorrelation matrix for the features using given set of objects descriptions.
		/// Can be used to find out some properties of the features (like eigenvectors for PCA).
		/// </summary>
		/// <param name="data">List of objects descriptions.</param>
		/// <returns>Autocorrelation matrix for features.</returns>
		public static float[,] ComputeAutocorrelationMatrix(List<TrainingSample> data)
		{
			var m = ConvertToMatrix(data);
			var mt = MatrixMath.Transpose(m);
			return MatrixMath.Mul(mt, m);
		}
		#endregion

		#region - Sampling. -
		/// <summary>
		/// [molecule]
		/// 
		/// Creates [count] subsets of training data using sequences of training samples.
		/// </summary>
		/// <param name="data">Training data.</param>
		/// <param name="count">Requested number of subsets.</param>
		/// <returns>List of training data subsets.</returns>
		public static List<List<TrainingSample>> ResampleSequential(List<TrainingSample> data, int count)
		{
			var res = new List<List<TrainingSample>>();

			int dataCount = data.Count;
			int subsetSize = dataCount * (count - 1) / count;
			int exclSize = dataCount - subsetSize;
			for (int i = 0; i < count; ++i)
			{
				var tempList = new List<TrainingSample>();
				int limit1 = i * exclSize, limit2 = (i + 1) * exclSize;
				for (int j = 0; j < dataCount; ++j)
				{
					// if [j] falls into 'restricted' region of the training data set.
					if (j >= limit1 && j <= limit2) continue;

					tempList.Add(data[j]);
				}
				res.Add(tempList);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates [count] subsets of training data using bootstrap approach.
		/// </summary>
		/// <param name="data">Training data.</param>
		/// <param name="count">Requested number of subsets.</param>
		/// <returns>List of training data subsets.</returns>
		public static List<List<TrainingSample>> ResampleBootstrap(List<TrainingSample> data, int count)
		{
			var res = new List<List<TrainingSample>>();

			int dataCount = data.Count;
			var rand = new Random();
			for (int i = 0; i < count; ++i)
			{
				var tempList = new List<TrainingSample>();
				for (int j = 0; j < dataCount; ++j)
				{
					var idx = rand.Next(dataCount);
					tempList.Add(data[idx]);
				}
				res.Add(tempList);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates random subset of the given data set consisting of [count] entries.
		/// </summary>
		/// <param name="data">Training data.</param>
		/// <param name="count">Requested number of data entries.</param>
		/// <returns>Resulting random subset.</returns>
		public static List<TrainingSample> SampleRandom(List<TrainingSample> data, int count)
		{
			var res = new List<TrainingSample>();

			int dataCount = data.Count;
			for (int i = 0; i < count; ++i)
			{
				var idx = ContextRandom.Next(dataCount);
				res.Add(data[idx]);
			}

			return res;
		} 

		/// <summary>
		/// [molecule]
		/// 
		/// Resamples given data using probability of selection of each entry.
		/// </summary>
		/// <param name="data">Data for resampling.</param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static List<TrainingSample> ResampleByDistribution (List<TrainingSample> data, float[] p)
		{
			var res = new List<TrainingSample>();
			for (int i = 0; i < data.Count; i++)
			{
				var idx = VectorMath.Roulette(p);
				res.Add(data[idx]);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Separates entries from [data] by their class IDs and returns array of lists, each containing samples with the same class ID.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static List<TrainingSample>[] SepararateByClassId(List<TrainingSample> data)
		{
			var clIds = GetClassIds(data.ToArray());

			// prepare resulting container.
			var res = new List<TrainingSample>[clIds.Count];
			for (int i = 0; i < clIds.Count; i++) {res[i] = new List<TrainingSample>();}

			// fill-in resulting array.
			foreach (var sample in data)
			{
				var idx = clIds.IndexOf(sample.ClassID);
				res[idx].Add(sample);
			}

			return res;
		}
		#endregion
		#endregion

		#region - Methods for [VectorProb]. -
		/// <summary>
		/// [molecule]
		/// 
		/// Calculate sums for vectors, probabilities and counters for the given list of [VectorProb] objects.
		/// </summary>
		/// <param name="vps"></param>
		/// <returns></returns>
		public static VectorProb Sum(List<VectorProb> vps)
		{
			if (vps.Count == 0) return new VectorProb();

			var vector = new float[vps[0].vector.Length];
			var counts = new Dictionary<int, int>();
			var probs = new Dictionary<int, float>();
			foreach (var pair in vps)
			{
				VectorMath.Accumulate(ref vector, pair.vector);
				StructMath.Accumulate(ref counts, pair.counts);
				StructMath.Accumulate(ref probs, pair.probs);
			}

			var res = new VectorProb();
			res.vector = vector;
			res.counts = counts;
			res.probs = probs;
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates average [VectorProb] object.
		/// </summary>
		/// <param name="vps">List of [VectorProb] objects.</param>
		/// <returns>Averaged vector-probability entity.</returns>
		public static VectorProb Average(List<VectorProb> vps)
		{
			var res = Sum(vps);

			// calculate averages.
			var size_1 = 1.0f / vps.Count;
			res.counts = StructMath.MulValues(res.counts, size_1);
			res.probs = StructMath.MulValues(res.probs, size_1);
			VectorMath.Mul(ref res.vector, size_1);

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates average [VectorProb] object, but keeping resulting counts as a sum of individual counts.
		/// Only probabilities and vectors' values are averaged.
		/// </summary>
		/// <param name="vps">List of [VectorProb] objects.</param>
		/// <returns>Averaged vector-probability entity.</returns>
		public static VectorProb AverageIgnoreCounts(List<VectorProb> vps)
		{
			var res = Sum(vps);

			// calculate averages.
			var size_1 = 1.0f / vps.Count;
			res.probs = StructMath.MulValues(res.probs, size_1);
			VectorMath.Mul(ref res.vector, size_1);

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Sums counters at each VectorProb objects.
		/// </summary>
		/// <param name="vps">List of [VectorProb] objects.</param>
		/// <returns>Sum of all counters with respect to classes IDs.</returns>
		public static Dictionary<int, int> SumCounts (List<VectorProb> vps)
		{
			if (vps.Count == 0) return new Dictionary<int, int>();

			var counts = new Dictionary<int, int>();
			foreach (var pair in vps)
			{
				StructMath.Accumulate(ref counts, pair.counts);
			}
			return counts;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Sums counters at each VectorProb objects.
		/// </summary>
		/// <param name="vps">List of [VectorProb] objects.</param>
		/// <returns>Sum of all counters with respect to classes IDs.</returns>
		public static Dictionary<int, float> WeightedSumCounts (List<VectorProb> vps)
		{

			throw new NotImplementedException("WeightedSumCounts");
			if (vps.Count == 0) return new Dictionary<int, float>();

			var counts = new Dictionary<int, int>();
			foreach (var pair in vps)
			{
				StructMath.Accumulate(ref counts, pair.counts);
			}
			return new Dictionary<int, float> ();
		}
		#endregion

		#region - Methods for calculation of inter- and intraclass distances. -
		/// <summary>
		/// [molecule]
		/// 
		/// Calculates intra and inter class distances.
		/// </summary>
		/// <param name="dist">Distance matrix correspondent to pairwise distances between objects descriptions.</param>
		/// <param name="classId">List of class IDs for each matrix row.</param>
		/// <param name="intraDist">Resulting array of intra-class distances.</param>
		/// <param name="interDist">Resulting array of inter-class distances.</param>
		public static void CalculateIntraInterDistances(float[,] dist, int[] classId, out Dictionary<int, float> intraDist, out Dictionary<int, float> interDist)
		{
			var maxCl = VectorMath.Max(classId);
			var clHist = VectorMath.GetPartialHistogram(classId, maxCl, classId.Length);
			interDist = new Dictionary<int, float> ();
			intraDist = new Dictionary<int, float>();

			//
			// init distances.
			foreach (var key in clHist.Keys)
			{
				interDist.Add(key, 0f);
				intraDist.Add(key, 0f);
			}

			//
			// inter-class distance is defined as a maximal distance between objects from this class.
			// intra-class distance is defined as a minimal distance between objects from different classes.
			int size = dist.GetLength(0);
			for (int i = 0; i < size; ++i)
			{
				for (int j = i + 1; j < size; ++j)
				{
					if (classId[i] != classId[j])
					{
						intraDist[classId[i]] += dist[i, j];
						intraDist[classId[j]] += dist[i, j];
					}
					else
					{
						interDist[classId[i]] += dist[i, j];
						//var temp = dist[i, j];
						//if (interDist[classId[i]] > temp)
						//{
						//    interDist[classId[i]] = temp;
						//}
						//if (interDist[classId[j]] > temp)
						//{
						//    interDist[classId[j]] = temp;
						//}
					}
				}
			}

			//
			// average distances.
			foreach (var pair in clHist)
			{
				// pairs = number of pairs among [pair.Value] objects.
				var pairs = pair.Value*(pair.Value - 1)*0.5f;
				interDist[pair.Key] /= pairs;
				intraDist[pair.Key] /= (classId.Length - pair.Value);
			}
		}
		#endregion

		#region - Training and test data loading. -
		/// <summary>
		/// [molecule]
		/// 
		/// Method to load data written in the Proben1 format.
		/// </summary>
		/// <param name="filename">Data file name.</param>
		/// <param name="trainData"></param>
		/// <param name="validData"></param>
		/// <param name="testData"></param>
		public static void LoadProben1Data(string filename, out List<TrainingSample> trainData, out List<TrainingSample> validData, out List<TrainingSample> testData)
		{
			int inputsNumber = 0;
			int outputsNumber = 0;

			#region - Read data from file. -
			// switch to neutral culture.
			var oldCI = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			using (var reader = new StreamReader(filename))
			{
				//
				// read number of inputs (bool + real)
				var str = reader.ReadLine();
				var index = str.IndexOf('=') + 1;
				inputsNumber += int.Parse(str.Substring(index));	// boolean inputs
				str = reader.ReadLine();
				index = str.IndexOf('=') + 1;
				inputsNumber += int.Parse(str.Substring(index));	// real inputs

				// read number of outputs (bool + real)
				str = reader.ReadLine();
				index = str.IndexOf('=') + 1;
				outputsNumber += int.Parse(str.Substring(index));	// boolean outputs
				str = reader.ReadLine();
				index = str.IndexOf('=') + 1;
				outputsNumber += int.Parse(str.Substring(index));	// real outputs

				// read number of training examples
				str = reader.ReadLine();
				index = str.IndexOf('=') + 1;
				int trainCount = int.Parse(str.Substring(index));

				// read number of validation examples
				str = reader.ReadLine();
				index = str.IndexOf('=') + 1;
				int validCount = int.Parse(str.Substring(index));

				// read number of test examples
				str = reader.ReadLine();
				index = str.IndexOf('=') + 1;
				int testCount = int.Parse(str.Substring(index));

				//
				// read training data.
				trainData = new List<TrainingSample>();
				var size = trainCount;
				char[] separators = { ' ', '\t', '\n' };
				for (var i = 0; i < size; i++)
				{
					str = reader.ReadLine();
					var numstr = str.Split(separators);
					numstr = StringUtils.RemoveEmptyElements(numstr);
					var nums = VectorMath.CreateFromStringsArray(numstr);
					trainData.Add(CreateTrainingSample(nums, inputsNumber, outputsNumber));
				}

				//
				// read validation data.
				validData = new List<TrainingSample>();
				size = validCount;
				for (var i = 0; i < size; i++)
				{
					str = reader.ReadLine();
					var numstr = str.Split(separators);
					numstr = StringUtils.RemoveEmptyElements(numstr);
					var nums = VectorMath.CreateFromStringsArray(numstr);
					validData.Add(CreateTrainingSample(nums, inputsNumber, outputsNumber));
				}

				//
				// read test data.
				testData = new List<TrainingSample>();
				size = testCount;
				for (var i = 0; i < size; i++)
				{
					str = reader.ReadLine();
					var numstr = str.Split(separators);
					numstr = StringUtils.RemoveEmptyElements(numstr);
					var nums = VectorMath.CreateFromStringsArray(numstr);
					testData.Add(CreateTrainingSample(nums, inputsNumber, outputsNumber));
				}
			}

			// restore culture.
			Thread.CurrentThread.CurrentCulture = oldCI;
			#endregion
		} 
		#endregion

		#region - Utility methods. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns Encog activation function using provided Mental Alchemy activation function.
		/// </summary>
		/// <param name="func">Mental Alchemy activation function.</param>
		/// <returns>Encog activation function.</returns>
		public static IActivationFunction GetActivationFunction (ActivationFunction func)
		{
			if (string.Compare(func.Method.Name, ActivationFunctions.SIGMOID_FUNCTION, true) == 0)
			{
				return new ActivationSigmoid();
			}
			if (string.Compare(func.Method.Name, ActivationFunctions.LINEAR_FUNCTION, true) == 0)
			{
				return new ActivationLinear();
			}
			if (string.Compare(func.Method.Name, ActivationFunctions.GAUSSIAN_FUNCTION, true) == 0)
			{
				return new ActivationGaussian(0, 0, 1);
			}
			throw new Exception(string.Format("[MachineLearningElements.GetActivationFunction] error: Incorrect activation function ({0})", func.Method.Name));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates all outputs of [net1] given the sets of inputs signals.
		/// </summary>
		/// <param name="net1">ANN.</param>
		/// <param name="data">Set of input signals.</param>
		/// <returns>Sequence of outputs.</returns>
		public static List<float[]> GetOutputs(LayeredNeuralNetwork net1, List<TrainingSample> data)
		{
			//
			// get vectors of signals from output nodes.
			int size = data.Count;
			var allOuts = new List<float[]>();
			for (var i = 0; i < size; i++)
			{
				var row = MatrixMath.GetRow(data[i].Data, 0);
				net1.Calculate(row);
				float[] outs;
				net1.GetOutputs(out outs);

				allOuts.Add(outs);
			}

			return allOuts;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Gets linearized sequence of [net1] output signals for the object described by the set of vectors.
		/// </summary>
		/// <param name="net1">ANN.</param>
		/// <param name="obj">Object description.</param>
		/// <returns>Sequence of outputs.</returns>
		public static List<float> GetOutputs (LayeredNeuralNetwork net1, float[,] obj)
		{
			var rows = MatrixMath.ConvertToRowsList(obj);
			var allOuts = new List<float>();
			foreach (var row in rows)
			{
				net1.Calculate(row);
				float[] outs;
				net1.GetOutputs(out outs);
				allOuts.AddRange(outs);
			}
			return allOuts;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes sum of variances for projections of the given training sample onto eigen vectors of autocorrelation matrix.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static float ComputeSumProjectionVariance (List<TrainingSample> data)
		{
			var ac = ComputeAutocorrelationMatrix(data);
			float[] eval;

			// computes eigenvector and eigenvalues and writes eigenvectors as columns.
			float[,] eVectors;
			Numerics.EigenMathNet.Eig(ac, out eVectors, out eval);

			// compute variance of the data along eigen-vectors.
			var trMatrix = ConvertToMatrix(data);
			var trInputs = MatrixMath.ConvertToRowsList(trMatrix);
			var dots = VectorMath.DotProduct(trInputs, MatrixMath.ConvertToColumnsList(eVectors));
			var dotCols = MatrixMath.ConvertToColumnsList(dots);
			var vars = VectorMath.VarianceList(dotCols);
			return VectorMath.Sum(vars);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates ANN w/o hidden layer and with orthogonal weight vectors.
		/// </summary>
		/// <returns></returns>
		public static LayeredNeuralNetwork CreateOrthogonalNetwork (int inputs, int outputs)
		{
			var w = new List<float[]>();
			for (int i = 0; i < outputs; i++)
			{
				w.Add( VectorMath.CreateRandomVector(ContextRandom.rand, inputs, -1, 1) );
			}

			w = Numerics.GramSchmidtOrthonormalization(w);
			var m = MatrixMath.CreateFromRowsList(w);	// weights matrix.

			var props = new NeuralNetProperties();
			props.nodesNumber = new int[]{inputs, outputs};
			props.actFunctions = new ActivationFunction[]{ActivationFunctions.Identity, ActivationFunctions.Linear};
			props.UseBias = false;

			var res = LayeredNeuralNetwork.CreateNetwork(props);
			res.SetConnectionWeights(MatrixMath.ConvertToVector(m));

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates layered ANN with sigmoid outputs.
		/// </summary>
		/// <param name="sizes"></param>
		/// <returns></returns>
		public static LayeredNeuralNetwork CreateLayeredNetworkSigmoid (int[] sizes)
		{
			var props = new NeuralNetProperties();
			props.UseBias = true;
			props.nodesNumber = sizes;

			var acts = new List<ActivationFunction>();
			acts.Add(ActivationFunctions.Identity);
			for (int i = 1; i < sizes.Length; i++)
			{
				acts.Add(ActivationFunctions.Sigmoid);
			}
			props.actFunctions = acts.ToArray();

			var res = LayeredNeuralNetwork.CreateNetwork(props);

			return res;
		}
		#endregion

		#region - Methods for the Encog library support. -
		public const string ENCOG_INPUT_LAYER_TAG = "INPUT";
		public const string ENCOG_OUTPUT_LAYER_TAG = "OUTPUT";

		/// <summary>
		/// [molecule]
		/// 
		/// Collects all weights from the given network including the biases.
		/// </summary>
		/// <param name="net">Encog network.</param>
		/// <returns>Array of connection weights.</returns>
		public static float[] GetWeights (BasicNetwork net)
		{
			var res = new List<double>();
			foreach (var tag in net.LayerTags)
			{
				var synapse = tag.Value.Next;
				foreach (var syn in synapse)
				{
					res.AddRange(syn.WeightMatrix.ToPackedArray());
				}
			}

			var resF = VectorMath.CreateFromDoubles(res.ToArray());
			return resF;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Create Encog BasicNetwork object using prescribed network properties.
		/// </summary>
		/// <param name="props">Network properties.</param>
		/// <returns></returns>
		public static BasicNetwork CreateEncogBasicNetwork(NeuralNetProperties props)
		{
			var layers = props.nodesNumber.Length;

			var network = new BasicNetwork();
			// add layers.
			for (int i = 0; i < layers; i++)
			{
				network.AddLayer(new BasicLayer(GetActivationFunction(props.actFunctions[i]), true, props.nodesNumber[i]));
			}
			network.Logic = new FeedforwardLogic();
			network.Structure.FinalizeStructure();
			network.Reset();

			return network;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Trains given ANN using the specified data set for the given number of epochs.
		/// </summary>
		/// <param name="net">Network to train.</param>
		/// <param name="samples">Training samples.</param>
		/// <param name="epochs">Training duration.</param>
		/// <returns>Errors dynamics.</returns>
		public static List<float> TrainEncogNetwork (BasicNetwork net, INeuralDataSet samples, int epochs)
		{
			ITrain train = new ResilientPropagation(net, samples);

			//
			// divide training epochs into 10 steps to display
			// training progress.
			//
			var ers = new List<float>();
			for (int i = 0; i < epochs; i++)
			{
				train.Iteration();
				ers.Add((float)train.Error);
			}

			return ers;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Trains Encog ANN using prescribed training data.
		/// </summary>
		/// <param name="props"></param>
		/// <param name="tData"></param>
		/// <param name="epochs"></param>
		/// <param name="ers"></param>
		/// <returns></returns>
		public static BasicNetwork TrainEncogNetwork (NeuralNetProperties props, List<TrainingSample> tData, int epochs, out List<float> ers)
		{
			var net = CreateEncogBasicNetwork(props);
			var data = CreateEncogTrainingData(tData);

			ers = TrainEncogNetwork(net, data, epochs);
			return net;
		}


		/// <summary>
		/// [molecule]
		/// 
		/// Trains given ANN using the specified training and validation data set for up to the given number of epochs. Early stopping is used to avoid overfitting.
		/// </summary>
		/// <param name="net">Network to train.</param>
		/// <param name="samples">Training samples.</param>
		/// <param name="validSamples">Validation samples.</param>
		/// <param name="epochs">Training duration.</param>
		/// <returns>Errors dynamics.</returns>
		public static List<float> TrainEncogNetwork(BasicNetwork net, INeuralDataSet samples, INeuralDataSet validSamples, int epochs)
		{
			ITrain train = new ResilientPropagation(net, samples);

			//
			// divide training epochs into 10 steps to display
			// training progress.
			//
			var ers = new List<float>();
			var oldValidationError = net.CalculateError(validSamples);
			for (int i = 0; i < epochs; i++)
			{
				train.Iteration();
				ers.Add((float)train.Error);

				var curValidationError = net.CalculateError(validSamples);
				// if current error on the validation set exceeds old one for more than 10%.
				if (curValidationError / oldValidationError > 1.1)
				{
					break;
				}
				// else update old error value.
				oldValidationError = curValidationError;
			}

			return ers;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Converts multi-layered network with 1 hidden layer into the Encog basic network.
		/// </summary>
		/// <param name="net"></param>
		/// <returns></returns>
		public BasicNetwork CreateFromLayeredNetwork (LayeredNeuralNetwork net)
		{
			var res = CreateEncogBasicNetwork(net.GetNetworkProperties());

			//
			// copy weights from ANN-1 into the [net2].
			var tempLayer = res.LayerTags[ENCOG_INPUT_LAYER_TAG].Next[0];	// now inputLayer.WeightMatrix -- contains weights of outcoming connections from the input layer.

			for (int i = 1; i < res.Structure.Layers.Count; i++)
			{
				tempLayer = res.Structure.Layers[i - 1].Next[0];

				// 1. Copy weights from ANN-1 into the first layer of ANN-2.
				var w1 = net.Layers[i].GetWeights();
				var w1tr = MatrixMath.Transpose(w1);

				var rows = w1tr.Count;
				var cols = w1.Count;
				for (int k = 0; k < rows - 1; k++)
				{
					for (int j = 0; j < cols; j++)
					{
						tempLayer.WeightMatrix[k, j] = w1tr[k + 1][j];	// k+1 because 0th row contains biases.
					}
				}

				// 0. copy bias values.
				var hidLayer = res.Structure.Layers[i];
				for (int j = 0; j < hidLayer.Threshold.Length; j++)
				{
					hidLayer.Threshold[j] = w1tr[0][j];
				}
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates Encog training smples from a given data.
		/// </summary>
		/// <param name="trainData"></param>
		public static INeuralDataSet CreateEncogTrainingData(List<TrainingSample> trainData)
		{
			INeuralDataSet samples = new BasicNeuralDataSet();
			for (int i = 0; i < trainData.Count; ++i)
			{
				// convert data from floats to doubles and create new training samples.
				var ins = MatrixMath.ConvertToVector(trainData[i].Data);
				var insD = VectorMath.ConvertToDoubles(ins);
				var outRow = MatrixMath.GetRow(trainData[i].Response, 0);
				var outRowD = VectorMath.ConvertToDoubles(outRow);
				samples.Add(new BasicNeuralData(insD), new BasicNeuralData(outRowD));
			}

			return samples;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes ANN responses to the given data.
		/// </summary>
		/// <param name="net"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static List<float[]> GetOutputs(BasicNetwork net, List<TrainingSample> data)
		{
			var testData = CreateEncogTrainingData(data);

			var res = new List<float[]>();
			foreach (var sample in testData)
			{
				var output = net.Compute(sample.Input);
				res.Add(VectorMath.CreateFromDoubles(output.Data));
			}

			return res;
		}
		#endregion

		#region - Methods for classifiers testing. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs testing of the given classification algorithm and returns classification results for the given data set.
		/// </summary>
		/// <param name="alg">Classification lgorithm to test.</param>
		/// <param name="testData">Test data.</param>
		/// <returns>Classification results.</returns>
		public static Dictionary<string, int> TestAlgorithm(IClassifier alg, IEnumerable<TrainingSample> testData)
		{
			//
			// perform testing.
			var res = new Dictionary<string, int>();
			var count = 0;
			foreach (var sample in testData)
			{
				var clId = alg.Recognize(sample.Data);
				var name = sample.Name;
				if (string.IsNullOrEmpty(name))
				{
					name = string.Format("sample {0}", count);
				}

				res.Add(name, clId);
				++count;
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs testing of the given classification algorithm and returns MSE for the given data set.
		/// </summary>
		/// <param name="net">ANN to test.</param>
		/// <param name="testData">Test data.</param>
		/// <returns>MSE.</returns>
		public static float ComputeMSE (IVectorFunction net, IEnumerable<TrainingSample> testData)
		{
			//
			// perform testing.
			var res = 0f;
			var count = 0;
			foreach (var sample in testData)
			{
				var input = MatrixMath.ConvertToVector(sample.Data);
				net.Calculate(input);

				float[] output;
				net.GetOutputs(out output);

				var groundOut = MatrixMath.ConvertToVector(sample.Response);
				var diff = VectorMath.Sub(output, groundOut);

				res += VectorMath.DotProduct(diff, diff);
				++count;
			}
			return res / count;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs testing of the given classification algorithm and returns classification error for the given data set.
		/// </summary>
		/// <param name="net">ANN to test.</param>
		/// <param name="testData">Test data.</param>
		/// <returns>Classification results.</returns>
		public static float TestNeuralNetwork(IVectorFunction net, IEnumerable<TrainingSample> testData)
		{
			//
			// perform testing.
			var count = 0;
			var ersCount = 0;
			foreach (var sample in testData)
			{
				var input = MatrixMath.ConvertToVector(sample.Data);
				net.Calculate(input);

				float[] output;
				net.GetOutputs(out output);
				var maxId = VectorMath.IndexOfMax(output);

				var groundOut = MatrixMath.ConvertToVector(sample.Response);
				var groundMaxId = VectorMath.IndexOfMax(groundOut);

				if (maxId != groundMaxId) ersCount++;
				++count;
			}
			return (float)ersCount / count;
		}

		/// <summary>
		/// [molecule] 
		/// 
		/// Calculates confusion matrix for the given classifier's outputs.
		/// </summary>
		/// <param name="refData">Reference data.</param>
		/// <param name="clOuts">Classification results.</param>
		/// <returns>Confusion matrix.</returns>
		public static int[,] CalculateConfusionMatrix(List<TrainingSample> refData, Dictionary<string, int> clOuts)
		{
			var classes = GetClassIds(refData.ToArray());
			var maxClassId = VectorMath.Max(classes.ToArray());
			var res = new int[maxClassId + 1, maxClassId + 1];

			var clIds = new int[clOuts.Count];
			clOuts.Values.CopyTo(clIds, 0);

			for (int i = 0; i < clOuts.Count; i++)
			{
				var idx1 = refData[i].ClassID;
				var idx2 = clIds[i];
				res[idx1, idx2]++;
			}

			return res;
		}
		#endregion
	}

	#region - Delegates. -
	public delegate float Regularizer(float[] w);
	#endregion

	#region - Regularizers. -
	/// <summary>
	/// Class for regularization coefficients.
	/// </summary>
	public class Regularizers
	{
		//private const string NO_REGULARIZER = "No regularizer";
		private const string L1REG = "L1 regularizer";
		private const string L2REG = "L2 regularizer";
		private const string L1REG_AVG = "L1 regularizer (avg)";
		private const string L2REG_AVG = "L2 regularizer (avg)";

		public static List<string> GetRegularizers ()
		{
			var res = new List<string>();
			res.Add(L1REG);
			res.Add(L2REG);
			res.Add(L1REG_AVG);
			res.Add(L2REG_AVG);

			return res;
		}

		public static Regularizer GetRegularizer (string name)
		{
			if (name == L1REG) return L1Regularizer;
			if (name == L2REG) return L2Regularizer;
			if (name == L1REG_AVG) return L1RegularizerAvg;
			if (name == L2REG_AVG) return L2RegularizerAvg;

			return null;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates regularization coefficient using given array of parameters.
		/// </summary>
		/// <param name="w">Input array.</param>
		/// <returns>Regularization coefficient.</returns>
		public static float L1Regularizer (float[] w)
		{
			return VectorMath.SumAbs(w);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates normalized regularization coefficient using given array of parameters.
		/// </summary>
		/// <param name="w">Input array.</param>
		/// <returns>Regularization coefficient.</returns>
		public static float L1RegularizerAvg(float[] w)
		{
			return L1Regularizer(w) / w.Length;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates regularization coefficient using given array of parameters.
		/// </summary>
		/// <param name="w">Input array.</param>
		/// <returns>Regularization coefficient.</returns>
		public static float L2Regularizer(float[] w)
		{
			return VectorMath.L2Norm(w);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates normalized regularization coefficient using given array of parameters.
		/// </summary>
		/// <param name="w">Input array.</param>
		/// <returns>Regularization coefficient.</returns>
		public static float L2RegularizerAvg(float[] w)
		{
			var sqrt = (float)Math.Sqrt(w.Length);
			return L2Regularizer(w) / sqrt;
		}
	}
	#endregion

	#region - Objective functions. -
	///// <summary>
	///// Delegate for the objective function.
	///// </summary>
	///// <param name="v">Vector of funciton parameters.</param>
	//public delegate float ObjectiveFunction(float[] v);
	//public delegate Fitness FitnessFunction(float[] v);

	//public abstract class FitnessFunction
	//{
	//    public abstract Fitness Compute(float[] v);
	//}

	public class ObjectiveFunctions
	{
		public static string SPHERE_FUNCTION = "Sphere function";
		public static string RASTRIGIN_FUNCTION = "Rastrigin's function";
		public static string ROSENBROCK_FUNCTION = "Rosenbrock's function";

		public static string[] Functions()
		{	// This is a really hardcoded function. But I just don't know how to make an automatic list of static strings (don't wanna use reflections).
			var res = new string[3];
			res[0] = SPHERE_FUNCTION;
			res[1] = RASTRIGIN_FUNCTION;
			res[2] = ROSENBROCK_FUNCTION;
			return res;
		}

		public static FitnessFunction GetFunction(string name)
		{
			if (string.Compare(name, SPHERE_FUNCTION, true) == 0) return new SphereFunction ();
			if (string.Compare(name, RASTRIGIN_FUNCTION, true) == 0) return new RastriginFunction();
			if (string.Compare(name, ROSENBROCK_FUNCTION, true) == 0) return new RosenbrockFunction();
			throw new Exception("Unknown objective function");
		}

		///// <summary>
		///// [molecule]
		///// 
		///// Sphere function.
		///// </summary>
		///// <param name="v">Vector of parameters.</param>
		///// <returns>Function value.</returns>
		//public static float SphereFunction(float[] v)
		//{
		//    var res = 0f;
		//    foreach (var elem in v)
		//    {
		//        res += elem*elem;
		//    }
		//    return res;
		//}
	}

	public class SphereFunction : FitnessFunction
	{
		#region Overrides of FitnessFunction
		public override Fitness Compute(float[] v)
		{
			var res = 0f;

			foreach (var elem in v)
			{
				res += elem * elem;
			}

			return new Fitness(res);
		}
		#endregion
	}

	public class RastriginFunction : FitnessFunction
	{
		#region Overrides of FitnessFunction
		public override Fitness Compute(float[] v)
		{
			var res = v.Length * 10f;

			foreach (var elem in v)
			{
				res += (float)(elem * elem - 10f * Math.Cos(Const.PI2 * elem));
			}

			return new Fitness(res);
		}
		#endregion
	}

	public class RosenbrockFunction : FitnessFunction
	{
		#region Overrides of FitnessFunction
		public override Fitness Compute(float[] v)
		{
			var res = 0f;
			var size_1 = v.Length - 1;

			for (int i = 0; i < size_1; i++)
			{
				var vi1 = v[i] - 1f;
				var vi2 = v[i]*v[i];
				var temp = v[i + 1] - vi2;
				res += 100f*temp*temp + vi1*vi1;
			}

			return new Fitness(res);
		}
		#endregion
	}
	#endregion

	#region - Activation functions. -
	/// <summary>
	/// Delegate for the agctivation function.
	/// </summary>
	/// <param name="w">Vector of weights.</param>
	/// <param name="x">Vector of input signals.</param>
	/// <param name="bias">Bias parameter value.</param>
	/// <param name="a">Free parameter.</param>
	/// <returns>Function output.</returns>
	public delegate float ActivationFunction(float[] w, float[] x, float bias, float a);

	public class ActivationFunctions
	{
		public const string GAUSSIAN_FUNCTION = "Gaussian";
		public const string IDENTITY_FUNCTION = "Identity";
		public const string LINEAR_FUNCTION = "Linear";
		public const string SIGMOID_FUNCTION = "Sigmoid";
		public const string SIGMOID_PROBBINARY_FUNCTION = "Binary (sigmoid prob)";

		/// <summary>
		/// [molecule]
		/// 
		/// Returns all available activation functions.
		/// </summary>
		/// <returns>List of available activation functions.</returns>
		public static string[] Functions ()
		{
			var res = new List<string>();
			res.Add(GAUSSIAN_FUNCTION);
			res.Add(SIGMOID_FUNCTION);
			res.Add(LINEAR_FUNCTION);
			res.Add(IDENTITY_FUNCTION);
			res.Add(SIGMOID_PROBBINARY_FUNCTION);
			return res.ToArray();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns activation function by its name.
		/// </summary>
		/// <param name="name">Activation function name.</param>
		/// <returns>Mental Alchemy activation function.</returns>
		public static ActivationFunction GetActivationFunction(string name)
		{
			// disable StringCompareIsCultureSpecific
			if (string.Compare(name, SIGMOID_FUNCTION, true) == 0) { return Sigmoid; }
			if (string.Compare(name, LINEAR_FUNCTION, true) == 0) { return Linear; }
			if (string.Compare(name, IDENTITY_FUNCTION, true) == 0) { return Identity; }
			if (string.Compare(name, GAUSSIAN_FUNCTION, true) == 0) { return Gaussian; }
			if (string.Compare(name, SIGMOID_PROBBINARY_FUNCTION, true) == 0) { return SigmoidProbBinary; }
			throw new Exception(string.Format("[ActivationFunctions.GetActivationFunction] error: Incorrect activation function ({0})", name));
		}

		/// <summary>
		/// Calculate quadratic output signal with weight matrix generated via dyadic product of the node's weights.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Quadratic(IList<float> w, IList<float> x, float bias, float a)
		{
			var size = x.Count;

			var m = VectorMath.DyadicProduct(w);

			var sum = bias;
			for (var i = 0; i < size; ++i)
			{
				sum += x[i] * x[i] * m[i, i];
				for (var j = i + 1; j < size; ++j)
				{
					sum += 2 * x[i] * x[j] * m[i, j];
				}
			}

			return sum * a;
		}

		/// <summary>
		/// Calculate step output signal.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Step(IList<float> w, IList<float> x, float bias, float a)
		{
			var m = VectorMath.DotProduct(w, x) + bias;
			return m >= 0 ? 1 : 0;
		}

		/// <summary>
		/// Calculate product of all input signals.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Product(IList<float> w, IList<float> x, float bias, float a)
		{
			var size = x.Count;

			var prod = 0.0f;
			for (int i = 0; i < size; ++i)
			{
				var temp = x[i] - w[i];
				prod *= temp;
			}

			return prod * a;
		}

		/// <summary>
		/// Calculate gaussian-shaped output.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Gaussian(IList<float> w, IList<float> x, float bias, float a)
		{
			var size = x.Count;

			var sum = 0.0f;
			for (var i = 0; i < size; ++i)
			{
				var temp = x[i] - w[i];
				sum += temp * temp;
			}

			return a != 0 ? (float)Math.Exp(-sum * 0.5 / a) : (float)Math.Exp(-sum * 0.5);
		}

		/// <summary>
		/// Calculates linear output signal.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Sigmoid(IList<float> w, IList<float> x, float bias, float a)
		{
			var res = bias;
			for (var i = 0; i < w.Count; ++i)
			{
				res += x[i] * w[i];
			}
			return (float)(1.0 / (1.0 + Math.Exp(-a * res)));
		}

		/// <summary>
		/// Calculates binary output using logistic function as a probability to have "1".
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float SigmoidProbBinary(IList<float> w, IList<float> x, float bias, float a)
		{
			var res = bias;
			for (var i = 0; i < w.Count; ++i)
			{
				res += x[i] * w[i];
			}
			var log = (float)(1.0 / (1.0 + Math.Exp(-a * res)));
			return ContextRandom.NextDouble () < log ? 1 : 0;
		}

		/// <summary>
		/// Always returns the first input signal.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Identity(IList<float> w, IList<float> x, float bias, float a)
		{
			return x[0];
		}

		/// <summary>
		/// Returns weighted sum of input signals.
		/// </summary>
		/// <param name="w">Vector of weights.</param>
		/// <param name="x">Vector of input signals.</param>
		/// <param name="bias">Bias parameter value.</param>
		/// <param name="a">Free parameter.</param>
		/// <returns>Function output.</returns>
		public static float Linear(float[] w, float[] x, float bias, float a)
		{
			var res = bias;
			//res += VectorMath.DotProduct(x, w);
			for (var i = 0; i < w.Length; ++i)
			{
				res += x[i] * w[i];
			}
			return res * a;
		}
	}
	#endregion
}
