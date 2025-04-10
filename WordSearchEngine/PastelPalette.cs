using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace WordSearchEngine
{
    public enum PastelColor
    {
        Lavender, Mint, Peach, SkyBlue, Rose, Butter, Lilac, Aqua, Coral, Sage
    }

    public static class PastelPalette
    {
        private static readonly Dictionary<PastelColor, Color> _colorMap = new()
        {
            { PastelColor.Lavender, Color.FromRgb(230, 230, 250) },
            { PastelColor.Mint, Color.FromRgb(189, 252, 201) },
            { PastelColor.Peach, Color.FromRgb(255, 218, 185) },
            { PastelColor.SkyBlue, Color.FromRgb(176, 224, 230) },
            { PastelColor.Rose, Color.FromRgb(255, 228, 225) },
            { PastelColor.Butter, Color.FromRgb(255, 255, 204) },
            { PastelColor.Lilac, Color.FromRgb(200, 162, 200) },
            { PastelColor.Aqua, Color.FromRgb(178, 236, 235) },
            { PastelColor.Coral, Color.FromRgb(255, 160, 160) },
            { PastelColor.Sage, Color.FromRgb(205, 230, 208) }
        };

        private static readonly Random rand = new();

        public static Color GetRandomColor()
        {
            var values = Enum.GetValues(typeof(PastelColor));
            var colorkey = (PastelColor)values.GetValue(rand.Next(values.Length))!;
            return _colorMap[colorkey];
        }

        public static SolidColorBrush GetBrush(PastelColor color, double opacity = 0.35)
        {
            var brush = new SolidColorBrush(_colorMap[color]);
            brush.Opacity = opacity;
            return brush;
        }

        public static (Brush Fill, Brush Stroke) GetRandomBrushPair()
        {
            var color = GetRandomColor();
            return (
                new SolidColorBrush(color) { Opacity = 0.35 },
                new SolidColorBrush(color)
            );
        }
    }
}
