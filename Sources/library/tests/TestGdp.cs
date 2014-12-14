using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentalAlchemy;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Tests
{
	/// <summary>
	/// Class to test GDP functionality.
	/// </summary>
	public class TestGdp
	{
		public static void TestXOR()
		{
			int iterCount = 100;
			GraphDecisionProcess gdp = new GraphDecisionProcess ();
			gdp.ObjFunction = NeuralObjectiveFunctions.GetFunction(NeuralObjectiveFunctions.XOR_FUNCTION);
			gdp.Run(iterCount);

			FileIO.WriteAllLines("TestXOR.log", gdp.Report);
		}
	}
}
