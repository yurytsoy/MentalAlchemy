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
