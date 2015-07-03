using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
	public class Euler
	{
		public static void Problem2()
		{
			var max = 4000000;

			int sum = 2;
			int el0 = 3, el1 = 5, el2 = el0 + el1;
			while (true && el2 < max)
			{
				sum += el2;

				// skip non even values.
				el0 = el2 + el1;
				el1 = el2 + el0;
				el2 = el0 + el1;
			}
			System.Console.WriteLine("Problem2 answer: " + sum);
			System.Console.ReadKey();
		}

		/// <summary>
		/// Warning: the code was tested to work with the particular number 600851475143.
		/// For the other numbers the correctness is not guaranteed, since no additional
		/// check of the factors was done to assert that they are primes.
		/// </summary>
		public static void Problem3()
		{
			Int64 num = 600851475143;

			var factors = new List<Int64>();
			for (int i = 7; i < num && num > 1; i += 2)
			{
				Int64 rest = num % i;
				if (rest == 0)
				{
					num /= i;
					factors.Add(i);
				}
			}

			var maxf = factors.Last();
			Int64 res = num;
			if (res < maxf) { res = maxf; }
			System.Console.WriteLine("Problem3 answer: " + res);
		}

		public static void Problem4()
		{
			var res = 0;
			for (int a = 999; a >= 100; --a )
			{
				if (res != 0 && res > a * a)  { break; }

				for (int b = a; b >= 100; --b )
				{
					var num = a * b;
					
					// check if this is a palindrome.
					var str = num.ToString();
					var answerFound = true;	// let's be optimists :)
					for (int i = 0; i < str.Length / 2; ++i)
					{
						if (str[i] != str[str.Length - i - 1])
						{
							answerFound = false;
							break;
						}
					}
					if (answerFound && res < num)
					{
						res = num;	// update the current answer.
						break;
					}
				}
			}
			System.Console.WriteLine("Problem4 answer: " + res);
		}
	}
}
