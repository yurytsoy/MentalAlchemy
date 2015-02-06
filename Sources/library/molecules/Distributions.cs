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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// [molecule]
	/// 
	/// Multivariate Gaussian distribution with arbitrary mean and covariance.
	/// </summary>
	public class GaussianDistribution
	{
		protected NormalDistribution stdDistr = new NormalDistribution(0, 1);
		protected float[,] sqrtEValues;
		protected float[,] vectorsValues;	// Q * L^{1/2}, Q -- matrix of eigenvectors, L -- matrix of eigenvalues.

		public bool FixCovariance = true;	// if provided covariance estimate turns out to be non-ositive definite then zero negative eigenvalues.

		public int Size { get; protected set; }
		public float[] Mean {get; protected set;}
		public float[][] Covariance{get; protected set;}
		public float[,] CovEigenvectors { get; protected set; }
		public float[,] CovEigenvalues { get; protected set; }


		#region - Public methods. -
		public GaussianDistribution(float[] mu, float[][] cov)
		{
			Size = mu.Length;
			Mean = mu;

			// compute eigenvectors and eigenvalues.
			SetCovariance(cov);
		}

		public GaussianDistribution(float[] mu, float[,] cov)
		{
			Size = mu.Length;
			Mean = mu;

			// compute eigenvectors and eigenvalues.
			int height = cov.GetLength (0), width = cov.GetLength(1);
			var tmpcov = new float[height][];
			for (int i = 0; i < height; ++i ) 
			{
				var row = new float[width];
				for (int j = 0; j < width; ++j )
				{
					row[j] = cov[i, j];
				}
				tmpcov[i] = row;
			}
			
			SetCovariance(tmpcov);
		}

		public float[] Next()
		{
			var res = new float[Size];
			for (int i = 0; i < Size; i++)
			{
				res[i] = (float)stdDistr.NextDouble();
			}
			res = MatrixMath.Mul(vectorsValues, res);
			res = VectorMath.Add(res, Mean);
			return res;
		} 

		public List<string> ToStrings ()
		{
			var res = new List<string>();
			res.Add("Mean:\t" + VectorMath.ConvertToString(Mean, '\t'));
			res.Add("Covariance matrix:");
			res.AddRange(MatrixMath.ConvertToRowsStringsList(Covariance, '\t'));
			res.Add("Eigenvalues:\t" + VectorMath.ConvertToString(MatrixMath.Diag(CovEigenvalues), '\t'));
			res.Add("Eigenvectors:");
			res.AddRange(MatrixMath.ConvertToRowsStringsList(MatrixMath.Transpose(CovEigenvectors), '\t'));
			return res;
		}
		#endregion

		#region - Protected methods. -
		protected void SetCovariance(float[][] cov)
		{
			Covariance = cov;
			var size = cov.Length;
			double[,] dcov = new double[cov.Length, cov[0].Length];
			for (int i = 0; i < size; ++i )
			{
				for (int j = 0; j < size; ++i)
				{
					dcov[i, j] = cov[i][j];
				}
			}

			var eigv = new double[0];
			var eigvec = new double[0,0];
			alglib.evd.smatrixevd(dcov, dcov.GetLength(0), 1, false, ref eigv, ref eigvec);

			var evals = VectorMath.ConvertToFloats(eigv);
			CovEigenvalues = MatrixMath.Diagonal(evals);
			CovEigenvectors = MatrixMath.ConvertToFloats(eigvec);

			if (FixCovariance)
			{
				for (int i = 0; i < cov.GetLength(0); i++)
				{
					if (CovEigenvalues[i, i] < 0) CovEigenvalues[i, i] = -CovEigenvalues[i, i];
				}
			}

			sqrtEValues = MatrixMath.Sqrt(CovEigenvalues);
			vectorsValues = MatrixMath.Mul(CovEigenvectors, sqrtEValues);

		} 
		#endregion
	}
}
