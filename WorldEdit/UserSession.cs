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

		public Vector3i? FirstPos { get; private set; }
		public Vector3i? SecondPos { get; private set; }

		public WorldEditClipboard Clipboard { get; }
		public AuthorInformation AuthorInfo { get; private set; }
		public LimitedStack<WorldEditCommand> ExecutedCommands { get => this.executedCommands; }
		private LimitedStack<WorldEditCommand> executedCommands = new LimitedStack<WorldEditCommand>(10);

		public UserSession(User user)
		{
			this.User = user ?? throw new ArgumentNullException(nameof(user));
			this.Clipboard = new WorldEditClipboard();
			this.AuthorInfo = new AuthorInformation(this.User);
		}

		public void SetFirstPosition(Vector3i? value)
		{
			this.FirstPos = value;
		}
		public void SetSecondPosition(Vector3i? value)
		{
			this.SecondPos = value;
		}
		public void SetImportedSchematicAuthor(AuthorInformation information)
		{
			this.AuthorInfo = information;
		}
	}
}
