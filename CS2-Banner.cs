using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2Banner;

public class CS2Banner : BasePlugin
{
    public override string ModuleName => "CS2 Banner Plugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "M1K@c";

    private PluginConfig Config = new();

    private class User
    {
        public bool HtmlPrint;
        public string Message = "";
        public int PrintTime;
    }

    private readonly User?[] _users = new User?[64];

    private bool _showImage = false;
    private string? _currentImage = null;
    private readonly Random _random = new();

    public override void Load(bool hotReload)
    {
        LoadConfigFile();

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;

            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            _users[player.Slot] = new User();

            if (!string.IsNullOrEmpty(Config.WelcomeImageUrl))
            {
                SetHtml(player, $"<img src='{Config.WelcomeImageUrl}' />");
            }

            return HookResult.Continue;
        });

        StartImageLoop();

        RegisterListener<Listeners.OnTick>(OnTick);
    }

    private void LoadConfigFile()
    {
        string path = Path.Combine(ModuleDirectory, "CS2-Banner.json");

        if (!File.Exists(path))
        {
            var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            return;
        }

        string content = File.ReadAllText(path);
        var cfg = JsonSerializer.Deserialize<PluginConfig>(content);

        if (cfg != null)
            Config = cfg;
    }

    private void StartImageLoop()
    {
        if (Config.ImageUrls.Count == 0)
            return;

        ScheduleNextImage();
    }

    private void ScheduleNextImage()
    {
        AddTimer(Config.Cooldown, () =>
        {
            _currentImage = Config.ImageUrls[_random.Next(Config.ImageUrls.Count)];
            _showImage = true;

            AddTimer(Config.ShowTime, () =>
            {
                _showImage = false;
                ScheduleNextImage();
            });
        });
    }

    private void SetHtml(CCSPlayerController player, string message)
    {
        var user = _users[player.Slot];

        if (user == null)
        {
            _users[player.Slot] = new User();
            user = _users[player.Slot];
        }

        user!.HtmlPrint = true;
        user.PrintTime = 0;
        user.Message = message;
    }

    private void OnTick()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot)
                continue;

            var pawn = player.Pawn?.Value;
            bool alive = pawn != null && pawn.IsValid &&
                         pawn.LifeState == (byte)LifeState_t.LIFE_ALIVE;

            if (_showImage && _currentImage != null)
            {
                if (!(Config.ShowOnlyToDead && alive))
                {
                    player.PrintToCenterHtml($"<img src='{_currentImage}' />");
                }
            }

            var user = _users[player.Slot];

            if (user is not null && user.HtmlPrint)
            {
                if (!(Config.ShowOnlyToDead && alive))
                {
                    int durationTicks = Config.ShowTime * 64;

                    if (user.PrintTime < durationTicks)
                    {
                        player.PrintToCenterHtml(user.Message);
                        user.PrintTime++;
                    }
                    else
                    {
                        user.HtmlPrint = false;
                    }
                }
            }
        }
    }
}