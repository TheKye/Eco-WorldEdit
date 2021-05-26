using Eco.Gameplay.Players;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class DirectionUtils
	{
		/// <summary>
		/// Get Looking Direction of User 
		/// <para>Can be Left, Right, Forward, Backward, Up, Down</para>
		/// </summary>
		public static Direction GetLookingDirection(User user)
		{
			float direction = user.Rotation.Forward.Y;

			if (direction > 0.85)
			{
				return Direction.Up;
			}
			else if (direction < -0.85)
			{
				return Direction.Down;
			}

			return user.FacingDir;
		}

		/// <summary>
		/// For example "Up" or "u" to  <see cref="Direction.Up"/>. <see cref="Direction.Unknown"/> if direction is unkown
		/// </summary>
		public static Direction GetDirection(string direction)
		{
			if (string.IsNullOrWhiteSpace(direction))
				return Direction.Unknown;

			switch (direction.Trim().ToLower())
			{
				case "up":
				case "u":
					return Direction.Up;

				case "down":
				case "d":
					return Direction.Down;

				case "left":
				case "l":
					return Direction.Left;

				case "right":
				case "r":
					return Direction.Right;

				case "back":
				case "b":
					return Direction.Back;

				case "forward":
				case "f":
					return Direction.Forward;

				default:
					return Direction.Unknown;
			}
		}

		public static Direction GetDirectionOrLooking(User user, string direction)
		{
			if (string.IsNullOrWhiteSpace(direction))
				return GetLookingDirection(user);

			return GetDirection(direction);
		}
	}
}
