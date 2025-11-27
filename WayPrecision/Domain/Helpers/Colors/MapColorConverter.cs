using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Helpers.Colors
{
    public static class MapColorConverter
    {
        private const string sBlack = "Negre";
        private const string sBlue = "Blau";
        private const string sGold = "Daurat";
        private const string sGreen = "Verd";
        private const string sDarkOliveGreen = "VerdOlivaFosc";
        private const string sGray = "Gris";
        private const string sOrange = "Taronja";
        private const string sLightOrange = "TaronjaClar";
        private const string sRed = "Vermell";
        private const string sViolet = "Violeta";
        private const string sPurple = "Porpra";
        private const string sYellow = "Groc";

        public static string ToString(MapMarkerColorEnum color)
        {
            return color.ToString().ToLower();
        }

        public static MapMarkerColorEnum ToColor(string color)
        {
            foreach (MapMarkerColorEnum colorMarker in Enum.GetValues(typeof(MapMarkerColorEnum)))
            {
                if (colorMarker.ToString().ToLower() == color.ToLower())
                    return colorMarker;
            }

            return MapMarkerColorEnum.Black;
        }

        public static string GetInsideHexadecimal(MapMarkerColorEnum color)
        {
            switch (color)
            {
                case MapMarkerColorEnum.Black:
                    return "#3D3D3D";

                case MapMarkerColorEnum.Blue:
                    return "#2A81CB";

                case MapMarkerColorEnum.Gold:
                    return "#FFD326";

                case MapMarkerColorEnum.Green:
                    return "#2AAD27";

                case MapMarkerColorEnum.DarkOliveGreen:
                    return "#7C9C44";

                case MapMarkerColorEnum.Grey:
                    return "#7B7B7B";

                case MapMarkerColorEnum.Orange:
                    return "#CB8427";

                case MapMarkerColorEnum.LightOrange:
                    return "#F4C082";

                case MapMarkerColorEnum.Red:
                    return "#CB2B3E";

                case MapMarkerColorEnum.Violet:
                    return "#9C2BCB";

                case MapMarkerColorEnum.Purple:
                    return "#7D3E80";

                case MapMarkerColorEnum.Yellow:
                    return "#CAC428";
            }

            return string.Empty;
        }

        public static string GetOutsideHexadecimal(MapMarkerColorEnum color)
        {
            switch (color)
            {
                case MapMarkerColorEnum.Black:
                    return "#313131";

                case MapMarkerColorEnum.Blue:
                    return "#3274A3";

                case MapMarkerColorEnum.Gold:
                    return "#C1A32D";

                case MapMarkerColorEnum.Green:
                    return "#31882A";

                case MapMarkerColorEnum.DarkOliveGreen:
                    return "#556B2F";

                case MapMarkerColorEnum.Grey:
                    return "#6B6B6B";

                case MapMarkerColorEnum.Orange:
                    return "#98652E";

                case MapMarkerColorEnum.LightOrange:
                    return "#F4A460";

                case MapMarkerColorEnum.Red:
                    return "#982E40";

                case MapMarkerColorEnum.Violet:
                    return "#742E98";

                case MapMarkerColorEnum.Purple:
                    return "#800080";

                case MapMarkerColorEnum.Yellow:
                    return "#988F2E";
            }
            return string.Empty;
        }

        public static string Translate(MapMarkerColorEnum color)
        {
            switch (color)
            {
                case MapMarkerColorEnum.Black:
                    return sBlack;

                case MapMarkerColorEnum.Blue:
                    return sBlue;

                case MapMarkerColorEnum.Gold:
                    return sGold;

                case MapMarkerColorEnum.Green:
                    return sGreen;

                case MapMarkerColorEnum.DarkOliveGreen:
                    return sDarkOliveGreen;

                case MapMarkerColorEnum.Grey:
                    return sGray;

                case MapMarkerColorEnum.Orange:
                    return sOrange;

                case MapMarkerColorEnum.LightOrange:
                    return sLightOrange;

                case MapMarkerColorEnum.Red:
                    return sRed;

                case MapMarkerColorEnum.Violet:
                    return sViolet;

                case MapMarkerColorEnum.Purple:
                    return sPurple;

                case MapMarkerColorEnum.Yellow:
                    return sYellow;
            }
            return sBlack;
        }

        public static MapMarkerColorEnum Translate(string color)
        {
            switch (color)
            {
                case sBlack:
                    return MapMarkerColorEnum.Black;

                case sBlue:
                    return MapMarkerColorEnum.Blue;

                case sGold:
                    return MapMarkerColorEnum.Gold;

                case sGreen:
                    return MapMarkerColorEnum.Green;

                case sDarkOliveGreen:
                    return MapMarkerColorEnum.DarkOliveGreen;

                case sGray:
                    return MapMarkerColorEnum.Grey;

                case sOrange:
                    return MapMarkerColorEnum.Orange;

                case sLightOrange:
                    return MapMarkerColorEnum.LightOrange;

                case sRed:
                    return MapMarkerColorEnum.Red;

                case sViolet:
                    return MapMarkerColorEnum.Violet;

                case sPurple:
                    return MapMarkerColorEnum.Purple;

                case sYellow:
                    return MapMarkerColorEnum.Yellow;
            }
            return MapMarkerColorEnum.Black;
        }
    }
}