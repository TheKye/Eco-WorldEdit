using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Model.BlockData;
using Eco.Mods.WorldEdit.Model.Components;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.IoC;
using Eco.Shared.Logging;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	internal class WorldEditManager : AutoSingleton<WorldEditManager>
	{
		private DataTypeRegistry<BlockDataType, IBlockData>? _blockDataRegistry;
		public DataTypeRegistry<BlockDataType, IBlockData> BlockDataRegistry => this._blockDataRegistry ?? throw new InvalidOperationException($"{nameof(WorldEditManager)} is not initialized.");

		private DataTypeRegistry<WorldObjectComponentType, IWorldObjectComponentData>? _componentRegistry;
		public DataTypeRegistry<WorldObjectComponentType, IWorldObjectComponentData> ComponentRegistry => this._componentRegistry ?? throw new InvalidOperationException($"{nameof(WorldEditManager)} is not initialized.");

		private readonly Dictionary<int, UserSession> _userSessions = new Dictionary<int, UserSession>();

		public Dictionary<string, EcoBlueprintInfo> BlueprintList { get; } = new Dictionary<string, EcoBlueprintInfo>();

		public void Initialize()
		{
			if (this._blockDataRegistry is not null || this._componentRegistry is not null) throw new InvalidOperationException($"{nameof(WorldEditManager)} is already initialized.");

			DataTypeRegistry<BlockDataType, IBlockData> blockDataRegistry = new();
			DataTypeRegistry<WorldObjectComponentType, IWorldObjectComponentData> componentRegistry = new();

			this._blockDataRegistry = blockDataRegistry;
			this._componentRegistry = componentRegistry;

			this.CleanupOrphanedHighlightingObjects();
			UserManager.OnUserLoggedOut.Add(this.OnUserLoggedOut);
		}

		public UserSession GetUserSession(User user)
		{
			if (!this._userSessions.TryGetValue(user.Id, out UserSession? session))
			{
				session = new UserSession(user);
				this._userSessions.Add(user.Id, session);
			}
			return session;
		}

		private void OnUserLoggedOut(User user)
		{
			if (!this._userSessions.Remove(user.Id, out UserSession? session)) return;
			session.DestroyHighlightingObject();
		}

		private void CleanupOrphanedHighlightingObjects()
		{
			WorldEditHighlightingObject[] objects = ServiceHolder<IWorldObjectManager>.Obj.All.OfType<WorldEditHighlightingObject>().ToArray();
			foreach (WorldEditHighlightingObject highlightingObject in objects)
				highlightingObject.Destroy();

			if (objects.Length > 0) Log.WriteLineLoc($"WorldEdit removed {objects.Length} orphaned highlighting object(s).");
		}

		public void UpdateBlueprintList()
		{
			string schematicsPath = SchematicUtils.GetSchematicDirectory();
			if (!Directory.Exists(schematicsPath))
			{
				this.BlueprintList.Clear();
				return;
			}
			string[] list = Directory.GetFiles(schematicsPath, $"*{EcoWorldEdit.SchematicDefaultExtension}", SearchOption.AllDirectories);

			string[] toRemove = this.BlueprintList.Keys.Where(k => !list.Contains(k)).ToArray();
			foreach (string file in toRemove)
			{
				this.BlueprintList.Remove(file);
			}

			WorldEditSerializer serializer = new WorldEditSerializer();
			foreach (string file in list)
			{
				EcoBlueprintInfo blueprintInfo = serializer.DeserializeInfo(file);

				try
				{
					if (!this.BlueprintList.TryAdd(file, blueprintInfo))
					{
						this.BlueprintList[file] = blueprintInfo;
					}
				}
				catch (Exception e) { Log.WriteWarningLineLoc($"Unable to load file [{file}] error: {e.Message}"); }
			}
		}
	}
}
