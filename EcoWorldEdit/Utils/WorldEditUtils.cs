using System;
using System.IO;
using System.Text.RegularExpressions;
using Eco.Gameplay.Players;
using Eco.Shared.Math;
using Eco.Shared.Voxel;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class WorldEditUtils
	{
		public static bool ParseCoordinateArgs(User user, string args, out Vector3i pos)
		{
			pos = user.Position.Round();
			args = args.Trim().Replace(" ", ",");
			string[] coords = args.Split(',', StringSplitOptions.RemoveEmptyEntries);
			if (coords.Length < 3) { user.Player.ErrorLocStr($"Ivalid coordinates format: [{args}]"); return false; }
			if (!int.TryParse(coords[0], out int x)) { user.Player.ErrorLocStr($"Ivalid value for coordinate X given: [{coords[0]}]"); return false; }
			if (!int.TryParse(coords[1], out int y)) { user.Player.ErrorLocStr($"Ivalid value for coordinate Y given: [{coords[1]}]"); return false; }
			if (!int.TryParse(coords[2], out int z)) { user.Player.ErrorLocStr($"Ivalid value for coordinate Z given: [{coords[2]}]"); return false; }
			pos = new Vector3i(x, y, z);
			return true;
		}

		public static Direction ParseDirectionAndAmountArgs(User user, string args, out int amount)
		{
			Direction direction = Direction.Unknown;
			amount = 1;
			string[] splitted = args.Split(' ', ',');

			switch (splitted.Length)
			{
				case 1:
					if (!int.TryParse(splitted[0], out amount))
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, splitted[0]);
						amount = 1;
					}
					else
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, null);
					}
					break;
				case 2:
					if (!int.TryParse(splitted[0], out amount))
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, splitted[0]);
						if (!int.TryParse(splitted[1], out amount))
						{
							throw new ArgumentException("Expects direction and amount");
						}
					}
					else
					{
						direction = DirectionUtils.GetDirectionOrLooking(user, splitted[1]);
					}
					break;
				default:
					direction = DirectionUtils.GetDirectionOrLooking(user, null);
					break;
			}

			return direction;

			//if (!int.TryParse(splitted[0], out amount))
			//{
			//	user.Player.ErrorLocStr($"Please provide an amount first");
			//	return Direction.Unknown;
			//}

			//return DirectionUtils.GetDirectionOrLooking(user, splitted.Length >= 2 ? splitted[1] : null);
		}

		// Is there a correct name for this operation?
		public static int SumAllAxis(Vector3i pVector)
		{
			return pVector.X + pVector.Y + pVector.Z;
		}

		public static Vector2i SecondPlotPos(Vector2i plotPos)
		{
			return plotPos + PlotUtil.PropertyPlotLength - 1;
		}

		public static void OutputToTxtFile(string data, string fileName)
		{
			data = Regex.Replace(data, "<pos=[1-9][0-9]*em>", "\t");
			data = Regex.Replace(data, "<.*?>", String.Empty);

			if (!Directory.Exists(WorldEditManager.GetSchematicDirectory())) { Directory.CreateDirectory(WorldEditManager.GetSchematicDirectory()); }
			string file = WorldEditManager.GetSchematicFileName(fileName, ".txt");
			File.WriteAllText(file, data);
		}

		public static Vector3i? GetPositionForUser(User user, string inputCoord)
		{
			Vector3i pos;

			if (!String.IsNullOrEmpty(inputCoord))
			{
				if (!WorldEditUtils.ParseCoordinateArgs(user, inputCoord, out pos)) { return null; }
			}
			else
			{
				pos = user.Position.Round();
			}

			pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
			pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;
			pos.X %= Shared.Voxel.World.VoxelSize.X;
			pos.Z %= Shared.Voxel.World.VoxelSize.Z;

			return pos;
		}
	}
}
