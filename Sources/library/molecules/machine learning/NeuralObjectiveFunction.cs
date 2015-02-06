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
using System.Drawing;
using System.Linq;
using System.Text;

using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Class for the abstract neural objective function.
	/// </summary>
	public abstract class NeuralObjectiveFunction
	{
		public abstract string Name { get; }
		public abstract List<int> InputIds { get; }
		public abstract List<int> OutputIds { get; }
		public abstract Fitness Calculate(IVectorFunction network);
		public abstract Fitness Test(IVectorFunction network);

		public static float CalculateMSE(IVectorFunction net, List<TrainingSample> data)
		{
			var er = 0f;
			foreach (var sample in data)
			{
				var input = MatrixMath.GetRow(sample.Data, 0);

				net.Calculate(input);
				float[] netOut;
				net.GetOutputs(out netOut);

				// calculate squared Euclidian distance between required and actual output signals.
				var output = MatrixMath.GetRow(sample.Response, 0);
				er += VectorMath.EuclidianDistanceSqr(output, netOut.ToArray());
			}
			return (float)(Math.Sqrt(er) / data.Count);
		}
	}

	public class NeuralObjectiveFunctions
	{
		public static string XOR_FUNCTION = "XOR Function";
		public static string ARTIFICIAL_ANT_PROBLEM = "Artificial Ant Problem";
		public static string SINGLE_POLE_BALANCING = "1-Pole Balancing";
		public static string DOUBLE_POLE_BALANCING = "2-Poles Balancing";
		public static string[] PROBEN1_PROBLEMS = { "Card1", "Cancer1", "Diabetes1", "Glass1", "Heart1", "Horse1", "Soybean1", "Thyroid1" };

		public static string[] Functions()
		{	// This is a really hardcoded function. But I just don't know how to make an automatic list of static strings (don't wanna use reflections).
			var res = new List<string>();
			res.Add(XOR_FUNCTION);
			res.Add(ARTIFICIAL_ANT_PROBLEM);
			res.Add(SINGLE_POLE_BALANCING);
			res.Add(DOUBLE_POLE_BALANCING);
			for (int i = 0; i < PROBEN1_PROBLEMS.Length; i++)
			{
				res.Add(PROBEN1_PROBLEMS[i]);
			}
			return res.ToArray();
		}

		public static NeuralObjectiveFunction GetFunction (string name)
		{
			if (string.Compare(name, XOR_FUNCTION, true) == 0) {return new NeuralXorFunction();}
			if (string.Compare(name, ARTIFICIAL_ANT_PROBLEM, true) == 0) { return new ArtificialAntProblem(); }
			if (string.Compare(name, SINGLE_POLE_BALANCING, true) == 0) { return new SinglePoleBalancing(); }
			if (string.Compare(name, DOUBLE_POLE_BALANCING, true) == 0) { return new DoublePoleBalancing(); }

			//
			// try to create Proben1 problem.
			for (int i=0; i<PROBEN1_PROBLEMS.Length; ++i)
			{
				if (string.Compare(name, PROBEN1_PROBLEMS[i], true) == 0) return Proben1Problem.Create(name);
			}
			throw new Exception(String.Format("[GetFunction]: Undefined function name ({0})", name));
		}
	}

	/// <summary>
	/// [molecule]
	/// 
	/// Class for the XOR-problem.
	/// </summary>
	public class NeuralXorFunction : NeuralObjectiveFunction
	{
		protected List<TrainingSample> train = new List<TrainingSample>();

		#region - Public properties. -
		public override string Name { get { return "XOR problem"; } }
		public override List<int> InputIds
		{
			get { return new List<int> { 1, 2 }; }
		}
		public override List<int> OutputIds
		{
			get { return new List<int> { 3 }; }
		} 
		#endregion

		#region - Construction. -
		public NeuralXorFunction ()
		{
			FitnessComparator.MinimizeFitness = true;
			//int inputsNumber = 2;
			//int outputsNumber = 1;

			var tempSample = new TrainingSample();
			tempSample.ClassID = 0;
			tempSample.Data = new float[,] {{0, 0}};
			tempSample.Response = new float[,] { { 0 } };
			train.Add(tempSample);
			
			tempSample = new TrainingSample();
			tempSample.ClassID = 0;
			tempSample.Data = new float[,] { { 1, 1 } };
			tempSample.Response = new float[,] { { 0 } };
			train.Add(tempSample);
			
			tempSample = new TrainingSample();
			tempSample.ClassID = 1;
			tempSample.Data = new float[,] { { 1, 0 } };
			tempSample.Response = new float[,] { { 1 } };
			train.Add(tempSample);
			
			tempSample = new TrainingSample();
			tempSample.ClassID = 1;
			tempSample.Data = new float[,] { { 0, 1 } };
			tempSample.Response = new float[,] { { 1 } };
			train.Add(tempSample);
		}
		#endregion

		#region - Calculation of the fitness value. -
		public override Fitness Calculate(IVectorFunction network)
		{
			var res = new Fitness();
			res.Value = CalculateMSE(network, train);
			//res.Value = Performance.MachineLearning.CalculateMSE(network, train);
			return res;
		}
		#endregion

		#region - Testing. -
		public override Fitness Test(IVectorFunction network)
		{
			var res = new Fitness();
			foreach (var sample in train)
			{
				var row = MatrixMath.GetRow(sample.Data, 0);
				network.Calculate(row);

				float[] netOut;
				network.GetOutputs(out netOut);

				var output = MatrixMath.GetRow(sample.Response, 0);
				res.Extra.Add(netOut[0] - output[0]);
				res.Value += Math.Abs(netOut[0] - output[0]);
			}
			return res;
		}
		#endregion
	}
	public class Proben1Problem : NeuralObjectiveFunction
	{
		private List<TrainingSample> trainData;
		protected List<TrainingSample> validData;
		protected List<TrainingSample> testData;
		protected List<int> inputIds = new List<int>(), outputIds = new List<int>();
		protected string name;

		#region - Public properties. -
		public override string Name
		{
			get { return name; }
		}

		public override List<int> InputIds
		{
			get { return inputIds; }
		}

		public override List<int> OutputIds
		{
			get { return outputIds; }
		}

		public List<TrainingSample> TrainData
		{
			get { return trainData; }
			set { trainData = value; }
		}

		#endregion

		public Proben1Problem ()
		{
			FitnessComparator.MinimizeFitness = true;
		}

		#region - Public methods. -
		public override Fitness Calculate(IVectorFunction network)
		{
			return NevaElements.CalculateFitnessMSE(network, trainData);
		}

		public override Fitness Test(IVectorFunction network)
		{
			//return new Fitness(Performance.MachineLearning.TestNetwork(network, testData));
			return NevaElements.TestNetwork(network, testData);
		}
		#endregion

		#region - Public static methods. -
		/// <summary>
		/// [molecule]
		///
		/// Creates proben1 problem given the problem's name.
		/// Note that the problem data file should be located by the path "proben1\problem_name\problem_name_num".
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Proben1Problem Create (string name)
		{
			#region - Construct filename. -
			var lowName = name.ToLower().Substring(0, name.Length - 1);
			var filename = "proben1\\" + lowName + "\\" + name.ToLower() + ".dt"; 
			#endregion
			
			var res = new Proben1Problem();
			MachineLearningElements.LoadProben1Data(filename, out res.trainData, out res.validData, out res.testData);

			#region - Define input and output signals' IDs. -
			var inputCount = res.trainData[0].Data.GetLength(1);
			var outputCount = MachineLearningElements.CalculateClasses(res.trainData.ToArray());
			var count = 1;
			for (int i = 0; i < inputCount; i++, count++)
			{
				res.inputIds.Add(count);
			}
			for (int i = 0; i < outputCount; i++, count++)
			{
				res.outputIds.Add(count);
			} 
			#endregion

			res.name = name;
			return res;
		}
		#endregion
	}

	/// <summary>
	/// [molecule]
	/// 
	/// The network should have the following inputs:
	/// - 'food ahead' sensor
	/// - previous actions states (think: either boolean vector indicating, which action was taken at the previous time-step, or real valued vector of actions activity)
	/// ... and the outputs:
	/// - turn left
	/// - turn right
	/// - move forward.
	///
	/// Total: 89 food pellets.
	/// </summary>
	public class ArtificialAntProblem : NeuralObjectiveFunction
	{
		// 37x37 array.
		protected short[,] world = { { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0},
								   { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0},
								   { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0},
								   { 0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
								 };

		/// <summary>
		/// Default number of time-steps to test the ANN.
		/// </summary>
		public const int TIME_STEPS = 400;

		#region - Properties. -
		public enum Actions
		{
			Move,
			Left,
			Right
		} ;

		public List<Point> LastPath { get; set; } 
		#endregion

		#region - Construction. -
		public ArtificialAntProblem ()
		{
			FitnessComparator.MinimizeFitness = false;
		}
		#endregion

		#region Overrides of NeuralObjectiveFunction

		public override string Name
		{
			get { return "Artificial Ant problem"; }
		}

		public override List<int> InputIds
		{
			get { return new List<int> { 1, 2, 3, 4 }; }
		}
		public override List<int> OutputIds
		{
			get { return new List<int> { 5, 6, 7 }; }
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Evaluates current network.
		/// </summary>
		/// <param name="network"></param>
		/// <returns></returns>
		public override Fitness Calculate(IVectorFunction network)
		{
			return Calculate(network, false);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Tests given network on the same world, but saves ant's path.
		/// </summary>
		/// <param name="network"></param>
		/// <returns></returns>
		public override Fitness Test(IVectorFunction network)
		{
			return Calculate(network, true);
		}

		#endregion

		public Fitness Calculate(IVectorFunction network, bool writePath)
		{
			var ins = new float[InputIds.Count];
			var outs = new float[OutputIds.Count];

			var curPos = new Point(0, 0);	// ant's position.
			var curDir = new Point(1, 0);	// ant's look direction. To the right by default.
			var curMap = (short[,])world.Clone();
			var worldSize = curMap.GetLength(0);
			var res = 0;
			if (writePath) { LastPath = new List<Point>(); }

			for (int i = 0; i < TIME_STEPS; ++i)
			{
				//Problem: Outputs eventually grow by absolute value without limit.
				ins[0] = IsFoodAhead(curMap, curPos, curDir) ? 1 : 0;
				// translate normalized outputs on input.
				var outSum_1 = VectorMath.SumAbs(outs);
				if (outSum_1 != 0)
				{
					outSum_1 = 1f/outSum_1;
					for (int j = 0; j < outs.Length; j++) { ins[j + 1] = outs[j] * outSum_1; }
				}
				else
				{	// zero-valued outputs.
					for (int j = 0; j < outs.Length; j++) { ins[j + 1] = 0; }
				}

				network.Calculate(ins);			// set inputs.
				network.GetOutputs(out outs);	// get response.

				var action = (Actions)VectorMath.IndexOfMax(outs);	// get action.

				// move ant and define the result.
				if (action == Actions.Move)
				{	// move forward.
					// ...and check that the movement is possible.
					curPos = GetNewPosition(curPos, curDir, worldSize);

					if (curMap[curPos.Y, curPos.X] != 0)
					{	// food is found!
						++res;
						curMap[curPos.Y, curPos.X] = 0;	// no food any more.
					}
				}
				else if (action == Actions.Left)
				{	// turn left
					// ... by multiplying on the matrix: [[0 1] [-1 0]].
					var dirX = curDir.X;
					curDir.X = curDir.Y;
					curDir.Y = -dirX;
				}
				else if (action == Actions.Right)
				{	// turn right
					// ... by multiplying on the matrix: [[0 -1] [1 0]].
					var dirX = curDir.X;
					curDir.X = -curDir.Y;
					curDir.Y = dirX;
				}
				else
				{
					throw new NotImplementedException("Strange movement action");
				}

				if (writePath) {LastPath.Add(new Point(curPos.X, curPos.Y));}
			}

			return new Fitness(res);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns if there's a food ahead wrt to current world state and ant position and look direction.
		/// </summary>
		/// <param name="curMap"></param>
		/// <param name="curPos"></param>
		/// <returns></returns>
		public static bool IsFoodAhead (short[,] curMap, Point curPos, Point curDir)
		{
			var ahead = GetNewPosition(curPos, curDir, curMap.GetLength(0));
			return curMap[ahead.Y, ahead.X] != 0;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns new position in the toroidal world using current position and movement direction.
		/// </summary>
		/// <param name="curPos"></param>
		/// <param name="curDir"></param>
		/// <param name="mapSize"></param>
		/// <returns></returns>
		public static Point GetNewPosition(Point curPos, Point curDir, int mapSize)
		{
			var res = new Point(curPos.X, curPos.Y);
            res.Offset(curDir);
			res.X %= mapSize;
			res.Y %= mapSize;

			if (res.X < 0) { res.X = mapSize + res.X; }	// correct negative values.
			if (res.Y < 0) { res.Y = mapSize + res.Y; }	// correct negative values.

			return res;
		}
	}
}