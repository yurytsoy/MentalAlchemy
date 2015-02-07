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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MentalAlchemy.Atoms;
using MathNet.Numerics;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Description of GraphElements.
	/// </summary>
	public class GraphElements
	{
		#region - Loading and conversion. -
		/// <summary>
		/// Loads graph from the file containing Matlab-style adjacency matrix.
		/// Matrix example:
		/// 
		/// 0,1,0,0
		/// 1,1,0,0
		/// 0,0,0,0
		/// 0,0,1,1
		/// 
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <returns>Directed graph.</returns>
		public static Graph LoadGraph(string filename)
		{
			char[] splitChars = { ',' };
			var lines = new List<string[]>();
			using (var reader = new StreamReader(filename))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var elems = line.Split(splitChars);
					if (elems.Length <= 1) break;

					lines.Add(elems);
				}
			}

			//
			// convert obtained array of string arrays into the matrix.
			var m = MatrixMath.CreateFromStringsList(lines);

			//
			// create resulting directed graph structure.
			var res = new Graph();
			res.Edges = ConvertFromMatrixToEdges( m );
			res.AdjacencyMatrix = m;

			return res;
		}

		/// <summary>
		/// Loads graphs description in the DIMACS format
		/// (http://www.dis.uniroma1.it/challenge9/download.shtml).
		/// </summary>
		/// <param name="filename">Name of the file with graph's description.</param>
		/// <param name="maxVertex">Max id of the vertex.</param>
		/// <returns></returns>
		public static Graph LoadDimacsGraph (string filename, int maxVertex)
		{
			var res = new Graph();
			res.Edges = new List<Edge>();

			var lines = File.ReadAllLines(filename);
			foreach (var line in lines)
			{
				var split = line.Split(' ');
				if (split.Length == 0 || split[0] != "a") { continue; }	// skip all non-edge lines.

				var start = int.Parse(split[1]);
				var finish = int.Parse(split[2]);
				var dist = int.Parse(split[3]);
				if (start > maxVertex || finish > maxVertex) {break;}

				res.Edges.Add(new Edge(start, finish, dist));
			}

			return res;
		}

		/// <summary>
		/// Loads graph description from a file in the TSPLIB format.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static Graph LoadTsplibGraph (string filename)
		{
			const string coordLine = "NODE_COORD_SECTION";
			const string eofLine = "EOF";

			var lines = File.ReadAllLines(filename, Encoding.Default);
			var idx = -1;
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				if (string.Compare(line, coordLine, false) == 0)
				{
					idx = i + 1;
					break;
				}
			}
			if (idx < 0) {return null;}

			// get cities coordinates.
			char[] splitChars = { ' ' };
			var coords = new List<PointF>();
			for (int i = idx; i < lines.Length; i++)
			{
				var line = lines[i];
				if (string.Compare(line, eofLine, false) == 0) {break;}

				var spl = StringUtils.SplitWithSeparators(line, splitChars);
				var temp = new PointF();
				temp.X = float.Parse(spl[1]);
				temp.Y = float.Parse(spl[2]);

				coords.Add(temp);
			}

			var res = new Graph();
			return res;
		}

		/// <summary>
		/// Converts given adjacency matrix into the list of edges.
		/// </summary>
		/// <param name="m">Input adjacency matrix.</param>
		/// <returns>List of edges.</returns>
		public static List<Edge> ConvertFromMatrixToEdges(IList<float[]> m)
		{
			var res = new List<Edge>();

			for (int i = 0; i < m.Count; i++)
			{
				var line = m[i];
				for (int j = 0; j < line.Length; j++)
				{
					var elem = line[j];
					if (elem != 0)
					{
						var tempEdge = new Edge(i, j, elem);
						res.Add(tempEdge);
					}
				}
			}

			return res;
		}
		#endregion

		#region - Calculate graph characteristics. -
		/// <summary>
		/// Calculates the Laplacian matrix of the given graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static int[][] CalculateLaplacianMatrix(Graph graph)
		{
			var degrees = CalculateDegrees(graph);
			return MatrixMath.Diag(degrees);
		}

		/// <summary>
		/// Calculates vertex degrees of the undirected graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static int[] CalculateDegrees(Graph graph)
		{
			var res = new int[graph.AdjacencyMatrix.Length];
			var colSum = MatrixMath.SumColumns(graph.AdjacencyMatrix);

			for (int i = 0; i < res.Length; i++)
			{
				res[i] = (int)(colSum[i] + 0.5f);
			}
			return res;
		}

		#region - Shortest paths. -
		/// <summary>
		/// Calculates matrix of the shortest distances for each graph vertex in undirected graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns>Distance matrix.</returns>
		public static float[][] CalculateDistanceMatrix (Graph graph)
		{
            var size = graph.AdjacencyMatrix.Length;
			var res = MatrixMath.Zeros (size, size);

			for (int i=0; i<size; ++i)
			{
				res[i][i] = 0;
				for (int j = i+1; j < size; j++)
				{
					var path = ShortestPathLength(graph, i, j);
					res[i][j] = res[j][i] = path;
				}
			}

			return res;
		}

		/// <summary>
		/// Calculates length of the shortest path from [start] to [finish] using Dijkstra algorithm.
		/// </summary>
		/// <param name="graph">Graph.</param>
		/// <param name="start">Starting vertex.</param>
		/// <param name="finish">Finishing vertex.</param>
		/// <returns>Shortest path length. Pos. infinity if the path is absent.</returns>
		public static float ShortestPathLength (Graph graph, int start, int finish)
		{
			var size = graph.AdjacencyMatrix.Length;

			// 1. Create and init array [d] of vertex weights.
			var d = new float[size];
			for (int i = 0; i < size; i++) { d[i] = float.PositiveInfinity; }
			d[start] = 0;

			// 2. Create array of vertex labels initially filled with zeroes.
			var m = new float[size];

			// 3. Set current vertex for the algorithm.
			var t = start;

			// Main loop.
			do
			{
				// 4. For all vertices with (m[i] == 0) update their weights.
				for (int i = 0; i < size; i++)
				{
					//if (m[i] == 0) d[i] = Math.Min(d[i], d[t] + graph.AdjacencyMatrix[t, i]);
					if (Math.Abs(m[i]) < float.Epsilon && graph.AdjacencyMatrix[t][i] != 0)
					{	// if the vertex is unlabeled and a path between vertices t and i exists.
						d[i] = Math.Min(d[i], d[t] + graph.AdjacencyMatrix[t][i]);
					}
					//if (m[i] == 0)
					//{
					//    if (d[i] == 0) d[i] = d[t] + graph.AdjacencyMatrix[t, i];
					//    else d[i] = Math.Min(d[i], d[t] + graph.AdjacencyMatrix[t, i]);
					//}
				}

				// 5. Find vertex with (m[i] == 0) and of minimal weight d[i].
				var idx = -1;
				var minD = float.PositiveInfinity;
				for (int i = 0; i < size; i++)
				{
					if (Math.Abs(m[i]) < float.Epsilon && minD > d[i])
					{
						minD = d[i];
						idx = i;
					}
				}

				// 5.1 If vertex with minmal weight is absent, then the path doesn't exist.
				if (idx == -1) return float.PositiveInfinity;

				// 6. The found vertex becomes the current one.
				t = idx;
				m[t] = 1;

				// 7. If current vertex is the finish one then the path is found and return d[t].
				if (t == finish) return d[t]; 
			} while (true);
		}
		#endregion
		#endregion

		#region - Generation of random graphs. -
		/// <summary>
		/// Creates random graph without loops.
		/// </summary>
		/// <param name="size">Number of vertices.</param>
		/// <param name="cRate">Connectivity rate.</param>
		/// <param name="directed">If [True] then the graph is directed and undirected otherwise.</param>
		/// <returns>Random graph.</returns>
		public static Graph Random (int size, float cRate, bool directed)
		{
			return directed ? RandomDirected(size, cRate) : RandomUndirected(size, cRate);
		}

		/// <summary>
		/// Creates random undirected graph without loops.
		/// </summary>
		/// <param name="size">Number of vertices.</param>
		/// <param name="cRate">Connectivity rate.</param>
		/// <returns>Random graph.</returns>
		public static Graph RandomUndirected(int size, float cRate)
		{
            var rand = new Random();
			var res = new Graph ();
			
            var m = MatrixMath.Zeros (size, size);
			for (int i = 0; i < size; i++)
			{
				for (int j = i+1; j < size; j++)
				{
                    if (cRate < rand.NextDouble())
					{
						m[i][j] = m[j][i] = 1;
					}
				}
			}

			res.AdjacencyMatrix = m;
			res.Edges = ConvertFromMatrixToEdges(m);

			return res;
		}

		/// <summary>
		/// Creates random directed graph without loops.
		/// </summary>
		/// <param name="size">Number of vertices.</param>
		/// <param name="cRate">Connectivity rate.</param>
		/// <returns>Random graph.</returns>
		public static Graph RandomDirected(int size, float cRate)
		{
            var rand = new Random();
            var res = new Graph();
            var m = MatrixMath.Zeros(size, size);

			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					if (j == i) continue;

                    if (cRate < rand.NextDouble())
					{
						m[i][j] = 1;
					}
				}
			}

			res.AdjacencyMatrix = m;
			res.Edges = ConvertFromMatrixToEdges(m);

			return res;
		}
		#endregion

        #region - Methods for trees. -
        /// <summary>
        /// Computes distance matrix on the dendrogram represented by a tree.
        /// 
        /// Note: Strictly speaking those are not distances, but rather data
        /// values allocated at the first join for each pair of leaves.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static T[][] ComputeDendrogramDistances<T>(Tree<T> tree)
        {
            var leaves = tree.GetLeaves();
            if (leaves.Count == 0) return null;

            var size = leaves.Count;
            var res = new T[size][]; for (int i = 0; i < size; ++i) { res[i] = new T[size]; }
            var paths = new List<List<TreeNode<T>>>();
            for (int i = 0; i < size; ++i )
            {
                paths.Add (leaves[i].GetPath());
            }

            for (int i = 0; i < size; ++i )
            {
                // leave T[i][i] undefined or default. Only upper/lower parts matter
                for (int j = i + 1; j < size; ++j )
                {
                    // for the paths i and j find the last matching node.
                    var firstJoinIndex = 0; // root.
                    for (int k = 1; k < paths[i].Count && k < paths[j].Count; ++k )
                    {
                        if (paths[i][k] == paths[j][k])
                        {
                            firstJoinIndex = k;
                        }
                        else break;
                    }

                    res[i][j] = res[j][i] = paths[i][firstJoinIndex].Data;
                }
            }

            return res;
        }
        #endregion
	}

	public static class GraphPolynomials
	{
		public static string CHARACTERISTIC_POLYNOM = "Characteristic polynomial";
		public static string LAPLACIAN_POLYNOM = "Laplacian polynomial";
		public static string DISTANCE_POLYNOM = "Distance polynomial";
		public static string MATCHING_POLYNOM = "Matching (acyclic) polynomial";

		/// <summary>
		/// Returns all available graph polynomials.
		/// </summary>
		/// <returns></returns>
		public static string[] Polynomials()
		{	// This is a really hardcoded function. But I just don't know how to make an automatic list of static strings (don't wanna use reflections).
			var res = new List<string>();
			res.Add(CHARACTERISTIC_POLYNOM);
			res.Add(LAPLACIAN_POLYNOM);
			res.Add(DISTANCE_POLYNOM);
			res.Add(MATCHING_POLYNOM);
			return res.ToArray();
		}

		/// <summary>
		/// Calculates roots of the specified polynomial for the given graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="polyName"></param>
		/// <returns>Array of roots.</returns>
		public static Complex[] CalculateRoots (Graph graph, string polyName)
		{
			if (polyName == CHARACTERISTIC_POLYNOM) return CalculateCharacteristicRoots (graph);
			if (polyName == LAPLACIAN_POLYNOM) return CalculateLaplacianRoots(graph);
			if (polyName == DISTANCE_POLYNOM) return CalculateDistanceRoots(graph);
			if (polyName == MATCHING_POLYNOM) return CalculateMatchingRoots(graph);
			throw new NotImplementedException("CalculateRoots");
		}

		/// <summary>
		/// Calculates roots of the characteristic polynomial for the given graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns>Array of roots.</returns>
		public static Complex[] CalculateCharacteristicRoots (Graph graph)
		{
			return MatrixMath.CalculateCharacteristicRoots(graph.AdjacencyMatrix);
		}

		/// <summary>
		/// Calculates roots of the Laplacian polynomial of the graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static Complex[] CalculateLaplacianRoots(Graph graph)
		{
			// 1. Calculate Laplacian matrix (L) of the graph.
			var lapl = GraphElements.CalculateLaplacianMatrix(graph);
			var matrL = MatrixMath.ConvertToFloats(lapl);

			// 2. Calculate D = L - A
			var matrD = MatrixMath.Sub(matrL, graph.AdjacencyMatrix);

			// 3. Calculate roots of the characteristic polynomial of the [matrD]
			return MatrixMath.CalculateCharacteristicRoots(matrD);
		}

		/// <summary>
		/// Calculates roots of the Laplacian polynomial of the graph.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static Complex[] CalculateDistanceRoots(Graph graph)
		{
			var distM = GraphElements.CalculateDistanceMatrix(graph);
			return MatrixMath.CalculateCharacteristicRoots(distM);
		}

		#region - Matching polynomial. -
		/// <summary>
		/// Calculates roots of the matching (acyclic) polynomial of the graph
		/// using recurrent formula
		///
		///		Ac(G) = Ac(G - e_{ij}) - Ac(G - v_i - v_j)
		///
		/// Ac -- acyclic polynomial, e_{ij} -- edge between vertices i and j,
		/// v_i and v_j -- i-th and j-th vertices.
		/// 
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static Complex[] CalculateMatchingRoots(Graph graph)
		{
            var coefs = CalculateMatchingPolCoefs(graph.AdjacencyMatrix, graph.AdjacencyMatrix.Length + 1);

			//
			// find polynomial roots via eigenvalues of the matrix (see method described in [http://mathworld.wolfram.com/PolynomialRoots.html]).
			var roots = Numerics.FindPolynomialRoots(coefs);
			return roots;
		}

		/// <summary>
		/// Calculates roots of the matching (acyclic) polynomial of the graph by its
		/// adjacency matrix using recurrent formula
		///
		///		Ac(G) = Ac(G - e_{ij}) - Ac(G - v_i - v_j)
		///
		/// Ac -- acyclic polynomial, e_{ij} -- edge between vertices i and j,
		/// v_i and v_j -- i-th and j-th vertices.
		/// 
		/// </summary>
		/// <param name="adjM">Adjacency matrix.</param>
		/// <param name="maxDegree"></param>
		/// <returns></returns>
		public static float[] CalculateMatchingPolCoefs(float[][] adjM, int maxDegree)
		{
            var size = adjM.Length;
			var res = new float[maxDegree];

			//
			// Implement recurrent formula
			//
			//		Ac(G) = Ac(G - e_{ij}) - Ac(G - v_i - v_j)
			//
			// Ac -- acyclic polynomial, e_{ij} -- edge between vertices i and j,
			// v_i and v_j -- i-th and j-th vertices.

			//
			// 1. Check if there are edges to remove. If graph contains only non-incident edges then
			//	calculate Ac directly.
			int zeros = MatrixMath.CountValuesTriUIgnoreDiag(adjM, 0);
			int maxZeros = (size) * (size - 1) / 2;	// max number of zeros that can appear at the upper part of the matrix.
			int edgeCount = maxZeros - zeros;

			if (edgeCount > 1)
			{	// if there are more than 1 edge.

				//
				// 2. remove the first found edge.
				var adjM2 = (float[][])adjM.Clone();
				var cycleBreak = false;
				int v1 = -1, v2 = -1;	// indices of vertices incident to the edge being removed.
				for (int i = 0; i < size; i++)
				{
					for (int j = 0; j < size; j++)
					{
						if (Math.Abs(adjM2[i][j]) > float.Epsilon) {
							adjM2[i][j] = adjM2[j][i] = 0;
							v1 = i;
							v2 = j;
							cycleBreak = true;
							break;
						}
					}
					if (cycleBreak) break;
				}
				if (v1 < 0 || v2 < 0) throw new Exception("[CalculateMatchingPolCoefs]: v1 < 0 || v2 < 0");
				if (v1 > v2)
				{	// swap values.
					var temp = v1;
					v1 = v2;
					v2 = temp;
				}

				//
				// 2.1. Compute matching polynomial coefficients for the graph represented by [adjM2]
				res = CalculateMatchingPolCoefs(adjM2, maxDegree);

				//
				// 3. remove vertices v1 and v2
				float[] roots3;
				if (size > 2)
				{
					var adjM3 = MatrixMath.RemoveRow(adjM, v1);
					adjM3 = MatrixMath.RemoveRow(adjM3, v2 - 1);	// since [adjM3] now contains less rows than [adjM]
					adjM3 = MatrixMath.RemoveColumn(adjM3, v1);
					adjM3 = MatrixMath.RemoveColumn(adjM3, v2 - 1);	// since [adjM3] now contains less columns than [adjM]

					//
					// 3.1. Compute matching polynomial coefficients for the graph represented by [adjM3]
					roots3 = CalculateMatchingPolCoefs(adjM3, maxDegree);
				}
				else
				{	// the graph, represented by [adjM3] is empty.
					roots3 = new float[maxDegree];
					roots3[size] = 1;
					//roots3[2] = -1;
				}

				//
				// 4. Compute: Ac(G - e_{ij}) - Ac(G - v_i - v_j).
				for (int i = 0; i < maxDegree; i++)
				{
					res[i] -= roots3[i];
				}
			}
			else if (edgeCount == 1)
			{	// if there is only one edge.
				// compute polynomial coefficients directly.

				res[size] = 1;	// a_0
				res[size - 2] = -1;	// a_2
				// all other coefficients equal to zero.
				//
			}
			else
			{	// the graph has no edges.
				// compute polynomial coefficients directly.
				res[size] = 1;	// a_0
			}

			return res;
		} 
		#endregion
	}
}
