using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorUtils
{
	public class Main
    {
		public static void RGB2HSLuv(float inR, float inG, float inB, out float outH, out float outS, out float outL)
        {
			IList<double> hsluv = HSLuv.RgbToHsluv(new List<double> { (double) inR, (double) inG, (double) inB });
			outH = (float) hsluv[0];
			outS = (float) hsluv[1];
			outL = (float) hsluv[2];
		}

        public static void HSLuv2RGB(float inH, float inS, float inL, out float outR, out float outG, out float outB)
        {
            IList<double> rgb = HSLuv.HsluvToRgb(new List<double> { (double)inH, (double)inS, (double)inL });
            outR = (float)rgb[0];
            outG = (float)rgb[1];
            outB = (float)rgb[2];
        }

		public static double colorDistance(Color a, Color b)
		{
			return colorDistanceRGB(a.r, a.g, a.b, b.r, b.g, b.b);
		}

		public static double colorDistanceRGB(float r1, float g1, float b1, float r2, float g2, float b2)
		{
			double lab_l1, lab_a1, lab_b1;
			RGBtoLAB.convert((double)r1, (double)g1, (double)b1, out lab_l1, out lab_a1, out lab_b1);
			double lab_l2, lab_a2, lab_b2;
			RGBtoLAB.convert((double)r2, (double)g2, (double)b2, out lab_l2, out lab_a2, out lab_b2);

			return CIEDE2000.DE00Difference(lab_l1, lab_a1, lab_b1, lab_l2, lab_a2, lab_b2);
		}

		public static double colorDistanceHSLuv(float H1, float S1, float L1, float H2, float S2, float L2)
        {
			float r1, g1, b1;
			HSLuv2RGB(H1, S1, L1, out r1, out g1, out b1);
			float r2, g2, b2;
			HSLuv2RGB(H2, S2, L2, out r2, out g2, out b2);

			return colorDistanceRGB(r1, g1, b1, r2, g2, b2);
		}

		public static bool darken(double threshold, float inH, float inS, float inL, float cmpH, float cmpS, float cmpL, out float outH, out float outS, out float outL)
        {
			outH = 0f;
			outS = 0f;
			outL = 0f;
			while (inL >= 0)
            {
				if (colorDistanceHSLuv(inH, inS, inL, cmpH, cmpS, cmpL) >= threshold)
                {
					outH = inH;
					outS = inS;
					outL = inL;
					return true;
                }
            
				inL -= 1f;
			}

			return false;
        }

		public static Color? darkenColor(double threshold, Color toDarken)
        {
			float h, s, l;
			RGB2HSLuv(toDarken.r * 0.8f, toDarken.g * 0.85f, toDarken.b * 0.95f, out h, out s, out l);
			float darkH, darkS, darkL;
            bool success = darken(threshold, h, s, l, h, s, l, out darkH, out darkS, out darkL);
			if (!success)
			{
				return null;
            }

			float finalR, finalG, finalB;
			HSLuv2RGB(darkH, darkS, darkL, out finalR, out finalG, out finalB);
			return new Color(finalR, finalG, finalB);
		}
	}

	public class RGBtoLAB
    {
		// https://stackoverflow.com/questions/4593469/java-how-to-convert-rgb-color-to-cie-lab
		public static void convert(double r, double g, double b, out double l, out double a, out double b_)
		{

			double X, Y, Z, xr, yr, zr;

			// D65/2°
			double Xr = 95.047;
			double Yr = 100.0;
			double Zr = 108.883;


			// --------- RGB to XYZ ---------//

			if (r > 0.04045)
				r = Math.Pow((r + 0.055) / 1.055, 2.4);
			else
				r = r / 12.92;

			if (g > 0.04045)
				g = Math.Pow((g + 0.055) / 1.055, 2.4);
			else
				g = g / 12.92;

			if (b > 0.04045)
				b = Math.Pow((b + 0.055) / 1.055, 2.4);
			else
				b = b / 12.92;

			r *= 100;
			g *= 100;
			b *= 100;

			X = 0.4124 * r + 0.3576 * g + 0.1805 * b;
			Y = 0.2126 * r + 0.7152 * g + 0.0722 * b;
			Z = 0.0193 * r + 0.1192 * g + 0.9505 * b;


			// --------- XYZ to Lab --------- //

			xr = X / Xr;
			yr = Y / Yr;
			zr = Z / Zr;

			if (xr > 0.008856)
				xr = (float)Math.Pow(xr, 1 / 3f);
			else
				xr = (float)((7.787 * xr) + 16 / 116.0);

			if (yr > 0.008856)
				yr = (float)Math.Pow(yr, 1 / 3f);
			else
				yr = (float)((7.787 * yr) + 16 / 116.0);

			if (zr > 0.008856)
				zr = (float)Math.Pow(zr, 1 / 3f);
			else
				zr = (float)((7.787 * zr) + 16 / 116.0);

			l = (116 * yr) - 16;
			a = 500 * (xr - yr);
			b_ = 200 * (yr - zr);
		}
	}

	// https://github.com/halirutan/CIEDE2000-Color-Difference/blob/master/Excel-CEIDE2000/Excel-CEIDE2000/CIEDE2000.cs
	public class CIEDE2000
	{
		private readonly double _l1S;
		private readonly double _a1S;
		private readonly double _b1S;

		private const double Pi = Math.PI;
		private const double Pi2 = 2.0 * Math.PI;

		private const double kL = 1.0, kC = 1.0, kH = 1.0;

		public static double DE00ColorWithDifference(double l1s, double a1s, double b1s, double difference, double angle)
		{
			var colDist = new CIEDE2000(l1s, a1s, b1s);
			return colDist.ColorWithDifference(difference, angle);
		}

		public static double DE00A(double l1s, double a1s, double b1s, double difference, double angle)
		{
			var colDist = new CIEDE2000(l1s, a1s, b1s);
			var r = colDist.ColorWithDifference(difference, angle);
			return a1s + r * Math.Cos(angle);
		}

		public static double DE00B(double l1s, double a1s, double b1s, double difference, double angle)
		{
			var colDist = new CIEDE2000(l1s, a1s, b1s);
			var r = colDist.ColorWithDifference(difference, angle);
			return b1s + r * Math.Sin(angle);
		}

		public static double DE00Difference(double l1s, double a1s, double b1s, double l2s, double a2s, double b2s)
		{
			var colDist = new CIEDE2000(l1s, a1s, b1s);
			return colDist.DE00(l2s, a2s, b2s);
		}


		public static double DE00DifferencePolar(double l1s, double a1s, double b1s, double radius, double angle)
		{
			var colDist = new CIEDE2000(l1s, a1s, b1s);
			return colDist.DE00Polar(radius, angle);
		}

		/**
         * Constructor that takes the reference color (L, a, b) and all calculations are done
         * using this as reference color.
         */
		public CIEDE2000(double l1SIn, double a1SIn, double b1SIn)
		{
			_l1S = l1SIn;
			_a1S = a1SIn;
			_b1S = b1SIn;
		}

		public double ColorWithDifference(double difference, double angle)
		{
			angle = NormalizeAngle(angle);
			var f = new Func<double, double>((r) => DE00Polar(r, angle) - difference);

			var r1 = 0.0;
			var r2 = 2.0;
			// Try to find a large enough upper bound
			for (var i = 0; i < 10; i++)
			{
				if (f(r2) < 0)
				{
					r2 *= 2.0;
				}
				else break;
			}
			// Simple second root finding. Stolen from www.geeksforgeeks.org
			var eps = 0.0001;
			double n = 0, xm, x0 = -1.0, c;
			if (f(r1) * f(r2) < 0)
			{
				do
				{

					// calculate the intermediate 
					// value 
					x0 = (r1 * f(r2) - r2 * f(r1))
						/ (f(r2) - f(r1));

					// check if x0 is root of 
					// equation or not 
					c = f(r1) * f(x0);

					// update the value of interval 
					r1 = r2;
					r2 = x0;

					// update number of iteration 
					n++;

					// if x0 is the root of equation  
					// then break the loop 
					if (c == 0)
						break;
					xm = (r1 * f(r2) - r2 * f(r1))
						/ (f(r2) - f(r1));

					// repeat the loop until  
					// the convergence  
				} while (Math.Abs(xm - x0) >= eps);
			}
			return x0;
		}

		public double DE00Polar(double radius, double angle)
		{
			var a2s = _a1S + radius * Math.Cos(angle);
			var b2s = _b1S + radius * Math.Sin(angle);
			return DE00(_l1S, a2s, b2s);
		}

		/**
         * Implementation of 
         * "The CIEDE2000 Color-Difference Formula: Implementation Notes, Supplementary Test Data, and Mathematical Observations".
         */
		public double DE00(double l2s, double a2s, double b2s)
		{
			var mCs = (Math.Sqrt(_a1S * _a1S + _b1S * _b1S) + Math.Sqrt(a2s * a2s + b2s * b2s)) / 2.0;
			var G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(mCs, 7) / (Math.Pow(mCs, 7) + Math.Pow(25.0, 7))));
			var a1p = (1.0 + G) * _a1S;
			var a2p = (1.0 + G) * a2s;
			var C1p = Math.Sqrt(a1p * a1p + _b1S * _b1S);
			var C2p = Math.Sqrt(a2p * a2p + b2s * b2s);

			var h1p = Math.Abs(a1p) + Math.Abs(_b1S) > double.Epsilon ? Math.Atan2(_b1S, a1p) : 0.0;
			if (h1p < 0.0) h1p += Pi2;
			var h2p = Math.Abs(a2p) + Math.Abs(b2s) > double.Epsilon ? Math.Atan2(b2s, a2p) : 0.0;
			if (h2p < 0.0) h2p += Pi2;

			var dLp = l2s - _l1S;
			var dCp = C2p - C1p;

			var dhp = 0.0;
			var cProdAbs = Math.Abs(C1p * C2p);
			if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) <= Pi)
			{
				dhp = h2p - h1p;
			}
			else if (cProdAbs > double.Epsilon && h2p - h1p > Pi)
			{
				dhp = h2p - h1p - Pi2;
			}
			else if (cProdAbs > Double.Epsilon && h2p - h1p < -Pi)
			{
				dhp = h2p - h1p + Pi2;
			}

			var dHp = 2.0 * Math.Sqrt(C1p * C2p) * Math.Sin(dhp / 2.0);

			var mLp = (_l1S + l2s) / 2.0;
			var mCp = (C1p + C2p) / 2.0;

			var mhp = 0.0;
			if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) <= Pi)
			{
				mhp = (h1p + h2p) / 2.0;
			}
			else if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) > Pi && h1p + h2p < Pi2)
			{
				mhp = (h1p + h2p + Pi2) / 2.0;
			}
			else if (cProdAbs > double.Epsilon && Math.Abs(h1p - h2p) > Pi && h1p + h2p >= Pi2)
			{
				mhp = (h1p + h2p - Pi2) / 2.0;
			}
			else if (cProdAbs <= double.Epsilon)
			{
				mhp = h1p + h2p;
			}

			var T = 1.0 - 0.17 * Math.Cos(mhp - Pi / 6.0) + .24 * Math.Cos(2.0 * mhp) +
				0.32 * Math.Cos(3.0 * mhp + Pi / 30.0) - 0.2 * Math.Cos(4.0 * mhp - 7.0 * Pi / 20.0);
			var dTheta = Pi / 6.0 * Math.Exp(-Math.Pow((mhp / (2.0 * Pi) * 360.0 - 275.0) / 25.0, 2));
			var RC = 2.0 * Math.Sqrt(Math.Pow(mCp, 7) / (Math.Pow(mCp, 7) + Math.Pow(25.0, 7)));
			var mlpSqr = (mLp - 50.0) * (mLp - 50.0);
			var SL = 1.0 + 0.015 * mlpSqr / Math.Sqrt(20.0 + mlpSqr);
			var SC = 1.0 + 0.045 * mCp;
			var SH = 1.0 + 0.015 * mCp * T;
			var RT = -Math.Sin(2.0 * dTheta) * RC;

			var de00 = Math.Sqrt(
				Math.Pow(dLp / (kL * SL), 2) + Math.Pow(dCp / (kC * SC), 2) + Math.Pow(dHp / (kH * SH), 2) +
				RT * dCp / (kC * SC) * dHp / (kH * SH)
			);
			return de00;
		}

		/**
         * Didn't find a better way to do this in C#.
         * It basically does what Mathematica does with Mod[angle, 2Pi].
         */
		private static double NormalizeAngle(double angle)
		{
			while (angle < 0.0)
			{
				angle += Pi2;
			}

			while (angle > Pi2)
			{
				angle -= Pi2;
			}

			return angle;
		}
	}

	// https://github.com/hsluv/hsluv-csharp/blob/master/Hsluv/Hsluv.cs
	public class HSLuv
	{
		protected static double[][] M = new double[][]
		{
			new double[] {  3.240969941904521, -1.537383177570093, -0.498610760293    },
			new double[] { -0.96924363628087,   1.87596750150772,   0.041555057407175 },
			new double[] {  0.055630079696993, -0.20397695888897,   1.056971514242878 },
		};

		protected static double[][] MInv = new double[][]
		{
			new double[] { 0.41239079926595,  0.35758433938387, 0.18048078840183  },
			new double[] { 0.21263900587151,  0.71516867876775, 0.072192315360733 },
			new double[] { 0.019330818715591, 0.11919477979462, 0.95053215224966  },
		};

		protected static double RefX = 0.95045592705167;
		protected static double RefY = 1.0;
		protected static double RefZ = 1.089057750759878;

		protected static double RefU = 0.19783000664283;
		protected static double RefV = 0.46831999493879;

		protected static double Kappa = 903.2962962;
		protected static double Epsilon = 0.0088564516;

		protected static IList<double[]> GetBounds(double L)
		{
			var result = new List<double[]>();

			double sub1 = Math.Pow(L + 16, 3) / 1560896;
			double sub2 = sub1 > Epsilon ? sub1 : L / Kappa;

			for (int c = 0; c < 3; ++c)
			{
				var m1 = M[c][0];
				var m2 = M[c][1];
				var m3 = M[c][2];

				for (int t = 0; t < 2; ++t)
				{
					var top1 = (284517 * m1 - 94839 * m3) * sub2;
					var top2 = (838422 * m3 + 769860 * m2 + 731718 * m1) * L * sub2 - 769860 * t * L;
					var bottom = (632260 * m3 - 126452 * m2) * sub2 + 126452 * t;

					result.Add(new double[] { top1 / bottom, top2 / bottom });
				}
			}

			return result;
		}

		protected static double IntersectLineLine(IList<double> lineA,
			IList<double> lineB)
		{
			return (lineA[1] - lineB[1]) / (lineB[0] - lineA[0]);
		}

		protected static double DistanceFromPole(IList<double> point)
		{
			return Math.Sqrt(Math.Pow(point[0], 2) + Math.Pow(point[1], 2));
		}

		protected static bool LengthOfRayUntilIntersect(double theta,
			IList<double> line,
			out double length)
		{
			length = line[1] / (Math.Sin(theta) - line[0] * Math.Cos(theta));

			return length >= 0;
		}

		protected static double MaxSafeChromaForL(double L)
		{
			var bounds = GetBounds(L);
			double min = Double.MaxValue;

			for (int i = 0; i < 2; ++i)
			{
				var m1 = bounds[i][0];
				var b1 = bounds[i][1];
				var line = new double[] { m1, b1 };

				double x = IntersectLineLine(line, new double[] { -1 / m1, 0 });
				double length = DistanceFromPole(new double[] { x, b1 + x * m1 });

				min = Math.Min(min, length);
			}

			return min;
		}

		protected static double MaxChromaForLH(double L, double H)
		{
			double hrad = H / 360 * Math.PI * 2;

			var bounds = GetBounds(L);
			double min = Double.MaxValue;

			foreach (var bound in bounds)
			{
				double length;

				if (LengthOfRayUntilIntersect(hrad, bound, out length))
				{
					min = Math.Min(min, length);
				}
			}

			return min;
		}

		protected static double DotProduct(IList<double> a,
			IList<double> b)
		{
			double sum = 0;

			for (int i = 0; i < a.Count; ++i)
			{
				sum += a[i] * b[i];
			}

			return sum;
		}

		protected static double Round(double value, int places)
		{
			double n = Math.Pow(10, places);

			return Math.Round(value * n) / n;
		}

		protected static double FromLinear(double c)
		{
			if (c <= 0.0031308)
			{
				return 12.92 * c;
			}
			else
			{
				return 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
			}
		}

		protected static double ToLinear(double c)
		{
			if (c > 0.04045)
			{
				return Math.Pow((c + 0.055) / (1 + 0.055), 2.4);
			}
			else
			{
				return c / 12.92;
			}
		}

		protected static IList<int> RgbPrepare(IList<double> tuple)
		{

			for (int i = 0; i < tuple.Count; ++i)
			{
				tuple[i] = Round(tuple[i], 3);
			}

			for (int i = 0; i < tuple.Count; ++i)
			{
				double ch = tuple[i];

				if (ch < -0.0001 || ch > 1.0001)
				{
					throw new System.Exception("Illegal rgb value: " + ch);
				}
			}

			var results = new int[tuple.Count];

			for (int i = 0; i < tuple.Count; ++i)
			{
				results[i] = (int)Math.Round(tuple[i] * 255);
			}

			return results;
		}

		public static IList<double> XyzToRgb(IList<double> tuple)
		{
			return new double[]
			{
				FromLinear(DotProduct(M[0], tuple)),
				FromLinear(DotProduct(M[1], tuple)),
				FromLinear(DotProduct(M[2], tuple)),
			};
		}

		public static IList<double> RgbToXyz(IList<double> tuple)
		{
			var rgbl = new double[]
			{
				ToLinear(tuple[0]),
				ToLinear(tuple[1]),
				ToLinear(tuple[2]),
			};

			return new double[]
			{
				DotProduct(MInv[0], rgbl),
				DotProduct(MInv[1], rgbl),
				DotProduct(MInv[2], rgbl),
			};
		}

		protected static double YToL(double Y)
		{
			if (Y <= Epsilon)
			{
				return (Y / RefY) * Kappa;
			}
			else
			{
				return 116 * Math.Pow(Y / RefY, 1.0 / 3.0) - 16;
			}
		}

		protected static double LToY(double L)
		{
			if (L <= 8)
			{
				return RefY * L / Kappa;
			}
			else
			{
				return RefY * Math.Pow((L + 16) / 116, 3);
			}
		}

		public static IList<double> XyzToLuv(IList<double> tuple)
		{
			double X = tuple[0];
			double Y = tuple[1];
			double Z = tuple[2];

			double varU = (4 * X) / (X + (15 * Y) + (3 * Z));
			double varV = (9 * Y) / (X + (15 * Y) + (3 * Z));

			double L = YToL(Y);

			if (L == 0)
			{
				return new double[] { 0, 0, 0 };
			}

			var U = 13 * L * (varU - RefU);
			var V = 13 * L * (varV - RefV);

			return new Double[] { L, U, V };
		}

		public static IList<double> LuvToXyz(IList<double> tuple)
		{
			double L = tuple[0];
			double U = tuple[1];
			double V = tuple[2];

			if (L == 0)
			{
				return new double[] { 0, 0, 0 };
			}

			double varU = U / (13 * L) + RefU;
			double varV = V / (13 * L) + RefV;

			double Y = LToY(L);
			double X = 0 - (9 * Y * varU) / ((varU - 4) * varV - varU * varV);
			double Z = (9 * Y - (15 * varV * Y) - (varV * X)) / (3 * varV);

			return new double[] { X, Y, Z };
		}

		public static IList<double> LuvToLch(IList<double> tuple)
		{
			double L = tuple[0];
			double U = tuple[1];
			double V = tuple[2];

			double C = Math.Pow(Math.Pow(U, 2) + Math.Pow(V, 2), 0.5);
			double Hrad = Math.Atan2(V, U);

			double H = Hrad * 180.0 / Math.PI;

			if (H < 0)
			{
				H = 360 + H;
			}

			return new double[] { L, C, H };
		}

		public static IList<double> LchToLuv(IList<double> tuple)
		{
			double L = tuple[0];
			double C = tuple[1];
			double H = tuple[2];

			double Hrad = H / 360.0 * 2 * Math.PI;
			double U = Math.Cos(Hrad) * C;
			double V = Math.Sin(Hrad) * C;

			return new Double[] { L, U, V };
		}

		public static IList<double> HsluvToLch(IList<double> tuple)
		{
			double H = tuple[0];
			double S = tuple[1];
			double L = tuple[2];

			if (L > 99.9999999)
			{
				return new Double[] { 100, 0, H };
			}

			if (L < 0.00000001)
			{
				return new Double[] { 0, 0, H };
			}

			double max = MaxChromaForLH(L, H);
			double C = max / 100 * S;

			return new double[] { L, C, H };
		}

		public static IList<double> LchToHsluv(IList<double> tuple)
		{
			double L = tuple[0];
			double C = tuple[1];
			double H = tuple[2];

			if (L > 99.9999999)
			{
				return new Double[] { H, 0, 100 };
			}

			if (L < 0.00000001)
			{
				return new Double[] { H, 0, 0 };
			}

			double max = MaxChromaForLH(L, H);
			double S = C / max * 100;

			return new double[] { H, S, L };
		}

		public static IList<double> HpluvToLch(IList<double> tuple)
		{
			double H = tuple[0];
			double S = tuple[1];
			double L = tuple[2];

			if (L > 99.9999999)
			{
				return new Double[] { 100, 0, H };
			}

			if (L < 0.00000001)
			{
				return new Double[] { 0, 0, H };
			}

			double max = MaxSafeChromaForL(L);
			double C = max / 100 * S;

			return new double[] { L, C, H };
		}

		public static IList<double> LchToHpluv(IList<double> tuple)
		{
			double L = tuple[0];
			double C = tuple[1];
			double H = tuple[2];

			if (L > 99.9999999)
			{
				return new Double[] { H, 0, 100 };
			}

			if (L < 0.00000001)
			{
				return new Double[] { H, 0, 0 };
			}

			double max = MaxSafeChromaForL(L);
			double S = C / max * 100;

			return new double[] { H, S, L };
		}

		public static string RgbToHex(IList<double> tuple)
		{
			IList<int> prepared = RgbPrepare(tuple);

			return string.Format("#{0}{1}{2}",
				prepared[0].ToString("x2"),
				prepared[1].ToString("x2"),
				prepared[2].ToString("x2"));
		}

		public static IList<double> HexToRgb(string hex)
		{
			return new double[]
			{
				int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) / 255.0,
				int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) / 255.0,
				int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) / 255.0,
			};
		}

		public static IList<double> LchToRgb(IList<double> tuple)
		{
			return XyzToRgb(LuvToXyz(LchToLuv(tuple)));
		}

		public static IList<double> RgbToLch(IList<double> tuple)
		{
			return LuvToLch(XyzToLuv(RgbToXyz(tuple)));
		}

		// Rgb <--> Hsluv(p)

		public static IList<double> HsluvToRgb(IList<double> tuple)
		{
			return LchToRgb(HsluvToLch(tuple));
		}

		public static IList<double> RgbToHsluv(IList<double> tuple)
		{
			return LchToHsluv(RgbToLch(tuple));
		}

		public static IList<double> HpluvToRgb(IList<double> tuple)
		{
			return LchToRgb(HpluvToLch(tuple));
		}

		public static IList<double> RgbToHpluv(IList<double> tuple)
		{
			return LchToHpluv(RgbToLch(tuple));
		}

		// Hex

		public static string HsluvToHex(IList<double> tuple)
		{
			return RgbToHex(HsluvToRgb(tuple));
		}

		public static string HpluvToHex(IList<double> tuple)
		{
			return RgbToHex(HpluvToRgb(tuple));
		}

		public static IList<double> HexToHsluv(string s)
		{
			return RgbToHsluv(HexToRgb(s));
		}

		public static IList<double> HexToHpluv(string s)
		{
			return RgbToHpluv(HexToRgb(s));
		}
	}
}
