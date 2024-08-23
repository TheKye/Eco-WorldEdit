using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class SelectionCommand : WorldEditCommand
	{
		private enum Command { EXPAND, REDUCE, SHIFT }
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

		public static SelectionCommand ReduceCommand(User user, string args)
		{
			return new SelectionCommand(user, Command.REDUCE, args);
		}

		private SelectionCommand(User user, Command commandType, string args) : base(user)
		{
			this._commandType = commandType;

			if (!this.UserSession.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown || (this._commandType == Command.SHIFT && this.direction == Direction.None)) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute(WorldRange selection)
		{
			Vector3i vector = this.direction.ToVec() * this.amount;
			switch (this._commandType)
			{
				case Command.SHIFT:
					selection.min += vector;
					selection.max += vector;
					break;
				case Command.EXPAND:
					switch (this.direction)
					{
						case Direction.Left:
						case Direction.Back:
						case Direction.Down:
							if (selection.min.x <= selection.max.x) selection.min.x += vector.x; else selection.max.x += vector.x;
							if (selection.min.y <= selection.max.y) selection.min.y += vector.y; else selection.max.y += vector.y;
							if (selection.min.z <= selection.max.z) selection.min.z += vector.z; else selection.max.z += vector.z;
							//selection.min += vector;
							break;
						case Direction.Right:
						case Direction.Forward:
						case Direction.Up:
							if (selection.min.x <= selection.max.x) selection.max.x += vector.x; else selection.min.x += vector.x;
							if (selection.min.y <= selection.max.y) selection.max.y += vector.y; else selection.min.y += vector.y;
							if (selection.min.z <= selection.max.z) selection.max.z += vector.z; else selection.min.z += vector.z;
							//selection.max += vector;
							break;
						case Direction.None:
							if (selection.min.x <= selection.max.x)
							{
								selection.min.x -= this.amount; selection.max.x += this.amount;
							}
							else
							{
								selection.max.x -= this.amount; selection.min.x += this.amount;
							}

							if (selection.min.y <= selection.max.y)
							{
								selection.min.y -= this.amount; selection.max.y += this.amount;
							}
							else
							{
								selection.max.y -= this.amount; selection.min.y += this.amount;
							}

							if (selection.min.z <= selection.max.z)
							{
								selection.min.z -= this.amount; selection.max.z += this.amount;
							}
							else
							{
								selection.max.z -= this.amount; selection.min.z += this.amount;
							}
							//selection.min -= amount;
							//selection.max += amount;
							break;
						default:
							throw new WorldEditCommandException("Unable to determine direction");
					}
					break;
				case Command.REDUCE:
					switch (this.direction)
					{
						case Direction.Left:
						case Direction.Back:
						case Direction.Down:
							if (selection.min.x <= selection.max.x) selection.max.x += vector.x; else selection.min.x += vector.x;
							if (selection.min.y <= selection.max.y) selection.max.y += vector.y; else selection.min.y += vector.y;
							if (selection.min.z <= selection.max.z) selection.max.z += vector.z; else selection.min.z += vector.z;
							//selection.max += vector;
							break;
						case Direction.Right:
						case Direction.Forward:
						case Direction.Up:
							if (selection.min.x <= selection.max.x) selection.min.x += vector.x; else selection.max.x += vector.x;
							if (selection.min.y <= selection.max.y) selection.min.y += vector.y; else selection.max.y += vector.y;
							if (selection.min.z <= selection.max.z) selection.min.z += vector.z; else selection.max.z += vector.z;
							//selection.min += vector;
							break;
						case Direction.None:
							if (selection.min.x <= selection.max.x)
							{
								selection.min.x += this.amount; selection.max.x -= this.amount;
							}
							else
							{
								selection.max.x += this.amount; selection.min.x -= this.amount;
							}

							if (selection.min.y <= selection.max.y)
							{
								selection.min.y += this.amount; selection.max.y -= this.amount;
							}
							else
							{
								selection.max.y += this.amount; selection.min.y -= this.amount;
							}

							if (selection.min.z <= selection.max.z)
							{
								selection.min.z += this.amount; selection.max.z -= this.amount;
							}
							else
							{
								selection.max.z += this.amount; selection.min.z -= this.amount;
							}
							//selection.min += amount;
							//selection.max -= amount;
							break;
						default:
							throw new WorldEditCommandException("Unable to determine direction");
					}
					break;
			}
			this.UserSession.SetSelection(selection);
		}
	}
}
