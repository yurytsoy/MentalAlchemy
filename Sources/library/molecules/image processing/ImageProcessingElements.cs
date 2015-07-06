using MentalAlchemy.Atoms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules
{
	public class ImageProcessingElements
	{
		#region - Conversion. -
		/// <summary>
		/// [molecule]
		/// 
		/// Creates a bitmap from the given table of floats.
		/// </summary>
		/// <param name="table">Source table.</param>
		/// <returns>Resulting bitmap.</returns>
		public static Bitmap ToBitmap(float[] data, int width, int height)
		{
			var res = new Bitmap(width, height);

			var rect = new Rectangle(0, 0, res.Size.Width, res.Size.Height);
			BitmapData bmpData = res.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int bytesSize = bmpData.Height * bmpData.Stride;
			var bytes = new byte[bytesSize];
			System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytesSize);

			int alignByte = bmpData.Stride - width * 3;
			int count = 0;
			for (int i = 0; i < height; ++i)
			{
				int rowOffset = i * width;
				for (int j = 0; j < width; ++j, count += 3)
				{
					bytes[count] = bytes[count + 1] = bytes[count + 2] = (byte)data[rowOffset + j];
				}
				count += alignByte;
			}

			System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bmpData.Scan0, bytesSize);
			res.UnlockBits(bmpData);

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Convert selected [channel] from given image into the 2d array.
		/// </summary>
		/// <param name="image">24-bit (3-channels) image.</param>
		/// <param name="channel">ID of the channel to be written into the output array.</param>
		/// <returns>2d array representation for the given [channel]</returns>
		public static float[] ToFloats(Bitmap image, int channel)
		{
			int width = image.Width, height = image.Height;

			var rect = new Rectangle(0, 0, image.Size.Width, image.Size.Height);
			BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int bytesSize = bmpData.Height * bmpData.Stride;
			var bytes = new byte[bytesSize];
			System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, bytesSize);

			int stride = bmpData.Stride;
			int alignByte = stride - width * 3, count = channel;
			var res = new float[height * width];
			for (int i = 0; i < height; ++i)
			{
				count = i * stride + channel;
				int rowOffset = i * width;
				for (int j = 0; j < width; ++j, count += 3)
				{
					res[rowOffset + j] = bytes[count];
				}
				//count += alignByte;
			}

			image.UnlockBits(bmpData);
			return res;
		}
		#endregion

		#region - Generation. -
		/// <summary>
		/// [molecule]
		/// 
		/// Creates random image filled with the randomly placed ellipses.
		/// </summary>
		/// <param name="size">Image size.</param>
		/// <param name="objCount">Image objects count.</param>
		/// <returns>Random image.</returns>
		public static Bitmap CreateRandomImageCircles(Size size, int objCount)
		{
			var res = new Bitmap(size.Width, size.Height);

			int height = size.Height, width = size.Width;
			var graph = Graphics.FromImage(res);
			for (int i = 0; i < objCount; i++)
			{
				int r = ContextRandom.Next(256),
					g = ContextRandom.Next(256),
					b = ContextRandom.Next(256);
				var pen = new Pen(Color.FromArgb(r, g, b), ContextRandom.Next(10));

				var x1 = ContextRandom.Next(width);
				var y1 = ContextRandom.Next(height);
				var r1 = ContextRandom.Next(width) / 2;
				var r2 = ContextRandom.Next(height) / 2;
				graph.DrawEllipse(pen, x1 - r1, y1 - r2, x1 + r1, y1 + r2);
			}

			return res;
		}
		#endregion

		#region - Plain morphology. -
		public static float[] ErodePlain(float[] data, int width, int height, float[] seData, int seWidth, int seHeight)
		{ 

			throw new NotImplementedException ();
		}
		#endregion

		#region - Implementation of the morphological dilation by Urbach and Wilkinson. -
		protected struct uwMorphContext
		{
			public Chord[] Chords;
			public int MinYOffset;
			public int MaxYOffset;
			public int MinX;
			public int MaxX;
			public int MaxChordLength;
			/// <summary>
			/// Ordered array of unique lengths of chords.
			/// The array is composed in such a way, that:
			///  ChordLengths[i] <= 2 * ChordLengths[i-1]
			/// </summary>
			public int[] ChordLengths;
			public float[] Result;

			public int[] LengthsIndices;
		}

		/// <summary>
		/// Fast morphology by Urbach and Wilkinson, Efficient 2-D Grayscale Morphological Transformations
		/// With Arbitrary Flat Structuring Elements, 2008.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="seData"></param>
		/// <param name="seWidth"></param>
		/// <param name="seHeight"></param>
		/// <returns></returns>
		public static float[] Erode(float[] data, int width, int height, float[] seData, int seWidth, int seHeight)
		{
			var chords = ToChords(seData, seWidth, seHeight);
			var context = makeContext(chords, width, height);

			var lut = computeLookupTable(data, width, height, 0, context);

			lineErode(width, height, 0, lut, context);
			for (int y = 1; y < height; ++y )
			{
				updateLookupTable(data, width, height, y, lut, context);
				lineErode(width, height, y, lut, context);
			}

			return context.Result;
		}

		protected static uwMorphContext makeContext(Chord[] chords, int width, int height)
		{
			int maxy = 0, maxx = 0, miny = int.MaxValue, minx = int.MaxValue, maxl = 0;
			List<int> lengths = new List<int> ();
			lengths.Add(1);
			foreach (var chord in chords)
			{
				if (chord.Y < miny) { miny = chord.Y; }
				if (chord.Y > maxy) { maxy = chord.Y; }
				if (chord.X < minx) { minx = chord.X; }
				if (chord.X + chord.Length > maxx) { maxx = chord.X + chord.Length; }
				if (chord.Length > maxl) { maxl = chord.Length; }

				if (!lengths.Contains(chord.Length)) { lengths.Add(chord.Length); }
			}
			lengths.Sort();
			// make sure that the constraint is satisfied:
			//  lengths[i] <= 2 * lengths[i-1].
			for (int i = 1; i < lengths.Count; ++i)
			{
				//if (lengths[i] == 1) continue;	// do not check, that's minimal length

				if (lengths[i] > 2 * lengths[i - 1])
				{
					// insert a new length.
					lengths.Insert(i, (lengths[i] + 1) / 2);	// small hack to ensure, that the constraint is satisfied.
					i--;	// rollback to test the newly added element on the next iteration.
				}
			}

			var ctx = new uwMorphContext();
			ctx.Chords = chords;
			ctx.MaxX = maxx;
			ctx.MinX = minx;
			ctx.MaxYOffset = maxy;
			ctx.MinYOffset = miny;
			ctx.MaxChordLength = maxl;
			ctx.ChordLengths = lengths.ToArray();
			ctx.Result = VectorMath.Create(width * height, 65535f);

			var lenIdx = new int[ctx.MaxChordLength + 1];	// array to store indices of chords of specific lengths.
			//var lenIdx = new Dictionary<int, int>();
			foreach (var c in ctx.Chords)
			{
				var idx = Array.IndexOf(ctx.ChordLengths, c.Length);
				lenIdx[c.Length] = idx;
			}
			ctx.LengthsIndices = lenIdx;

			return ctx;
		}

		/// <summary>
		/// Computes T_{rowIndex}.
		/// Algorithm II.1.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		protected static float[][][] computeLookupTable(float[] data, int width, int height, int rowIndex, uwMorphContext ctx)
		{
			int ymax = ctx.MaxYOffset, ymin = ctx.MinYOffset;
			int deltaY = ymax - ymin + 1;
			int radx = (ctx.MaxX - ctx.MinX) / 2, rady = deltaY / 2;
			int chordLengthsCount = ctx.ChordLengths.Length;

			var dim1 = width + radx * 2;
			//var dim1 = width;
			var table = new float[deltaY][][];	//, dim1, deltaY];	// enlarge the 2nd dimension to avoid index out of bounds.
			for (int i = 0; i < deltaY; ++i)
			{
				table[i] = new float[dim1][];
				for (int j = 0; j < dim1; ++j)
				{
					//table[i][j] = new float[chordLengthsCount];
					table[i][j] = VectorMath.Create(chordLengthsCount, 65535f);
				}
			}

			for (int y = ymin; y <= ymax; ++y)
			{
				int r = y - ymin;

				// copy original values.
				//var rowOffset = (rowIndex + r) * width;
				var rowOffset = (rowIndex + y) * width;

				if (rowOffset >= 0)
				//if (y >= 0)
				{
					for (int x = 0; x < width; ++x) { table[r][x + radx][0] = data[rowOffset + x]; }
					//for (int x = 0; x < dim1; ++x) { table[r][x][0] = x - radx >= 0? data[rowOffset + x - radx] : 0; }

					for (int i = 1; i < chordLengthsCount; ++i)
					{
						for (var x = 0; x < dim1 - ctx.ChordLengths[i - 1]; ++x)
						{
							var d = ctx.ChordLengths[i] - ctx.ChordLengths[i - 1];
							//table[r][x][i] = Math.Min(table[r][x][i - 1], table[r][ x + d ][i - 1]);
							table[r][x][i] = Math.Min(table[r][x][i - 1], table[r][x + d][i - 1]);
						}
					}
				}
			}
			return table;
		}

		protected static void updateLookupTable(float[] data, int width, int height, int rowIndex, float[][][] lut, uwMorphContext ctx)
		{
			int ymax = ctx.MaxYOffset, ymin = ctx.MinYOffset;
			var deltaY = ymax - ymin;
			int radx = (ctx.MaxX - ctx.MinX) / 2;
			//int dim1 = lut[0].Length;
			lut[deltaY] = lut[0];	// make a carousel.
			for (int y = ymin; y < ymax; ++y)
			{
				int r = y - ymin;
				lut[r] = lut[r + 1];
			}

			// update the last row.
			//var rvalue = ymax-1;
			//var rowOffset = (rowIndex + ymax) * width;
			//var rowOffset = (rowIndex) * width;
			var rowOffset = (rowIndex + ctx.MaxYOffset) * width;
			if (rowOffset < 0 || rowOffset >= data.Length) return;

			var pixelOffset = rowOffset - radx;
			int chordLengthsCount = ctx.ChordLengths.Length;
			int ixLimit = rowOffset + width;
			for (int x = 0; x < width + 2 * radx; ++x)
			{
				var lutYX = lut[deltaY][x];
				int curX = pixelOffset + x;
				for (int i = 0; i < chordLengthsCount; ++i)
				{
					var tmpMin = float.MaxValue;

					int ix = curX;
					int aLimit = Math.Min(ctx.ChordLengths[i], ixLimit - ix);
					//for (int a = 0; a < curLength && ix < ixLimit; ++a, ++ix)
					for (int a = 0; a < aLimit; ++a, ++ix)
					{
						if (ix < rowOffset) continue;
						if (tmpMin > data[ix]) { tmpMin = data[ix]; }
						//if (ix < 0) continue;
						//if (tmpMin > data[x + a]) { tmpMin = data[ix]; }
					}
					//lut[deltaY][x][i] = tmpMin;
					lutYX[i] = tmpMin;
				}
			}
		}

		/// <summary>
		/// Algorithm II.2.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="rowIndex"></param>
		/// <param name="lut"></param>
		/// <param name="ctx"></param>
		protected static void lineErode(int width, int height, int rowIndex, float[][][] lut, uwMorphContext ctx)
		{
			// skip init of the resulting row, since the result was preallocated during context creation.

			int radx = (ctx.MaxX - ctx.MinX) / 2;
			//int rady = (ctx.MaxYOffset - ctx.MinYOffset) / 2;

			var rowOffset = (rowIndex) * width;

			var lenIdx = ctx.LengthsIndices;
			var chordCount = ctx.Chords.Length;
			for (int x = 0; x < width; ++x)
			{
				//var coord = rowOffset + x + radx;	// -radx;
				var coord = rowOffset + x;	// -radx;
				if (coord < 0) continue;

				var resVal = ctx.Result[coord];
				for (int k=0; k<chordCount; ++k)
				{
					//if (c.X + x < 0) continue;
					var c = ctx.Chords[k];
					int row = c.Y - ctx.MinYOffset;
					int col = x + c.X + radx;
					//int col = x + c.X;
					if (row < 0 || col < 0) continue;

					//var len = c.Length;
					//var ic = Array.IndexOf(ctx.ChordLengths, len);
					var ic = lenIdx[c.Length];
					var v = lut[row][col][ic];
					if (v < resVal) { resVal = v; }
				}
				ctx.Result[coord] = resVal;
			}
		}
		#endregion

		/// <summary>
		/// Convert given image into colleciton of horizontal chords.
		/// The image is supposed to be black and white, however the method
		/// simply compares if each pixel is > 0 or not.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="witdh"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Chord[] ToChords (float[] data, int width, int height, int centerX = 0, int centerY = 0)
		{
			if (centerX == 0) centerX = width / 2;
			if (centerY == 0) centerY = height / 2;

			var res = new List<Chord>();
			for (int i = 0; i < height; ++i )
			{
				var rowOffset = i * width;
				var chordY = i - centerY;
				var rowChords = ToChords(VectorMath.Subvector (data, rowOffset, rowOffset + width - 1));
				if (rowChords.Length > 0)
				{
 					foreach (var chord in rowChords)
					{
						chord.Y = chordY;
						chord.X -= centerX;
					}
					res.AddRange(rowChords);
				}
			}
			return res.ToArray();
		}

		/// <summary>
		/// Converts 1D array into chords.
		/// The image data is supposed to be black and white, however the method
		/// simply compares if each pixel is > 0 or not.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Chord[] ToChords(float[] data)
		{
			var res = new List<Chord>();

			// find the first positive element.
			var start = 0;
			foreach (var el in data)
			{
				if (el > 0) { break; }
				start++;
			}

			var tmpChord = new Chord() { X = start, Y = 0, Length = 1 };
			for (int i = start + 1; i < data.Length; ++i )
			{
				if (data[i] > 0 && data[i - 1] > 0)
				{	// continue current chord.
					tmpChord.Length++;
				}
				else if (data[i] > 0 && data[i - 1] <= 0)
				{	// new chord.
					tmpChord = new Chord() { X = i, Y = 0, Length = 1 };
				}
				else if (data[i] <= 0 && data[i - 1] > 0)
				{	// break the current chord.
					res.Add(tmpChord);
					tmpChord = null;
				} 
				// ignore the case when both data[i] and data[i-1] < 0
			}

			if (tmpChord != null) 
			{
				res.Add(tmpChord);
			}

			return res.ToArray();
		}
	}

	public class Chord 
	{
		public int Y;
		public int X;
		public int Length;
	}
}
