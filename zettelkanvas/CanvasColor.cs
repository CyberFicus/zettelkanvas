using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zettelkanvas.Static;

namespace zettelkanvas
{
    internal class CanvasColor
    {
        public string Color { get; private set; }

        private CanvasColor(string color)
        {
            Color = color;
        }

        public static CanvasColor? TryBuild(string color)
        {
            if (int.TryParse(color, out var parseResult))
            {
                if (parseResult < 1 || parseResult > 6)
                    return null;
                return new CanvasColor(color);
            }

            var match = Regexes.HexColor().Match(color);
            if (match.Success)
            {
                return new CanvasColor(match.Value);
            }

            return null;
        }

        public override string ToString()
        {
            return Color;
        }
    }
}
