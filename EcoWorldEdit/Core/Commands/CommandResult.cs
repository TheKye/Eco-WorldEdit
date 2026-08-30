using Eco.Core.Utils;

namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal sealed record CommandResult(Result Result, long BlocksChanged, TimeSpan Elapsed);
}
