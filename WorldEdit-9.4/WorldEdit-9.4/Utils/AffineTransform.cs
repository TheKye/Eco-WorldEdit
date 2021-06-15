using System;
using System.Runtime.CompilerServices;
using Eco.Shared;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Utils
{
	public class AffineTransform
	{
		/**
			 * coefficients for x coordinate.
			 */
		private float m00, m01, m02, m03;

		/**
		 * coefficients for y coordinate.
		 */
		private float m10, m11, m12, m13;

		/**
		 * coefficients for z coordinate.
		 */
		private float m20, m21, m22, m23;

		// ===================================================================
		// constructors

		/**
		 * Creates a new affine transform3D set to identity
		 */
		public AffineTransform()
		{
			// init to identity matrix
			this.m00 = this.m11 = this.m22 = 1;
			this.m01 = this.m02 = this.m03 = 0;
			this.m10 = this.m12 = this.m13 = 0;
			this.m20 = this.m21 = this.m23 = 0;
		}

		public AffineTransform(float[] coefs)
		{
			if (coefs.Length == 9)
			{
				this.m00 = coefs[0];
				this.m01 = coefs[1];
				this.m02 = coefs[2];
				this.m10 = coefs[3];
				this.m11 = coefs[4];
				this.m12 = coefs[5];
				this.m20 = coefs[6];
				this.m21 = coefs[7];
				this.m22 = coefs[8];
			}
			else if (coefs.Length == 12)
			{
				this.m00 = coefs[0];
				this.m01 = coefs[1];
				this.m02 = coefs[2];
				this.m03 = coefs[3];
				this.m10 = coefs[4];
				this.m11 = coefs[5];
				this.m12 = coefs[6];
				this.m13 = coefs[7];
				this.m20 = coefs[8];
				this.m21 = coefs[9];
				this.m22 = coefs[10];
				this.m23 = coefs[11];
			}
			else
			{
				throw new ArgumentException("Input array must have 9 or 12 elements");
			}
		}

		public AffineTransform(float xx, float yx, float zx, float tx,
							   float xy, float yy, float zy, float ty, float xz, float yz,
							   float zz, float tz)
		{
			this.m00 = xx;
			this.m01 = yx;
			this.m02 = zx;
			this.m03 = tx;
			this.m10 = xy;
			this.m11 = yy;
			this.m12 = zy;
			this.m13 = ty;
			this.m20 = xz;
			this.m21 = yz;
			this.m22 = zz;
			this.m23 = tz;
		}

		// ===================================================================
		// accessors


		public bool IsIdentity()
		{
			if (this.m00 != 1)
				return false;
			if (this.m11 != 1)
				return false;
			if (this.m22 != 1)
				return false;
			if (this.m01 != 0)
				return false;
			if (this.m02 != 0)
				return false;
			if (this.m03 != 0)
				return false;
			if (this.m10 != 0)
				return false;
			if (this.m12 != 0)
				return false;
			if (this.m13 != 0)
				return false;
			if (this.m20 != 0)
				return false;
			if (this.m21 != 0)
				return false;
			if (this.m23 != 0)
				return false;
			return true;
		}

		/**
		 * Returns the affine coefficients of the transform. Result is an array of
		 * 12 float.
		 */

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float[] Coefficients()
		{
			return new float[] { this.m00, this.m01, this.m02, this.m03, this.m10, this.m11, this.m12, this.m13, this.m20, this.m21, this.m22, this.m23 };
		}

		/**
		 * Computes the determinant of this transform. Can be zero.
		 *
		 * @return the determinant of the transform.
		 */
		private float Determinant()
		{
			return this.m00 * (this.m11 * this.m22 - this.m12 * this.m21) - this.m01 * (this.m10 * this.m22 - this.m20 * this.m12)
					+ this.m02 * (this.m10 * this.m21 - this.m20 * this.m11);
		}

		/**
		 * Computes the inverse affine transform.
		 */

		public AffineTransform Inverse()
		{
			float det = this.Determinant();
			return new AffineTransform(
					(this.m11 * this.m22 - this.m21 * this.m12) / det,
					(this.m21 * this.m01 - this.m01 * this.m22) / det,
					(this.m01 * this.m12 - this.m11 * this.m02) / det,
					(this.m01 * (this.m22 * this.m13 - this.m12 * this.m23) + this.m02 * (this.m11 * this.m23 - this.m21 * this.m13)
							- this.m03 * (this.m11 * this.m22 - this.m21 * this.m12)) / det,
					(this.m20 * this.m12 - this.m10 * this.m22) / det,
					(this.m00 * this.m22 - this.m20 * this.m02) / det,
					(this.m10 * this.m02 - this.m00 * this.m12) / det,
					(this.m00 * (this.m12 * this.m23 - this.m22 * this.m13) - this.m02 * (this.m10 * this.m23 - this.m20 * this.m13)
							+ this.m03 * (this.m10 * this.m22 - this.m20 * this.m12)) / det,
					(this.m10 * this.m21 - this.m20 * this.m11) / det,
					(this.m20 * this.m01 - this.m00 * this.m21) / det,
					(this.m00 * this.m11 - this.m10 * this.m01) / det,
					(this.m00 * (this.m21 * this.m13 - this.m11 * this.m23) + this.m01 * (this.m10 * this.m23 - this.m20 * this.m13)
							- this.m03 * (this.m10 * this.m21 - this.m20 * this.m11)) / det);
		}

		// ===================================================================
		// general methods

		/**
		 * Returns the affine transform created by applying first the affine
		 * transform given by {@code that}, then this affine transform.
		 *
		 * @param that the transform to apply first
		 * @return the composition this * that
		 */
		public AffineTransform Concatenate(AffineTransform that)
		{
			float n00 = this.m00 * that.m00 + this.m01 * that.m10 + this.m02 * that.m20;
			float n01 = this.m00 * that.m01 + this.m01 * that.m11 + this.m02 * that.m21;
			float n02 = this.m00 * that.m02 + this.m01 * that.m12 + this.m02 * that.m22;
			float n03 = this.m00 * that.m03 + this.m01 * that.m13 + this.m02 * that.m23 + this.m03;
			float n10 = this.m10 * that.m00 + this.m11 * that.m10 + this.m12 * that.m20;
			float n11 = this.m10 * that.m01 + this.m11 * that.m11 + this.m12 * that.m21;
			float n12 = this.m10 * that.m02 + this.m11 * that.m12 + this.m12 * that.m22;
			float n13 = this.m10 * that.m03 + this.m11 * that.m13 + this.m12 * that.m23 + this.m13;
			float n20 = this.m20 * that.m00 + this.m21 * that.m10 + this.m22 * that.m20;
			float n21 = this.m20 * that.m01 + this.m21 * that.m11 + this.m22 * that.m21;
			float n22 = this.m20 * that.m02 + this.m21 * that.m12 + this.m22 * that.m22;
			float n23 = this.m20 * that.m03 + this.m21 * that.m13 + this.m22 * that.m23 + this.m23;
			return new AffineTransform(
					n00, n01, n02, n03,
					n10, n11, n12, n13,
					n20, n21, n22, n23);
		}

		/**
		 * Return the affine transform created by applying first this affine
		 * transform, then the affine transform given by {@code that}.
		 *
		 * @param that the transform to apply in a second step
		 * @return the composition that * this
		 */
		public AffineTransform PreConcatenate(AffineTransform that)
		{
			float n00 = that.m00 * this.m00 + that.m01 * this.m10 + that.m02 * this.m20;
			float n01 = that.m00 * this.m01 + that.m01 * this.m11 + that.m02 * this.m21;
			float n02 = that.m00 * this.m02 + that.m01 * this.m12 + that.m02 * this.m22;
			float n03 = that.m00 * this.m03 + that.m01 * this.m13 + that.m02 * this.m23 + that.m03;
			float n10 = that.m10 * this.m00 + that.m11 * this.m10 + that.m12 * this.m20;
			float n11 = that.m10 * this.m01 + that.m11 * this.m11 + that.m12 * this.m21;
			float n12 = that.m10 * this.m02 + that.m11 * this.m12 + that.m12 * this.m22;
			float n13 = that.m10 * this.m03 + that.m11 * this.m13 + that.m12 * this.m23 + that.m13;
			float n20 = that.m20 * this.m00 + that.m21 * this.m10 + that.m22 * this.m20;
			float n21 = that.m20 * this.m01 + that.m21 * this.m11 + that.m22 * this.m21;
			float n22 = that.m20 * this.m02 + that.m21 * this.m12 + that.m22 * this.m22;
			float n23 = that.m20 * this.m03 + that.m21 * this.m13 + that.m22 * this.m23 + that.m23;
			return new AffineTransform(
					n00, n01, n02, n03,
					n10, n11, n12, n13,
					n20, n21, n22, n23);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform Translate(Vector3i vec)
		{
			return this.Translate(vec.x, vec.y, vec.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform Translate(float x, float y, float z)
		{
			return this.Concatenate(new AffineTransform(1, 0, 0, x, 0, 1, 0, y, 0, 0, 1, z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform RotateXDeg(float pAngleDeg)
		{
			return this.RotateX(Mathf.DegToRad(pAngleDeg));
		}

		public AffineTransform RotateX(float theta)
		{
			float cot = Mathf.Cos(theta);
			float sit = Mathf.Sin(theta);
			return this.Concatenate(
					new AffineTransform(
							1, 0, 0, 0,
							0, cot, -sit, 0,
							0, sit, cot, 0));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform RotateYDeg(float pAngleDeg)
		{
			return this.RotateY(Mathf.DegToRad(pAngleDeg));
		}

		public AffineTransform RotateY(float theta)
		{
			float cot = Mathf.Cos(theta);
			float sit = Mathf.Sin(theta);
			return this.Concatenate(
					new AffineTransform(
							cot, 0, sit, 0,
							0, 1, 0, 0,
							-sit, 0, cot, 0));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform RotateZDeg(float pAngleDeg)
		{
			return this.RotateZ(Mathf.DegToRad(pAngleDeg));
		}

		public AffineTransform RotateZ(float theta)
		{
			float cot = Mathf.Cos(theta);
			float sit = Mathf.Sin(theta);
			return this.Concatenate(
					new AffineTransform(
							cot, -sit, 0, 0,
							sit, cot, 0, 0,
							0, 0, 1, 0));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform Scale(float s)
		{
			return this.Scale(s, s, s);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform Scale(float sx, float sy, float sz)
		{
			return this.Concatenate(new AffineTransform(sx, 0, 0, 0, 0, sy, 0, 0, 0, 0, sz, 0));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform Scale(Vector3i vec)
		{
			return this.Scale(vec.x, vec.y, vec.z);
		}

		public Vector3i Apply(Vector3i vector)
		{
			return new Vector3i(
				   (int)Math.Round(vector.x * this.m00 + vector.y * this.m01 + vector.Z * this.m02 + this.m03),
					  (int)Math.Round(vector.x * this.m10 + vector.y * this.m11 + vector.Z * this.m12 + this.m13),
					 (int)Math.Round(vector.x * this.m20 + vector.y * this.m21 + vector.Z * this.m22 + this.m23));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AffineTransform Combine(AffineTransform other)
		{
			return this.Concatenate(other);
		}

		public override string ToString()
		{
			return String.Format("Affine[%g %g %g %g, %g %g %g %g, %g %g %g %g]}", this.m00, this.m01, this.m02, this.m03, this.m10, this.m11, this.m12, this.m13, this.m20, this.m21, this.m22, this.m23);
		}
	}
}
