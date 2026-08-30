using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class CommandParsing
	{
		public static Vector3i GetPosition(User user, string? coordinates = null)
		{
			Vector3i position = user.Position.Round();
			if (!string.IsNullOrWhiteSpace(coordinates))
			{
				string[] values = coordinates.Trim().Replace(" ", ",").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (values.Length != 3 || !int.TryParse(values[0], out int x) || !int.TryParse(values[1], out int y) || !int.TryParse(values[2], out int z)) throw new WorldEditCommandException($"Invalid coordinates format: [{coordinates}]");
				position = new Vector3i(x, y, z);
			}

			Vector3i size = Shared.Voxel.World.VoxelSize;
			position.x = ((position.x % size.x) + size.x) % size.x;
			position.z = ((position.z % size.z) + size.z) % size.z;
			return position;
		}

		public static Direction GetLookingDirection(User user)
		{
			float vertical = user.Rotation.Forward.Y;
			return vertical > 0.85f ? Direction.Up : vertical < -0.85f ? Direction.Down : user.FacingDir;
		}

		public static (Direction Direction, int Amount) ParseDirectionAndAmount(User user, string? args)
		{
			string[] parts = (args ?? string.Empty).Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			int amount = 1;
			Direction direction = GetLookingDirection(user);
			foreach (string part in parts)
			{
				if (int.TryParse(part, out int parsedAmount)) amount = parsedAmount;
				else direction = ParseDirection(part);
			}
			if (amount < 0) throw new WorldEditCommandException("Amount must not be negative.");
			return (direction, amount);
		}

		public static Direction ParseDirection(string value) => value.Trim().ToLowerInvariant() switch
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

		public static Vector2i SecondPlotPosition(Vector2i plotPosition) => plotPosition + Shared.Voxel.PlotUtil.PropertyPlotLength - 1;
	}
}
