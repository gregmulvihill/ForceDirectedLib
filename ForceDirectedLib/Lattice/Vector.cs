using System;
using System.Collections.Generic;

namespace Lattice
{
	public struct Vector : IEquatable<Vector>
	{
		public static readonly Vector Zero = new Vector();
		public static readonly Vector XAxis = new Vector(1.0, 0.0, 0.0);
		public static readonly Vector YAxis = new Vector(0.0, 1.0, 0.0);
		public static readonly Vector ZAxis = new Vector(0.0, 0.0, 1.0);
		public double X;
		public double Y;
		public double Z;

		public Vector(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Vector Multiply(Vector a, double b)
		{
			return new Vector(a.X * b, a.Y * b, a.Z * b);
		}

		public static Vector operator *(Vector a, double b) => Multiply(a, b);

		public static Vector operator *(double a, Vector b) => Multiply(b, a);

		public static Vector Divide(Vector a, double b)
		{
			double num = 1.0 / b;
			return new Vector(a.X * num, a.Y * num, a.Z * num);
		}

		public static Vector operator /(Vector a, double b) => Divide(a, b);

		public static Vector Add(Vector a, Vector b)
		{
			return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector operator +(Vector a, Vector b) => Add(a, b);

		public static Vector Subtract(Vector a, Vector b)
		{
			return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vector operator -(Vector a, Vector b) => Subtract(a, b);

		public static Vector Negate(Vector a)
		{
			return new Vector(-a.X, -a.Y, -a.Z);
		}

		public static Vector operator -(Vector a) => Negate(a);

		public bool Equals(Vector a)
		{
			return X == a.X && Y == a.Y && Z == a.Z;
		}

		public static bool operator ==(Vector a, Vector b) => Equals(a, b);

		public static bool operator !=(Vector a, Vector b) => !Equals(a, b);

		public static double Dot(Vector a, Vector b)
		{
			return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
		}

		public static Vector Cross(Vector a, Vector b)
		{
			return new Vector((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
		}

		public static double Angle(Vector a, Vector b)
		{
			return Math.Acos(Dot(a, b) / (a.Magnitude() * b.Magnitude()));
		}

		public static double Distance(Vector a, Vector b)
		{
			return Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)) + ((a.Z - b.Z) * (a.Z - b.Z)));
		}

		public static Vector Projection(Vector a, Vector b)
		{
			return Dot(a, b) / Dot(b, b) * b;
		}

		public static Vector Rejection(Vector a, Vector b)
		{
			return a - Projection(a, b);
		}

		public Vector Rotate(
		  double pointX,
		  double pointY,
		  double pointZ,
		  double directionX,
		  double directionY,
		  double directionZ,
		  double angle)
		{
			double num1 = 1.0 / Math.Sqrt((directionX * directionX) + (directionY * directionY) + (directionZ * directionZ));
			directionX *= num1;
			directionY *= num1;
			directionZ *= num1;
			double num2 = Math.Cos(angle);
			double num3 = Math.Sin(angle);
			return new Vector((((pointX * ((directionY * directionY) + (directionZ * directionZ))) - (directionX * ((pointY * directionY) + (pointZ * directionZ) - (directionX * X) - (directionY * Y) - (directionZ * Z)))) * (1.0 - num2)) + (X * num2) + (((-pointZ * directionY) + (pointY * directionZ) - (directionZ * Y) + (directionY * Z)) * num3), (((pointY * ((directionX * directionX) + (directionZ * directionZ))) - (directionY * ((pointX * directionX) + (pointZ * directionZ) - (directionX * X) - (directionY * Y) - (directionZ * Z)))) * (1.0 - num2)) + (Y * num2) + (((pointZ * directionX) - (pointX * directionZ) + (directionZ * X) - (directionX * Z)) * num3), (((pointZ * ((directionX * directionX) + (directionY * directionY))) - (directionZ * ((pointX * directionX) + (pointY * directionY) - (directionX * X) - (directionY * Y) - (directionZ * Z)))) * (1.0 - num2)) + (Z * num2) + (((-pointY * directionX) + (pointX * directionY) - (directionY * X) + (directionX * Y)) * num3));
		}

		public Vector Rotate(Vector point, Vector direction, double angle)
		{
			return Rotate(point.X, point.Y, point.Z, direction.X, direction.Y, direction.Z, angle);
		}

		public Vector To(Vector a)
		{
			return a - this;
		}

		public Vector Unit()
		{
			return this / Magnitude();
		}

		public double Magnitude()
		{
			return Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
		}

		public static Vector Sum(ICollection<Vector> vectors)
		{
			var vector1 = new Vector();

			foreach (Vector vector2 in (IEnumerable<Vector>)vectors)
			{
				vector1 += vector2;
			}

			return vector1;
		}

		public static Vector Average(ICollection<Vector> vectors)
		{
			return Sum(vectors) / vectors.Count;
		}

		public override string ToString()
		{
			return "[" + X + " " + Y + " " + Z + "]";
		}

		public override bool Equals(object a)
		{
			return a is Vector a1 && Equals(a1);
		}

		public override int GetHashCode()
		{
			return (((BitConverter.DoubleToInt64Bits(X).GetHashCode() * 31) + BitConverter.DoubleToInt64Bits(Y).GetHashCode()) * 31) + BitConverter.DoubleToInt64Bits(Z).GetHashCode();
		}
	}
}
