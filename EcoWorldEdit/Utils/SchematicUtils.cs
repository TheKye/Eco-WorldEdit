using Eco.Core.Plugins;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class SchematicUtils
	{
		public static string SanitizeFileName(string name) => new string(name.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());
		public static string GetSchematicDirectory() => Path.Combine(StorageManager.Config.StorageDirectory, EcoWorldEdit.SchematicDirectoryName);
		public static string GetSchematicFilePath(string name, string extension = EcoWorldEdit.SchematicDefaultExtension)
		{
			string fileName = SanitizeFileName(name);
			//Strip extension from the filename if provided as we will add it manually
			if (fileName.EndsWith(extension))
			{
				fileName = fileName[..^extension.Length];
			}
			return Path.Combine(GetSchematicDirectory(), fileName + extension);
		}
	}
}
