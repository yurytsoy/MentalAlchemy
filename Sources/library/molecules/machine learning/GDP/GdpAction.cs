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
