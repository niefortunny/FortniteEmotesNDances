using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace FortniteEmotes.Extensions;

public static class PlayerExtensions
{
    public static void PrintToChatWithSound(
        this CCSPlayerController? player,
        string message,
        string soundName = "HudChat.Message",
        bool isWarning = false,
        bool withPrefix = true)
    {
        if (player == null || !player.IsValid)
            return;

        if (withPrefix)
        {
            string prefix = !isWarning ? Plugin.Prefix : Plugin.PrefixRed;
            player.PrintToChat(prefix + message);
        }
        else
        {
            player.PrintToChat(message);
        }

        RecipientFilter filter = [player];
        player.EmitSound(soundName, filter);
    }
}
