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

		protected float _learningRate = 0.1f;
		protected int _miniBatchSize = 10;

		public int HiddenNodesCount { get { return _hBiases != null ? _hBiases.Length : 0; } }
		public int VisibleNodesCount { get { return _vBiases != null ? _vBiases.Length : 0; } }

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
			MatrixMath.MulInplace(_weights, 2/count * 0.01f);	// scale to have values within [-.01; +.01]
		}

		/// <summary>
		/// Train using CD1.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="epochs"></param>
		public void Train(List<float[]> data, int epochs)
		{
			for (int t = 0; t < epochs; ++t)
			{
				foreach (var v in data)
				{
					var statsData = MatrixMath.Zeros(VisibleNodesCount, HiddenNodesCount);
					var statsModel = MatrixMath.Zeros(VisibleNodesCount, HiddenNodesCount);

					// compute the network response (hidden units) to the [data].
					var hstate = ComputeHiddenStates(v, binary: true);
					UpdateStatsInplace(statsData, v, hstate);	// update the <v_i h_j>_{data}

					// compute the network response by itself (model) using a reconstruction technique from Hinton (2002).
					// update the <v_i h_j>_{recon}
					var vstate = ComputeVisibleStates(hstate);
					UpdateStatsInplace(statsModel, vstate, hstate);	// update the <v_i h_j>_{data}

					// update weights.
					ComputeWeightDeltas(statsData, statsModel);
					UpdateWeights ();
				}
			}
		}

		public float Test(List<float[]> data)
		{
			var er = 0f;
			foreach (var v in data)
			{
				var hstate = ComputeHiddenStates(v, binary: true);
				var vstate = ComputeVisibleStates(hstate);

				var dist = VectorMath.EuclidianDistance(v, vstate);
				er += dist;
			}
			return er / data.Count;
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

		public float[] ComputeVisibleStates(float[] hstates)
		{
			var res = new float[VisibleNodesCount];
			for (int i = 0; i < VisibleNodesCount; ++i )
			{
				var tmp = ActivationFunctions.Sigmoid(_weights[i], hstates, _vBiases[i], 1f);
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
