using System;

namespace Eco.Mods.WorldEdit.Commands
{
	internal class WorldEditCommandException : Exception
	{
		public WorldEditCommandException(string message) : base(message)
		{
		}

		public WorldEditCommandException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
