using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class SelectionCommand : WorldEditCommand
	{
		private enum Command { EXPAND, CONTRACT, SHIFT }
		private readonly Command _commandType;
		public readonly Direction direction;
		public readonly int amount;

		public static SelectionCommand ShiftCommand(User user, string args)
		{
			return new SelectionCommand(user, Command.SHIFT, args);
		}

		public static SelectionCommand ExpandCommand(User user, string args)
		{
			return new SelectionCommand(user, Command.EXPAND, args);
		}

		public static SelectionCommand ContractCommand(User user, string args)
		{
			return new SelectionCommand(user, Command.CONTRACT, args);
		}

		private SelectionCommand(User user, Command commandType, string args) : base(user)
		{
			this._commandType = commandType;

			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown || (this._commandType == Command.SHIFT && this.direction == Direction.None)) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute()
		{
			Vector3i vector = this.direction.ToVec() * this.amount;
			WorldRange range = this.UserSession.Selection;
			switch (this._commandType)
			{
				case Command.SHIFT:
					range.min += vector;
					range.max += vector;
					break;
				case Command.EXPAND:
					switch (this.direction)
					{
						case Direction.Left:
						case Direction.Back:
						case Direction.Down:
							if (range.min.x <= range.max.x) range.min.x += vector.x; else range.max.x += vector.x;
							if (range.min.y <= range.max.y) range.min.y += vector.y; else range.max.y += vector.y;
							if (range.min.z <= range.max.z) range.min.z += vector.z; else range.max.z += vector.z;
							//range.min += vector;
							break;
						case Direction.Right:
						case Direction.Forward:
						case Direction.Up:
							if (range.min.x <= range.max.x) range.max.x += vector.x; else range.min.x += vector.x;
							if (range.min.y <= range.max.y) range.max.y += vector.y; else range.min.y += vector.y;
							if (range.min.z <= range.max.z) range.max.z += vector.z; else range.min.z += vector.z;
							//range.max += vector;
							break;
						case Direction.None:
							if (range.min.x <= range.max.x)
							{
								range.min.x -= amount; range.max.x += amount;
							}
							else
							{
								range.max.x -= amount; range.min.x += amount;
							}

							if (range.min.y <= range.max.y)
							{
								range.min.y -= amount; range.max.y += amount;
							}
							else
							{
								range.max.y -= amount; range.min.y += amount;
							}

							if (range.min.z <= range.max.z)
							{
								range.min.z -= amount; range.max.z += amount;
							}
							else
							{
								range.max.z -= amount; range.min.z += amount;
							}
							//range.min -= amount;
							//range.max += amount;
							break;
						default:
							throw new WorldEditCommandException("Unable to determine direction");
					}
					break;
				case Command.CONTRACT:
					switch (this.direction)
					{
						case Direction.Left:
						case Direction.Back:
						case Direction.Down:
							if (range.min.x <= range.max.x) range.max.x += vector.x; else range.min.x += vector.x;
							if (range.min.y <= range.max.y) range.max.y += vector.y; else range.min.y += vector.y;
							if (range.min.z <= range.max.z) range.max.z += vector.z; else range.min.z += vector.z;
							//range.max += vector;
							break;
						case Direction.Right:
						case Direction.Forward:
						case Direction.Up:
							if (range.min.x <= range.max.x) range.min.x += vector.x; else range.max.x += vector.x;
							if (range.min.y <= range.max.y) range.min.y += vector.y; else range.max.y += vector.y;
							if (range.min.z <= range.max.z) range.min.z += vector.z; else range.max.z += vector.z;
							//range.min += vector;
							break;
						case Direction.None:
							if (range.min.x <= range.max.x)
							{
								range.min.x += amount; range.max.x -= amount;
							}
							else
							{
								range.max.x += amount; range.min.x -= amount;
							}

							if (range.min.y <= range.max.y)
							{
								range.min.y += amount; range.max.y -= amount;
							}
							else
							{
								range.max.y += amount; range.min.y -= amount;
							}

							if (range.min.z <= range.max.z)
							{
								range.min.z += amount; range.max.z -= amount;
							}
							else
							{
								range.max.z += amount; range.min.z -= amount;
							}
							//range.min += amount;
							//range.max -= amount;
							break;
						default:
							throw new WorldEditCommandException("Unable to determine direction");
					}
					break;
			}
			this.UserSession.SetSelection(range);
		}
	}
}
