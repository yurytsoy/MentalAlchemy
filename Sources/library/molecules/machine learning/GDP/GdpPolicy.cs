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
