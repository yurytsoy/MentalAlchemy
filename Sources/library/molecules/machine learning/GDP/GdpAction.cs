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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Class to represent action over structured object.
	/// Can also be referred as operator/agent and so on.
	/// </summary>
	public abstract class GdpAction
	{
		public string Name;
		/// <summary>
		/// Reference to the policy.
		/// </summary>
		public GdpPolicy Policy;

		public abstract FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net);

		/// <summary>
		/// Creates list of the following actions:
		/// - add connection.
		/// - remove connection.
		/// - add node.
		/// - remove node.
		/// - change weight.
		/// </summary>
		/// <returns></returns>
		public static List<GdpAction> GetStandardActionsSet()
		{
			var res = new List<GdpAction>();

			res.Add(new AddConnectionAction ());
			res.Add(new DeleteConnectionAction());
			res.Add(new AddNodeAction());
			res.Add(new DeleteNodeAction());
			res.Add(new ChangeWeightAction());

			return res;
		}
	}

	public class AddConnectionAction : GdpAction 
	{
		public AddConnectionAction()
		{
			Name = "AddConn";
		}

		public override FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net)
		{
			return GdpElements.MutateAddConnection(net, this.Policy.Parameters);
		}
	}

	public class DeleteConnectionAction : GdpAction
	{
		public DeleteConnectionAction()
		{
			Name = "DelConn";
		}

		public override FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net)
		{
			return GdpElements.MutateDeleteConnection(net, this.Policy.Parameters);
		}
	}

	public class AddNodeAction : GdpAction
	{
		public AddNodeAction()
		{
			Name = "AddNode";
		}

		public override FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net)
		{
			return GdpElements.MutateAddNode(net, this.Policy.Parameters);
		}
	}

	public class DeleteNodeAction : GdpAction
	{
		public DeleteNodeAction()
		{
			Name = "DelNode";
		}

		public override FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net)
		{
			return GdpElements.MutateDeleteNode(net, this.Policy.Parameters);
		}
	}

	public class ChangeWeightAction : GdpAction
	{
		public ChangeWeightAction()
		{
			Name = "ModWeight";
		}

		public override FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net)
		{
			return GdpElements.MutateWeight(net, this.Policy.Parameters);
		}
	}

	//public class ChangeNodeAction : GdpAction
	//{
	//	public ChangeNodeAction()
	//	{
	//		Name = "ModNode";
	//	}

	//	//public override FlexibleNeuralNetwork2 Operate(FlexibleNeuralNetwork2 net)
	//	//{
	//	//	return GdpElements.MutateWeight(net, this.Policy.Parameters);
	//	//}
	//}
}
