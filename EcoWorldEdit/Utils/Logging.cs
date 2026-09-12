using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using Eco.Shared.Services;

namespace Eco.Mods.WorldEdit.Utils
{
	internal static class Logging
	{
		public static void Success(FormattableString message, Player? player = null)
		{
#if DEBUG
			Log.WriteLineLoc(message);
#endif
			Notify(player, message, NotificationStyle.Chat);
		}

		public static void Success(LocString message, Player? player = null)
		{
#if DEBUG
			Log.WriteLine(message);
#endif
			Notify(player, message, NotificationStyle.Chat);
		}

		public static void SuccessLocStr(string message, Player? player = null) => Success(Localizer.DoStr(message), player);

		public static void CommandFailed(string commandName, CommandResult result, Player? player = null, bool recommendUndo = true)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
			ArgumentNullException.ThrowIfNull(result);
			FormattableString message;
			if (result.BlocksChanged > 0 && recommendUndo)
			{
				message = $"{commandName} stopped with an error after changing {result.BlocksChanged} blocks in {result.Elapsed.TotalMilliseconds}ms. Run /undo to revert the partial changes. Reason: {result.Result.Message}";
			}
			else if (result.BlocksChanged > 0)
			{
				message = $"{commandName} stopped with an error after changing {result.BlocksChanged} blocks in {result.Elapsed.TotalMilliseconds}ms. Reason: {result.Result.Message}";
			}
			else
			{
				message = $"{commandName} failed in {result.Elapsed.TotalMilliseconds}ms. Reason: {result.Result.Message}";
			}
			Log.WriteErrorLineLoc(message);
			Notify(player, message, NotificationStyle.Chat);
		}

		public static void Info(FormattableString message) => Log.WriteLineLoc(message);

		public static void Info(LocString message) => Log.WriteLine(message);

		public static void InfoLocStr(string message) => Info(Localizer.DoStr(message));

		public static void Error(FormattableString message, Player? player = null)
		{
			Log.WriteErrorLineLoc(message);
			Notify(player, message, NotificationStyle.Error);
		}

		public static void Error(LocString message, Player? player = null)
		{
			Log.WriteErrorLine(message);
			Notify(player, message, NotificationStyle.Error);
		}

		public static void ErrorLocStr(string message, Player? player = null) => Error(Localizer.DoStr(message), player);

		public static void Warning(FormattableString message, Player? player = null)
		{
			Log.WriteWarningLineLoc(message);
			Notify(player, message, NotificationStyle.Error);
		}

		public static void Warning(LocString message, Player? player = null)
		{
			Log.WriteWarningLine(message);
			Notify(player, message, NotificationStyle.Error);
		}

		public static void Exception(Exception exception, Player? player = null)
		{
			Log.WriteException(exception);
			Notify(player, Localizer.DoStr("WorldEdit encountered an exception. Check the server log for details."), NotificationStyle.Error);
		}

		private static void Notify(Player? player, FormattableString message, NotificationStyle style)
		{
			if (player is null) return;
			try { player.MsgLoc(message, style); }
			catch (Exception exception) { Log.WriteException(exception); }
		}

		private static void Notify(Player? player, LocString message, NotificationStyle style)
		{
			if (player is null) return;
			try { player.Msg(message, style); }
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}
