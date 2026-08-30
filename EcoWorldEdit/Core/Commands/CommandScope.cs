using Eco.Mods.WorldEdit.Core.Managers;

namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal sealed class CommandScope
	{
		public CommandChangeSet Changes { get; }
		public BlockManager BlockManager { get; }

		internal CommandScope(UserSession userSession)
		{
			ArgumentNullException.ThrowIfNull(userSession);

			this.Changes = new CommandChangeSet();
			this.BlockManager = new BlockManager(userSession, this.Changes);
		}
	}
}
