using MentalAlchemy.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Implementation of a Restricted Boltzman Machine.
	/// See for details G. Hinton's guide:
	/// https://www.cs.toronto.edu/~hinton/absps/guideTR.pdf
	/// </summary>
	class RBM
	{
		/// <summary>
		/// _weights[i][j] -- weight between i-th visible and j-th hidden node.
		/// </summary>
		protected float[][] _weights;
		protected float[][] _weightDeltas;
		protected float[] _hBiases;	// hidden biases.
		protected float[] _vBiases;	// visible biases.

		protected float _learningRate = 0.01f;
		// TODO: protected int _miniBatchSize = 10;

		public int HiddenNodesCount { get { return _hBiases != null ? _hBiases.Length : 0; } }
		public int VisibleNodesCount { get { return _vBiases != null ? _vBiases.Length : 0; } }
		public float LearningRate { get { return _learningRate; } set { _learningRate = value; } }

		/// <summary>
		/// Weights[i][:] -- weights from i-th input node.
		/// Weights[:][j] -- weights (receptive field) to the j-th hidden node.
		/// </summary>
		public float[][] Weights { get { return _weights; } }

		/// <summary>
		/// Creates RBM using the number of visible and hidden units.
		/// </summary>
		/// <param name="vCount"></param>
		/// <param name="hCount"></param>
		public RBM(int vCount, int hCount)
		{
			Init(vCount, hCount);
		}

		protected void Init(int vCount, int hCount)
		{
			_vBiases = new float[vCount];
			_hBiases = new float[hCount];

			_weightDeltas = MatrixMath.Zeros(vCount, hCount);

			// init weights.
			_weights = MatrixMath.Zeros(vCount, hCount);
			var rand = new Random();
			int count = 6;
			for (int i = 0; i < count; ++i)
			{
				var tmp = MatrixMath.Random(vCount, hCount, rand);
				MatrixMath.AccumulateInplace (_weights, tmp);
			}
			MatrixMath.SubInplace(_weights, count/2);
			MatrixMath.MulInplace(_weights, 2f/count * 0.01f);	// scale to have values within [-.01; +.01]
		}

		/// <summary>
		/// Performs training for the given number of epochs.
		/// The algorithm follows Algorithm 1 from (p. 60):
		/// http://www.iro.umontreal.ca/~bengioy/papers/ftml_book.pdf
		/// </summary>
		/// <param name="data"></param>
		/// <param name="epochs"></param>
		public void Train(List<float[]> data, int epochs, float momentum = 0f)
		{
			// preallocate matrices.
			var x1h1 = MatrixMath.Zeros(VisibleNodesCount, HiddenNodesCount);
			var x2h2 = MatrixMath.Zeros(VisibleNodesCount, HiddenNodesCount);
			for (int t = 0; t < epochs; ++t)
			{
				foreach (var x1 in data)
				{
					// compute the network response (hidden units) to the [data].
					var h1 = ComputeHiddenStates(x1, binary: true);
					var x2 = ComputeVisibleStates(h1, binary: true);
					var h2 = ComputeHiddenStates(x2, binary: false);

					// update weights
					VectorMath.OuterProduct(x1, h1, x1h1);
					VectorMath.OuterProduct(x2, h2, x2h2);
					MatrixMath.SubInplace(x1h1, x2h2);
					MatrixMath.MulInplace(x1h1, _learningRate);
					if (momentum > 0)
					{
						var delta = MatrixMath.Mul(_weightDeltas, momentum);
						MatrixMath.MulInplace(x1h1, (1 - momentum));
						MatrixMath.AccumulateInplace (x1h1, delta);
					}
					_weightDeltas = x1h1;
					MatrixMath.AccumulateInplace(_weights, _weightDeltas);

					// update visible biases.
					var subx = VectorMath.Sub (x1, x2);
					VectorMath.MulInplace(subx, _learningRate);
					VectorMath.AccumulateInplace(_vBiases, subx);

					// update hidden biases.
					VectorMath.SubInplace(h1, h2);
					VectorMath.MulInplace(h1, _learningRate);
					VectorMath.AccumulateInplace(_hBiases, h1);
				}
			}
		}

		public float Test(List<float[]> data)
		{
			var recons = Reconstruct(data);

			var er = 0f;
			for (int i=0; i<data.Count; ++i)
			{
				var dist = VectorMath.EuclidianDistance(data[i], recons[i]);
				er += dist;
			}
			return er / data.Count;
		}

		public List<float[]> Reconstruct(IList<float[]> data, bool binaryHidden = true, bool binaryVisible = true)
		{
			var res = new List<float[]>();
			foreach (var v in data)
			{
				var hstate = ComputeHiddenStates(v, binary: binaryHidden);
				var vstate = ComputeVisibleStates(hstate, binary: binaryVisible);

				res.Add(vstate);
			}
			return res;
		}

		protected void ComputeWeightDeltas(float[][] statData, float[][] statModel)
		{
			// TODO: update learning rate according to the ad hoc rule from 7.1.
			for (int i = 0; i < VisibleNodesCount; ++i )
			{
				for (int j = 0; j < HiddenNodesCount; ++j )
				{
					_weightDeltas[i][j] = _learningRate * ( statData[i][j] - statModel[i][j] );
				}
			}
		}

		protected void UpdateWeights()
		{
			MatrixMath.AccumulateInplace(_weights, _weightDeltas);
		}

		protected void UpdateStatsInplace(float[][] statM, float[] vstate, float[] hstate)
		{
			for (int i = 0; i < VisibleNodesCount; ++i)
			{
				for (int j = 0; j < HiddenNodesCount; ++j)
				{
					statM[i][j] += vstate[i] * hstate[j];
				}
			}
		}

		public float[] ComputeVisibleStates(float[] hstates, bool binary = true)
		{
			var actFunc = binary == true ? (ActivationFunction)ActivationFunctions.SigmoidProbBinary : (ActivationFunction)ActivationFunctions.Sigmoid;

			var res = new float[VisibleNodesCount];
			for (int i = 0; i < VisibleNodesCount; ++i )
			{
				var tmp = actFunc(_weights[i], hstates, _vBiases[i], 1f);
				res[i] = tmp;
			}
			return res;
		}

		public float[] ComputeHiddenStates(float[] vstates, bool binary = true)
		{
			var actFunc = binary == true ? (ActivationFunction)ActivationFunctions.SigmoidProbBinary : (ActivationFunction)ActivationFunctions.Sigmoid;

			var res = new float[HiddenNodesCount];
			var w = new float[VisibleNodesCount];
			for (int i = 0; i < HiddenNodesCount; ++i )
			{
				for (int j = 0; j < VisibleNodesCount; ++j )
				{
					w[j] = _weights[j][i];
				}
				var tmp = actFunc(w, vstates, _hBiases[i], 1);
				res[i] = tmp;
			}
			return res;
		}
	}
}
