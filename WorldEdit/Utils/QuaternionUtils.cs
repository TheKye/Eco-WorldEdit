﻿using Eco.Shared;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class QuaternionUtils
	{
		public static Quaternion FromAxisAngle(Vector3 axis, float angleRadian)
		{
			Quaternion q = new Quaternion();
			float m = axis.Magnitude;
			if (m > 0.0001)
			{
				float ca = Mathf.Cos(angleRadian / 2);
				float sa = Mathf.Sin(angleRadian / 2);
				q.X = axis.X / m * sa;
				q.Y = axis.Y / m * sa;
				q.Z = axis.Z / m * sa;
				q.W = ca;
			}
			else
			{
				q.W = 1; q.X = 0; q.Y = 0; q.Z = 0;
			}
			return q;
		}
	}
}
