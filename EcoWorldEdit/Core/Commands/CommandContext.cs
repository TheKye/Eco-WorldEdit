using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Shared.Math;

namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal sealed class CommandContext
	{
		private CommandScope? _primaryScope;
		private List<CommandScope>? _additionalScopes;

		public UserSession UserSession { get; }
		public User User => this.UserSession.User;
		public Player Player => this.UserSession.Player;
		public WorldRange Selection { get; }
		public CommandScope Scope => this._primaryScope ?? throw new InvalidOperationException("The command context has no initialized scope.");
		public CommandChangeSet Changes => this.Scope.Changes;
		public BlockManager BlockManager => this.Scope.BlockManager;
		public IEnumerable<CommandScope> Scopes
		{
			get
			{
				if (this._primaryScope is not null) yield return this._primaryScope;
				if (this._additionalScopes is null) yield break;
				foreach (CommandScope scope in this._additionalScopes) yield return scope;
			}
		}
		public int ChangedBlocks => this.Scopes.Sum(x => x.Changes.ChangedBlocks);

		public CommandContext(UserSession userSession, WorldRange selection)
		{
			this.UserSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
			this.Selection = selection;
		}

		public CommandScope CreateScope()
		{
			CommandScope scope = new(this.UserSession);
			if (this._primaryScope is null)
			{
				this._primaryScope = scope;
			}
			else
			{
				(this._additionalScopes ??= new()).Add(scope);
			}
			return scope;
		}
	}
}
