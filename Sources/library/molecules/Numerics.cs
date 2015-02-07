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

/*
 * Some comments preceeding the methods are referring to Golub et al., which is the following book:
 *	Golub G.H., Van Loan C.F. Matrix Computation. 2nd edition. The John Hopkins Univ. Press, 1989.
 */

using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MentalAlchemy.Atoms;

using complex = alglib.complex;

// disable StringCompareIsCultureSpecific

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Utils for numerical computations.
	/// </summary>
	public class Numerics
	{
		#region - Bounds for complex polynomials' roots. -
		#region - Classical bounds for complex polynomials. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		A. Joyal, G. Labelle, Q. I. Rhaman, On the Location of polynomials, Canad.
		///		Math. Bull. 10 (1967) 53–63.
		/// 
		/// Coefficient a[n] should not be equal to 0, otherwise +inf is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float JoyalBounds(Complex[] a)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			var delim = 1f / (a[n] * a[n]);
			var b = 0f;
			for (int i = 1; i < a.Length; i++)
			{
				var temp = i != n
						? a[n - 1] * a[n - i] - a[n] * a[n - i - 1]
						: a[n - 1] * a[n - i];
				temp *= delim;

				var mod = (float)temp.Modulus;
				if (b < mod) b = mod;
			}

			return (float)(0.5 * (1d + Math.Sqrt(1d + 4 * b)));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		V. K. Jain, On the Zeros of Polynomials II, Jour. Math. Phy. Sci 20 (1986)
		///		259–267.
		/// 
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float MohammadBounds(Complex[] a)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			var mods = new float[a.Length];
			for (int i = 0; i < mods.Length; ++i) mods[i] = (float)a[i].Modulus;

			var b = 0f;
			for (int k = 0; k < n; k++)
			{
				if (mods[k+1] == 0) continue;

				var temp = mods[k]/mods[k+1];
				if (b < temp) b = temp;
			}

			return 2*b;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		V. K. Jain, On the Zeros of Polynomials II, Jour. Math. Phy. Sci 20 (1986)
		///		259–267.
		/// 
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float KojimaBounds(Complex[] a)
		{

			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			var mods = new float[a.Length];
			for (int i = 0; i < mods.Length; ++i) mods[i] = (float)a[i].Modulus;

			var b1 = mods[1] == 0f ? 0 : mods[0] / mods[1];
			var b = 0f;
			for (int k = 1; k < n; k++)
			{
				if (mods[k+1] == 0f) continue;

				var temp = 2 * mods[k] / mods[k+1];
				if (b < temp) b = temp;
			}

			return Math.Max(b1, b);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		V. K. Jain, On the Zeros of Polynomials II, Jour. Math. Phy. Sci 20 (1986)
		///		259–267.
		/// 
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float JainBounds(Complex[] a)
		{

			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			var mods = new float[a.Length];
			for (int i = 0; i < mods.Length; ++i) mods[i] = (float)a[i].Modulus;

			var b = 0f;
			for (int k = n; k > 0; k--)
			{
				if (mods[k] == 0f) continue;

				var temp = (n - k + 1) * mods[k-1] / mods[k];
				if (b < temp) b = temp;
			}

			return b / (float)Math.Log(2);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Marden, Geometry of polynomials, Mathematical Surveys of the American
		///		Mathematical Society, Vol. 3, Rhode Island, USA, 1966.
		/// 
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float CauchyBounds1(Complex[] a)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			var mods = new float[a.Length];
			for (int i = 0; i < mods.Length; ++i) mods[i] = (float)a[i].Modulus;
			mods[n] = -mods[n];

			return FindLargestPositiveRoot(mods);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Marden, Geometry of polynomials, Mathematical Surveys of the American
		///		Mathematical Society, Vol. 3, Rhode Island, USA, 1966.
		/// 
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float CauchyBounds2(Complex[] a)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			var mods = new float[a.Length];
			for (int i = 0; i < mods.Length; ++i) mods[i] = (float)a[i].Modulus;

			var delim = 1f/mods[n];
			var b = 0f;
			for (int i = 0; i < n; ++i)
			{
				var temp = mods[i] * delim;
				if (b < temp) b = temp;
			}

			return 1 + b;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Marden, Geometry of polynomials, Mathematical Surveys of the American
		///		Mathematical Society, Vol. 3, Rhode Island, USA, 1966.
		/// 
		/// [p] coefficient is used to define metrics parameters.
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <param name="p"></param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float PowerBounds(Complex[] a, float p)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			if (p == 0) return float.MaxValue;

			// compute q so that 1/p + 1/q = 1;
			var q = p / (p - 1);

			var delim = 1f / a[n];
			var b = 0f;
			for (int i = 0; i < n; ++i)
			{
				var temp = a[i] * delim;
				b += (float)Math.Pow(temp.Modulus, p);
			}
			b = 1f + (float)Math.Pow(b, q/p);
			b = (float)Math.Pow(b, 1 / q);

			return b;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Marden, Geometry of polynomials, Mathematical Surveys of the American
		///		Mathematical Society, Vol. 3, Rhode Island, USA, 1966.
		/// 
		/// [p] coefficient is used to define metrics parameters.
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float KuniyedaBounds(Complex[] a, float p)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return float.MaxValue;

			if (p == 0) return float.MaxValue;

			var b = 0f;
			for (int i = 0; i <= n; ++i)
			{
				var temp = a[n-i].Modulus;
				b += (float)Math.Pow(temp, 1 + p);
			}
			b = (float)(Math.Pow(b, 1/p)/Math.Pow(a[n].Modulus, (p + 1)/p));
			b = (float)(Math.Pow(b + 1, p / (p+1)));

			return b;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Marden, Geometry of polynomials, Mathematical Surveys of the American
		///		Mathematical Society, Vol. 3, Rhode Island, USA, 1966.
		/// 
		/// [p] coefficient is used to define metrics parameters.
		/// Coefficient a[n] should not be equal to 0, otherwise 0 is returned.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float MardenBounds(Complex[] a, float p)
		{
			var n = a.Length - 1;
			if (a[n].IsZero) return 0f;

			if (p == 0) return float.MaxValue;

			// compute q so that 1/p + 1/q = 1;
			var q = p / (p - 1);

			var delim = 1f/(a[n]*a[n]);
			var m = 0f;
			for (int k = 1; k <= n; k++)
			{
				var temp = k != n
						? a[n - 1] * a[n - k] - a[n] * a[n - k - 1]
						: a[n - 1] * a[n - k];
				temp *= delim;

				var mod = Math.Pow(temp.Modulus, p);
				m += (float)mod;
			}
			m = (float)Math.Pow(m, 1/p);
			m = (float)Math.Sqrt(1 + 4*Math.Pow(m, q));
			m = (float)Math.Pow(0.5*(1 + m), 1/q);

			return m;
		}
		#endregion

		#region - New bounds for complex polynomials. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise 0 is returned.
		/// 
		/// Thm. 14 in the PLoS paper.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float DehmerBounds1(Complex[] a)
		{
			var n = a.Length - 1;
			if ((a[n] * a[n - 1]).IsZero) return float.MaxValue;

			var delim = 1 / a[n];
			var m2 = 0f;
			for (int i = 0; i <= n - 1; i++)
			{
				var temp = (a[i] * delim).Modulus;
				if (m2 < temp) m2 = (float)temp;
			}

			var c = new float[a.Length + 1];
			c[0] = m2;
			c[n] = -(1 + m2);
			c[n + 1] = 1;

			return FindLargestPositiveRoot(c);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise 0 is returned.
		/// 
		/// Thm. 15 from PLoS paper.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float DehmerBounds2(Complex[] a)
		{
			var n = a.Length - 1;
			if ((a[n] * a[n - 1]).IsZero) return float.MaxValue;

			var delim = 1 / a[n];
			var m1 = 0f;
			for (int i = 0; i <= n - 2; i++)
			{
				var temp = (a[i] * delim).Modulus;
				if (m1 < temp) m1 = (float)temp;
			}
			var m = (float)(a[n - 1]*delim).Modulus;

			var c = new float[a.Length + 1];
			c[0] = m1;
			c[n - 1] = m - m1;
			c[n] = -(1 + m);
			c[n + 1] = 1;

			return FindLargestPositiveRoot(c);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise 0 is returned.
		/// 
		/// Thm. 16 in PLoS paper.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float DehmerBounds3(Complex[] a)
		{
			var n = a.Length - 1;
			if ((a[n] * a[n - 1]).IsZero) return float.MaxValue;

			var delim = 1f / (a[n] * a[n]);
			var m3 = 0f;
			for (int i = 2; i <= n; ++i)
			{
				var temp = i != n
						? a[n - 1] * a[n - i] - a[n] * a[n - i - 1]
						: a[n - 1] * a[n - i];
				temp *= delim;

				var mod = (float)temp.Modulus;
				if (m3 < mod) m3 = mod;
			}

			var anmod = a[n].Modulus;
			var phi1 = (a[n-1] * a[n-1] - a[n]*a [ n-2]).Modulus / (anmod* anmod);

			var c = new float[4];
			c[0] = (float)phi1;
			c[1] = (float)(-(m3 + phi1));
			c[2] = -1;
			c[3] = 1;

			return FindLargestPositiveRoot(c);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise 0 is returned.
		/// 
		/// Thm. 9 in PLoS paper.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float DehmerBounds4(Complex[] a)
		{
			var n = a.Length - 1;
			if ((a[n] * a[n - 1]).IsZero) return float.MaxValue;

			var delim = 1/a[n];
			var m1 = 0f;
			for (int i = 0; i <= n-2; i++)
			{
				var temp = (a[i] * delim).Modulus;
				if (m1 < temp) m1 = (float)temp;
			}

			var phi2 = (a[n - 1]/a[n]).Modulus;
			var b = Math.Sqrt( (phi2 - 1)*(phi2 - 1) + 4 * m1 )/2;
			b += 0.5*(1 + phi2);
			return (float)b;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise float.MaxValue is returned.
		/// 
		/// Thm. 10 in PLoS paper.
		/// </summary>
		/// <param name="a">Vector of polynomial coefficients.</param>
		/// <returns>Radius of the closed disk, where the roots are located.</returns>
		public static float DehmerBounds5(Complex[] a)
		{
			var n = a.Length - 1;
			if ((a[n] * a[n - 1]).IsZero) return float.MaxValue;

			//
			// note: should the condition for a[i] be checked?
			//for (int i = 0; i <= n-2; i++)
			//{
			//    if (a[i].Modulus >= 1) return DehmerBounds4(a);
			//}

			var phi2 = (a[n - 1] / a[n]).Modulus;
			var b = Math.Sqrt((phi2 - 1) * (phi2 - 1) + 4 / a[n].Modulus) / 2;
			b += 0.5 * (1 + phi2);
			return (float)b;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Polynomial roots bounds are computed using corollary 4.4 from the paper
		/// Kalantari B. An infinite family of bounds on zeros of analytic functions and relationship to Smale's bound
		/// Mathematics of computation, 2004, Volume 74, Number 250, Pages 841–852.
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise float.MaxValue is returned.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float Kalantari44 (Complex[] a)
		{
			var n = a.Length - 1;
			if (a[0] * a[n] == 0) return float.MaxValue;

			var delim = 1f/a[n];
			var b0 = a[n - 1]*delim;
			var b = b0.Modulus;
			for (int k = 2; k <= n; k++)
			{
				var temp = (a[n - k]*delim).Modulus;
				temp = Math.Pow(temp, 1f/k);
				if (temp > b) { b = temp; }
			}

			return (float)(2*b);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Polynomial roots bounds are computed using corollary 4.5 from the paper
		/// Kalantari B. An infinite family of bounds on zeros of analytic functions and relationship to Smale's bound
		/// Mathematics of computation, 2004, Volume 74, Number 250, Pages 841–852.
		/// 
		/// Coefficients a[n] and a[n-1] should not be equal to 0, otherwise float.MaxValue is returned.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float Kalantari45(Complex[] a)
		{
			var n = a.Length - 1;
			if (a[0] * a[n] == 0) return float.MaxValue;

			var delim = (1d / (a[n] * a[n]).Modulus);
			var b = (a[n - 1] * a[0]).Modulus * delim;
			b = Math.Pow(b, 1d/(n + 1));
			for (int k = 2; k <= n; k++)
			{
				var temp = (a[n - 1] * a[n - k + 1] - a[n] * a[n-k]).Modulus * delim;
				temp = Math.Pow(temp, 1d / k);
				if (temp > b) { b = temp; }
			}

			return (float)(b * (1 + Math.Sqrt(5)) *0.5);
		}
		#endregion

		#region - New bounds for lacunary polynomials. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should be > 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, -1, 0, ..., 0, -a1, a0) and has 2 positive zeroes.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary1 (float[] a)
		{
			if (a.Length <= 2 || a[0] <= 0f || a[1] <= 0f) return float.MaxValue;
			var posRoots = GetPositiveRoots(a);
			if (posRoots.Length < 2) return float.MaxValue;

			return 1 + (float)Math.Sqrt(Math.Abs(a[1]));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should be > 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, 0, ..., 0, -a1, a0) and has 2 positive zeroes.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary3(float[] a)
		{
			if (a.Length <= 2 || a[0] <= 0f || a[1] <= 0f ) return float.MaxValue;
			if (!HasNPositiveRoots(a, 2)) { return float.MaxValue; }

			return (float)(0.5f * (1 + Math.Sqrt(4 * Math.Abs(a[1]) + 1)));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should be > 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, 0, ..., 0, -a1, a0) and has 2 positive zeroes.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary5(Complex[] a)
		{
			if (a.Length <= 2 || a[0].IsZero || a[1].IsZero) return float.MaxValue;

			var n = a.Length-1;
			var c = new float[a.Length + 1];
			c[0] = (float)a[0].Modulus;
			c[1] = (float)(a[1].Modulus - a[0].Modulus);
			c[2] = (float)(-a[1].Modulus);
			c[n] = -2f;
			c[n+1] = 1f;

			return FindLargestPositiveRoot(c);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should be > 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, 0, ..., 0, -a1, a0) and has 2 positive zeroes.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary6(Complex[] a)
		{
			if (a.Length <= 2 || a[0].IsZero || a[1].IsZero) return float.MaxValue;

			var m4 = (float)Math.Max(a[1].Modulus, a[0].Modulus);

			var n = a.Length-1;
			var c = new float[a.Length + 1];
			c[0] = m4;
			c[2] = -m4;
			c[n] = -2f;
			c[n+1] = 1f;

			return FindLargestPositiveRoot(c);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should not be 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, -1, 0, ..., 0, -a1, a0).
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary7(Complex[] a)
		{
			if (a.Length <= 2 || (a[0] * a[1]).IsZero) return float.MaxValue;

			return (float)(1f + a[0].Modulus + a[1].Modulus);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should not be 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, -1, 0, ..., 0, -a1, a0).
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary8(Complex[] a)
		{
			if (a.Length <= 2 || (a[0] * a[1]).IsZero) return float.MaxValue;

			return (float)(0.5f * (1 + Math.Sqrt(1 + 4 * a[1].Modulus + 4 * a[0].Modulus)));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication.
		/// 
		/// Coefficients a[0] and a[1] should not be 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, 0, ..., 0, -a1, a0).
		/// 
		/// Corresponds to Thm. 17 in PLoS paper.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary9(Complex[] a)
		{
			if ((a[0] * a[1]).IsZero || a.Length <= 2) return float.MaxValue;

			var b = (Complex[])a.Clone();

			// correct coefficients if required.
			var bn = b[b.Length-1];
			if (bn.Real != 1f || bn.Imag != 0)
			{
				b = VectorMath.Mul(b, 1f / (float)bn.Real);
				b[b.Length - 1].Imag = 0f;
			}

			var c = new float[b.Length];
			c[0] = (float)(-b[0].Modulus);
			c[1] = (float)(-b[1].Modulus);
			c[a.Length-1] = 1f;

			return Math.Max(1f, FindUniquePositiveRoot(c));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should be > 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, 0, ..., 0, -a1, a0) and has 2 positive zeroes.
		/// 
		/// Corresponds to Thm. 18 in PLoS paper.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary10(Complex[] a)
		{
			if (a.Length <= 2 || a[0].IsZero || a[1].IsZero) return float.MaxValue;

			var b = (Complex[])a.Clone();

			// correct coefficients if required.
			var bn = b[b.Length - 1];
			if (Math.Abs(bn.Real - 1f) > float.Epsilon || Math.Abs(bn.Imag) > float.Epsilon)
			{
				b = VectorMath.Mul(b, 1f / (float)bn.Real);
				b[b.Length - 1].Imag = 0f;
			}

			var m4 = (float)Math.Max(b[1].Modulus, b[0].Modulus);

			var n = b.Length-1;
			var c = new float[b.Length];
			c[0] = -m4;
			c[1] = -m4;
			c[n] = 1f;

			var uroot = FindUniquePositiveRoot(c);
			return Math.Max(1f, uroot);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes radius of the closed disk containing polynomial roots described in
		///		M. Dehmer, A. Mowshowitz, A Classical Problem Revisited: Bounds on the
		///		Moduli of Zeros of Polynomials, submitted for publication .
		/// 
		/// Coefficients a[0] and a[1] should not be 0, otherwise 0 is returned.
		/// It is supposed that a polynomial should have coeffs: (1, 0, ..., 0, -a1, a0).
		/// 
		/// Corresponds to Thm. 19 in PLoS paper.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static float DehmerBoundsLacunary11(Complex[] a)
		{
			if (a.Length <= 2 || (a[0] * a[1]).IsZero) return float.MaxValue;

			var b = (Complex[])a.Clone();

			// correct coefficients if required.
			var bn = b[b.Length - 1];
			if (bn.Real != 1f || bn.Imag != 0)
			{
				b = VectorMath.Mul(b, 1f / (float)bn.Real);
				b[b.Length - 1].Imag = 0f;
			}

			var a1mod = b[1].Modulus;
			return (float)(0.5f * (a1mod + Math.Sqrt(a1mod * a1mod + 4 * b[0].Modulus + 4)));
		}
		#endregion
		#endregion

		#region - Polynomials roots calculation. -
		/// <summary>
		/// [molecule]
		/// 
		/// Finds roots of the polynomial provided with the polynomial's coefficients.
		/// The coefficients are supposed to be assigned according to
		///		sum_i{a_i * x^i}, i = 0..n
		/// 
		/// The method uses eigenvalues of the matrix composed using these coefficients.
		/// (see method described in [http://mathworld.wolfram.com/PolynomialRoots.html]).
		/// </summary>
		/// <param name="a"></param>
		/// <returns>Roots list.</returns>
		public static Complex[] FindPolynomialRoots(float[] a)
		{
			var order = a.Length - 1;

			var roots = new Complex[order];
			if (Math.Abs(a[0]) > float.Epsilon)
			{
				var m = new Matrix(order, order);

				//
				// fill-in the matrix.
				// the 1st line.
				for (int j = 0; j < order; j++) {
					m[0, j] = -a[j + 1] / a[0];
				}
				// all other lines (to obtain differentiation operator).
				for (int i = 1; i < order; i++) {
					m[i, i - 1] = 1;
				}

				//
				// convert eigenvalues into roots.
				for (int i = 0; i < order; i++) {
					roots[i] = 1 / m.EigenValues[i];
				}
			} else {
				// one of the roots is always 0.
				roots[0] = 0;
				if (order > 1) {	// reduce polynomial order by 1.
					var b = VectorMath.Subvector(a, 1, order);
					var rts = FindPolynomialRoots(b);

					for (int i = 1; i < order; i++) {
						roots[i] = rts[i - 1];
					}
				}
			}

			return roots;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Finds roots of the polynomial provided with the polynomial's coefficients.
		/// The coefficients are supposed to be assigned according to
		///		sum_i{a_i * x^i}, i = 0..n
		/// 
		/// The method uses eigenvalues of the matrix composed using these coefficients.
		/// (see method described in [http://mathworld.wolfram.com/PolynomialRoots.html]).
		/// </summary>
		/// <param name="a"></param>
		/// <returns>Roots list.</returns>
		public static Complex[] FindPolynomialRoots(double[] a)
		{
			var order = a.Length - 1;

			var roots = new Complex[order];
			if (a[0] != 0)
			{
				var m = new Matrix(order, order);

				//
				// fill-in the matrix.
				// the 1st line.
				for (int j = 0; j < order; j++)
				{
					m[0, j] = -a[j + 1] / a[0];
				}
				// all other lines (to obtain differentiation operator).
				for (int i = 1; i < order; i++)
				{
					m[i, i - 1] = 1;
				}

				//
				// convert eigenvalues into roots.
				for (int i = 0; i < order; i++)
				{
					roots[i] = 1 / m.EigenValues[i];
				}
			}
			else
			{
				// one of the roots is always 0.
				roots[0] = 0;
				if (order > 1)
				{	// reduce polynomial order by 1.
					var b = VectorMath.Subvector(a, 1, order);
					var rts = FindPolynomialRoots(b);

					for (int i = 1; i < order; i++) { roots[i] = rts[i - 1]; }
				}
			}

			return roots;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns the largest positive real root of the given real polynomial.
		/// If there's no such root then 0 is returned.
		/// </summary>
		/// <returns></returns>
		public static float FindLargestPositiveRoot (float[] a)
		{
			var roots = FindPolynomialRoots(a);
			var max = float.MinValue;
			foreach (var root in roots)
			{
				//if (root.Imag == 0 && max < root.Real) {max = (float)root.Real; }
				if (Math.Abs (root.Imag) <= 0.001 && max < root.Real) { max = (float)root.Real; }
			}
			//return max != 0? max : float.MaxValue;
			return max;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns the first encountered positive real root of the given real polynomial.
		/// If there's no such root then 0 is returned.
		/// </summary>
		/// <returns></returns>
		public static float FindPositiveRoot(float[] a)
		{
			var roots = FindPolynomialRoots(a);
			foreach (var root in roots)
			{
				if (root.Imag == 0 && root.Real > 0) return (float)root.Real;
			}
			return 0;
		}

		public static float[] GetPositiveRoots (float[] a)
		{
			var roots = FindPolynomialRoots(a);
			var realPosRoots = new List<float>();
			foreach (var root in roots)
			{
				if (Math.Abs(root.Imag) < float.Epsilon && root.Real >= 0)
				{
					realPosRoots.Add((float)root.Real);
				}
			}
			return realPosRoots.ToArray();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns the first encountered unique positive real root of the given real polynomial.
		/// If there's no such root then 0 is returned.
		/// </summary>
		/// <returns></returns>
		public static float FindUniquePositiveRoot(float[] a)
		{
			var realPosRoots = GetPositiveRoots(a);
			for (int i = 0; i < realPosRoots.Length; i++)
			{
				if (VectorMath.FirstIndexOf(realPosRoots, realPosRoots[i], i+1) < 0)
				{
					return realPosRoots[i];
				}
			}

			return 0;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Detects whether polynomial with the given coefficients has specified number
		/// of real positive roots.
		/// </summary>
		/// <param name="a">Polynomial coefficients.</param>
		/// <param name="n">Number of requried real positive roots.</param>
		/// <returns></returns>
		public static bool HasNPositiveRoots (float[] a, int n)
		{
			var realPosRoots = GetPositiveRoots(a);
			return realPosRoots.Length >= n;
		}
		#endregion

		#region - Combinatorial functions. -
		/// <summary>
		/// [molecule]
		/// 
		/// Calculates number from the Pascal triangle located at n-th row and m-th column.
		/// </summary>
		/// <param name="n">Row index.</param>
		/// <param name="m">Column index.</param>
		/// <returns>Coefficient from the Pascal's triangle.</returns>
		public static int PascalTriangleCoefficient (int n, int m)
		{
			if (n == 0 || m == 0 || n == m) return 1;

			var size = Math.Max(n, m) + 1;
			var table = new int[size, size];
			table[0, 0] = 1;
			for (int i=1; i<=n; ++i)
			{
				table[i, i] = table[i, 0] = 1;
				for (int j = 1; j < i && j <= m; j++)
				{
					table[i, j] = table[i-1, j] + table[i-1, j-1];
				}
			}
			return table[n, m];
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns lower triangle matrix containing Pascal's triangle.
		/// </summary>
		/// <param name="size">pascal's triangle size.</param>
		/// <returns>Matrix containing Pascal's triangle.</returns>
		public static int[,] PascalTriangle(int size)
		{
			var table = new int[size, size];
			table[0, 0] = 1;
			for (int i = 1; i < size; ++i)
			{
				table[i, i] = table[i, 0] = 1;
				for (int j = 1; j < i; j++)
				{
					table[i, j] = table[i - 1, j] + table[i - 1, j - 1];
				}
			}
			return table;
		}

        /// <summary>
        /// Creates subset of size |prevSubset|, which is represented by a number > prevSubset.
        /// </summary>
        /// <param name="prevSubset"></param>
        /// <param name="setSize"></param>
        /// <param name="subsetSize"></param>
        /// <returns></returns>
        public static bool[] NextSubset(bool[] prevSubset, int setSize, int subsetSize)
        {
            if (prevSubset == null)
            {   // generate 'smallest'number correspondent to the subset.
                // make temporary array containing the 'smallest' binary representation of a subset.
                var res = new bool[setSize];
                for (int i = 0; i < subsetSize; ++i) { res[i] = true; }
                return res;
            }

            var tempArray = (bool[])prevSubset.Clone();
            var zerosToIgnore = 0;
            for (int i = 0; i < setSize; ++i )
            {
                if (tempArray[i] == false) zerosToIgnore++;
                else break;
            }
            if (zerosToIgnore + subsetSize == setSize) return null; // maximal subset is given on input.

            bool finish = true; // add '1' to [tempArray].
            for (int i = 0; i < setSize; ++i)
            {
                if (tempArray[i]) 
                {
                    tempArray[i] = false; 
                }
                else if (zerosToIgnore == 0)
                {
                    tempArray[i] = true;
                    finish = false;
                    break;
                }
                else 
                {
                    zerosToIgnore--;
                }
            }

            if (!finish)
            {
                // check the number of units.
                var count1 = 0;
                for (int i = 0; i < setSize; ++i)
                {
                    count1 += tempArray[i] ? 1 : 0;
                }

                if (count1 <= setSize)
                {   // add units.
                    for (int i = 0; i < subsetSize - count1; ++i)
                    {
                        tempArray[i] = true;
                    }
                    return tempArray;
                }
                else if (count1 > setSize)
                {
                    throw new Exception("[NextSubset]: count1 > setSize");
                }
            }
            throw new Exception("[NextSubset]: finish");
        }
		#endregion

		#region - Eigenvalues and eigenvectors finding. -
		/// <summary>
		/// Interface for the Math.Net library.
		/// </summary>
		public static class EigenMathNet
		{
			/// <summary>
			/// [molecule]
			/// 
			/// Computes eigenvectors and eigenvalues of the given *symmetric* matrix.
			/// Analogue of the Matlab's [eig] command.
			/// </summary>
			/// <param name="m"></param>
			/// <param name="vect"></param>
			/// <param name="eval"></param>
			public static void Eig (float[,] m, out float[,] vect, out float[] eval)
			{
				var dm = MatrixMath.ConvertToDoubles(m);
				var evDecomp = new EigenvalueDecomposition(Matrix.Create(dm));
				vect = MatrixMath.ConvertToFloats(evDecomp.EigenVectors.CopyToArray());
				var evalues = evDecomp.EigenValues;
				eval = VectorMath.Re(evalues);
			}
		}

		/// <summary>
		/// This class performes eigenvalues calculation using QR decomposition as described in
		/// Ustinov S.M., Zimnitskiy V.A. Numerical methods.
		/// </summary>
		public static class Eigenvalues
		{
			public const int MAX_ITER = 1000;
			public const double EPS = 0.0005f;
			public const double EPS_HESSENBERG = 0.0005f;

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues for the given matrix a.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="iterCount">Number of the QR algrotihm iterations.</param>
			/// <returns>Eigenvalues.</returns>
			public static Complex[] ComputeEigenvalues (float[,] a, int iterCount)
			{
				//
				// perform iterCount iterations of the QR decomposition algorithm.
				var tempA = (float[,])a.Clone();
				for (int i = 0; i < iterCount; i++)
				{
					var r = new float[1, 1];
					var q = new float[1, 1];
					QRDecomposition.GetQR(tempA, ref q, ref r);

					tempA = MatrixMath.Mul(r, q);
				}

				return ExtractRoots(tempA, EPS);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues for the given matrix a.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="iterCount">Number of the QR algrotihm iterations.</param>
			/// <returns>Eigenvalues.</returns>
			public static Complex[] ComputeEigenvalues(double[,] a, int iterCount)
			{
				//
				// perform iterCount iterations of the QR decomposition algorithm.
				var tempA = (double[,])a.Clone();
				for (int i = 0; i < iterCount; i++)
				{
					var r = new double[1, 1];
					var q = new double[1, 1];
					QRDecomposition.GetQR(tempA, ref q, ref r);

					tempA = MatrixMath.Mul(r, q);
				}

				return ExtractRoots(tempA, EPS);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues for the given matrix a.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <returns>Eigenvalues.</returns>
			public static Complex[] ComputeEigenvalues(double[,] a)
			{
				//
				// perform iterCount iterations of the QR decomposition algorithm.
				var tempA = (double[,])a.Clone();
				int count = 0;
				while (!MatrixMath.CheckTriU(tempA, EPS_HESSENBERG) && count < MAX_ITER)
				{
					var r = new double[1, 1];
					var q = new double[1, 1];
					QRDecomposition.GetQR(tempA, ref q, ref r);

					tempA = MatrixMath.Mul(r, q);
					++count;
				}

				return ExtractRoots(tempA, EPS);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues for the given matrix a using preliminary
			///		conversion to the Hessenberg form.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="iterCount">Number of the QR algrotihm iterations.</param>
			/// <returns>Eigenvalues.</returns>
			public static Complex[] EigenvaluesHessenberg(float[,] a, int iterCount)
			{
				// todo: use optimized multiplications.

				//
				// perform iterCount iterations of the QR decomposition algorithm.
				var tempA = HessenbergTransform.Transform(a);
				for (int i = 0; i < iterCount; i++)
				{
					var r = new float[1, 1];
					var q = new float[1, 1];
					QRDecomposition.GetQR(tempA, ref q, ref r);

					tempA = MatrixMath.Mul(r, q);
				}

				return ExtractRoots(tempA, EPS_HESSENBERG);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues using prior transform to the Hessenberg form
			/// with an update step using 7.4.1. algorithm with Givens rotations from
			/// Golub et al.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="iterCount">Number of iterations.</param>
			/// <returns>Eigenvalues vector.</returns>
			public static Complex[] EigenvaluesHessenbergGolub (float[,] a, int iterCount)
			{
				//
				// transform to Hessenberg form.
				var m = HessenbergTransform.Transform(a);
				for (int i = 0; i < iterCount; i++)
				{
					m = QRDecomposition.HessenbergRQ(m);
				}

				//return ExtractRoots(m, EPS_HESSENBERG);
				return ExtractRootsEx(m, EPS_HESSENBERG);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues for the given matrix a using preliminary
			///		conversion to the Hessenberg form.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="iterCount">Number of the QR algrotihm iterations.</param>
			/// <returns>Eigenvalues.</returns>
			public static Complex[] EigenvaluesHessenberg(double[,] a, int iterCount)
			{
				// todo: use optimized multiplications.

				//
				// perform iterCount iterations of the QR decomposition algorithm.
				var tempA = HessenbergTransform.Transform(a);
				for (int i = 0; i < iterCount; i++)
				{
					var r = new double[1, 1];
					var q = new double[1, 1];
					QRDecomposition.GetQR(tempA, ref q, ref r);

					tempA = MatrixMath.Mul(r, q);
					if (MatrixMath.CheckTriU(tempA, EPS_HESSENBERG)) break;
				}

				return ExtractRoots(tempA, EPS_HESSENBERG);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Calculates eigenvalues for the given matrix a using preliminary
			///		conversion to the Hessenberg form.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <returns>Eigenvalues.</returns>
			public static Complex[] EigenvaluesHessenberg(double[,] a)
			{
				// todo: use optimized multiplications.

				//
				// perform iterCount iterations of the QR decomposition algorithm.
				var tempA = HessenbergTransform.Transform(a);
				int count = 0;
				while (!MatrixMath.CheckTriU(tempA, EPS_HESSENBERG) && count < MAX_ITER)
				{
					var r = new double[1, 1];
					var q = new double[1, 1];
					QRDecomposition.GetQR(tempA, ref q, ref r);

					tempA = MatrixMath.Mul(r, q);
					++count;
					//if (MatrixMath.CheckTriU(tempA, EPS_HESSENBERG)) break;
				}

				return ExtractRoots(tempA, EPS_HESSENBERG);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs roots extraction from the given block-diagonal matrix.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="eps">Accuracy parameter.</param>
			/// <returns></returns>
			public static Complex[] ExtractRoots (float[,] a, double eps)
			{
				int size = a.GetLength(0);
				var res = new Complex[size];
				for (int i = 0; i < size;)
				{
					if (i == size - 1) 
					{	// no check for block size is required.
						res[i] = a[i, i];
						break;
					}

					// check whether current block is 1x1 or 2x2.
					if (eps > Math.Abs(a[i+1, i]))
					{	// 1x1 block.
						res[i] = a[i, i];
						i++;
					}
					else
					{	// 2x2 block.
						var re = 0.5f * (a[i, i] + a[i + 1, i + 1]);
						var det = (a[i, i] * a[i + 1, i + 1] - a[i, i + 1] * a[i + 1, i]);
						var im = Math.Sqrt(det - re*re);
						res[i] = new Complex(re, im);
						i++;
						res[i] = new Complex(re, -im);
						i++;
					}
				}

				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs roots extraction from the given block-diagonal matrix.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="eps">Accuracy parameter.</param>
			/// <returns></returns>
			public static Complex[] ExtractRootsEx(float[,] a, double eps)
			{
				int size = a.GetLength(0);
				var res = new Complex[size];
				for (int i = 0; i < size; )
				{
					if (i == size - 1)
					{	// no check for block size is required.
						res[i] = a[i, i];
						break;
					}

					//
					// try to extract complex roots from a 2x2 block.
					var re = 0.5f * (a[i, i] + a[i + 1, i + 1]);
					var det = (a[i, i] * a[i + 1, i + 1] - a[i, i + 1] * a[i + 1, i]);
					var sqr = det - re*re;
					if (sqr >= 0)
					{	// the value is truly complex.
						var im = Math.Sqrt(sqr);
						res[i] = new Complex(re, im);
						i++;
						res[i] = new Complex(re, -im);
						i++;
					}
					else
					{	// the value is real.
						res[i] = a[i, i];
						i++;
					}
				}

				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs roots extraction from the given block-diagonal matrix.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="eps">Accuracy parameter.</param>
			/// <returns></returns>
			public static Complex[] ExtractRoots(double[,] a, double eps)
			{
				int size = a.GetLength(0);
				var res = new Complex[size];
				for (int i = 0; i < size; )
				{
					if (i == size - 1)
					{	// no check for block size is required.
						res[i] = a[i, i];
						break;
					}

					// check whether current block is 1x1 or 2x2.
					if (eps >= Math.Abs(a[i + 1, i]))
					{	// 1x1 block.
						res[i] = a[i, i];
						i++;
					}
					else
					{	// 2x2 block.
						var re = 0.5f * (a[i, i] + a[i + 1, i + 1]);
						var det = (a[i, i] * a[i + 1, i + 1] - a[i, i + 1] * a[i + 1, i]);
						var im = Math.Sqrt(det - re * re);
						res[i] = new Complex(re, im);
						i++;
						res[i] = new Complex(re, -im);
						i++;
					}
				}

				return res;
			}
		}
		#endregion

		#region - QR decomposition. -
		public static class QRDecomposition
		{
			/// <summary>
			/// [molecule]
			/// 
			/// Returns orthogonal matrix Q which appears in result of QR-decomposition of the given matrix a.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <returns>Orthogonal matrix Q.</returns>
			public static float[,] GetQEx (float[,] a)
			{
				var size = a.GetLength(0);
				var identity = MatrixMath.Identity(size);
				var q = MatrixMath.Identity(size);
				var tempA = (float[,])a.Clone ();
				for (int i = 0; i < size-1; i++)
				{
					var col = MatrixMath.GetColumn(tempA, i);
					var v = VectorMath.Subvector(col, i, size - 1);
					var vnorm = VectorMath.L2Norm(v);
					v[0] += vnorm;
					var house = HouseholderTransform.GetMatrix(v);

					var u = MatrixMath.SetSubmatrix(identity, house, i, i);
					q = MatrixMath.Mul(q, u);
					tempA = MatrixMath.Mul(u, tempA);
				}

				return q;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Returns orthogonal matrix Q which appears in result of QR-decomposition of the given matrix a.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="q">Resulting Q matrix.</param>
			/// <param name="r">Resulting R matrix.</param>
			/// <returns>Orthogonal matrix Q.</returns>
			public static void GetQR(float[,] a, ref float[,] q, ref float[,] r)
			{
				var size = a.GetLength(0);
				var identity = MatrixMath.Identity(size);
				q = MatrixMath.Identity(size);

				var tempA = (float[,])a.Clone();
				for (int i = 0; i < size - 1; i++)
				{
					var col = MatrixMath.GetColumn(tempA, i);
					var v = VectorMath.Subvector(col, i, size - 1);
					var vnorm = VectorMath.L2Norm(v);
					v[0] += vnorm;
					var house = HouseholderTransform.GetMatrix(v);

					var u = MatrixMath.SetSubmatrix(identity, house, i, i);
					q = MatrixMath.Mul(q, u);
					tempA = MatrixMath.Mul(u, tempA);
				}

				var qt = MatrixMath.Transpose(q);
				r = MatrixMath.Mul(qt, a);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Returns orthogonal matrix Q which appears in result of QR-decomposition of the given matrix a.
			/// </summary>
			/// <param name="a">Input matrix.</param>
			/// <param name="q">Resulting Q matrix.</param>
			/// <param name="r">Resulting R matrix.</param>
			/// <returns>Orthogonal matrix Q.</returns>
			public static void GetQR(double[,] a, ref double[,] q, ref double[,] r)
			{
				var size = a.GetLength(0);
				var identity = MatrixMath.IdentityD(size);
				q = MatrixMath.IdentityD(size);

				var tempA = (double[,])a.Clone();
				for (int i = 0; i < size - 1; i++)
				{
					var col = MatrixMath.GetColumn(tempA, i);
					var v = VectorMath.Subvector(col, i, size - 1);
					var vnorm = VectorMath.L2Norm(v);
					v[0] += vnorm;
					var house = HouseholderTransform.GetMatrix(v);

					var u = MatrixMath.SetSubmatrix(identity, house, i, i);
					q = MatrixMath.Mul(q, u);
					tempA = MatrixMath.Mul(u, tempA);
				}

				var qt = MatrixMath.Transpose(q);
				r = MatrixMath.Mul(qt, a);
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Having matrix in the Hessenberg form on input, performs QR decomposition and returns R'Q.
			/// This method implements algorithm 7.4.1. from Golub et al. and has complexity ~6n^2.
			/// </summary>
			/// <param name="matr"></param>
			/// <returns></returns>
			public static float[,] HessenbergRQ (float[,] matr)
			{
				var h = (float[,]) matr.Clone();
				var n = h.GetLength(0);
				var m = h.GetLength(1);
				float[] c = new float[n], s = new float[n];
				for (int k = 0; k < n - 1; ++k )
				{
					// generate Givens rotation G_k and apply it to h.
					GivensRotations.Givens(h[k, k], h[k + 1, k], out c[k], out s[k]);	// get Givens coefficients.
					
					// rotate rows k and k+1.
					GivensRotations.RowsRotation(ref h, k, k + 1, k, n - 1, c[k], s[k]);	// rotation starts at column k because elements at [k,k'] k' < k are zeroes.
					//GivensRotations.RowsRotation(ref h, k, k + 1, 0, n - 1, c[k], s[k]);
				}

				//for (int k = n - 2; k >= 0; --k)
				for (int k = 0; k < n - 1; ++k )
				{
					// Update H = H * G.
					// rotate columns k and k+1.
					GivensRotations.ColsRotation(ref h, k, k + 1, 0, k + 1, c[k], s[k]);	// rotation ends at row k+1 because elements at [k',k+1] k' > k+1 are zeroes.
					//GivensRotations.ColsRotation(ref h, k, k + 1, 0, m-1, c[k], s[k]);
				}

				return h;
			}
		}
		#endregion

		#region - Singular Value Decomposition. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs Singular Value Decomposition using algorithm 8.3.2 from Golub et al.
		/// </summary>
		public static class SVDecomposition
		{
			public static void GetSVD (float[,] a, float eps, out float[,] u, out float[,] d, out float[,] v)
			{
				// todo: bidiagonalization using 5.4.2
				// todo: subsequent diagonalization using 8.3.2 and 8.3.1.


				throw new NotImplementedException();
			}
		}
		#endregion

		#region - Utility methods. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs tridiagonalization using Housholder transform.
		/// See: Golub, Van Loan 'Matrix Calculus', algorithm at p. 377
		/// </summary>
		/// <param name="a">Input matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] HouseholderTridiag (float[,] a)
		{
			float[,] t = MatrixMath.Clone (a);
			int n = t.GetLength(0);
			for (int k = 0; k < n - 2; ++k)
			{
				var column = MatrixMath.GetColumn(t, k);
				float[] h = VectorMath.Subvector(column, k + 1, n - 1);
				float[] v = VectorMath.HouseholderVector(h);	// v = house (А (k + 1: n, k)) 
				float vNorm = VectorMath.DotProduct(v, v);
				var vN = (float[])v.Clone();
				VectorMath.Mul(ref vN, 1f / vNorm);	// [vN] = normalized [v].

				float[,] subA = MatrixMath.Submatrix(t, k + 1, n - 1);
				float[] p = MatrixMath.Mul(subA, vN);
				VectorMath.Mul(ref p, 2f);
				var w =  (float[])vN.Clone ();
				VectorMath.Mul(ref w, VectorMath.DotProduct(p, v));
				w = VectorMath.Sub(p, w);

				var temp = MatrixMath.Sub(VectorMath.DyadicProduct(v, w), VectorMath.DyadicProduct(w, v));
				subA = MatrixMath.Sub(subA, temp);

				MatrixMath.SetSubmatrix(ref t, subA, k + 1, k + 1);
			}

			return t;
		}
		#endregion

		#region - Householder transformation. -
		/// <summary>
		/// Class for Householder transform.
		/// </summary>
		public static class HouseholderTransform
		{
			/// <summary>
			/// [molecule]
			/// 
			/// Returns Householder transform matrix generated from the given vector.
			/// </summary>
			/// <param name="v">Source vector.</param>
			/// <returns>Householder transform matrix.</returns>
			public static float[,] GetMatrix (float[] v)
			{
				var size = v.Length;
				var res = MatrixMath.Identity(size);

				var norm = VectorMath.DotProduct(v, v);
				var vvt = VectorMath.DyadicProduct(v);
				vvt = MatrixMath.Mul(vvt, 2f / norm);

				res = MatrixMath.Sub(res, vvt);
				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Returns Householder transform matrix generated from the given vector.
			/// </summary>
			/// <param name="v">Source vector.</param>
			/// <returns>Householder transform matrix.</returns>
			public static double[,] GetMatrix(double[] v)
			{
				var size = v.Length;
				var res = MatrixMath.IdentityD(size);

				var norm = VectorMath.DotProduct(v, v);
				var vvt = VectorMath.DyadicProduct(v);
				vvt = MatrixMath.Mul(vvt, 2.0 / norm);

				res = MatrixMath.Sub(res, vvt);
				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Returns Householder's vector generated from the vector x.
			/// Implements algorithm *house* (5.1.1) from Golub et al.
			/// </summary>
			/// <param name="x">Source vector.</param>
			/// <returns>Householder's vector.</returns>
			public static float[] GetVector (float[] x)
			{
				var v = (float[])x.Clone();
				float mu = VectorMath.L2Norm(v);
				if (mu != 0.0f)
				{
					float beta = x[0] + mu * (x[0] >= 0 ? 1 : -1);
					VectorMath.Mul(ref v, 1f / beta);
				}
				v[0] = 1f;
				return v;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Perfoms left multiplication on the Householder matrix.
			/// The Householder's matrix is presented by a Householder vector.
			/// This method implements function *row.house* (5.1.2) from Golub et al.
			/// The result is P*A, where P is the Householder's matrix.
			/// </summary>
			/// <param name="a">Source matrix.</param>
			/// <param name="v">Householder vector.</param>
			/// <returns>The result of multiplication.</returns>
			public static float[,] MulLeft (float[,] a, float[] v)
			{
				var beta = -2f / VectorMath.DotProduct(v, v);
				var at = MatrixMath.Transpose(a);
				var w = MatrixMath.Mul(at, v);
				w = VectorMath.Mul(w, beta);
				
				var res = (float[,])a.Clone();
				var vwt = VectorMath.DyadicProduct(v, w);
				res = MatrixMath.Add(res, vwt);

				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Perfoms right multiplication on the Householder matrix.
			/// The Householder's matrix is presented by a Householder vector.
			/// This method implements function *col.house* (5.1.3) from Golub et al.
			/// The result is A*P, where P is the Householder's matrix.
			/// </summary>
			/// <param name="a">Source matrix.</param>
			/// <param name="v">Householder vector.</param>
			/// <returns>The result of multiplication.</returns>
			public static float[,] MulRight (float[,] a, float[] v)
			{
				var beta = -2f / VectorMath.DotProduct(v, v);
				var w = MatrixMath.Mul(a, v);
				w = VectorMath.Mul(w, beta);

				var res = (float[,])a.Clone();
				var wvt = VectorMath.DyadicProduct(w, v);
				res = MatrixMath.Add(res, wvt);

				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs bidiagonalization using algorithm 5.4.2 from Golub et al.
			/// 
			/// </summary>
			/// <param name="matr"></param>
			/// <returns></returns>
			public static float[,] Bidiagonalize (float[,] matr)
			{
				var a = (float[,])matr.Clone();

				int m = a.GetLength(0), n = a.GetLength(1);
				bool transp = false;
				if (m < n)
				{	// if the matrix is 'horizontal' then perform transpose.
					a = MatrixMath.Transpose(a);

					var t = n;
					n = m;
					m = t;

					transp = true;
				}

				//
				// perform bidiagonalization.
				for (int j=0; j<n; ++j)
				{
					var col = MatrixMath.GetColumn(a, j);
					var v = VectorMath.Subvector(col, j, m - 1);
					v = GetVector(v);

					var suba = MatrixMath.Submatrix(a, j, m - 1, j, n - 1);
					suba = MulLeft(suba, v);	// nullify j-th column at elements [j+1:m-1].
					MatrixMath.SetSubmatrix(ref a, suba, j, j);

					//int count = 1;
					//for (int i = j + 1; i < m; ++i, ++count) { a[i, j] = v[count]; }	// set column subvector.

					if (j <= n-2)
					{
						var row = MatrixMath.GetRow(a, j);
						v = VectorMath.Subvector(row, j + 1, n - 1);
						v = GetVector(v);

						suba = MatrixMath.Submatrix(a, j, m - 1, j + 1, n-1);
						suba = MulRight(suba, v);
						MatrixMath.SetSubmatrix(ref a, suba, j, j + 1);

						//count = 1;
						//for (int i = j + 2; i < n; ++i, ++count) { a[j, i] = v[count]; }	// set row subvector.
					}
					//FileIO.WriteColumns("log"+j, a, "Matrix A");
				}

				return transp? MatrixMath.Transpose (a) : a;
			}
		}
		#endregion

		#region - Hessenberg transformation. -
		/// <summary>
		/// Performs transformation of the matrix to the Hessenberg form.
		/// </summary>
		public static class HessenbergTransform
		{
			/// <summary>
			/// [molecule]
			/// 
			/// Performs transformation of the given matrix to the Hessenberg form.
			/// </summary>
			/// <returns>Matrix in the Hessenberg form.</returns>
			public static float[,] Transform (float[,] a)
			{
				var size = a.GetLength(0);

				//
				// accumulate transform matrix U.
				var size_1 = size - 1;
				var res = (float[,])a.Clone();
				for (int i = 0; i < size - 2; i++)
				{
					var col = MatrixMath.GetColumn(res, i);
					var v = VectorMath.Subvector(col, i + 1, size_1);
					var normv = VectorMath.L2Norm(v);
					v[0] += normv;
					var h = HouseholderTransform.GetMatrix(v);	// Hausholder matrix.

					// create orthogonal [size x size] matrix.
					var u = MatrixMath.Identity(size);
					MatrixMath.SetSubmatrix(ref u, h, i + 1, i + 1);

					// update resulting matrix.
					res = MatrixMath.Mul(u, res);
					res = MatrixMath.Mul(res, u);

					// force zeroes below the 1st subdiagonal.
					//for (int j = i + 2; j < size; j++)
					//{
					//    res[j, i] = 0;
					//}
				}

				return res;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs transformation of the given matrix to the Hessenberg form.
			/// </summary>
			/// <returns>Matrix in the Hessenberg form.</returns>
			public static double[,] Transform(double[,] a)
			{
				var size = a.GetLength(0);

				//
				// accumulate transform matrix U.
				var size_1 = size - 1;
				var res = (double[,])a.Clone();
				for (int i = 0; i < size - 2; i++)
				{
					var col = MatrixMath.GetColumn(res, i);
					var v = VectorMath.Subvector(col, i + 1, size_1);
					var normv = VectorMath.L2Norm(v);
					v[0] += normv;
					var h = HouseholderTransform.GetMatrix(v);	// Hausholder matrix.

					// create orthogonal [size x size] matrix.
					var u = MatrixMath.IdentityD(size);
					MatrixMath.SetSubmatrix(ref u, h, i + 1, i + 1);

					// update resulting matrix.
					res = MatrixMath.Mul(u, res);
					res = MatrixMath.Mul(res, u);

					// force zeroes below the 1st subdiagonal.
					for (int j = i + 2; j < size; j++)
					{
						res[j, i] = 0;
					}
				}

				return res;
			}

			/// <summary>
			/// Performs transformation to the Hessenberg form using
			///		algorithm 7.4.2. from Golub et al.
			/// The algorithm's complexity is 10n^3/3.
			/// </summary>
			/// <param name="m">Input matrix.</param>
			/// <returns></returns>
			public static float[,] TransformGolub (float[,] m)
			{
				var a = (float[,])m.Clone();
				var n = a.GetLength(0);

				for (int k = 0; k < n-2; k++)
				{
					var v = MatrixMath.GetColumn(a, k);
					v = HouseholderTransform.GetVector(VectorMath.Subvector(v, k+1, n-1));

					// А(k+l:n, k:n) = row.house (A (k+1:n, k:n), v(k+1:n))
					var suba = MatrixMath.Submatrix(a, k + 1, n - 1, k, n - 1);
					suba = HouseholderTransform.MulLeft(suba, v);
					MatrixMath.SetSubmatrix(ref a, suba, k+1, k);

					// А(1:n, k+1:n) = col.house(A(1:n, k+1:n), v(k+1:n))
					suba = MatrixMath.Submatrix(a, 0, n - 1, k+1, n - 1);
					suba = HouseholderTransform.MulRight(suba, v);
					MatrixMath.SetSubmatrix(ref a, suba, 0, k+1);

					// A(k+2:n, k) = v(k+2:n)
                    for (int i=k+2, count=0; i<n; ++i, ++count)
					{
						a[i, k] = v[count];
					}
				}

				return a;
			}
		}
		#endregion

		#region - Givens rotations. -
		/// <summary>
		/// [molecule]
		/// 
		/// This class implements Givens rotations as per Golub et al.
		/// </summary>
		public class GivensRotations
		{
			/// <summary>
			/// [molecule]
			/// 
			/// Returns components of the Givens rotational matrix with defence for overflow.
			/// Implements algorithm 5.1.5 from Golub et al.
			/// It uses 5 flops and 1 sqrt.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			public static void Givens(float a, float b, out float c, out float s)
			{
				if (b == 0)
				{
					c = 1;
					s = 0;
				}
				else if (Math.Abs(b) > Math.Abs(a)) 
				{
					//var tau = -a / b;
					var tau = a / b;
					s = (float)(1f/Math.Sqrt(1 + tau*tau));
					c = s*tau;
				}
				else
				{
					//var tau = -b / a;
					var tau = b / a;
					c = (float)(1f / Math.Sqrt(1 + tau * tau));
					s = c * tau;
				}
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs multiplication of the Givens matrix on a 2 rows matrix A.
			/// Returns G' * A (in Matlab notation).
			/// Implements algorithm 5.1.6 from Golub et al.
			/// </summary>
			/// <returns></returns>
			public static float[,] RowsRotation (float[,] m, float c, float s)
			{
				var a = (float[,])m.Clone();
				var q = a.GetLength(1);	// number of columns.

				for (int i = 0; i < q; i++)
				{
					var tau1 = a[0, i];
					var tau2 = a[1, i];

					a[0, i] = c * tau1 - s * tau2;
					a[1, i] = s * tau1 + c * tau2;
				}

				return a;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs Givens rotation for specified rows and columns of matrix A as G'A.
			/// The result is stored in the source matrix.
			/// Implements algorithm 5.1.6 from Golub et al.
			/// </summary>
			public static void RowsRotation(ref float[,] m, int row1, int row2, int colBeg, int colEnd, float c, float s)
			{
				for (int i = colBeg; i <= colEnd; i++)
				{
					var tau1 = m[row1, i];
					var tau2 = m[row2, i];

					m[row1, i] = c * tau1 + s * tau2;
					m[row2, i] = -s * tau1 + c * tau2;
				}
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs multiplication of 2 columns matrix A on the Givens matrix.
			/// Returns A * G (in Matlab notation).
			/// Implements algorithm 5.1.6 from Golub et al.
			/// </summary>
			/// <returns></returns>
			public static float[,] ColsRotation(float[,] m, float c, float s)
			{
				var a = (float[,])m.Clone();
				var q = a.GetLength(0);	// number of rows.

				for (int i = 0; i < q; i++)
				{
					var tau1 = a[i, 0];
					var tau2 = a[i, 1];

					a[i, 0] = c * tau1 - s * tau2;
					a[i, 1] = s * tau1 + c * tau2;
				}

				return a;
			}

			/// <summary>
			/// [molecule]
			/// 
			/// Performs multiplication of 2 columns matrix A on the Givens matrix.
			/// Returns A * G (in Matlab notation).
			/// Implements algorithm 5.1.6 from Golub et al.
			/// </summary>
			/// <returns></returns>
			public static void ColsRotation(ref float[,] m, int col1, int col2, int rowBeg, int rowEnd, float c, float s)
			{
				for (int i = rowBeg; i <= rowEnd; i++)
				{
					var tau1 = m[i, col1];
					var tau2 = m[i, col2];

					m[i, col1] = c * tau1 + s * tau2;
					m[i, col2] = -s * tau1 + c * tau2;
				}
			}
		}
		#endregion

		#region - Linear algebra and analytical geometry. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes rank of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="eps">Accuracy.</param>
		/// <returns>Rank calculated using bidiagonalization.</returns>
		public static int Rank (float[,] m, float eps)
		{
			var bid = HouseholderTransform.Bidiagonalize(m);

			// calculate elements  on the main diagonal
			// which are larger than [eps]
			var size = Math.Min(bid.GetLength(0),bid.GetLength(1));
			var rank = 0;
			for (int i = 0; i < size; i++)
			{
				if (Math.Abs(bid[i, i]) > eps) rank++;
			}
			return rank;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes rank of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Rank calculated using SVD.</returns>
		public static int RankEx(float[,] m)
		{
			var md = Matrix.Create(MatrixMath.ConvertToDoubles(m));
			var svd = new SingularValueDecomposition(md);
			return svd.Rank();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes determinant of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="eps">Accuracy.</param>
		/// <returns>Determinant value.</returns>
		public static float Determinant (float[,] m, float eps)
		{
			var height = m.GetLength(0);

			var matr = (float[,])m.Clone();
			bool evenPermut = true;	// permutations count
			for (int i = 0; i < height-1; i++)
			{
				bool perms;
				if (!Eliminate(matr, i, eps, out perms)) return 0f;
				if (perms) { evenPermut ^= true; }
			}

			var det = DeterminantDiagonal(matr);
			return evenPermut? det : -det;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes determinant of the diagonal matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Determinant value.</returns>
		public static float DeterminantDiagonal(float[,] m)
		{
			var res = 1f;
			var width = m.GetLength(1);
			for (int i = 0; i < width; i++)
			{
				res *= m[i, i];
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs Gaussian elimination procedure for the given pivot column.
		/// The result is stored at the same matrix.
		/// If the elimination fails, i.e. each element in the column starting
		/// from the row [pivorCol] is below the [eps] value, then [false] is returned.
		/// 
		/// The rows exchange fact, if it took place, is written in the [perms].
		/// 
		/// This method assumes that for all columns before [pivotCol] the elimination is made.
		/// </summary>
		/// <param name="matr"></param>
		/// <param name="pivotCol"></param>
		/// <returns></returns>
		public static bool Eliminate (float[,] matr, int pivotCol, float eps, out bool perms)
		{
			perms = false;

			var m = matr.GetLength(0);	// height;

			//
			// look for pivot row
			int pivotRow = pivotCol;
			var pivotVal = Math.Abs(matr[pivotCol, pivotCol]);
			for (int i = pivotCol + 1; i < m; i++)
			{
				var curVal = Math.Abs(matr[i, pivotCol]);
				if (curVal > pivotVal)
				{
					pivotVal = curVal;
					pivotRow = i;
				}
				//if (Math.Abs(matr[i, pivotCol]) > eps)
				//{	// look for the pivot in the next row.
				//    pivotRow = i;
				//    break;
				//}
			}
			//if (pivotRow < 0) {return false;}
			if (pivotVal <= eps) { return false; }

			//
			// check if row exchange is required.
			if (pivotRow != pivotCol)
			{	// swap rows and set [perms]
				perms = true;
				MatrixMath.ExchangeRows(matr, pivotRow, pivotCol);
				pivotRow = pivotCol;
			}

			//
			// perform elimination starting from the row [pivotCol+1].
			for (int i = pivotCol+1; i < m; i++)
			{
				if (matr[i, pivotCol] == 0f) continue;
				var coef = -matr[i, pivotCol] / matr[pivotRow, pivotCol];
				MatrixMath.CombineRows(matr, pivotRow, i, pivotCol, coef);
			}

			return true;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns orthonormal basis in R^{n-1}, which spans linear subspace perpendicular to the given vector v \in R^n.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Basis vectors.</returns>
		public static List<float[]> GetOrthonormalBasisPerp (float[] v)
		{
			var res = new List<float[]> ();
			var size = v.Length;
			for (int i = 0; i < size - 1; ++i)
			{
				var tempV = new float[size];
				// todo: fix bug when v[i] or v[i+1] = 0.
				//if (v[i+1] == 0)
				//{
				//    tempV[i] = 0;
				//    tempV[i + 1] = 1;
				//}
				//else if (v[i] == 0)
				//{
					
				//}
				tempV[i] = 1f;
				tempV[i + 1] = -v[i]/v[i + 1];
				res.Add(tempV);
			}

			return GramSchmidtOrthonormalization(res);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs Gram–Schmidt process.
		/// </summary>
		/// <param name="v">Input set of vectors.</param>
		/// <returns>Set of orthogonal vectors.</returns>
		public static List<float[]> GramSchmidtOrthogonalization (List<float[]> v)
		{
			var res = new List<float[]>();

			int size = v.Count;
			for (int i = 0; i < size; i++)
			{
				var u = v[i];
				for (int j = 0; j < i; j++)
				{
					var proj = VectorMath.Projection(res[j], v[i]);
					u = VectorMath.Sub(u, proj);
				}
				res.Add(u);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs Gram–Schmidt process.
		/// </summary>
		/// <param name="v">Input set of vectors.</param>
		/// <returns>Set of orthogonal vectors.</returns>
		public static List<float[]> GramSchmidtOrthonormalization(List<float[]> v)
		{
			var res = GramSchmidtOrthogonalization(v);

			for (int i = 0; i < v.Count; i++)
			{
				res[i] = VectorMath.NormalizeL2(res[i]);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates distances between given set of points and the line, containing two points p1 and p2.
		/// </summary>
		/// <param name="p1">1st point.</param>
		/// <param name="p2">2nd point.</param>
		/// <param name="pts">Set of points.</param>
		/// <returns>List of distances.</returns>
		public static List<float> CalculateDistances (float[] p1, float[] p2, List<float[]> pts)
		{
			var res = new List<float>();
			foreach (var pt in pts)
			{
				var dist = CalculateDistance(p1, p2, pt);
				res.Add(dist);
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates distance between given point pt and the line through points p1 and p2.
		/// </summary>
		/// <param name="p1">1st point.</param>
		/// <param name="p2">2nd point.</param>
		/// <param name="pt">Point of interest.</param>
		/// <returns>Distance value.</returns>
		public static float CalculateDistance(float[] p1, float[] p2, float[] pt)
		{
			//
			// find point on the line, which corresponds to the [pt] projection.
			var dir = VectorMath.Sub(p2, p1);
			var tempV = VectorMath.Sub(pt, p1);

			// find projection parameter in line equation in the vector form.
			var norm2 = VectorMath.DotProduct(dir, dir);
			//if (norm2 == 0) return 0;

			var t = VectorMath.DotProduct(tempV, dir) / norm2;

			// define project point coordinates.
			var offset = VectorMath.Mul(dir, t);
			var projPt = VectorMath.Add(p1, offset);

			// calculate distance.
			return VectorMath.L2Norm(VectorMath.Sub(projPt, pt));
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates distance for 2D case between given point pt and the line through points p1 and p2.
		/// </summary>
		/// <param name="p1">1st point.</param>
		/// <param name="p2">2nd point.</param>
		/// <param name="pt">Point of interest.</param>
		/// <returns>Distance value.</returns>
		public static float CalculateDistance2D (float[] p1, float[] p2, float [] pt)
		{
			var dx = p2[0] - p1[0];
			var dy = p2[1] - p1[1];

			//
			// find coefficients for line equation in the form
			//
			//	ax + by + c = 0
			//
			// from (x-x_1) / (x_2-x_1) = (y-y_1) / (y_2-y_1).
			//
			var a = dy;
			var b = -dx;
			var c = -dy * p1[0] + dx * p1[1];

			//
			// calculate distance using formula:
			//
			//	d = abs { (a x_1 + b y_1 + c) / sqrt{a^2 + b^2} }
			//
			var d = Math.Abs( (a*pt[0] + b*pt[1] + c) / Math.Sqrt(a*a + b*b) );
			return (float)d;
		}
		#endregion
	}

	public static class Polynomials
	{
		public const string CHEBYSHEV1_POL = "Chebyshev polynomial (1 kind)";
		public const string CHEBYSHEV2_POL = "Chebyshev polynomial (2 kind)";
		public const string BERNSTEIN_POL = "Bernstein polynomial";
		public const string LAGUERRE_POL = "Laguerre polynomial";
		public const string RANDOM_POL_GAUSSIAN = "Random polynomial (Gaussian)";
		public const string RANDOM_POL_GEOMETRIC = "Random polynomial (Geometric)";
		public const string RANDOM_POL_POISSON = "Random polynomial (Poisson)";
		public const string RANDOM_POL_UNIFORM = "Random polynomial (Uniform)";
		public const string RANDOM_POL_UNIFORM_1 = "Random polynomial (Uniform, abs{c_i} less 1)";
		// special random lacunary polynomials.
		public const string RANDOM_POL_LACUNARY1 = "Lacunary: z^n - z^{n-1} - a_1*z + a_0, a_1, a_0 > 0 (Gaussian)";
		public const string RANDOM_POL_LACUNARY2 = "Lacunary: z^n - a_1*z + a_0, a_1, a_0 > 0 (Gaussian)";
		public const string RANDOM_POL_LACUNARY3 = "Lacunary: z^n - z^{n-1} - a_1*z + a_0, a_1*a_0 != 0 (Gaussian)";
		public const string RANDOM_POL_LACUNARY4 = "Lacunary: z^n - a_1*z + a_0, a_1*a_0 != 0 (Gaussian)";	// def. 6 in PLoS paper.
		// special random polynomials with constrained coefficients.
		public const string RANDOM_POL_CONSTR1 = "Constrained: |a_i| less 1 (Gaussian)";
		public const string RANDOM_POL_CONSTR2 = "Constrained: |a_i| less 1, a_n - arbitrary (Gaussian)";
		public const string RANDOM_POL_CONSTR3 = "Constrained: |a_i| less 1, a_n, a_{n-1} - arbitrary (Gaussian)";
		public const string RANDOM_POL_CONSTR4 = "Constrained: |a_i|/|a_n| less 1 (Gaussian)";
		public const string RANDOM_POL_CONSTR5 = "Constrained Multiple: f = f_1 * f_2, |c_{nj}| > |c_i| (Gaussian)";
		public const string RANDOM_POL_MULTIPLE = "Multiple: f = f_1 * f_2 (Gaussian)";



		/// <summary>
		/// [molecule]
		/// 
		/// Returns all available special polynomials.
		/// </summary>
		/// <returns></returns>
		public static string[] GetPolynomials ()
		{
			var res = new List<string>
			          	{
			          		CHEBYSHEV1_POL,
			          		CHEBYSHEV2_POL,
			          		BERNSTEIN_POL,
			          		LAGUERRE_POL,
			          		RANDOM_POL_GAUSSIAN,
			          		RANDOM_POL_GEOMETRIC,
			          		RANDOM_POL_POISSON,
			          		RANDOM_POL_UNIFORM,
			          		RANDOM_POL_UNIFORM_1,
							RANDOM_POL_LACUNARY1,
							RANDOM_POL_LACUNARY2,
							RANDOM_POL_LACUNARY3,
							RANDOM_POL_LACUNARY4,
							RANDOM_POL_CONSTR1,
							RANDOM_POL_CONSTR2,
							RANDOM_POL_CONSTR3,
							RANDOM_POL_CONSTR4,
							RANDOM_POL_CONSTR5,
							RANDOM_POL_MULTIPLE,
			          	};

			return res.ToArray();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns all available polynomials with random coefficients.
		/// </summary>
		/// <returns></returns>
		public static string[] GetRandomPolynomials()
		{
			var res = new List<string>
			          	{
			          		RANDOM_POL_GAUSSIAN,
			          		RANDOM_POL_GEOMETRIC,
			          		RANDOM_POL_POISSON,
			          		RANDOM_POL_UNIFORM,
			          		RANDOM_POL_UNIFORM_1,
							RANDOM_POL_LACUNARY1,
							RANDOM_POL_LACUNARY2,
							RANDOM_POL_LACUNARY3,
							RANDOM_POL_LACUNARY4,
							RANDOM_POL_CONSTR1,
							RANDOM_POL_CONSTR2,
							RANDOM_POL_CONSTR3,
							RANDOM_POL_CONSTR4,
							RANDOM_POL_CONSTR5,
							RANDOM_POL_MULTIPLE,
			          	};

			return res.ToArray();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates special polynomial with random coefficients taken from the prescribed distributions.
		/// </summary>
		/// <param name="poly">Polynomial type name.</param>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="distr">RNG for polynomial coefficients.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial GetPolynomial (string poly, int order, RandomDistribution distr)
		{
			if (string.Compare(poly, CHEBYSHEV1_POL) == 0) return ChebyshevPolynomial(order);
			if (string.Compare(poly, CHEBYSHEV2_POL) == 0) return ChebyshevPolynomial2(order);
			if (string.Compare(poly, BERNSTEIN_POL) == 0) return BernsteinPolynomial(order, distr);
			if (string.Compare(poly, LAGUERRE_POL) == 0) return LaguerrePolynomial(order);
			if (string.Compare(poly, RANDOM_POL_GAUSSIAN) == 0
				|| string.Compare(poly, RANDOM_POL_UNIFORM) == 0
				|| string.Compare(poly, RANDOM_POL_UNIFORM_1) == 0)
				return RandomPolynomial(order, ((ContinuousRandomDistribution)distr).Distribution);
			if (string.Compare(poly, RANDOM_POL_POISSON) == 0
				|| string.Compare(poly, RANDOM_POL_GEOMETRIC) == 0)
				return RandomPolynomial(order, ((DiscreteRandomDistribution)distr).Distribution);
			if (string.Compare(poly, RANDOM_POL_LACUNARY1) == 0) return Lacunary1(order, ((ContinuousRandomDistribution)distr).Distribution);
			if (string.Compare(poly, RANDOM_POL_LACUNARY2) == 0) return Lacunary2(order, ((ContinuousRandomDistribution)distr).Distribution);
			if (string.Compare(poly, RANDOM_POL_LACUNARY3) == 0)
			{
				var pol = Lacunary3(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_LACUNARY4) == 0)
			{
				var pol = Lacunary4(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR1) == 0)
			{
				var pol = Constrained1(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR2) == 0)
			{
				var pol = Constrained2(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR3) == 0)
			{
				var pol = Constrained3(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR4) == 0)
			{
				var pol = Constrained4(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR5) == 0)
			{
				var pol = Constrained5(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}
			if (string.Compare(poly, RANDOM_POL_MULTIPLE) == 0)
			{
				var pol = Multiple(order, ((ContinuousRandomDistribution)distr).Distribution);
				var rpol = GetRealCoefficients(pol);
				var dpol = VectorMath.ConvertToDoubles(rpol);
				return new Polynomial(dpol);
			}

			throw new NotImplementedException("Unrecognized polynomial name: " + poly);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates special polynomial with random complex coefficients taken from the prescribed distributions.
		/// </summary>
		/// <param name="poly">Polynomial type name.</param>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="distr">RNG for polynomial coefficients.</param>
		/// <returns>Random complex polynomial.</returns>
		public static Complex[] GetComplexPolynomial(string poly, int order, RandomDistribution distr)
		{
			if (string.Compare(poly, RANDOM_POL_GAUSSIAN) == 0
				|| string.Compare(poly, RANDOM_POL_UNIFORM) == 0)
			{
				return RandomComplexPolynomial(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_UNIFORM_1) == 0)
			{
				// ensure that resulting polynomial has |coefficients| < 1.
				var pol = RandomComplexPolynomial(order, ((ContinuousRandomDistribution)distr).Distribution);
				var mods = VectorMath.Abs(pol);
				var max = VectorMath.Max(mods);
				return VectorMath.Divide(pol, max + float.Epsilon);
			}
			if (string.Compare(poly, RANDOM_POL_POISSON) == 0
				|| string.Compare(poly, RANDOM_POL_GEOMETRIC) == 0)
			{
				return RandomComplexPolynomial(order, ((DiscreteRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_LACUNARY1) == 0)
			{
				var rpol = Lacunary1(order, ((ContinuousRandomDistribution)distr).Distribution);
				return GetComplexCoefficients(rpol);
			}
			if (string.Compare(poly, RANDOM_POL_LACUNARY2) == 0)
			{
				var rpol = Lacunary2(order, ((ContinuousRandomDistribution)distr).Distribution);
				return GetComplexCoefficients(rpol);
			}
			if (string.Compare(poly, RANDOM_POL_LACUNARY3) == 0)
			{
				return Lacunary3(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_LACUNARY4) == 0)
			{
				return Lacunary4(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR1) == 0)
			{
				return Constrained1(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR2) == 0)
			{
				return Constrained2(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR3) == 0)
			{
				return Constrained3(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR4) == 0)
			{
				return Constrained4(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_CONSTR5) == 0)
			{
				return Constrained5(order, ((ContinuousRandomDistribution)distr).Distribution);
			}
			if (string.Compare(poly, RANDOM_POL_MULTIPLE) == 0)
			{
				return Multiple(order, ((ContinuousRandomDistribution)distr).Distribution);
			}

			throw new NotImplementedException("Unrecognized polynomial name: " + poly);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates special polynomial with random coefficients taken from the prescribed distributions.
		/// </summary>
		/// <param name="poly">Polynomial type name.</param>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="rng">RNGs for polynomial coefficients.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial GetPolynomial(string poly, int order, DiscreteDistribution rng)
		{
			if (string.Compare(poly, RANDOM_POL_POISSON) == 0) return RandomPolynomial(order, rng);

			throw new NotImplementedException("Unrecognized polynomial name: " + poly);
		}

		#region - Orthogonal polynomials. -
		/// <summary>
		/// [molecule]
		/// 
		/// Creates Chebyshev polynomial of the 1st kind.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial ChebyshevPolynomial(int order)
		{
			var res = new Polynomial(order);

			if (order > 1)
			{
				var polx2 = new Polynomial(new double[] { 0, 2 });
				var pol1 = ChebyshevPolynomial(order - 1) * polx2;
				res = pol1 - ChebyshevPolynomial(order - 2);
			}
			else if (order == 1)
			{
				res[0] = 0;
				res[1] = 1;
			}
			else if (order == 0)
			{
				res[0] = 1;
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates Chebyshev polynomial of the 2nd kind.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial ChebyshevPolynomial2(int order)
		{
			var res = new Polynomial(order);

			if (order > 1)
			{
				var polx2 = new Polynomial(new double[] { 0, 2 });
				var pol1 = ChebyshevPolynomial2(order - 1) * polx2;
				res = pol1 - ChebyshevPolynomial2(order - 2);
			}
			else if (order == 1)
			{
				res[0] = 0;
				res[1] = 2;
			}
			else if (order == 0)
			{
				res[0] = 1;
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates Laguerre polynomial of the 2nd kind.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial LaguerrePolynomial(int order)
		{
			var res = new Polynomial(order);

			if (order > 1)
			{
				var polx2 = new Polynomial(new double[] { 2 * (order - 1) + 1, -1 });
				var pol1 = LaguerrePolynomial(order - 1) * polx2;
				res = pol1 - LaguerrePolynomial(order - 2) * (order - 1);
				res *= order;
			}
			else if (order == 1)
			{
				res[0] = 1;
				res[1] = -1;
			}
			else if (order == 0)
			{
				res[0] = 1;
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns polynomials composed from the basis Bernstein polynomials with random weights.
		/// The resulting polynomial coefficients are defined using the following representation:
		/// 
		///  B(t) = [1 t t^2 ... t^n] [B] [c_0 c_1 ... c_n]^T
		/// 
		/// where c_i -- coefficients for the linear combination of Bernstein's basis polynomials;
		/// [B] -- is a lower triangle matrix of the Bernstein polynomial coefficients in the power basis expansion.
		///	(see Kenneth I. Joy "BERNSTEIN POLYNOMIALS", pages 8 and 12-13)
		/// 
		/// </summary>
		/// <param name="order">Required polynomial order.</param>
		/// <param name="distr">Random distribution for basis polynomials' coefficients.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial BernsteinPolynomial(int order, RandomDistribution distr)
		{
			var size = order + 1;

			//
			// form randomized vector of coefficients for linear combination of Bernstein's basis polynomials.
			var c = new float[size];
			for (int i = 0; i < size; i++) { c[i] = distr.NextRandom(); }

			//
			// Calculate [B] column-by-column.
			var pascal = Numerics.PascalTriangle(size);
			var b = new float[size, size];
			b[0, 0] = 1;
			for (int j = 0; j < size; j++)
			{
				var sign = 1;
				for (int i = j; i < size; i++, sign *= -1)
				{
					b[i, j] = sign * pascal[order, i] * pascal[i, j];
				}
			}

			//
			// Calculate resulting polynomial coefficients.
			var mul = MatrixMath.Mul(b, c);
			var coefs = VectorMath.ConvertToDoubles(mul);
			var res = new Polynomial(coefs);

			return res;
		}
		#endregion

		/// <summary>
		/// [molecule]
		/// 
		/// Generates polynomial of the required order with coefficients values taken from the standart Gaussian distribution.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial RandomGaussianPolynomial(int order)
		{
			var res = new Polynomial(order);

			var rnd = new NormalDistribution(0, 1);

			for (int i = 0; i <= order; i++)
			{
				res[i] = rnd.NextDouble();
			}

			return res;
		}

		#region - Random polynomials with arbitrary distribution. -
		/// <summary>
		/// [molecule]
		/// 
		/// Generates polynomial of the required order with coefficients values taken from the specified distribution.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="rnd">Continuous distribution.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial RandomPolynomial(int order, ContinuousDistribution rnd)
		{
			var res = new Polynomial(order);

			for (int i = 0; i <= order; i++)
			{
				res[i] = rnd.NextDouble();
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Generates polynomial of the required order with coefficients values taken from the specified distribution.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="rnd">Continuous distribution.</param>
		/// <returns>Random polynomial.</returns>
		public static Complex[] RandomComplexPolynomial(int order, ContinuousDistribution rnd)
		{
			var res = new Complex[order + 1];

			for (int i = 0; i <= order; i++)
			{
				res[i] = Complex.Random(rnd);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Generates polynomial of the required order with coefficients values taken from the specified distribution.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="rnd">Discrete distribution.</param>
		/// <returns>Random polynomial.</returns>
		public static Polynomial RandomPolynomial(int order, DiscreteDistribution rnd)
		{
			var res = new Polynomial(order);

			for (int i = 0; i <= order; i++)
			{
				res[i] = rnd.NextInt32();
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Generates polynomial of the required order with coefficients values taken from the specified distribution.
		/// </summary>
		/// <param name="order">Order of required polynomial.</param>
		/// <param name="rnd">Discrete distribution.</param>
		/// <returns>Random polynomial.</returns>
		public static Complex[] RandomComplexPolynomial(int order, DiscreteDistribution rnd)
		{
			var res = new Complex[order + 1];

			for (int i = 0; i <= order; i++)
			{
				res[i] = Complex.Random(rnd.RandomSource);
			}

			return res;
		}

        /// <summary>
        /// Returns random complex polynomial with complex roots within specified interval.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="rnd"></param>
        /// <param name="minRoot"></param>
        /// <param name="maxRoot"></param>
        /// <returns></returns>
        public static Complex[] RandomComplexPolynomial(int order, ContinuousDistribution rnd, float minRoot, float maxRoot, out Complex[] roots)
        {
            #region - Generate roots and make sure that their modulus are within the prescribed interval. -
            roots = new Complex[order];
            var maxModulus = 0f;
            for (int i = 0; i < order; ++i )
            {
                roots[i] = Complex.Random(rnd);
                if (maxModulus < roots[i].Modulus) { maxModulus = (float)roots[i].Modulus; }
            }
            var diffModulus = maxRoot - minRoot;
            var coef = diffModulus / maxModulus + minRoot;
            for (int i = 0; i < order; ++i )
            {
                roots[i] *= coef;
            } 
	        #endregion

			#region - Compute coefficients. -
			// compute polynomial coefficients using generated roots.
			var res = new Complex[order + 1];
			for (int i = 0; i <= order; ++i)
			{
				if (i == 0)
				{   // a_0
					res[0] = roots[0];
					for (int j = 1; j < order; ++j)
					{
						res[0] *= roots[j];
					}
					continue;
				}

				// iterate through all subsets of size [i].
				bool[] subset = Numerics.NextSubset(null, order, i);
				while (true)
				{
					var temp = new Complex();
					for (int k = 0; k < order; ++k)
					{
						if (subset[k] == true)
						{
							temp *= roots[k];
						}
					}

					res[i] += temp;
					subset = Numerics.NextSubset(subset, order, i);
					if (subset == null) break;
				}
			}
			res[order] = 1; 
			#endregion

			return res;
        }

		/// <summary>
		/// Returns random polynomial with complex roots within specified interval.
		/// Guassian distribution is used for generation of roots.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="rnd"></param>
		/// <param name="maxRoot"></param>
		/// <returns></returns>
		public static double[] RandomPolynomial(int order, float maxRoot, out Complex[] roots)
		{
			bool isEvenOrder = (order % 2 == 0);
			var maxRoot2 = 2 * maxRoot;

			var rnd = new MathNet.Numerics.Distributions.NormalDistribution(0, maxRoot / 3);
			//var rnd = new ContinuousRandomDistribution() { Distribution = distr };

			#region - Generate roots and make sure that their modulus are within the prescribed interval. -
			roots = new Complex[order];
			var maxModulus = 0f;
			for (int i = 0; i < order-1; i+=2)
			{	// create conjugate roots to ensure that pols have float coefficients.
				roots[i] = Complex.Random(rnd);
				roots[i + 1] = new Complex (roots[i].Conjugate.Real, roots[i].Conjugate.Imag);
				
				if (maxModulus < roots[i].Modulus) { maxModulus = (float)roots[i].Modulus; }
			}
			if (!isEvenOrder)
			{
				roots[order - 1] = (float)ContextRandom.NextDouble() * maxRoot2 - maxRoot;
				if (maxModulus < roots[order - 1].Modulus) { maxModulus = (float)roots[order - 1].Modulus; }
			}

			// set some random roots to be real.
			var realRootsCount = ContextRandom.Next(order);
			for (int i = 0; i < realRootsCount; i+=2 )
			{	// even though the resulting number of real roots is not obligatory [realRootsCount]
				// I don't really care since this number should be random any way.
				var idx = ContextRandom.Next(order);

				if (idx % 2 == 1)
				{	// [idx] is odd.
					roots[idx] = (float)ContextRandom.NextDouble() * maxRoot2 - 1;
					roots[idx - 1] = (float)ContextRandom.NextDouble() * maxRoot2 - 1;
					if (maxModulus < roots[idx - 1].Modulus) { maxModulus = (float)roots[idx - 1].Modulus; }
				}
				else 
				{	// [idx] is even
					if (idx < order - 1)
					{
						roots[idx] = (float)ContextRandom.NextDouble() * maxRoot2 - 1;
						roots[idx + 1] = (float)ContextRandom.NextDouble() * maxRoot2 - 1;
						if (maxModulus < roots[idx + 1].Modulus) { maxModulus = (float)roots[idx + 1].Modulus; }
					}
					else if (isEvenOrder)
					{
						roots[idx] = (float)ContextRandom.NextDouble() * maxRoot2 - 1;
						roots[idx - 1] = (float)ContextRandom.NextDouble() * maxRoot2 - 1;
						if (maxModulus < roots[idx - 1].Modulus) { maxModulus = (float)roots[idx - 1].Modulus; }
					}
					// otherwise don't do anything the root is already float.
				}
				if (maxModulus < roots[idx].Modulus) { maxModulus = (float)roots[idx].Modulus; }
			}

			if (maxModulus > maxRoot)
			{
				// fit roots into prescribed interval.
				var coef = maxRoot / maxModulus;
				for (int i = 0; i < order; ++i)
				{
					roots[i] *= coef;
				}
			}
			#endregion

			#region - Compute coefficients. -
			// compute polynomial coefficients using generated roots.
			var res = new Complex[2] {roots[0], 1};
			for (int i = 1; i < order; ++i)
			{
				var temp = new Complex[2] {roots[i], 1 };
				res = Mul(res, temp);

				//// scale [res] coefficients to avoid getting infinitelysmall values.
				//for (int j = 0; j < res.Length; ++j )
				//{
				//	res[j] *= 2;
				//}

				//if (i == 0)
				//{   // a_0
				//	res[0] = roots[0];
				//	for (int j = 1; j < order; ++j)
				//	{
				//		res[0] *= roots[j];
				//	}
				//	continue;
				//}

				//// iterate through all subsets of size [i].
				//bool[] subset = Numerics.NextSubset(null, order, i);
				//while (true)
				//{
				//	var temp = new Complex(1, 0);
				//	for (int k = 0; k < order; ++k)
				//	{
				//		if (subset[k] == true)
				//		{
				//			temp *= roots[k];
				//		}
				//	}

				//	res[i] += temp;
				//	subset = Numerics.NextSubset(subset, order, i);
				//	if (subset == null) break;
				//}
			}
			//res[order] = 1;
			#endregion

			//return res;
			return GetRealCoefficientsDouble(res);
		}

        /// <summary>
        /// Returns random polynomial with complex roots within specified interval.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="rnd"></param>
        /// <param name="minRoot"></param>
        /// <param name="maxRoot"></param>
        /// <returns></returns>
        public static float[] RandomPolynomialEx(int order, ContinuousDistribution rnd, float minRoot, float maxRoot, out Complex[] roots)
        {
            #region - Generate roots and make sure that their modulus are within the prescribed interval. -
            roots = new Complex[order];
            var maxModulus = 0f;
            for (int i = 0; i < order; ++i )
            {
				roots[i] = Complex.Random(rnd);
				if (maxModulus < roots[i].Modulus) { maxModulus = (float)roots[i].Modulus; }
            }
            var diffModulus = maxRoot - minRoot;
            var coef = diffModulus / maxModulus + minRoot;
            for (int i = 0; i < order; ++i )
            {
                roots[i] *= coef;
            } 
	        #endregion

			#region - Compute coefficients. -
			// compute polynomial coefficients using generated roots.
			var res = new Complex[order + 1];
			for (int i = 0; i <= order; ++i)
			{
				if (i == 0)
				{   // a_0
					res[0] = roots[0];
					for (int j = 1; j < order; ++j)
					{
						res[0] *= roots[j];
					}
					continue;
				}

				// iterate through all subsets of size [i].
				bool[] subset = Numerics.NextSubset(null, order, i);
				while (true)
				{
					var temp = new Complex(1, 0);
					for (int k = 0; k < order; ++k)
					{
						if (subset[k] == true)
						{
							temp *= roots[k];
						}
					}

					res[i] += temp;
					subset = Numerics.NextSubset(subset, order, i);
					if (subset == null) break;
				}
			}
			res[order] = 1; 
			#endregion

			return GetRealCoefficients(res);
        }
		#endregion

		#region - Special polynomials. -
		/// <summary>
		/// [molecule]
		/// 
		/// Creates real polynomial in the form:
		///   x^n - x^{n-1} - a_1 * x + a_0,
		/// where a_1, a_0 > 0.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Polynomial Lacunary1(int order, ContinuousDistribution distr)
		{
			var pol = RandomPolynomial(order, distr);
			for (int i = 2; i < order - 1; i++)
			{
				pol[i] = 0;
			}
			pol[order] = 1;
			pol[order - 1] = -1;
			if (pol[1] > 0) pol[1] = -pol[1];
			if (pol[0] < 0) pol[0] = -pol[0];
			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates real polynomial in the form:
		///   x^n - a_1 * x + a_0,
		/// where a_1, a_0 > 0.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Polynomial Lacunary2(int order, ContinuousDistribution distr)
		{
			var pol = RandomPolynomial(order, distr);
			for (int i = 2; i < order; i++)
			{
				pol[i] = 0;
			}
			pol[order] = 1;
			if (pol[1] > 0) pol[1] = -pol[1];
			if (pol[0] < 0) pol[0] = -pol[0];
			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial in the form:
		///   x^n - x^{n-1} - a_1 * x + a_0,
		/// where a_1 * a_0 != 0.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Lacunary3(int order, ContinuousDistribution distr)
		{
			var pol = RandomComplexPolynomial(order, distr);
			for (int i = 2; i < order - 1; i++)
			{
				pol[i] = 0;
			}
			pol[order] = 1;
			pol[order - 1] = -1;
			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial of the form:
		///   x^n - a_1 * x + a_0,
		/// where a_1 * a_0 != 0.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Lacunary4(int order, ContinuousDistribution distr)
		{
			var pol = RandomComplexPolynomial(order, distr);
			for (int i = 2; i < order; i++)
			{
				pol[i] = 0;
			}
			pol[order] = 1;
			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial where |a_i| < 1 (i=0,1....,n).
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Constrained1(int order, ContinuousDistribution distr)
		{
			var pol = RandomComplexPolynomial(order, distr);
			for (int i = 0; i <= order; i++)
			{
				var mod = pol[i].Modulus;
				if (mod >= 1)
				{
					pol[i] /= mod + 1;
				}
			}

			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial where |a_i| < 1 (i=0,1.... n-1), a_n arbitary.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Constrained2(int order, ContinuousDistribution distr)
		{
			var pol = Constrained1(order, distr);
			pol[order] = Complex.Random(distr);

			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial where |a_i| < 1 (i=0,1.... n-1), a_n and a_{n-1} arbitary.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Constrained3(int order, ContinuousDistribution distr)
		{
			var pol = Constrained2(order, distr);
			pol[order-1] = Complex.Random(distr);
			pol[order] = Complex.Random(distr);

			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial where |a_i|/|a_n| < 1 (i=0,1....,n-2).
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Constrained4(int order, ContinuousDistribution distr)
		{
			var pol = RandomComplexPolynomial(order, distr);
			var mods = VectorMath.Abs(pol);
			var idx = VectorMath.IndexOfMax(mods);

			if (idx != order)
			{	// swap two elements in [pol].
				var tmp = pol[idx];
				pol[idx] = pol[order];
				pol[order] = tmp;
			}
			return pol;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial f = f_1 * f_2,
		/// where f_1 :=a_{n1}z^n1 + a_{n1-1}z^{n1-1} + ... + a_1z + a_0
		///       f_2 :=b_{n2}z^n2 + b_{n2-1}z^{n2-1} + ... + b_1z + b_0
		/// and the following conditions are satisfied:
		/// |a_{n1}| > |a_i|; (i=0,1....,n1-1)
		/// |b_{n2}| > |a_i|; (i=0,1....,n2-1)
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Constrained5(int order, ContinuousDistribution distr)
		{
			var n1 = ContextRandom.Next(order / 2) + order / 2;
			var n2 = order - n1;

			var pol1 = Constrained4(n1, distr);
			var pol2 = Constrained4(n2, distr);
			var res = Mul(pol1, pol2);
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Creates complex polynomial f = f_1 * f_2,
		/// where f_1 :=a_{n1}z^n1 + a_{n1-1}z^{n1-1} + ... + a_1z + a_0
		///       f_2 :=b_{n2}z^n2 + b_{n2-1}z^{n2-1} + ... + b_1z + b_0
		/// </summary>
		/// <param name="order"></param>
		/// <param name="distr"></param>
		/// <returns></returns>
		public static Complex[] Multiple(int order, ContinuousDistribution distr)
		{
			var n1 = ContextRandom.Next(order / 2) + order / 2;
			var n2 = order - n1;

			var pol1 = RandomComplexPolynomial(n1, distr);
			var pol2 = RandomComplexPolynomial(n2, distr);
			var res = Mul(pol1, pol2);
			return res;
		}
		#endregion

		#region - Coefficients transform. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns complex coefficients of the given polynomial.
		/// </summary>
		/// <param name="pol">Polynomial.</param>
		/// <returns>Array of complex polynomial coefficients.</returns>
		public static Complex[] GetComplexCoefficients(Polynomial pol)
		{
			var res = new Complex[pol.Order + 1];
			for (int i = 0; i <= pol.Order; ++i)
			{
				res[i] = new Complex(pol[i], 0);
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns complex coefficients for the given polynomial.
		/// </summary>
		/// <param name="pol">Real polynomial coefficients.</param>
		/// <returns>Array of complex polynomial coefficients.</returns>
		public static Complex[] GetComplexCoefficients(float[] pol)
		{
			var res = new Complex[pol.Length];
			for (int i = 0; i < pol.Length; ++i)
			{
				res[i].Real = pol[i];
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns real parts of complex coefficients of the given polynomial.
		/// </summary>
		/// <param name="pol">Complex polynomial coefficients.</param>
		/// <returns>Array of real parts of polynomial coefficients.</returns>
		public static float[] GetRealCoefficients(Complex[] pol)
		{
			var res = new float[pol.Length];
			for (int i = 0; i < pol.Length; ++i)
			{
				res[i] = (float)pol[i].Real;
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns real parts of complex coefficients of the given polynomial as array of doubles.
		/// </summary>
		/// <param name="pol">Complex polynomial coefficients.</param>
		/// <returns>Array of real parts of polynomial coefficients.</returns>
		public static double[] GetRealCoefficientsDouble(Complex[] pol)
		{
			var res = new double[pol.Length];
			for (int i = 0; i < pol.Length; ++i)
			{
				res[i] = (float)pol[i].Real;
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns real parts of complex coefficients of the given polynomial.
		/// </summary>
		/// <param name="pol">Complex polynomial coefficients.</param>
		/// <returns>Array of real parts of polynomial coefficients.</returns>
		public static float[] GetCoefficients(Polynomial pol)
		{
			var res = new float[pol.Order + 1];
			for (int i = 0; i <= pol.Order; ++i)
			{
				res[i] = (float)pol[i];
			}
			return res;
		} 
		#endregion

		#region - Polynomial operations. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs multiplication of two complex polynomials given their coefficients.
		/// It's assumed that vectors of coefficients are passed as non-sparse vectors (even if they have a lot
		/// of zero elements, all these zero coefficients should be passed explicitely).
		/// 
		/// The result is calculated using quadratic form:
		///   (x^T a) (b y^T) = \sum_{ij}{c_{ij} x_i y_j},
		/// where x and y are vectors of elements: {x^0, x^1, ..., x^n}, c_{ij} = a_i * b_j.
		///  Notice, that x and y can be of different orders.
		/// </summary>
		/// <param name="a">First polynomial coefficients.</param>
		/// <param name="b">Second polynomial coefficients.</param>
		/// <returns>Coefficients of the resulting polynomial.</returns>
		public static Complex[] Mul (Complex[] a, Complex[] b)
		{
			var n1 = a.Length - 1;
			var n2 = b.Length - 1;
			var c = new Complex[n1 + n2 + 1];	// +1 to take into consideration c_0.

			for (int i = 0; i <= n1; i++)
			{
				for (int j = 0; j <= n2; j++)
				{
					c[i + j] += a[i]*b[j];
				}
			}

			return c;
		}
		#endregion
	}

	public abstract class RandomDistribution
	{
		public abstract float NextRandom();
		public abstract string GetParametersString();
	}

	public class DiscreteRandomDistribution : RandomDistribution
	{
		public DiscreteDistribution Distribution { get; set; }
		public override float NextRandom() {return Distribution.NextInt32(); }
		public override string GetParametersString()
		{
			const string fString = "mean:\t{0}\tvariance:\t{1}";
			return string.Format(fString, Distribution.Mean, Distribution.Variance);
		}
	}

	public class ContinuousRandomDistribution : RandomDistribution
	{
		public ContinuousDistribution Distribution { get; set; }
		public override float NextRandom() { return (float)Distribution.NextDouble(); }
		public override string GetParametersString()
		{
			const string fString = "mean:\t{0}\tvariance:\t{1}";
			return string.Format(fString, Distribution.Mean, Distribution.Variance);
		}
	}

	public static class PolynomialRootsBounds
	{
		// classical bounds.
		public const string JOYAL_BOUNDS = "Joyal, Th. (2)";
		public const string MOHAMMAD_BOUNDS = "Mohammad, Th. (3)";
		public const string KOJIMA_BOUNDS = "Koijma, Th. (4)";
		public const string JAIN_BOUNDS = "Jain, Th. (5)";
		public const string CAUCHY_BOUNDS_1 = "Cauchy, Th. (11)";
		public const string CAUCHY_BOUNDS_2 = "Cauchy, Th. (1)";
		public const string POWER_BOUNDS = "Kuniyeda, Th. (6)";
		public const string KUNIYEDA_BOUNDS = "Kuniyeda, Th. (7)";
		public const string MARDEN_BOUNDS = "Joyal, Th. (8)";
		// new bounds.
		public const string DEHMER_BOUNDS_14 = "Dehmer, Th. (14)";	// former "Dehmer, Th. (12)"
		public const string DEHMER_BOUNDS_15 = "Dehmer, Th. (15)";	// former "Dehmer, Th. (13)"
		public const string DEHMER_BOUNDS_16 = "Dehmer, Th. (16)";	// former "Dehmer, Th. (14)"
		public const string DEHMER_BOUNDS_9 = "Dehmer, Th. (9)";	// former "Dehmer, Th. (9)"
		public const string DEHMER_BOUNDS_10 = "Dehmer, Th. (10)";	// former "Dehmer, Th. (10)"
		public const string KALANTARI_BOUNDS_44 = "Kalantari, Th. (11)";	// former "Kalantari, Cor. (4.4)"
		public const string KALANTARI_BOUNDS_45 = "Kalantari, Th. (12)";	// former "Kalantari, Cor. (4.5)"
		// new bounds for lacunary polynomials.
		public const string DEHMER_BOUNDS_LACUNARY_1 = "Dehmer Bounds Lacunary (1)";
		public const string DEHMER_BOUNDS_LACUNARY_3 = "Dehmer Bounds Lacunary (3)";
		public const string DEHMER_BOUNDS_LACUNARY_5 = "Dehmer Bounds Lacunary (5)";
		public const string DEHMER_BOUNDS_LACUNARY_6 = "Dehmer Bounds Lacunary (6)";
		public const string DEHMER_BOUNDS_LACUNARY_7 = "Dehmer Bounds Lacunary, Th. (?)";	//"Dehmer Bounds Lacunary (7)";
		public const string DEHMER_BOUNDS_LACUNARY_8 = "Dehmer Bounds Lacunary, Th. (?)";
		public const string DEHMER_BOUNDS_LACUNARY_17 = "Dehmer, Th. (17)";
		public const string DEHMER_BOUNDS_LACUNARY_18 = "Dehmer, Th. (18)";
		public const string DEHMER_BOUNDS_LACUNARY_19 = "Dehmer, Th. (19)";

		#region - Generic methods. -
		/// <summary>
		/// [molecule]
		/// 
		/// Computes given bounds for polynomial roots with specified coefficients.
		/// </summary>
		/// <param name="bounds">List of bounds to compute.</param>
		/// <param name="ra">Real polynomial coefs.</param>
		/// <param name="ca">Complex polynomial coefs.</param>
		/// <param name="p">Metrics parameter.</param>
		/// <returns></returns>
		public static Dictionary<string, float> ComputeBounds(List<string> bounds, float[] ra, Complex[] ca, float p)
		{
			var boundValues = new Dictionary<string, float>();

			foreach (var bound in bounds)
			{
				var b = GetPolynomialRootsBounds(bound, ca, ra, p);
				boundValues.Add(bound, b);
			}

			return boundValues;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes all available bounds for polynomial roots.
		/// </summary>
		/// <param name="ra">Real polynomial coefs.</param>
		/// <param name="ca">Complex polynomial coefs.</param>
		/// <param name="p">Metrics parameter.</param>
		/// <returns></returns>
		public static Dictionary<string, float> ComputeBounds(float[] ra, Complex[] ca, float p)
		{
			var bounds = GetClassicalPolynomialRootsBounds();
			var newBounds = GetNewPolynomialRootsBounds();
			var newLacBounds = GetNewLacunaryPolynomialRootsBounds();
			var boundValues = new Dictionary<string, float>();

			foreach (var bound in bounds)
			{
				var b = GetPolynomialRootsBounds(bound, ca, ra, p);
				boundValues.Add(bound, b);
			}
			foreach (var bound in newBounds)
			{
				var b = GetPolynomialRootsBounds(bound, ca, ra, p);
				boundValues.Add(bound, b);
			}
			foreach (var bound in newLacBounds)
			{
				var b = GetPolynomialRootsBounds(bound, ca, ra, p);
				boundValues.Add(bound, b);
			}

			return boundValues;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes bounds, obtained using general case methods, for polynomial roots.
		/// </summary>
		/// <param name="ra">Real polynomial coefs.</param>
		/// <param name="ca">Complex polynomial coefs.</param>
		/// <param name="p">Metrics parameter.</param>
		/// <returns></returns>
		public static Dictionary<string, float> ComputeBoundsGeneralPolynomials(float[] ra, Complex[] ca, float p)
		{
			var bounds = GetGeneralCaseBounds();
			var boundValues = new Dictionary<string, float>();

			foreach (var bound in bounds)
			{
				var b = GetPolynomialRootsBounds(bound, ca, ra, p);
				boundValues.Add(bound, b);
			}

			return boundValues;
		}

		public static string[] GetGeneralCaseBounds()
		{
			var res = new List<string>();
			res.Add(JOYAL_BOUNDS);
			res.Add(MOHAMMAD_BOUNDS);
			res.Add(KOJIMA_BOUNDS);
			res.Add(JAIN_BOUNDS);
			res.Add(CAUCHY_BOUNDS_1);
			res.Add(CAUCHY_BOUNDS_2);
			res.Add(POWER_BOUNDS);
			res.Add(KUNIYEDA_BOUNDS);
			res.Add(MARDEN_BOUNDS);
			res.Add(DEHMER_BOUNDS_14);
			res.Add(DEHMER_BOUNDS_15);
			res.Add(DEHMER_BOUNDS_16);
			res.Add(DEHMER_BOUNDS_9);
			res.Add(KALANTARI_BOUNDS_44);
			res.Add(KALANTARI_BOUNDS_45);
			return res.ToArray();
		}

		public static string[] GetClassicalPolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(JOYAL_BOUNDS);
			res.Add(MOHAMMAD_BOUNDS);
			res.Add(KOJIMA_BOUNDS);
			res.Add(JAIN_BOUNDS);
			res.Add(CAUCHY_BOUNDS_1);
			res.Add(CAUCHY_BOUNDS_2);
			res.Add(POWER_BOUNDS);
			res.Add(KUNIYEDA_BOUNDS);
			res.Add(MARDEN_BOUNDS);
			return res.ToArray();
		}

		public static string[] GetNewPolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(DEHMER_BOUNDS_14);
			res.Add(DEHMER_BOUNDS_15);
			res.Add(DEHMER_BOUNDS_16);
			res.Add(DEHMER_BOUNDS_9);
			res.Add(DEHMER_BOUNDS_10);
			res.Add(KALANTARI_BOUNDS_44);
			res.Add(KALANTARI_BOUNDS_45);
			return res.ToArray();
		}

		public static string[] GetNewLacunaryPolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(DEHMER_BOUNDS_LACUNARY_1);
			res.Add(DEHMER_BOUNDS_LACUNARY_3);
			res.Add(DEHMER_BOUNDS_LACUNARY_5);
			res.Add(DEHMER_BOUNDS_LACUNARY_6);
			res.Add(DEHMER_BOUNDS_LACUNARY_7);
			res.Add(DEHMER_BOUNDS_LACUNARY_8);
			res.Add(DEHMER_BOUNDS_LACUNARY_17);
			res.Add(DEHMER_BOUNDS_LACUNARY_18);
			res.Add(DEHMER_BOUNDS_LACUNARY_19);
			return res.ToArray();
		}

		public static string[] GetNewLacunary1PolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(DEHMER_BOUNDS_LACUNARY_1);
			res.Add(DEHMER_BOUNDS_LACUNARY_5);
			res.Add(DEHMER_BOUNDS_LACUNARY_6);
			res.Add(DEHMER_BOUNDS_LACUNARY_7);
			res.Add(DEHMER_BOUNDS_LACUNARY_8);
			return res.ToArray();
		}

		public static string[] GetNewLacunary2PolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(DEHMER_BOUNDS_LACUNARY_3);
			res.Add(DEHMER_BOUNDS_LACUNARY_17);
			res.Add(DEHMER_BOUNDS_LACUNARY_18);
			res.Add(DEHMER_BOUNDS_LACUNARY_19);
			return res.ToArray();
		}

		public static string[] GetNewLacunary3PolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(DEHMER_BOUNDS_LACUNARY_1);
			res.Add(DEHMER_BOUNDS_LACUNARY_5);
			res.Add(DEHMER_BOUNDS_LACUNARY_6);
			res.Add(DEHMER_BOUNDS_LACUNARY_7);
			res.Add(DEHMER_BOUNDS_LACUNARY_8);
			return res.ToArray();
		}

		public static string[] GetNewLacunary4PolynomialRootsBounds()
		{
			var res = new List<string>();
			res.Add(DEHMER_BOUNDS_LACUNARY_3);
			res.Add(DEHMER_BOUNDS_LACUNARY_17);
			res.Add(DEHMER_BOUNDS_LACUNARY_18);
			res.Add(DEHMER_BOUNDS_LACUNARY_19);
			return res.ToArray();
		}

		public static float GetPolynomialRootsBounds(string name, Complex[] ca, float[] ra, float p)
		{
			if (name == POWER_BOUNDS) return Numerics.PowerBounds(ca, p);
			if (name == KUNIYEDA_BOUNDS) return Numerics.KuniyedaBounds(ca, p);
			if (name == MARDEN_BOUNDS) return Numerics.MardenBounds(ca, p);
			if (name == JOYAL_BOUNDS) return Numerics.JoyalBounds(ca);
			if (name == MOHAMMAD_BOUNDS) return Numerics.MohammadBounds(ca);
			if (name == KOJIMA_BOUNDS) return Numerics.KojimaBounds(ca);
			if (name == KALANTARI_BOUNDS_44) return Numerics.Kalantari44(ca);
			if (name == KALANTARI_BOUNDS_45) return Numerics.Kalantari45(ca);
			if (name == JAIN_BOUNDS) return Numerics.JainBounds(ca);
			if (name == CAUCHY_BOUNDS_1) return Numerics.CauchyBounds1(ca);
			if (name == CAUCHY_BOUNDS_2) return Numerics.CauchyBounds2(ca);
			if (name == DEHMER_BOUNDS_14) return Numerics.DehmerBounds1(ca);
			if (name == DEHMER_BOUNDS_15) return Numerics.DehmerBounds2(ca);
			if (name == DEHMER_BOUNDS_16) return Numerics.DehmerBounds3(ca);
			if (name == DEHMER_BOUNDS_9) return Numerics.DehmerBounds4(ca);
			if (name == DEHMER_BOUNDS_10) return Numerics.DehmerBounds5(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_1) return Numerics.DehmerBoundsLacunary1(ra);
			if (name == DEHMER_BOUNDS_LACUNARY_3) return Numerics.DehmerBoundsLacunary3(ra);
			if (name == DEHMER_BOUNDS_LACUNARY_5) return Numerics.DehmerBoundsLacunary5(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_6) return Numerics.DehmerBoundsLacunary6(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_7) return Numerics.DehmerBoundsLacunary7(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_8) return Numerics.DehmerBoundsLacunary8(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_17) return Numerics.DehmerBoundsLacunary9(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_18) return Numerics.DehmerBoundsLacunary10(ca);
			if (name == DEHMER_BOUNDS_LACUNARY_19) return Numerics.DehmerBoundsLacunary11(ca);

			throw new Exception(string.Format("Unknown polynomial roots bounds: {0}", name));
		}

		public static float GetPolynomialRootsBounds(string name, Complex[] a, float p)
		{
			if (name == POWER_BOUNDS) return Numerics.PowerBounds(a, p);
			if (name == KUNIYEDA_BOUNDS) return Numerics.KuniyedaBounds(a, p);
			if (name == MARDEN_BOUNDS) return Numerics.MardenBounds(a, p);

			throw new Exception(string.Format("Unknown complex polynomial roots bounds: {0}", name));
		}

		public static float GetPolynomialRootsBounds(string name, Complex[] a)
		{
			if (name == JOYAL_BOUNDS) return Numerics.JoyalBounds(a);
			if (name == MOHAMMAD_BOUNDS) return Numerics.MohammadBounds(a);
			if (name == KOJIMA_BOUNDS) return Numerics.KojimaBounds(a);
			if (name == JAIN_BOUNDS) return Numerics.JainBounds(a);
			if (name == CAUCHY_BOUNDS_1) return Numerics.CauchyBounds1(a);
			if (name == CAUCHY_BOUNDS_2) return Numerics.CauchyBounds2(a);
			if (name == DEHMER_BOUNDS_14) return Numerics.DehmerBounds1(a);
			if (name == DEHMER_BOUNDS_15) return Numerics.DehmerBounds2(a);
			if (name == DEHMER_BOUNDS_16) return Numerics.DehmerBounds3(a);
			if (name == DEHMER_BOUNDS_9) return Numerics.DehmerBounds4(a);
			if (name == DEHMER_BOUNDS_10) return Numerics.DehmerBounds5(a);

			throw new Exception(string.Format("Unknown complex polynomial roots bounds: {0}", name));
		}

		public static float GetLacunaryPolynomialRootsBounds(string name, float[] a)
		{
			if (name == DEHMER_BOUNDS_LACUNARY_1) return Numerics.DehmerBoundsLacunary1(a);
			if (name == DEHMER_BOUNDS_LACUNARY_3) return Numerics.DehmerBoundsLacunary3(a);

			throw new Exception(string.Format("Unknown real lacunary polynomial roots bounds: {0}", name));
		}

		public static float GetLacunaryPolynomialRootsBounds(string name, Complex[] a)
		{
			if (name == DEHMER_BOUNDS_LACUNARY_5) return Numerics.DehmerBoundsLacunary5(a);
			if (name == DEHMER_BOUNDS_LACUNARY_6) return Numerics.DehmerBoundsLacunary6(a);
			if (name == DEHMER_BOUNDS_LACUNARY_7) return Numerics.DehmerBoundsLacunary7(a);
			if (name == DEHMER_BOUNDS_LACUNARY_8) return Numerics.DehmerBoundsLacunary8(a);
			if (name == DEHMER_BOUNDS_LACUNARY_17) return Numerics.DehmerBoundsLacunary9(a);
			if (name == DEHMER_BOUNDS_LACUNARY_18) return Numerics.DehmerBoundsLacunary10(a);
			if (name == DEHMER_BOUNDS_LACUNARY_19) return Numerics.DehmerBoundsLacunary11(a);

			throw new Exception(string.Format("Unknown complex lacunary polynomial roots bounds: {0}", name));
		}
		#endregion
	}
}
