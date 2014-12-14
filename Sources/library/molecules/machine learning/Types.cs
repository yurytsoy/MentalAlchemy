using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Class, representing training sample for classification and recognition problems.
	/// </summary>
	[Serializable]
	public class TrainingSample
	{
		/// <summary>
		/// Sample's name.
		/// </summary>
		public string Name;

		/// <summary>
		/// Input vector.
		/// </summary>
		public float[,] Data { get; set; }

		/// <summary>
		/// Desired response.
		/// </summary>
		public float[,] Response { get; set; }

		/// <summary>
		/// ID of the class.
		/// </summary>
		public int ClassID { get; set; }
	}


	/// <summary>
	/// [molecule]
	/// 
	/// Class to store fitness value and some optional additional information.
	/// </summary>
	[Serializable]
	public class Fitness
	{
		#region - Public properties. -
		public float Value { get; set; }
		public List<float> Extra { get; set; }
		#endregion

		#region - Construction. -
		public Fitness()
		{
			Extra = new List<float>();
		}
		public Fitness(Fitness argFitness)
		{
			Assign(argFitness);
		}
		public Fitness(float val)
		{
			Value = val;
			Extra = new List<float>();
		}
		#endregion

		#region - Public methods. -
		public Fitness Clone() { return new Fitness(this); }

		#region - Assignment. -
		public Fitness Assign(Fitness argFitness)
		{
			Value = argFitness.Value;
			Extra = new List<float>(argFitness.Extra);
			return this;
		}

		public Fitness Assign(float argD)
		{
			Value = argD;
			Extra.Clear();
			return this;
		}
		#endregion

		#region - Arithmetics. -
		public static Fitness operator -(Fitness arg1, Fitness arg2)
		{
			var res = new Fitness();
			res.Value = arg1.Value - arg2.Value;

			var size = arg1.Extra.Count;
			res.Extra = new List<float>(size);
			for (int i = 0; i < size; i++)
			{
				res.Extra.Add(arg1.Extra[i] - arg2.Extra[i]);
			}

			//res.Extra = new List<float>(VectorMath.Sub(arg1.Extra.ToArray(), arg2.Extra.ToArray()));
			return res;
		}
		#endregion

		#region - Comparison. -
		public bool Equals(Fitness fit)
		{
			if (Extra.Count != fit.Extra.Count) { return false; }
			var size = Extra.Count;
			for (int i = 0; i < size; i++)
			{
				if (Extra[i] != fit.Extra[i]) return false;
			}
			return Value == fit.Value;
		}
		#endregion

		#region - Utility methods. -
		public void Clear()
		{
			Value = 0f;
			Extra.Clear();
		}

		public void Print(TextWriter writer)
		{
			var res = Value.ToString();
			if (Extra.Count > 0)
			{
				res += " ";
				foreach (var extra in Extra)
				{
					res += "[" + extra + "]";
				}
			}
			writer.Write(res);
		}

		public override string ToString()
		{
			var res = Value.ToString();
			if (Extra.Count > 0)
			{
				res += " ";
				foreach (var extra in Extra)
				{
					res += "[" + extra + "]";
				}
			}
			return res;
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// Class to represent abstract fitness function to be used by evolutionary
	/// and neuroevolutionary algorithms.
	/// </summary>
	public abstract class FitnessFunction
	{
		public abstract Fitness Compute(float[] v);
	}
}
