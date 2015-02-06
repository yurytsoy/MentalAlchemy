﻿/*************************************************************************
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
