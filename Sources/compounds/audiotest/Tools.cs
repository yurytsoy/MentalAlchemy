using Accord.Audio;
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace audiotest
{
	public class Utils
	{
		public static string GetNote(double[] power, double[] freq)
		{
			var idx = VectorMath.IndexOfMax(power);
			var maxFreq = freq[idx];
			var res = MusicUtils.ToNote((float)maxFreq).ToString ();
			res += ", ";

			var medPower = VectorMath.Median(power);
			var peaks = power.FindPeaks();
			foreach (var peak in peaks)
			{
				if (power[peak] < medPower) continue;
				res += MusicUtils.ToNote((float)freq[peak]).ToString();
			}
			return res;
		}
	}
}
