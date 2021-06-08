using System;
using Eco.Gameplay.Players;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class WorldEditUtils
	{
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
	}
}
