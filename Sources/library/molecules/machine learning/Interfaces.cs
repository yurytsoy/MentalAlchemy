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
	/// Interface for classification algorithms.
	/// </summary>
	public interface IClassifier : ICloneable
	{
		/// <summary>
		/// Train the probabilistic classifier using given [data] and setting some portion of it as a seed.
		/// </summary>
		void Train(List<TrainingSample> trainData);

		/// <summary>
		/// Performs recognition by the given object description.
		/// </summary>
		/// <param name="obj">Object description.</param>
		/// <returns>Class ID or '-1' if the class is unrecognized.</returns>
		int Recognize(float[,] obj);

		/// <summary>
		/// Perform voting for the given object using training data.
		/// </summary>
		/// <param name="obj">Object description.</param>
		/// <returns>Dictionary of votes per class.</returns>
		Dictionary<int, int> GetClassVotes(float[,] obj);

		/// <summary>
		/// For the given object get probabilities that it belongs to one of known classes.
		/// </summary>
		/// <param name="obj">Object description.</param>
		/// <returns>Dictionary of probabilities per class.</returns>
		Dictionary<int, float> GetClassProbabilities(float[,] obj);
	}
}
