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
			if (string.IsNullOrWhiteSpace(direction)) return Direction.Unknown;

			return direction.Trim().ToLower() switch
			{
				"up" or "u" => Direction.Up,
				"down" or "d" => Direction.Down,
				"left" or "l" => Direction.Left,
				"right" or "r" => Direction.Right,
				"back" or "b" => Direction.Back,
				"forward" or "f" => Direction.Forward,
				"all" or "a" => Direction.None,
				_ => Direction.Unknown,
			};
		}

		public static Direction GetDirectionOrLooking(User user, string direction)
		{
			return string.IsNullOrWhiteSpace(direction) ? GetLookingDirection(user) : GetDirection(direction);
		}
	}
}
