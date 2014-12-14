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
