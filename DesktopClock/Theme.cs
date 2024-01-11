using System;
using System.Collections.Generic;

namespace DesktopClock;

public readonly record struct Theme
{
    public string Name { get; }
    public string PrimaryColor { get; }
    public string SecondaryColor { get; }

    public Theme(string name, string primaryColor, string secondaryColor)
    {
        Name = name;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    // https://www.materialui.co/colors - A100, A700.
    public static IReadOnlyList<Theme> DefaultThemes { get; } = new Theme[]
    {
        new Theme("浅色", "#F5F5F5", "#212121"),
        new Theme("深色", "#212121", "#F5F5F5"),
        new Theme("红色", "#D50000", "#FF8A80"),
        new Theme("粉色", "#C51162", "#FF80AB"),
        new Theme("紫色", "#AA00FF", "#EA80FC"),
        new Theme("蓝色", "#2962FF", "#82B1FF"),
        new Theme("青色", "#00B8D4", "#84FFFF"),
        new Theme("绿色", "#00C853", "#B9F6CA"),
        new Theme("橙色", "#FF6D00", "#FFD180"),
    };

    public static Theme GetRandomDefaultTheme()
    {
        var random = new Random();
        return DefaultThemes[random.Next(0, DefaultThemes.Count)];
    }
}