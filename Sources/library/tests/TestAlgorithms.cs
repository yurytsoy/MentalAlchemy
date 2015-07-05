using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			//int width = 512, height = 512;
			
			//var bmp = ImageProcessingElements.CreateRandomImageCircles(new System.Drawing.Size(width, height), 20);
			var bmp = (Bitmap)Bitmap.FromFile("lena_bw.png");
			//var bmp = (Bitmap)Bitmap.FromFile("lena.png");
			//var bmp = (Bitmap)Bitmap.FromFile("test_bw.png");
			//var bmp = (Bitmap)Bitmap.FromFile("test.tif");
			int width = bmp.Width, height = bmp.Height;

			var data = ImageProcessingElements.ToFloats(bmp, 0);

			int[] sizes = { 3, 5, 7, 9, 11, 15, 25, 35, 45, 55 };

            var mspertick = 1e3f / Stopwatch.Frequency;
			var lines = new List<string>();
			lines.Add("Radius\tAvg time\tSD time");
			foreach (var size in sizes)
			{
				int seW = size, seH = size;
				var se = VectorMath.Ones(seW * seH);

				int runCount = 10;
				float[] res = null;
				float[] times = new float [runCount];
				for (int i = 0; i < runCount; ++i)
				{
					var clock = Stopwatch.StartNew();
					res = ImageProcessingElements.Erode(data, width, height, se, seW, seH);
					clock.Stop();
					times[i] = clock.ElapsedTicks * mspertick;
				}
				var meant = (float)times.Average();
				var sdt = VectorMath.StdDev(times);
				lines.Add(string.Format ("{0}\t{1}\t{2}", size, meant, sdt));
				Console.WriteLine(lines.Last ());
				var resBmp = ImageProcessingElements.ToBitmap(res, width, height);
				resBmp.Save("image1_" + size + ".png");
			}

			FileIO.WriteAllLines("morphprofile.log", lines.ToArray ());

		}
	}
}
