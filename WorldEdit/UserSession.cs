using System;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Commands;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit
{
	internal class UserSession
	{
		public User User { get; private set; }
		public Player Player => this.User.Player;

		public WorldRange Selection { get; private set; } = WorldRange.Invalid;

		public WorldEditClipboard Clipboard { get; } = new WorldEditClipboard();
		public AuthorInformation AuthorInfo { get; private set; }
		public LimitedStack<WorldEditCommand> ExecutedCommands { get => this.executedCommands; }
		private LimitedStack<WorldEditCommand> executedCommands = new LimitedStack<WorldEditCommand>(10);
		public WorldEditCommand ExecutingCommand { get => this.executingCommand; internal set => this.executingCommand = value; }
		private volatile WorldEditCommand executingCommand = null;

		public UserSession(User user)
		{
			this.User = user ?? throw new ArgumentNullException(nameof(user));
			this.AuthorInfo = new AuthorInformation(this.User);
		}

		public void SetFirstPosition(Vector3i pos)
		{
			WorldRange range = this.Selection;
			range.min = pos;
			this.SetSelection(range);
		}
		public void SetSecondPosition(Vector3i pos)
		{
			WorldRange range = this.Selection;
			range.max = pos;
			this.SetSelection(range);
		}

		public void ResetSelection()
		{
			this.SetSelection(WorldRange.Invalid);
		}

		public void SetSelection(WorldRange range)
		{
			this.Selection = range;
		}

		public void SetImportedSchematicAuthor(AuthorInformation information)
		{
			this.AuthorInfo = information;
		}
	}
}
