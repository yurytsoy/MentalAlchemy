using MentalAlchemy.Molecules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Tests
{
	public class TestMachineLearning
	{
		public static void TestRBM()
		{
			var xorData = new List<float[]>();
			xorData.Add(new[] { 1f, 0f, 1f });
			xorData.Add(new[] { 0f, 1f, 1f });
			xorData.Add(new[] { 0f, 0f, 0f });
			xorData.Add(new[] { 1f, 1f, 0f });

			var rbm = new RBM(3, 4);
			var error0 = rbm.Test(xorData);
			rbm.Train(xorData, 10000);
			var errorT = rbm.Test(xorData);
			Console.WriteLine(string.Format ("{0}\t{1}", error0, errorT));
			Console.ReadKey();
		}
	}
}
