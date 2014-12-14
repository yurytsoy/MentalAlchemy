using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MentalAlchemy.Atoms
{
	#region - Histograms. -
	[Serializable]
	public class HistogramInt : List<int>
	{
		private const float MIN_VALUE = 0.0f;

		public int BinsCount
		{
			get { return Count; }
			set
			{
				Clear();
				AddRange(new int[value]);
				for (int i = 0; i < value; ++i) { this[i] = 0; }	// reset to zero.
			}
		}
		public float BinSize { get; set; }

		public void CountValue(float value)
		{
			int binIndex = (int)((value - MIN_VALUE) / BinSize);
			if (binIndex < 0) { binIndex = 0; }
			if (binIndex >= BinsCount) { binIndex = BinsCount - 1; }

			++this[binIndex];
		}

		public void CountValues(float[] values)
		{
			foreach (var value in values)
			{
				CountValue(value);
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of all counts.
		/// </summary>
		/// <returns>Sum of all counts.</returns>
		public int GetSum ()
		{
			int sum = 0;
			for (int i = 0; i < Count; ++i)
			{
				sum += this[i];
			}
			return sum;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate Euclidian distance between histogram counts.
		/// </summary>
		/// <param name="hist">Reference histogram.</param>
		/// <returns>Euclidian distance value.</returns>
		public float GetDistance(HistogramInt hist)
		{
			if (Count != hist.Count) { return float.PositiveInfinity; }

			float dist = 0.0f;
			for (int i = 0; i < Count; ++i)
			{
				float temp = this[i] - hist[i];
				dist += temp * temp;
			}

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts histogram to dictionary so that key = histogram bin value, and value = histogram count.
		/// </summary>
		/// <returns>Dictionary containing bins' values and counts.</returns>
		public Dictionary<float, int> ConvertToDictionary()
		{
			var res = new Dictionary<float, int>();

			var key = MIN_VALUE;
			for (int i = 0; i < BinsCount; i++, key += BinSize)
			{
				res.Add(key, this[i]);
			}

			return res;
		}

		public static int GetCountIndex(float value, int binsCount, float binSize)
		{
			int binIndex = (int)((value - MIN_VALUE) / binSize);
			if (binIndex < 0) { binIndex = 0; }
			if (binIndex >= binsCount) { binIndex = binsCount - 1; }

			return binIndex;
		}

		public static HistogramFloat Normalize(HistogramInt hist)
		{
			float sum = hist.GetSum();

			var res = new HistogramFloat();
			if (sum != 0.0f)
			{
				sum = 1.0f / sum;
				foreach (int count in hist)
				{
					res.Add(count * sum);
				}
			}
			else
			{
				res.AddRange(new float[hist.Count]);
			}

			return res;
		}

		public static float Entropy(HistogramInt hist)
		{
			HistogramFloat dHist = Normalize(hist);

			float res = 0.0f;
			foreach (var p in dHist)
			{
				if (p != 0.0)
				{
					res -= (float)(p*Math.Log(p));
				}
			}

			return res;
		}
	}

	[Serializable]
	public class HistogramFloat : List<float>
	{
		private const float MIN_VALUE = 0.0f;

		public int BinsCount
		{
			get { return Count; }
			set
			{
				Clear();
				AddRange(new float[value]);
				for (int i = 0; i < value; ++i) { this[i] = 0.0f; }	// reset to zero.
			}
		}
		public float BinSize { get; set; }

		public void CountValue(float value)
		{
			int binIndex = (int)((value - MIN_VALUE) / BinSize);
			if (binIndex < 0) { binIndex = 0; }
			if (binIndex >= BinsCount) { binIndex = BinsCount - 1; }

			this[binIndex] += 1.0f;
		}

		public void CountValues (float[] values)
		{
			foreach (var value in values)
			{
				CountValue(value);
			}
		}

		public float GetSum()
		{
			float sum = 0;
			for (int i = 0; i < Count; ++i)
			{
				sum += this[i];
			}
			return sum;
		}

		public float GetDistance(HistogramInt hist)
		{
			if (Count != hist.Count) { return float.PositiveInfinity; }

			float dist = 0.0f;
			for (int i = 0; i < Count; ++i)
			{
				float temp = this[i] - hist[i];
				dist += temp * temp;
			}

			return (float)Math.Sqrt(dist);
		}

		public static int GetCountIndex(float value, int binsCount, float binSize)
		{
			int binIndex = (int)((value - MIN_VALUE) / binSize);
			if (binIndex < 0) { binIndex = 0; }
			if (binIndex >= binsCount) { binIndex = binsCount - 1; }

			return binIndex;
		}
	}
	#endregion

	#region - Delegate. -
	/// <summary>
	/// [atomic]
	/// 
	/// Delegate for calculation a specific distance measure between two 2D arrays of floats.
	/// </summary>
	/// <param name="agr1">1-st argument.</param>
	/// <param name="agr2">2-nd argument.</param>
	/// <returns>Calculated distance.</returns>
	public delegate float DistanceMeasure2D(float[,] agr1, float[,] agr2);

	/// <summary>
	/// [atomic]
	/// 
	/// Delegate for calculation a specific distance measure between two 1D arrays of floats.
	/// </summary>
	/// <param name="agr1">1-st argument.</param>
	/// <param name="agr2">2-nd argument.</param>
	/// <returns>Calculated distance.</returns>
	public delegate float DistanceMeasure1D(float[] agr1, float[] agr2);
	#endregion

	#region - Enumerations. -
	/// <summary>
	/// [atomic]
	/// 
	/// Enumeration to denote locations of movable points for the bounding rectangle.
	/// </summary>
	public enum BoundingRectPoint { Undefined, UpperLeft, UpperRight, LowerLeft, LowerRight, VerticalResizeUpper, VerticalResizeLower, HorizontalResizeRight, HorizontalResizeLeft, All }
	#endregion

	#region - Interface. -

	/// <summary>
	/// General interface for functions that accepts vectors
	/// and produce result also as a vector.
	/// </summary>
	public interface IVectorFunction : ICloneable
	{
		/// <summary>
		/// Returns vector of output signals for ANN.
		/// </summary>
		/// <param name="outputs">Resulting output signals array.</param>
		void GetOutputs(out float[] outputs);

		/// <summary>
		/// Calculates network output.
		/// </summary>
		/// <param name="inputs">Input signals.</param>
		void Calculate(float[] inputs);
	}
	#endregion

	#region - Classes. -
	/// <summary>
	/// [atomic]
	/// 
	/// Class to represent point in 3D coordinates.
	/// </summary>
	public class Point3D
	{
		public float X;
		public float Y;
		public float Z;
	}

	/// <summary>
	/// Class for recognizing systems, containing class name, description, and id.
	/// </summary>
	public class ClassInfo
	{
		public static string Name = "";
		public static string Description = "";
		public static int ClassId = -1;
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Inner structure for the quadruple (vector; probability; counts; weight)
	/// </summary>
	[Serializable]
	public struct VectorProb
	{
		public float[] vector;
		public Dictionary<int, float> probs;
		public Dictionary<int, int> counts;
		public float weight;

		public bool IsEmpty()
		{
			return vector == null;
		}

		/// <summary>
		/// Initializes current [VectorProb] object with the given vector.
		/// </summary>
		/// <param name="v">Init vector.</param>
		public void Init(float[] v)
		{
			vector = v;
			probs = new Dictionary<int, float>();
			counts = new Dictionary<int, int>();
			weight = 1;
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Class to represent random numbers generator without necessity to reinitialize it.
	/// </summary>
	public class ContextRandom
	{
		public static Random rand = new Random();
		public static double NextDouble() { return rand.NextDouble(); }
		public static int Next() { return rand.Next(); }
		public static int Next(int max) { return rand.Next(max); }
		public static int Next(int min, int max) { return rand.Next(min, max); }
		public static bool NextBoolean() { return rand.Next(-1000, 1000) >= 0; }
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Class to represent brief description of the experiment.
	/// </summary>
	[Serializable]
	public class ExperimentDescription
	{
		public DateTime DateTimeBegin;
		public DateTime DateTimeEnd;
		public string StudyGoal;
		public string ProblemName;
		public int RunsCount;

		public List<string> ToStrings()
		{
			var res = new List<string>();
			res.Add("Beginning:\t" + DateTimeBegin.ToShortDateString() + "\t" + DateTimeBegin.ToLongTimeString());
			res.Add("End:\t" + DateTimeEnd.ToShortDateString() + "\t" + DateTimeEnd.ToLongTimeString());
			res.Add("Aim of the study:\t" + StudyGoal);
			res.Add("Problem name:\t" + ProblemName);
			res.Add("Runs count:\t" + RunsCount);
			return res;
		}
	}


	/// <summary>
	/// Class to represent node on 2d plane.
	/// The node can have weighted connections with weights defined by [W] property.
	/// Also this class allows deactivation of the node by toggling its [IsActive] property.
	/// </summary>
	[Serializable]
	public class Node2D
	{
		/// <summary>
		/// Weights vector.
		/// </summary>
		public List<float> W { get; set; }

		/// <summary>
		/// Node's coordinates on 2d plane.
		/// </summary>
		public PointF Coord { get; set; }

		/// <summary>
		/// Indicates whether node is active or not.
		/// </summary>
		public bool IsActive { get; set; }
	}

	/// <summary>
	/// [atomic]
	/// 
	/// Class to represent basic weighted edge.
	/// </summary>
	[Serializable]
	public class Edge
	{
		public int BeginIdx { get; set; }
		public int EndIdx { get; set; }
		public float Weight { get; set; }
		public bool Enabled { get; set; }

		#region - Construction. -
		public Edge() { }
		public Edge(int begin, int end, float w)
		{
			BeginIdx = begin;
			EndIdx = end;
			Weight = w;
			Enabled = true;
		}
		public Edge(Edge edge)
		{
			BeginIdx = edge.BeginIdx;
			EndIdx = edge.EndIdx;
			Weight = edge.Weight;
			Enabled = edge.Enabled;
		}
		#endregion

		public Edge Clone() { return new Edge(this); }

		/// <summary>
		/// Returns [true] if two edges have the same direction regardless to their weights.
		/// </summary>
		/// <param name="edge">Edge to compare.</param>
		/// <returns>[true] if two edges have the same direction regardless to their weights and [false] otherwise.</returns>
		public bool EqualDirection(Edge edge)
		{
			return ((BeginIdx == edge.BeginIdx) && (EndIdx == edge.EndIdx));
		}
	}

	/// <summary>
	/// Class to represent parameters for 2D Gaussian Envelope which is used for signal/image processing.
	/// 
	/// The parameters usage in Gaussian calculation (from the 'Tutorial on Gabor Filters' by Javier R. Movellan):
	/// 
	///		G(x,y) = Magnitude \exp{ -\pi (ScaleX^2 (x-Center.X)_r^2 + ScaleY^2 (y-Center.Y)_r^2)}
	/// 
	/// where
	/// 
	///		(x-Center.X)_r = (x-Center.X) \cos{Angle} + (y-Center.Y) \sin{Angle}
	///		(y-Center.Y)_r = -(x-Center.X) \sin{Angle} + (y-Center.Y) \cos{Angle}
	/// 
	/// </summary>
	[Serializable]
	public class GaussianEnvelopeParameters
	{
		public PointF Center = PointF.Empty;	// Gaussian center.
		public float Angle = 0.0f;		// Rotation value for Gaussian.
		public float ScaleXSqr = 1.0f;	// Squared(!) scale along X axis.
		public float ScaleYSqr = 1.0f;	// Squared(!) scale along Y axis.
		public float Magnitude = 1.0f;	// Magnitude value.
	}

	/// <summary>
	/// [atomic]
	/// 
	/// Class to represent directed graphs.
	/// </summary>
	[Serializable]
	public class Graph
	{
		/// <summary>
		/// Graph edges.
		/// </summary>
		public List<Edge> Edges { get; set; }

		/// <summary>
		/// Adjacency matrix of the graph.
		/// Needs checking for null value!
		/// </summary>
		public float[][] AdjacencyMatrix { get; set; }
	}

	/// <summary>
	/// Structure to represent basic statistic measures.
	/// </summary>
	[Serializable]
	public class Stats
	{
		public float Min;
		public float Max;
		public float Mean;
		public float Median;
		public float Variance;
		public float Total;

		public List<float> Data = new List<float>();	// additional data.

		public string Header = "Min\tMax\tMean\tVariance\tMedian\tTotal";

		/// <summary>
		/// Returns header, which enumerates stats names in the order, correspondent to stats output in the [Write] method.
		/// </summary>
		/// <returns>Header string.</returns>
		public string GetStatsHeader() { return Header; }

		/// <summary>
		/// Adds custom data value to the stats.
		/// </summary>
		/// <param name="value">Value to append.</param>
		/// <param name="name">Data name.</param>
		public void AppendData(float value, string name)
		{
			Data.Add(value);
			Header += "\t" + name;
		}

		/// <summary>
		/// Converts given [Stats] variable into string representation with values separated by tabs.
		/// </summary>
		/// <returns>String representation of the stats.</returns>
		public string GetStatsString()
		{
			string res = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", Min, Max, Mean, Variance, Median, Total);
			for (int i = 0; i < Data.Count; i++)
			{
				res += "\t" + Data[i];
			}
			return res;
		}
	}

	/// <summary>
	/// Class to represent 2D line.
	/// </summary>
	[Serializable]
	public class Line
	{
		public PointF Begin { get; set; }
		public PointF End { get; set; }
	}
	#endregion
}