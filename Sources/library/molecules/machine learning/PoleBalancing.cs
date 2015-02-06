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
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// This is implementation of the PoleBalancing problem, which is based upon the one from Colin Green's SharpNEAT project:
	/// sharpneat.sourceforge.net
	/// 
	/// </summary>
	public abstract class PoleBalancing : NeuralObjectiveFunction{}

	/// <summary>
	/// [molecule]
	/// 
	/// 1-pole balancing problem. Adapted from Colin Green's implementation (see sharpneat.sourceforge.net).
	/// </summary>
	public class SinglePoleBalancing : PoleBalancing
	{
		#region Overrides of NeuralObjectiveFunction

		public override string Name
		{
			get { return "Single Pole Balancing Problem"; }
		}

		public override List<int> InputIds
		{
			get{ return new List<int>{ 1, 2, 3, 4 }; }
		}

		public override List<int> OutputIds
		{
			get { return new List<int> { 5 }; }
		}

		public override Fitness Calculate(IVectorFunction network)
		{
			return Evaluate(network);
		}

		public override Fitness Test(IVectorFunction network)
		{
			return Evaluate(network);
		}

		#endregion

		#region Constants

		// Some physical model constants.
		const double Gravity = 9.8;
		const double MassCart = 1.0;
		const double MassPole = 0.1;
		const double TotalMass = (MassPole + MassCart);
		const double Length = 0.5;	  // actually half the pole's length.
		const double PoleMassLength = (MassPole * Length);
		const double ForceMag = 10.0;
        /// <summary>Time increment interval in seconds.</summary>
		public const double TimeDelta = 0.02;
		const double FourThirds = 4.0/3.0;

		// Some precalced angle constants.
		//const double OneDegree			= Math.PI / 180.0;	//= 0.0174532;
		const double SixDegrees		    = Math.PI / 30.0;	//= 0.1047192;
		const double TwelveDegrees		= Math.PI / 15.0;	//= 0.2094384;
		//const double TwentyFourDegrees  = Math.PI / 7.5;	//= 0.2094384;
		//const double ThirtySixDegrees	= Math.PI / 5.0;	//= 0.628329;
		//const double FiftyDegrees		= Math.PI / 3.6;	//= 0.87266;

		#endregion

        #region Class Variables

        // Domain parameters.
		double _trackLength;
		double _trackLengthHalf;
		int	_maxTimesteps;
		double _poleAngleThreshold;

        // Evaluator state.
        ulong _evalCount;
        bool _stopConditionSatisfied;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct evaluator with default task arguments/variables.
        /// </summary>
		public SinglePoleBalancing() : this(4.8, 100000, TwelveDegrees)
		{}

        /// <summary>
        /// Construct evaluator with the provided task arguments/variables.
        /// </summary>
		public SinglePoleBalancing(double trackLength, int maxTimesteps, double poleAngleThreshold)
		{
			_trackLength = trackLength;
			_trackLengthHalf = trackLength / 2.0;
			_maxTimesteps = maxTimesteps;
			_poleAngleThreshold = poleAngleThreshold;
		}

		#endregion

        #region IPhenomeEvaluator<IBlackBox> Members

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox.
        /// </summary>
		public Fitness Evaluate(IVectorFunction net)
        {
            _evalCount++;

			// Initialise state. 
            var state = new SinglePoleStateData();
            state._poleAngle = SixDegrees;

			// Run the pole-balancing simulation.
            int timestep = 0;
			for(; timestep < _maxTimesteps; timestep++)
			{
				// Provide state info to the black box inputs (normalised to +-1.0).
				var inputs = new float[4];
                inputs[0] = (float)(state._cartPosX / _trackLengthHalf);	// cart_pos_x range is +-trackLengthHalfed. Here we normalize it to [-1,1].
				inputs[1] = (float)(state._cartVelocityX / 0.75);			// cart_velocity_x is typically +-0.75
				inputs[2] = (float)(state._poleAngle / TwelveDegrees);		// pole_angle is +-twelve_degrees. Values outside of this range stop the simulation.
				inputs[3] = (float)(state._poleAngularVelocity);			// pole_angular_velocity is typically +-1.0 radians. No scaling required.

				// Activate the network.
				net.Calculate(inputs);

                // Calculate state at next timestep given the black box's output action (push left or push right).
				float[] outputs;
				net.GetOutputs(out outputs);
				SimulateTimestep(state, outputs[0] > 0.5);
		
				// Check for failure state. Has the cart run off the ends of the track or has the pole
				// angle gone beyond the threshold.
				if(     (state._cartPosX < -_trackLengthHalf) || (state._cartPosX > _trackLengthHalf)
					||  (state._poleAngle > _poleAngleThreshold) || (state._poleAngle < -_poleAngleThreshold)) 
                {
					break;
                }
			}

            if(timestep == _maxTimesteps) {
                _stopConditionSatisfied = true;
            }

            // The controller's fitness is defined as the number of timesteps that elapse before failure.
            double fitness = timestep;
            return new Fitness((float)fitness);
        }

        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// </summary>
        public void Reset()
        {   
        }

        #endregion

        #region Private Methods

		/// <summary>
        /// Calculates a state update for the next timestep using current model state and a single 'action' from the
        /// controller. The action specifies if the controller is pushing the cart left or right. Note that this is a binary 
        /// action and therefore full force is always applied to the cart in some direction. This is the standard model for
        /// the single pole balancing task.
		/// </summary>
        /// <param name="state">Model state.</param>
		/// <param name="action">push direction, left(false) or right(true). Force magnitude is fixed.</param>
		private void SimulateTimestep(SinglePoleStateData state, bool action)
		{
			//float xacc,thetaacc,force,costheta,sintheta,temp;
			double force = action ? ForceMag : -ForceMag;
			double cosTheta = Math.Cos(state._poleAngle);
			double sinTheta = Math.Sin(state._poleAngle);
			double tmp = (force + (PoleMassLength * state._poleAngularVelocity * state._poleAngularVelocity * sinTheta)) / TotalMass;

			double thetaAcc = ((Gravity * sinTheta) - (cosTheta * tmp)) 
                            / (Length * (FourThirds - ((MassPole * cosTheta * cosTheta) / TotalMass)));
			  
			double xAcc  = tmp - ((PoleMassLength * thetaAcc * cosTheta) / TotalMass);
			  

			// Update the four state variables, using Euler's method.
			state._cartPosX				+= TimeDelta * state._cartVelocityX;
			state._cartVelocityX		+= TimeDelta * xAcc;
			state._poleAngle			+= TimeDelta * state._poleAngularVelocity;
			state._poleAngularVelocity	+= TimeDelta * thetaAcc;
            state._action = action;
		}

        #endregion

	}

    /// <summary>
    /// By Colin Green (see sharpneat.sourceforge.net)
    /// 
    /// Model state variables for single pole balancing task.
    /// </summary>
    public class SinglePoleStateData
    {
        /// <summary>
        /// Cart position (meters from origin).
        /// </summary>
		public double _cartPosX;
        /// <summary>
        /// Cart velocity (m/s).
        /// </summary>
		public double _cartVelocityX;
        /// <summary>
        /// Pole angle (radians). Straight up = 0.
        /// </summary>
		public double _poleAngle;
        /// <summary>
        /// Pole angular velocity (radians/sec).
        /// </summary>
		public double _poleAngularVelocity;
        /// <summary>
        /// Action applied during most recent timestep.
        /// </summary>
		public bool _action;
    }

	/// <summary>
	/// [molecule]
	/// 
	/// 2-poles balancing problem. Adapted from Colin Green's implementation (see sharpneat.sourceforge.net).
	/// </summary>
	public class DoublePoleBalancing : PoleBalancing
	{
		#region Overrides of NeuralObjectiveFunction

		public override string Name
		{
			get { return "Double Pole Balancing Problem"; }
		}

		public override List<int> InputIds
		{
			get { return new List<int>{1, 2, 3, 4, 5, 6}; }
		}

		public override List<int> OutputIds
		{
			get { return new List<int>{7}; }
		}

		public override Fitness Calculate(IVectorFunction network)
		{
			return Evaluate(network);
		}

		public override Fitness Test(IVectorFunction network)
		{
			return Evaluate(network);
		}

		#endregion

		#region Constants

        // Disable comment warnings for constants with clear names.
        #pragma warning disable 1591

		// Some physical model constants.
		protected const double Gravity	= -9.8;
		protected const double MassCart	= 1.0;
		protected const double Length1	= 0.5;	  /* actually half the pole's length */
		protected const double MassPole1 = 0.1;
		protected const double Length2 = 0.05;
		protected const double MassPole2 = 0.01;
		protected const double ForceMag	= 10.0;
        /// <summary>Time increment interval in seconds.</summary>
		public const double TimeDelta = 0.01;
		protected const double FourThirds = 4.0/3.0;
        /// <summary>Uplifting moment?</summary>
		protected const double MUP = 0.000002;

		// Some useful angle constants.
		protected const double OneDegree			= Math.PI / 180.0;	//= 0.0174532;
		protected const double FourDegrees			= Math.PI / 45.0;	//= 0.06981317;
		protected const double SixDegrees			= Math.PI / 30.0;	//= 0.1047192;
		protected const double TwelveDegrees		= Math.PI / 15.0;	//= 0.2094384;
		protected const double EighteenDegrees		= Math.PI / 10.0;	//= 0.3141592;
		protected const double TwentyFourDegrees	= Math.PI / 7.5;	//= 0.4188790;
		protected const double ThirtySixDegrees	    = Math.PI / 5.0;	//= 0.628329;
		protected const double FiftyDegrees		    = Math.PI / 3.6;	//= 0.87266;
		protected const double SeventyTwoDegrees	= Math.PI / 2.5;	//= 1.256637;

        #pragma warning restore 1591
		#endregion

		#region Class Variables
        #pragma warning disable 1591

        // Domain parameters.
		double _trackLength;
		protected double _trackLengthHalf;
		protected int	_maxTimesteps;
		protected double _poleAngleThreshold;

        // Evaluator state.
        protected ulong _evalCount;
        protected bool _stopConditionSatisfied;

        #pragma warning restore 1591
		#endregion

        #region Constructors

        /// <summary>
        /// Construct evaluator with default task arguments/variables.
        /// </summary>
		public DoublePoleBalancing() : this(4.8, 100000, ThirtySixDegrees)
		{}

        /// <summary>
        /// Construct evaluator with the provided task arguments/variables.
        /// </summary>
		public DoublePoleBalancing(double trackLength, int maxTimesteps, double poleAngleThreshold)
		{
			_trackLength = trackLength;
			_trackLengthHalf = trackLength / 2.0;
			_maxTimesteps = maxTimesteps;
			_poleAngleThreshold = poleAngleThreshold;
		}

		#endregion

        #region IPhenomeEvaluator<IBlackBox> Members

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox.
        /// </summary>
		public virtual Fitness Evaluate(IVectorFunction net)
        {
            _evalCount++;

		    // [0] - Cart Position (meters).
		    // [1] - Cart velocity (m/s).
		    // [2] - Pole 1 angle (radians)
		    // [3] - Pole 1 angular velocity (radians/sec).
		    // [4] - Pole 2 angle (radians)
		    // [5] - Pole 2 angular velocity (radians/sec).
            double[] state = new double[6];
            state[2] = FourDegrees;
            
			// Run the pole-balancing simulation.
            int timestep = 0;
			for(; timestep < _maxTimesteps; timestep++)
			{
				// Provide state info to the network (normalised to +-1.0).
				// Markovian (With velocity info)
				var inputs = new float[6];
				inputs[0] = (float)(state[0] / _trackLengthHalf);    // Cart Position is +-trackLengthHalfed}
				inputs[1] = (float)(state[1] / 0.75);                  // Cart velocity is typically +-0.75
				inputs[2] = (float)(state[2] / ThirtySixDegrees);      // Pole Angle is +-thirtysix_degrees. Values outside of this range stop the simulation.
				inputs[3] = (float)(state[3]);                         // Pole angular velocity is typically +-1.0 radians. No scaling required.
				inputs[4] = (float)(state[4] / ThirtySixDegrees);      // Pole Angle is +-thirtysix_degrees. Values outside of this range stop the simulation.
				inputs[5] = (float)(state[5]);                         // Pole angular velocity is typically +-1.0 radians. No scaling required.

				// Activate the black box.
				net.Calculate(inputs);

                // Get black box response and calc next timestep state.
				float[] outputs;
				net.GetOutputs(out outputs);
				performAction(state, outputs[0]);
		
				// Check for failure state. Has the cart run off the ends of the track or has the pole
				// angle gone beyond the threshold.
				if(     (state[0]< -_trackLengthHalf) || (state[0]> _trackLengthHalf)
					||  (state[2] > _poleAngleThreshold) || (state[2] < -_poleAngleThreshold)
					||  (state[4] > _poleAngleThreshold) || (state[4] < -_poleAngleThreshold))
                {
					break;
                }
			}

            if(timestep == _maxTimesteps) {
                _stopConditionSatisfied = true;
            }

            // The controller's fitness is defined as the number of timesteps that elapse before failure.
            double fitness = timestep;
            return new Fitness((float)fitness);
        }

        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// </summary>
        public void Reset()
        {
        }

        #endregion

		#region Private Methods

		/// <summary>
        /// Calculates a state update for the next timestep using current model state and a single action from the
        /// controller. The action is a continuous variable with range [0:1]. 0 -> push left, 1 -> push right.
		/// </summary>
        /// <param name="state">Model state.</param>
		/// <param name="output">Push force.</param>
		protected void performAction(double[] state, double output)
		{ 
			int i;
			double[] dydx = new double[6];

			/*--- Apply action to the simulated cart-pole ---*/
			// Runge-Kutta 4th order integration method
			for(i=0; i<2; ++i)
			{
				dydx[0] = state[1];
				dydx[2] = state[3];
				dydx[4] = state[5];
				step(output, state, ref dydx);
				rk4(output, state, dydx, ref state);
			}
		}

		private void step(double action, double[] st, ref double[] derivs)
		{
			double	force,
				costheta_1,
				costheta_2,
				sintheta_1,
				sintheta_2,
				gsintheta_1,
				gsintheta_2,
				temp_1,
				temp_2,
				ml_1,
				ml_2,
				fi_1,
				fi_2,
				mi_1,
				mi_2;

			force		= (action - 0.5) * ForceMag * 2;
			costheta_1	= Math.Cos(st[2]);
			sintheta_1	= Math.Sin(st[2]);
			gsintheta_1 = Gravity * sintheta_1;
			costheta_2	= Math.Cos(st[4]);
			sintheta_2	= Math.Sin(st[4]);
			gsintheta_2 = Gravity * sintheta_2;

			ml_1		= Length1 * MassPole1;
			ml_2		= Length2 * MassPole2;
			temp_1		= MUP * st[3] / ml_1;
			temp_2		= MUP * st[5] / ml_2;

			fi_1		= (ml_1 * st[3] * st[3] * sintheta_1) +
				(0.75 * MassPole1 * costheta_1 * (temp_1 + gsintheta_1));

			fi_2		= (ml_2 * st[5] * st[5] * sintheta_2) +
				(0.75 * MassPole2 * costheta_2 * (temp_2 + gsintheta_2));

			mi_1 = MassPole1 * (1 - (0.75 * costheta_1 * costheta_1));
			mi_2 = MassPole2 * (1 - (0.75 * costheta_2 * costheta_2));

			derivs[1] = (force + fi_1 + fi_2) / (mi_1 + mi_2 + MassCart);
			derivs[3] = -0.75 * (derivs[1] * costheta_1 + gsintheta_1 + temp_1) / Length1;
			derivs[5] = -0.75 * (derivs[1] * costheta_2 + gsintheta_2 + temp_2) / Length2;
		}

		private void rk4(double f, double[] y, double[] dydx, ref double[] yout)
		{
			int i;

			double hh,h6;
			double[] dym = new double[6];
			double[] dyt = new double[6];
			double[] yt = new double[6];

			hh = TimeDelta * 0.5;
			h6 = TimeDelta / 6.0;
			for(i=0; i<=5; i++) {
                yt[i] = y[i] + (hh * dydx[i]);
            }
			step(f, yt, ref dyt);
			dyt[0] = yt[1];
			dyt[2] = yt[3];
			dyt[4] = yt[5];
			for (i=0; i<=5; i++) {
                yt[i] = y[i] + (hh * dyt[i]);
            }
			step(f,yt, ref dym);
			dym[0] = yt[1];
			dym[2] = yt[3];
			dym[4] = yt[5];
			for(i=0; i<=5; i++) 
			{
				yt[i] = y[i] + (TimeDelta * dym[i]);
				dym[i] += dyt[i];
			}
			step(f,yt, ref dyt);
			dyt[0] = yt[1];
			dyt[2] = yt[3];
			dyt[4] = yt[5];
			for (i=0;i<=5;i++) {
				yout[i] = y[i] + h6 * (dydx[i]+dyt[i]+2.0*dym[i]);
            }
		}

		#endregion
	}
}
