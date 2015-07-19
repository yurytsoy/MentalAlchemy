using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Tests
{
	public class TestMachineLearning
	{
		public static void TestRBM()
		{
			var xorData = new List<float[]>();
			xorData.Add(new[] { 1f, 0f, 1f });
			xorData.Add(new[] { 0f, 1f, 1f });
			xorData.Add(new[] { 0f, 0f, 0f });
			xorData.Add(new[] { 1f, 1f, 0f });

			var rbm = new RBM(3, 4);
			var error0 = rbm.Test(xorData);
			rbm.Train(xorData, 10000);
			var errorT = rbm.Test(xorData);
			Console.WriteLine(string.Format ("{0}\t{1}", error0, errorT));
			Console.ReadKey();
		}

		public static void TestRBMImage()
		{
			var bmp = (Bitmap)Bitmap.FromFile("lena_bw.png");
			int width = bmp.Width, height = bmp.Height;

			var data = ImageProcessingElements.ToFloats(bmp, 0);

			// make patches.
			var patchSize = 32;
			var patches = MatrixMath.ToPatches(data, width, height, patchSize);
			for (int i = 0; i < patches.Count; ++i )
			{
				patches[i] = VectorMath.ToRange01(patches[i]);
			}
			int count = 0;
			foreach (var patch in patches)
			{
				var patchBmp = ImageProcessingElements.ToBitmap(VectorMath.ToRange (patch, 0, 255), patchSize, patchSize);
				patchBmp.Save(count.ToString ("D3") + ".png");
				++count;
			}

			var gridSize = 8;
			var rbm = new RBM(patchSize * patchSize, gridSize * gridSize);

			var error = rbm.Test(patches); Console.WriteLine(string.Format("Error: {0}", error));
			var wt = MatrixMath.Transpose(rbm.Weights);
			var resWBmp = ImageProcessingElements.ToBitmapGrid(wt, patchSize, patchSize, gridSize, gridSize);
			resWBmp.Save("weights_0.png");
			var gram = MatrixMath.ComputeGramMatrixAsJaggedArray(wt);
			gram = ImageProcessingElements.ScaleNearestNeighbor(gram, 10);
			var gramBmp = ImageProcessingElements.ToBitmap(gram, normalize: true);
			gramBmp.Save("gram_0.png");

			for (int i = 0; i < 50; ++i )
			{
				rbm.Train(patches, 1, 0.9f);
				error = rbm.Test(patches); Console.WriteLine(string.Format("Error: {0}", error));
				
				wt = MatrixMath.Transpose(rbm.Weights);
				resWBmp = ImageProcessingElements.ToBitmapGrid(wt, patchSize, patchSize, gridSize, gridSize);
				resWBmp.Save("weights_" + (i + 1) +  ".png");

				gram = MatrixMath.ComputeGramMatrixAsJaggedArray(wt);
				gram = ImageProcessingElements.ScaleNearestNeighbor(gram, 10);
				gramBmp = ImageProcessingElements.ToBitmap(gram, normalize: true);
				gramBmp.Save("gram_" + (i+1) + ".png");
			}

			var recons = rbm.Reconstruct(patches, binaryHidden: false, binaryVisible: false);
			var reconsBmp = ImageProcessingElements.ToBitmapGrid(recons, patchSize, patchSize, width / patchSize, height / patchSize);
			reconsBmp.Save("recons.png");

			Console.ReadKey();
		}
	}
}
