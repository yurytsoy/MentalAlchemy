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

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// TODO: When changing state track also what action lead to the state change.
	/// </summary>
	public class SystemStateGraph
	{
		#region - Variables. -
		/// <summary>
		/// List of nodes comprising the graph.
		/// </summary>
		public List<SsgNode> Nodes = new List<SsgNode>();
		/// <summary>
		/// List of links that connect nodes.
		/// Note that the SSG is a multi-graph, thus
		/// multiple links between the same two nodes
		/// are possible.
		/// </summary>
		public List<SsgLink> Links = new List<SsgLink>();

		/// <summary>
		/// Pointer to the current node where the search is at the moment.
		/// </summary>
		public SsgNode CurNode; 
		#endregion

		#region - Construction and init. -
		/// <summary>
		/// Creates default SSG with 1 starting node.
		/// </summary>
		public SystemStateGraph()
		{
			Nodes.Add(new SsgNode(0,0));
			CurNode = Nodes[0];
		}
		#endregion

		#region - Operations on states. -
		/// <summary>
		/// Changes [CurNode] depending on the objects structure parameters.
		/// </summary>
		/// <param name="connNum"></param>
		/// <param name="nodeNum"></param>
		public void StateTransition(int connNum, int nodeNum, string actionName)
		{
			// do not change state if structure has not been changed.
			if (CurNode.ConnCount == connNum && CurNode.NodeCount == nodeNum)
			{
				RegisterLink(CurNode, CurNode, actionName);
				return;
			}

			var prevNode = CurNode;
			CurNode = null;
			foreach (var node in Nodes)
			{
				if (node.ConnCount == connNum && node.NodeCount == nodeNum)
				{
					CurNode = node;
					break;
				}
			}

			if (CurNode == null)
			{	// if no appropriate node is found, then create a new node.
				CurNode = new SsgNode(connNum, nodeNum);
				Nodes.Add(CurNode);
			}

			RegisterLink(prevNode, CurNode, actionName);
		}

		/// <summary>
		/// Looks for the state with specified parameters and if absent
		/// creates a new one.
		/// </summary>
		/// <param name="connNum"></param>
		/// <param name="nodeNum"></param>
		/// <param name="actionName"></param>
		public SsgNode StateFindCreate(int connNum, int nodeNum)
		{
			SsgNode res = null;
			foreach (var node in Nodes)
			{
				if (node.ConnCount == connNum && node.NodeCount == nodeNum)
				{
					res = node;
				}
			}

			return res?? new SsgNode (connNum, nodeNum);
		}

		/// <summary>
		/// Register attempt to change the state.
		/// </summary>
		public void RegisterAttempt(int connNum, int nodeNum, string actionName, Fitness quality)
		{
			var state = StateFindCreate(connNum, nodeNum);
			var link = FindLink(CurNode, state);
			if (link == null) RegisterLink(CurNode, state, actionName);
			else link.RegisterPath(actionName);

			state.Quality.Add(quality);
		}
		#endregion

		#region - Topology. -
		/// <summary>
		/// Returns neighborhood of the current node.
		/// </summary>
		/// <returns></returns>
		public void GetNeighborhood(out List<SsgNode> neighNodes, out List<SsgLink> links)
		{
			// check if SSG is empty.
			if (CurNode == null) 
			{
				neighNodes = null;
				links = null;
			}

			links = FindLinksFrom(CurNode);
			neighNodes = new List<SsgNode>();
			foreach (var link in links)
			{
				neighNodes.Add(link.Finish);
			}
		}
		#endregion

		#region - Methods to work with links. -
		public void RegisterLink(SsgNode start, SsgNode finish, string actionName)
		{
			var link = FindLink(start, finish);
			if (link == null)
			{
				link = new SsgLink();
				link.Start = start;
				link.Finish = finish;
			}

			link.RegisterPath(actionName);
		}

		public SsgLink FindLink(SsgNode start, SsgNode finish)
		{
			foreach (var link in Links)
			{
				if (link.Start == start && link.Finish == finish)
				{
					return link;
				}
			}
			return null;
		}

		public List<SsgLink> FindLinksFrom(SsgNode start)
		{
			var res = new List<SsgLink>();
			foreach (var link in Links)
			{
				if (link.Start == start)
				{
					res.Add (link);
				}
			}
			return res;
		}
		#endregion
	}

	public class SsgNode
	{
		public int ConnCount;
		public int NodeCount;
		public List<Fitness> Quality = new List<Fitness>();

		public string Id
		{
			get { return "N" + NodeCount + "C" + ConnCount; }
		}

		public float MeanQuality
		{
			get
			{
				if (Quality == null || Quality.Count == 0) return float.NaN;

				var res = 0f;
				foreach (var q in Quality)
				{
					res += q.Value;
				}
				return res / Quality.Count;
			}
		}

		public SsgNode() {}

		public SsgNode(int connNum, int nodeNum)
		{
			ConnCount = connNum;
			NodeCount = nodeNum;
		}
	}

	/// <summary>
	/// Link between two GSS nodes.
	/// </summary>
	public class SsgLink
	{
		public SsgNode Start;
		public SsgNode Finish;

		/// <summary>
		/// Contains list of actions that activated the link,
		/// and the number of times when the ink was activated by
		/// specific action.
		/// </summary>
		public Dictionary<string, int> Paths;

		public SsgLink()
		{
			Start = Finish = null;
			Paths = new Dictionary<string, int>();
		}

		/// <summary>
		/// Registers transition or transition attempt corresponding to the link.
		/// </summary>
		/// <param name="actionName"></param>
		public void RegisterPath(string actionName)
		{
			if (Paths.ContainsKey(actionName))
			{
				Paths[actionName]++;
			}
			else
			{
				Paths.Add(actionName, 1);
			}
		}
	}
}
