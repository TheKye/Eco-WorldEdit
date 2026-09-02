using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Commands.Info;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Model;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils.Exceptions;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class ImportCommand(string FileName) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Imports a blueprint file into your clipboard.", shortCut: "importbp", level: ChatAuthorizationLevel.Admin)]
		public static void Import(User user, string? fileName = null)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(fileName))
				{
					CommandResult result = CommandDispatcher.Obj.Execute(user, new ImportCommand(SchematicUtils.GetSchematicFilePath(fileName)));
					if (result.Result.Success)
					{
						Logging.Success($"Import done in {result.Elapsed.TotalMilliseconds}ms. Use /paste", user.Player);
						return;
					}
					else
					{
						Logging.Error(result.Result.Message, user.Player);
						BlueprintListCommand.Print(user);
					}
				}
				else
				{
					BlueprintListCommand.Print(user);
				}
			}
			catch (Exception e) { Logging.Exception(e, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			if (!File.Exists(FileName)) throw new WorldEditCommandException($"Schematic file {FileName} not found!");
			WorldEditSerializer serializer = new WorldEditSerializer();
			EcoBlueprint blueprint = serializer.Deserialize(FileName);
			context.UserSession.Clipboard = Clipboard.Create(blueprint);
		}
	}
}
