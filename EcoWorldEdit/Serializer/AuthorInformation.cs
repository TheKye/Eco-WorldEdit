using Eco.Gameplay.Players;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Serializer
{
	internal class AuthorInformation
	{
		public string Name { get; private set; }
		public string SlgID { get; private set; }
		public string SteamID { get; private set; }

		public AuthorInformation(User user)
		{
			this.Name = user.Player.DisplayName;
			this.SlgID = user.StrangeId;
			this.SteamID = user.SteamId;
		}

		[JsonConstructor]
		public AuthorInformation(string name, string slgID, string steamID)
		{
			this.Name = name;
			this.SlgID = slgID;
			this.SteamID = steamID;
		}

		public static AuthorInformation Unowned()
		{
			return new AuthorInformation("Unowned", string.Empty, string.Empty);
		}
	}
}
