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
	public class TestAlgorithms
	{
		public static void TestErode()
		{
			int width = 512, height = 512;
			
			//var bmp = ImageProcessingElements.CreateRandomImageCircles(new System.Drawing.Size(width, height), 20);
			var bmp = (Bitmap)Bitmap.FromFile("lena_bw.png");
			//var bmp = (Bitmap)Bitmap.FromFile("test.tif");
			bmp.Save("image0.png");

			var data = ImageProcessingElements.ToFloats(bmp, 0);

			int[] sizes = { 3, 5, 7, 9, 11, 15, 25, 35, 45, 55 };

			foreach (var size in sizes)
			{
				int seW = size, seH = size;
				var se = VectorMath.Ones(seW * seH);

				var res = ImageProcessingElements.Erode(data, width, height, se, seW, seH);
				var resBmp = ImageProcessingElements.ToBitmap(res, width, height);
				resBmp.Save("image1_" + size + ".png");
			}

		}
	}
}
