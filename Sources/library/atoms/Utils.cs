using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MentalAlchemy.Atoms
{
	public class Utils
	{
		/// <summary>
		/// [atomic]
		/// 
		/// Reverses bytes order for the given number.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static uint ReverseBytesOrder (uint n)
		{
			uint res = 0;

			res |= (n & 0xFF) << 24;
			res |= (n & 0xFF00) << 8;
			res |= (n & 0xFF0000) >> 8;
			res |= (n & 0xFF000000) >> 24;
			return res;
		}
	}
}
