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

using System.Collections.Generic;
//using System.Linq;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.NeuralData;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// [molecule]
	/// 
	/// Baseline class for all objective functions for neuroevolutionary algorithms.
	/// </summary>
	public abstract class NEObjFunction : FitnessFunction
	{
		protected List<TrainingSample> trData;

		/// <summary>
		/// Neural network which is required for fitness evaluation.
		/// </summary>
		public LayeredNeuralNetwork Network { get; set; }

		/// <summary>
		/// Training data (if available).
		/// </summary>
		public List<TrainingSample> TrainData
		{
			get { return trData; }
			set { SetTrainingData(value); }
		}

		/// <summary>
		/// Regularizer (set to null if not required).
		/// </summary>
		public Regularizer Regularizer { get; set; }

		/// <summary>
		/// Indicates whether objective function to be minimized or maximized.
		/// </summary>
		public bool MinimizeFitness { get; set; }

		///// <summary>
		///// Delegate to the objective function for a 'traditional' evolutionary algorithm.
		///// </summary>
		//public abstract Fitness ObjectiveFunction(float[] v);

		public virtual void SetTrainingData(List<TrainingSample> data)
		{
			trData = data;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for NE algorithm with [LayeredNeuralNetwork] ANN which aims at creating network which provides separability for objects descriptions from different classes.
	/// </summary>
	public class SeparabilityObjFunction : NEObjFunction
	{
		//public LayeredNeuralNetwork Network { get; set; }
		//public List<TrainingSample> TrainData { get; set; }

		#region - Static methods. -
		public static void CalculateIntraInterDistances(LayeredNeuralNetwork net, List<TrainingSample> trainData, out Dictionary<int, float> intraDist, out Dictionary<int, float> interDist)
		{
			//
			// calculate network output.
			var stepOuts = new List<float[,]>();
			foreach (var tData in trainData)
			{
				var outs = MachineLearningElements.GetOutputs(net, tData.Data);
				var mOut = MatrixMath.CreateFromVector(outs.ToArray(), outs.Count);
				stepOuts.Add(mOut);
			}

			//
			// process network outputs.
			// calculate inter- and intra-distances.
			var dist = MatrixMath.CalculateDistances(stepOuts, MatrixMath.EuclidianDistance);
			var clIds = MachineLearningElements.GetClassIds(trainData.ToArray());
			MachineLearningElements.CalculateIntraInterDistances(dist, clIds.ToArray(), out intraDist, out interDist);
		}
		#endregion

		public SeparabilityObjFunction()
		{
			MinimizeFitness = true;
		}

		public void CalculateIntraInterDistances(out Dictionary<int, float> intraDist, out Dictionary<int, float> interDist)
		{
			CalculateIntraInterDistances(Network, TrainData, out intraDist, out interDist);
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(v);

			//
			// process network outputs.
			// calculate inter- and intra-distances.
			Dictionary<int, float> intraDist, interDist;
			CalculateIntraInterDistances(Network, TrainData, out intraDist, out interDist);

			#region - Calculate fitness as difference between inter and intra distances. -
			var fitness = 0f;
			var r = 1f;
			for (int i = 0; i < interDist.Count; ++i)
			{
				if (interDist[i] != 0.0)
				{
					var temp = intraDist[i] - interDist[i];
					// if intra-class distance is smaller than the 'inner' one.
					// then encourage fitness.
					if (temp >= 0) { fitness -= temp; }
					// else if inner distance is smaller than the intra-class one.
					else if (temp < 0) { fitness -= r * temp; }

					// encourage large intra-class distance.
					fitness -= intraDist[i];
				}
				else
				{
					fitness += r;
				}
			}
			#endregion

			var res = new Fitness(fitness);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for maximization or minimization of outputs correlation.
	/// </summary>
	public class CorrelationObjFunction : NEObjFunction
	{
		public CorrelationObjFunction()
		{
			MinimizeFitness = false;
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));

			//
			// get vectors of signals from output nodes.
			var allOuts = MachineLearningElements.GetOutputs(Network, TrainData);

			//
			// get list of column vectors.
			var tempM = MatrixMath.CreateFromRowsList(allOuts);
			var cols = MatrixMath.ConvertToColumnsList(tempM);

			//
			// calculate matrix of absolute correlation coefficients.
			var cor = MatrixMath.ComputeCorrelationMatrix(cols);
			var corabs = MatrixMath.Abs(cor);

			//
			// calculate error as mean value in the matrix of absolute correlation coefficients.
			float delim = cols.Count * (cols.Count - 1);
			var error = MatrixMath.SumIgnoreDiag(corabs) / delim;

			//
			// calculate resulting fitness.
			var res = new Fitness(error);

			//
			// perform normalization of the columns.
			for (int i = 0; i < cols.Count; i++)
			{
				cols[i] = VectorMath.NormalizeL2(cols[i]);
			}
			var gram = MatrixMath.ComputeGramMatrix(cols);
			res.Extra.Add(Numerics.Determinant(gram, 0f));
			res.Extra.Add(Numerics.Rank(gram, 1e-3f));
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for maximization or minimization of the non-diagonal Gram matrix elements.
	/// </summary>
	public class GramMatrixObjFunction : NEObjFunction
	{
		public GramMatrixObjFunction()
		{
			MinimizeFitness = true;
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));

			//
			// get vectors of signals from output nodes.
			var allOuts = MachineLearningElements.GetOutputs(Network, TrainData);

			//
			// get list of column vectors.
			var tempM = MatrixMath.CreateFromRowsList(allOuts);
			var cols = MatrixMath.ConvertToColumnsList(tempM);

			//
			// perform normalization of the columns.
			for (int i = 0; i < cols.Count; i++)
			{
				cols[i] = VectorMath.NormalizeL2(cols[i]);
			}

			//
			// calculate matrix of absolute correlation coefficients.
			var gram = MatrixMath.ComputeGramMatrix(cols);
			var corabs = MatrixMath.Abs(gram);

			//
			// count number of zero columns.
			int zeroCount = 0;
			for (int i = 0; i < cols.Count; i++)
			{
				if (corabs[i, i] == 0f) zeroCount++;
			}

			//
			// calculate error as mean value in the matrix of absolute correlation coefficients.
			float delim = cols.Count * (cols.Count - 1);
			var error = MatrixMath.SumIgnoreDiag(corabs) / delim;

			//
			// calculate resulting fitness.
			var res = new Fitness(error + zeroCount);
			res.Extra.Add(Numerics.Determinant(gram, 0f));
			res.Extra.Add(Numerics.Rank(gram, 1e-3f));
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for maximization or minimization of the non-diagonal Gram matrix elements.
	/// </summary>
	public class GramMatrixVarianceObjFunction : NEObjFunction
	{
		public GramMatrixVarianceObjFunction()
		{
			MinimizeFitness = true;
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));
			// normalize ANN weights so that |w| = 1 for each node.
			Network.NormalizeWeights();

			//
			// get vectors of signals from output nodes.
			var allOuts = MachineLearningElements.GetOutputs(Network, TrainData);

			//
			// compute variance of outputs.
			var transp = MatrixMath.Transpose(MatrixMath.CreateFromRowsList(allOuts));
			var var = VectorMath.VarianceList(MatrixMath.ConvertToRowsList(transp));
			var recvar = VectorMath.Reciprocals(var);
			var sumvar = VectorMath.Sum(recvar);

			//
			// get list of column vectors.
			var tempM = MatrixMath.CreateFromRowsList(allOuts);
			var cols = MatrixMath.ConvertToColumnsList(tempM);

			//
			// perform normalization of the columns.
			for (int i = 0; i < cols.Count; i++)
			{
				cols[i] = VectorMath.NormalizeL2(cols[i]);
			}

			//
			// calculate matrix of absolute correlation coefficients.
			var gram = MatrixMath.ComputeGramMatrix(cols);
			var corabs = MatrixMath.Abs(gram);

			//
			// count number of zero columns.
			int zeroCount = 0;
			for (int i = 0; i < cols.Count; i++)
			{
				if (corabs[i, i] == 0f) zeroCount++;
			}

			//
			// calculate error as mean value in the matrix of absolute correlation coefficients.
			float delim = cols.Count * (cols.Count - 1);
			var error = MatrixMath.SumIgnoreDiag(corabs) / delim;

			//
			// calculate resulting fitness.
			var res = new Fitness(error + zeroCount + sumvar);
			res.Extra.Add(Numerics.Determinant(gram, 0f));
			res.Extra.Add(Numerics.Rank(gram, 1e-3f));
			res.Extra.Add(sumvar);

			// set the last Extras equal to nodes' variances.
			foreach (var va in var) { res.Extra.Add(va); }
			res.Extra.Add(var.Length);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for maximization or minimization of the Gram's matrix determinant and rank.
	/// </summary>
	public class GramDetRankObjFunction : NEObjFunction
	{
		public GramDetRankObjFunction()
		{
			MinimizeFitness = false;
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));

			//
			// get vectors of signals from output nodes.
			var allOuts = MachineLearningElements.GetOutputs(Network, TrainData);

			//
			// get list of column vectors.
			var tempM = MatrixMath.CreateFromRowsList(allOuts);
			var cols = MatrixMath.ConvertToColumnsList(tempM);

			//
			// perform normalization of the columns.
			for (int i = 0; i < cols.Count; i++)
			{
				cols[i] = VectorMath.NormalizeL2(cols[i]);
			}

			//
			// calculate matrix of absolute correlation coefficients.
			var gram = MatrixMath.ComputeGramMatrix(cols);

			//
			// calculate error as sum of determinant and rank.
			var det = Numerics.Determinant(gram, 0f);
			var rank = Numerics.Rank(gram, 1e-3f);
			var error = det + (float)rank / cols.Count;

			//
			// calculate resulting fitness.
			var res = new Fitness(error);
			res.Extra.Add(det);
			res.Extra.Add(rank);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Regularized objective function for maximization or minimization of the Gram matrix determinant.
	/// The regularization is made via normalized variances of ANN output signals.
	/// </summary>
	public class RegGramMatrixObjFunction : NEObjFunction
	{
		public float Alpha { get; set; }

		public RegGramMatrixObjFunction()
		{
			MinimizeFitness = false;
			Alpha = 0f;
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));

			//
			// get vectors of signals from output nodes.
			var allOuts = MachineLearningElements.GetOutputs(Network, TrainData);

			//
			// get list of column vectors.
			var tempM = MatrixMath.CreateFromRowsList(allOuts);
			var cols = MatrixMath.ConvertToColumnsList(tempM);

			//
			// perform normalization of the columns.
			for (int i = 0; i < cols.Count; i++)
			{
				cols[i] = VectorMath.NormalizeL2(cols[i]);
			}

			//
			// calculate determinant of the Gram matrix.
			var gram = MatrixMath.ComputeGramMatrix(cols);
			var det = Numerics.Determinant(gram, 0f);

			//
			// calculate variances of the normalized output signals.
			var vars = VectorMath.VarianceList(cols);
			var sum = VectorMath.Sum(vars);
			sum /= (allOuts.Count * vars.Length);	// calculate mean 'normalized' variance.

			var res = new Fitness((1 - Alpha) * det + Alpha * sum);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for maximization of the rank of matrix.
	/// </summary>
	public class RankObjFunction : NEObjFunction
	{
		public RankObjFunction()
		{
			MinimizeFitness = false;
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));

			//
			// get vectors of signals from output nodes.
			var allOuts = MachineLearningElements.GetOutputs(Network, TrainData);

			//
			// get list of column vectors.
			var tempM = MatrixMath.CreateFromRowsList(allOuts);
			var er = (float)Numerics.Rank(tempM, 0);

			if (Regularizer != null)
			{	// penalty for a large regularization coefficient.
				er -= Regularizer(MatrixMath.ConvertToVector(tempM));
			}

			var res = new Fitness(er);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for maximization or minimization of the variances of ANN outputs.
	/// </summary>
	public class OutputsVarianceObjFunction : NEObjFunction
	{
		protected float[,] autoCor;
		public int DataAmount = 100;	// amount (%) of training data to be used.

		public OutputsVarianceObjFunction()
		{
			MinimizeFitness = true;
		}

		public override void SetTrainingData(List<TrainingSample> data)
		{
			base.SetTrainingData(data);

			// compute autocorrelation matrix.
			autoCor = MachineLearningElements.ComputeAutocorrelationMatrix(trData);
		}

		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(new List<float>(v));
			// normalize ANN weights so that |w| = 1 for each node.
			Network.NormalizeWeights();

			//
			// get vectors of signals from output nodes.
			var data = DataAmount == 100 ? TrainData : MachineLearningElements.SampleRandom(TrainData, TrainData.Count*DataAmount/100);	// create sample from a training data.
			var allOuts = MachineLearningElements.GetOutputs(Network, data);
			//var allOuts = Performance.MachineLearning.GetOutputs(Network, TrainData);

			//
			// compute variance of outputs.
			//var transp = MatrixMath.Transpose(MatrixMath.CreateFromRowsList(allOuts));
			var transp = MatrixMath.TransposeRows(allOuts);
			var var = VectorMath.VarianceList(MatrixMath.ConvertToRowsList(transp));

			//
			// compute y_i = Ac * w_i, where Ac -- autocorrelation matrix, w_i -- weights of the i-th node.
			// and then compute cosine of angle between y_i and w_i.
			//var cos = new float[outsCount];
			var wi = Network.Layers[1].Nodes[0].Weights;
			var yi = MatrixMath.Mul(autoCor, wi);
			var cos = VectorMath.Cosine(wi, yi);

			var wvar = VectorMath.Mul(var, cos * cos);

			//
			// calculate error as sum of inverse variances of ANN outputs.
			var sum = VectorMath.Sum(wvar);
			var error = sum;

			//
			// calculate resulting fitness.
			var res = new Fitness(error);

			// set the last Extras equal to nodes' variances.
			res.Extra.AddRange(var);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function for minimization of the network2 error on the training set.
	/// </summary>
	public class MinErrorObjFunction : NEObjFunction
	{
		public const int DEFAULT_TRAIN_TRIALS = 3;
		public const int DEFAULT_TRAIN_EPOCHS = 50;

		#region - Public properties. -
		/// <summary>
		/// Properties of the network, which is trained during the fitness evaluation.
		/// </summary>
		public NeuralNetProperties Network2Properties { get; set; }

		/// <summary>
		/// Number of trials to train network2.
		/// </summary>
		public int TrainTrials { get; set; }

		/// <summary>
		/// Number of epochs to train network2.
		/// </summary>
		public int TrainEpochs { get; set; }
		#endregion

		#region - Construction. -
		public MinErrorObjFunction()
		{
			TrainTrials = DEFAULT_TRAIN_TRIALS;
			TrainEpochs = DEFAULT_TRAIN_EPOCHS;
			MinimizeFitness = true;
		}
		#endregion

		public override Fitness Compute(float[] v)
		{
			// copy individual genes into [Vector].
			Network.SetConnectionWeights(new List<float>(v));

			#region - Train neural network using training dataset. -
			int outputsNumber = TrainData[0].Response.Length;
			var net2Props = new NeuralNetProperties();
			net2Props.nodesNumber = new[] { Network.OutputsNumber, outputsNumber };
			net2Props.actFunctions = new ActivationFunction[] { ActivationFunctions.Sigmoid, ActivationFunctions.Linear };
			var net2 = MachineLearningElements.CreateEncogBasicNetwork(net2Props);

			#region - Create set of training samples using [Network]'s output. -
			INeuralDataSet trainingSet = new BasicNeuralDataSet();
			int trainCount = TrainData.Count;
			for (int j = 0; j < trainCount; ++j)
			{
				var row = MatrixMath.GetRow(TrainData[j].Data, 0);
				Network.Calculate(row);
				float[] outs;
				Network.GetOutputs(out outs);

				var outsD = VectorMath.ConvertToDoubles(outs);
				var outsCorRow = MatrixMath.GetRow(TrainData[j].Response, 0);
				var outsCorD = VectorMath.ConvertToDoubles(outsCorRow);
				trainingSet.Add(new BasicNeuralData(outsD), new BasicNeuralData(outsCorD));
			}
			#endregion

			var er = 0.0f;
			for (int i = 0; i < TrainTrials; ++i)
			{
				net2.Reset();
				ITrain train = new ResilientPropagation(net2, trainingSet);
				int epoch = 0;

				do
				{
					train.Iteration();
					epoch++;
				} while (epoch < TrainEpochs);

				if (er < train.Error)
				{	// er is a maximal obtained error.
					er = (float)train.Error;
				}
			}
			#endregion

			var res = new Fitness(er);
			return res;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Objective function as standard MSE.
	/// </summary>
	public class LeastSquaresObjFunction : NEObjFunction
	{
		public LeastSquaresObjFunction ()
		{
			MinimizeFitness = true;
			FitnessComparator.MinimizeFitness = true;
		}

		#region Overrides of FitnessFunction
		public override Fitness Compute(float[] v)
		{
			Network.SetConnectionWeights(v);
			var er = MachineLearningElements.ComputeMSE(Network, trData);
			if (Regularizer != null)
				er += Regularizer(v);
			return new Fitness(er);
		}
		#endregion
	}

	///// <summary>
	///// [molecule]
	///// 
	///// Objective function as standard MSE.
	///// </summary>
	//public class LeastSquaresObjFunction : NEObjFunction
	//{
	//    public LeastSquaresObjFunction()
	//    {
	//        MinimizeFitness = true;
	//        FitnessComparator.MinimizeFitness = true;
	//    }

	//    #region Overrides of FitnessFunction
	//    public override Fitness Compute(float[] v)
	//    {
	//        Network.SetConnectionWeights(v);
	//        var er = MachineLearningElements.ComputeMSE(Network, trData);
	//        return new Fitness(er);
	//    }
	//    #endregion
	//}
}