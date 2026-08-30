using Newtonsoft.Json.Linq;

namespace Eco.Mods.WorldEdit.Serializer.Migrations
{
	public interface IBlueprintMigration
	{
		Version TargetVersion { get; }
		int Order { get; }

		void Migrate(JObject root, MigrationInfo info);
	}

	public interface IBlueprintVersionMigration : IBlueprintMigration;
	public interface IBlueprintEcoVersionMigration : IBlueprintMigration;
}
