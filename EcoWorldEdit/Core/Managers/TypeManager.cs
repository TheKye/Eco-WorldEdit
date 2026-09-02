using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	internal class TypeManager : AutoSingleton<TypeManager>
	{
		public string GetString(Type type) => type.AssemblyQualifiedName ?? throw new NullReferenceException("Provided type not supported (generic)");
		public Type? GetType(string? typeString)
		{
			if (string.IsNullOrEmpty(typeString)) return null;
			try
			{
				return Type.GetType(typeString);
			}
			catch (Exception ex)
			{
				Logging.Exception(ex);
				return null;
			}
		}
	}
}
