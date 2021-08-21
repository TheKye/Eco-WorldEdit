using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eco.Core.Plugins;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Serializer;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit
{
	internal class WorldEditManager
	{
		//protected static int mDefaultWorldSaveFrequency;
		//protected static bool mWorldSaveStopped = false;
		//protected static object mLocker = new object();
		public static Dictionary<string, EcoBlueprintInfo> BlueprintList { get; } = new Dictionary<string, EcoBlueprintInfo>();

		private static readonly Dictionary<int, UserSession> UserSessions = new Dictionary<int, UserSession>();

		public static ItemStack GetWandItemStack()
		{
			Item item = Item.Get("WandAxeItem");
			return new ItemStack(item, 1);
		}

		public static UserSession GetUserSession(User user)
		{
			UserSession session;

			//Log.Debug($"GetUserSession UserID: {user.Id}");

			if (!UserSessions.TryGetValue(user.Id, out session))
			{
				session = new UserSession(user);
				UserSessions.Add(user.Id, session);
			}

			return session;
		}

		public static void UpdateBlueprintList()
		{
			string schematicsPath = GetSchematicDirectory();
			if (!Directory.Exists(schematicsPath)) { BlueprintList.Clear(); return; }
			string[] list = Directory.GetFiles(schematicsPath, $"*{EcoWorldEdit.SchematicDefaultExtension}", SearchOption.AllDirectories);

			string[] toRemove = BlueprintList.Keys.Where(k => !list.Contains(k)).ToArray();
			foreach (string file in toRemove)
			{
				BlueprintList.Remove(file);
			}

			foreach (string file in list)
			{
				try
				{
					if (BlueprintList.TryGetValue(file, out EcoBlueprintInfo existInfo))
					{
						FileInfo info = new FileInfo(file);
						if (!info.Exists) { throw new FileNotFoundException("File not found", file); }
						if (existInfo.FileCreatedDate != info.CreationTime ||
							existInfo.FileChangedDate != info.LastWriteTime ||
							existInfo.FileSize != info.Length)
						{
							BlueprintList[file] = EcoBlueprintInfo.FromFile(file);
						}
					}
					else
					{
						BlueprintList.Add(file, EcoBlueprintInfo.FromFile(file));
					}
				}
				catch (Exception e) { Log.WriteWarningLineLoc($"Unable to load file [{file}] error: {e.Message}"); }
			}
		}

		public static string SanitizeFileName(string name)
		{
			return new string(name.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());
		}
		public static string GetSchematicDirectory()
		{
			return Path.Combine(StorageManager.Config.StorageDirectory, EcoWorldEdit.SchematicDirectoryName);
		}

		public static string GetSchematicFileName(string name, string extension = EcoWorldEdit.SchematicDefaultExtension)
		{
			string fileName = SanitizeFileName(name);
			return Path.Combine(GetSchematicDirectory(), fileName + extension);
		}
	}
}
