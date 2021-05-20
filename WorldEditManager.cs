using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;

namespace Eco.Mods.WorldEdit
{
	internal class WorldEditManager
	{
		//protected static int mDefaultWorldSaveFrequency;
		protected static bool mWorldSaveStopped = false;
		protected static object mLocker = new object();

		private static readonly Dictionary<int, UserSession> UserSessions = new Dictionary<int, UserSession>();


		public static ItemStack getWandItemStack()
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

		public static string GetSchematicFileName(string name)
		{
			string fileName = new string(name.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());
			return Path.Combine(EcoWorldEdit.SchematicDirectoryPath, fileName + ".ecobp");
		}
	}
}
