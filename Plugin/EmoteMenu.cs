using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using CoreSettings;
using MenuManager;
using Microsoft.Extensions.Logging;
using FortniteEmotes.API;
using FortniteEmotes.Extensions;

namespace FortniteEmotes;

public partial class Plugin
{
    public static string Prefix => CoreSettingsPlugin.ApiCapability.Get()?.Get("prefix") ?? string.Empty;
    public static string PrefixRed => CoreSettingsPlugin.ApiCapability.Get()?.Get("prefix_red") ?? string.Empty;

    private IMenuApi? _menuApi;
    private readonly PluginCapability<IMenuApi?> _menuCapability = new("menu:nfcore");

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        if (CoreSettingsPlugin.ApiCapability.Get() is null)
            Logger.LogError("[FortniteEmotes] coresettings:config is not registered. Is CoreSettings loaded, and is CoreSettings.Api.dll only in shared/?");

        _menuApi = _menuCapability.Get();
        if (_menuApi == null)
            Logger.LogError("[FortniteEmotes] MenuManager Core not found...");
    }

    private void ShowEmoteMenu(CCSPlayerController player, bool isDance)
    {
        if (!player.IsValid)
        {
            Logger.LogInformation($"[ShowEmoteMenu] {player.PlayerName} was invalid");
            return;
        }

        if (_menuApi == null)
        {
            Logger.LogInformation("[ShowEmoteMenu] _menuApi is null");
            return;
        }

        string title = isDance
            ? Localizer.ForPlayer(player, "emote.dancemenutitle")
            : Localizer.ForPlayer(player, "emote.emotemenutitle");

        var menu = _menuApi.GetMenu(title);
        bool hasAny = false;

        foreach (var emote in Config.EmoteDances)
        {
            if (isDance && emote.IsEmote)
                continue;
            if (!isDance && !emote.IsEmote)
                continue;

            hasAny = true;
            bool hasPerm = HasPermision(player, emote.Permission);
            string emoteName = Localizer.ForPlayer(player, emote.Name);

            menu.AddMenuOption(emoteName, (p, _) =>
            {
                string error = "";
                if (!PlayEmote(p, emote, ref error) && !string.IsNullOrEmpty(error))
                    p.PrintToChatWithSound(error, "Vote.Failed", isWarning: true);
            }, !hasPerm);
        }

        if (!hasAny)
        {
            player.PrintToChatWithSound(
                $"No {(isDance ? "dances" : "emotes")} found.",
                "Vote.Failed",
                isWarning: true);
            return;
        }

        menu.PostSelectAction = PostSelectAction.Close;
        menu.ExitButton = true;
        menu.Open(player);
    }

    public string GetEmoteTranslation(string name)
    {
        return g_EmoteTransMap.ContainsKey(name) ? g_EmoteTransMap[name] : name;
    }

    public Emote? GetEmoteByName(string name, bool emote, bool partial = false)
    {
        if (partial)
        {
            var sname = name.ToLower();

            var list = Config.EmoteDances
                .Where(s => s.IsEmote == emote)
                .OrderBy(s =>
                {
                    var emoteName = s.Name.ToLower();
                    var emoteNameTranslated = GetEmoteTranslation(s.Name).ToLower();

                    var ems = emoteName.Split('_').ToList();

                    foreach (var em in ems)
                    {
                        // Don't search Emote_ and Dance_ prefix
                        if (em.Equals("Emote_") || em.Equals("Dance_"))
                        {
                            continue;
                        }

                        if (em.Equals(sname))
                            return -1;
                    }

                    ems = emoteNameTranslated.Split(' ').ToList();

                    foreach (var em in ems)
                    {
                        if (em.Equals(sname))
                            return -1;
                    }

                    return 1;
                })
                .Where(s =>
                {
                    var ems = s.Name.ToLower().Split('_').ToList();
                    foreach (var em in ems)
                    {
                        if (em.Equals(sname))
                            return true;
                    }

                    ems = GetEmoteTranslation(s.Name).ToLower().Split(' ').ToList();
                    foreach (var em in ems)
                    {
                        if (em.Equals(sname))
                            return true;
                    }

                    return false;
                })
                .ToList();

            return list.Count > 0 ? list.First() : null;
        }
        return Config.EmoteDances.FirstOrDefault(x => GetEmoteTranslation(x.Name).Equals(name) && x.IsEmote == emote);
    }
}
