using MentalAlchemy.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules
{
	public class Morphology
	{
		public static float[] Erode(float[] data, int width, int height, float[] seData, int seWidth, int seHeight)
		{
			return ImageProcessingElements.Erode(data, width, height, seData, seWidth, seHeight);
		}

		public static float[] ErodeBox(float[] data, int height, int width, int seSize)
		{
			var seData = VectorMath.Ones(seSize * seSize);
			return Erode(data, width, height, seData, seSize, seSize);
		}

		public static float[] ErodeBox(float[] data, int height, int width, int seWidth, int seHeight)
		{
			var seData = VectorMath.Ones(seWidth * seHeight);
			return Erode(data, width, height, seData, seWidth, seHeight);
		}

		public static float[] Dilate(float[] data, int width, int height, float[] seData, int seWidth, int seHeight)
		{
			return ImageProcessingElements.Dilate(data, width, height, seData, seWidth, seHeight);
		}

		public static float[] DilateBox(float[] data, int height, int width, int seSize)
		{
			var seData = VectorMath.Ones(seSize * seSize);
			return Dilate(data, width, height, seData, seSize, seSize);
		}

		public static float[] DilateBox(float[] data, int height, int width, int seWidth, int seHeight)
		{
			var seData = VectorMath.Ones(seWidth * seHeight);
			return Dilate(data, width, height, seData, seWidth, seHeight);
		}

		public static float[] Opening(float[] data, int width, int height, float[] seData, int seWidth, int seHeight)
		{
			var tmpData = Erode(data, width, height, seData, seWidth, seHeight);
			return Dilate(tmpData, width, height, seData, seWidth, seHeight);
		}

		public static float[] OpeningBox(float[] data, int width, int height, int seWidth, int seHeight)
		{
			var tmpData = ErodeBox(data, width, height, seWidth, seHeight);
			return DilateBox(tmpData, width, height, seWidth, seHeight);
		}

		public static float[] OpeningBox(float[] data, int width, int height, int seSize)
		{
			var tmpData = ErodeBox(data, width, height, seSize);
			return DilateBox(tmpData, width, height, seSize);
		}

		public static float[] Closing(float[] data, int width, int height, float[] seData, int seWidth, int seHeight)
		{
			var tmpData = Dilate(data, width, height, seData, seWidth, seHeight);
			return Erode(tmpData, width, height, seData, seWidth, seHeight);
		}

		public static float[] ClosingBox(float[] data, int width, int height, int seWidth, int seHeight)
		{
			var tmpData = DilateBox(data, width, height, seWidth, seHeight);
			return ErodeBox(tmpData, width, height, seWidth, seHeight);
		}

		public static float[] ClosingBox(float[] data, int width, int height, int seSize)
		{
			var tmpData = DilateBox(data, width, height, seSize);
			return ErodeBox(tmpData, width, height, seSize);
		}
	}
}
