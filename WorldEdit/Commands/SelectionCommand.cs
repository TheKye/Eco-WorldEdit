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

			if (!this.UserSession.FirstPos.HasValue || !this.UserSession.SecondPos.HasValue) { throw new WorldEditCommandException("Please set both points first!"); }

			this.direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out this.amount);
			if (this.direction == Direction.Unknown) { throw new WorldEditCommandException("Unable to determine direction"); }
		}

		protected override void Execute()
		{
			Vector3i vector;
			bool useFirst;
			switch (this._commandType)
			{
				case Command.SHIFT:
					vector = this.direction.ToVec() * this.amount;
					this.UserSession.SetFirstPosition(this.UserSession.FirstPos + vector);
					this.UserSession.SetSecondPosition(this.UserSession.SecondPos + vector);
					break;
				case Command.EXPAND:
					vector = this.direction.ToVec() * this.amount;
					useFirst = this.UseFirstPos(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value, vector);
					this.SetPos(useFirst, vector);
					break;
				case Command.CONTRACT:
					vector = this.direction.ToVec() * -this.amount;
					useFirst = this.UseFirstPos(this.UserSession.FirstPos.Value, this.UserSession.SecondPos.Value, vector, true);
					this.SetPos(useFirst, vector);
					break;
			}
		}

		private void SetPos(bool useFirst, Vector3i vector)
		{
			if (useFirst)
			{
				this.UserSession.SetFirstPosition(this.UserSession.FirstPos + vector);
			}
			else
			{
				this.UserSession.SetSecondPosition(this.UserSession.SecondPos + vector);
			}
		}

		private bool UseFirstPos(Vector3i firstPos, Vector3i secondPos, Vector3i direction, bool contract = false)
		{
			Vector3i pos1 = firstPos;
			Vector3i pos2 = secondPos;
			int typeOfVolume = WorldEditUtils.FindTypeOfVolume(ref pos1, ref pos2, out _, out _);
			var firstResult = WorldEditUtils.SumAllAxis(direction * firstPos);
			var secondResult = WorldEditUtils.SumAllAxis(direction * secondPos);

			bool useFirst = firstResult > secondResult;
			if (typeOfVolume == 1) useFirst = !useFirst;
			if (contract) useFirst = !useFirst;

			return useFirst;
		}
	}
}
