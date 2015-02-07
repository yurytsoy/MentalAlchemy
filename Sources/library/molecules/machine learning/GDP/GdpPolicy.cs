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

using MentalAlchemy.Atoms;

using GdpPolicyParameters = MentalAlchemy.Molecules.NevaParameters;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Baseline policy class for a graph decision process.
	/// 
	/// TODO: add initialization for a policy.
	/// TODO: add initialization to create actions. Possibly load actions or supply them from outside.
	/// </summary>
	public abstract class GdpPolicy
	{
		/// <summary>
		/// List of actions available to the policy.
		/// </summary>
		public List<GdpAction> Actions;
		/// <summary>
		/// Parameters to guide the policy and operators.
		/// </summary>
		public GdpPolicyParameters Parameters = new GdpPolicyParameters ();

		public SystemStateGraph Ssg;

		public static GdpPolicy CreateRandomPolicy()
		{
			var res = new GdpPolicyRandom();
			res.Actions = GdpAction.GetStandardActionsSet();
			foreach (var act in res.Actions)
			{
				act.Policy = res;
			}

			return res;
		}

		public abstract GdpAction SelectAction();
	}

	/// <summary>
	/// Random selection of actions.
	/// </summary>
	public class GdpPolicyRandom : GdpPolicy
	{
		public override GdpAction SelectAction()
		{
			var idx = ContextRandom.Next(Actions.Count);
			return Actions[idx];
		}
	}

	/// <summary>
	/// The policy which selects action depending on the state of SSG and quality of neighboring nodes.
	/// 
	/// TODO
	/// </summary>
	public class GdpPolicySsg : GdpPolicy 
	{
		public override GdpAction SelectAction()
		{
			throw new NotImplementedException();
		}
	}
}
