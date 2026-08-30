namespace Eco.Mods.WorldEdit.Utils.Exceptions
{
	internal class WorldEditCommandException : Exception
	{
		public WorldEditCommandException(string message) : base(message) { }
		public WorldEditCommandException(string message, Exception innerException) : base(message, innerException) { }
	}
}
