using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Serializer.Migrations;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	public class MigrationManager : AutoSingleton<MigrationManager>
	{
		private readonly object _syncRoot = new();
		private readonly List<IBlueprintVersionMigration> _blueprintVersionMigrations = new();
		private readonly List<IBlueprintEcoVersionMigration> _blueprintEcoVersionMigrations = new();
		private readonly HashSet<Type> _registeredMigrationTypes = new();
		private bool _initialized;

		public static void Register(IBlueprintEcoVersionMigration migration)
		{
			ArgumentNullException.ThrowIfNull(migration);
			Obj.RegisterInternal(migration);
		}

		public void Initialize()
		{
			lock (this._syncRoot)
			{
				if (this._initialized) throw new InvalidOperationException($"{nameof(MigrationManager)} is already initialized.");

				foreach (Type migrationType in typeof(IBlueprintVersionMigration).CreatableTypes(typeof(MigrationManager).Assembly))
				{
					this.RegisterInternal(CreateMigration(migrationType));
				}

				foreach (Type migrationType in typeof(IBlueprintEcoVersionMigration).CreatableTypes())
				{
					this.RegisterInternal(CreateMigration(migrationType));
				}

				this._initialized = true;
			}
		}

		internal IEnumerable<IBlueprintMigration> GetMigrations(MigrationInfo info)
		{
			lock (this._syncRoot)
			{
				Version currentEcoVersion = Version.Parse(WorldEditSerializer.CurrentEcoVersion);

				return this._blueprintVersionMigrations
					.Where(migration => migration.TargetVersion > info.Version && migration.TargetVersion <= WorldEditSerializer.CurrentVersion)
					.Cast<IBlueprintMigration>()
					.Concat(this._blueprintEcoVersionMigrations
						.Where(migration => migration.TargetVersion > info.EcoVersion && migration.TargetVersion <= currentEcoVersion))
					.OrderBy(migration => migration is IBlueprintVersionMigration ? 0 : 1)
					.ThenBy(migration => migration.TargetVersion)
					.ThenBy(migration => migration.Order)
					.ThenBy(migration => migration.GetType().FullName, StringComparer.Ordinal)
					.ToArray();
			}
		}

		private static IBlueprintMigration CreateMigration(Type migrationType)
		{
			return (IBlueprintMigration)(Activator.CreateInstance(migrationType) ?? throw new InvalidOperationException($"Unable to create migration {migrationType.FullName}."));
		}

		private void RegisterInternal(IBlueprintMigration migration)
		{
			lock (this._syncRoot)
			{
				bool isVersionMigration = migration is IBlueprintVersionMigration;
				bool isEcoVersionMigration = migration is IBlueprintEcoVersionMigration;
				if (isVersionMigration == isEcoVersionMigration)
				{
					throw new ArgumentException($"Migration {migration.GetType().FullName} must implement exactly one concrete migration interface.", nameof(migration));
				}

				if (!this._registeredMigrationTypes.Add(migration.GetType())) return;

				if (migration is IBlueprintVersionMigration versionMigration)
				{
					this._blueprintVersionMigrations.Add(versionMigration);
				}
				else
				{
					this._blueprintEcoVersionMigrations.Add((IBlueprintEcoVersionMigration)migration);
				}
			}
		}
	}
}
