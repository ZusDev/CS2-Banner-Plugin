using System.Collections.Generic;

namespace CS2Banner;

public class PluginConfig
{
    public List<string> ImageUrls { get; set; } = new()
    {
        "https://i.imgur.com/example1.png",
        "https://i.imgur.com/example2.png"
    };

    public string WelcomeImageUrl { get; set; } = "";

    public int ShowTime { get; set; } = 5;
    public int Cooldown { get; set; } = 20;

    public bool ShowOnlyToDead { get; set; } = false;
}