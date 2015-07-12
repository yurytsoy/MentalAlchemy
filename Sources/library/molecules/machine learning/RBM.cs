using MentalAlchemy.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.molecules.machine_learning
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
		protected float[] _hBiases;	// hidden biases.
		protected float[] _vBiases;	// visible biases.

		public int HiddenNodesCount { get { return _hBiases != null ? _hBiases.Length : 0; } }
		public int VisibleNodesCount { get { return _vBiases != null ? _vBiases.Length : 0; } }

		/// <summary>
		/// Creates RBM using the number of visible and hidden units.
		/// </summary>
		/// <param name="vCount"></param>
		/// <param name="hCount"></param>
		public RBM(int vCount, int hCount)
		{
			_vBiases = new float[vCount];
			_hBiases = new float[hCount];

			_weights = MatrixMath.Zeros(vCount, hCount);
		}

		public void Train(List<float[]> data, int epochs)
		{
			for (int i = 0; i < epochs; ++i)
			{
				foreach (var v in data)
				{
					// compute the network response (hidden units) to the [data].
					// update the <v_i h_j>_{data}

					// compute the network response by itself (model) using a reconstruction technique from Hinton (2002).
					// update the <v_i h_j>_{recon}

					// compute expectations.
				}
			}
		}

		public float[] ComputeVisibleStates(float[] hstates)
		{
			throw new NotImplementedException();
		}


		public float[] ComputeHiddenStates(float[] vstates)
		{
			throw new NotImplementedException();
		}
	}
}
