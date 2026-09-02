using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Mods.WorldEdit.Utils.Exceptions;

namespace Eco.Mods.WorldEdit.Commands.Buffer
{
	[ChatCommandHandler]
	internal sealed class ExportCommand(string FileName) : IWorldEditCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Exports your clipboard to a shareable blueprint file.", shortCut: "exportbp", level: ChatAuthorizationLevel.Admin)]
		public static void Export(User user, string fileName)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new ExportCommand(SchematicUtils.GetSchematicFilePath(fileName)));
				if (result.Result.Success) Logging.Success($"Export done in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Clipboard clipboard = context.UserSession.Clipboard;
			if (clipboard.Count == 0) throw new WorldEditCommandException("Please /copy a selection first!");
			Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);
			using FileStream stream = File.Create(FileName);
			new WorldEditSerializer().Serialize(stream, clipboard.ToBlueprint());
		}
	}
}
