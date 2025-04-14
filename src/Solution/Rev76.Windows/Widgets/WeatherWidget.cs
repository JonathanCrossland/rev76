using Rev76.DataModels;
using Rev76.Windows.Rendering;
using System.Collections.Generic;
using System.Drawing;
using Svg;
using System.Diagnostics;

namespace Rev76.Windows.Widgets
{
    public class WeatherWidget : OverlayWindow
    {
        private DirectXRenderer _renderer = new DirectXRenderer();

        public WeatherWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
        }

        public override string Title => "Weather";

        public override bool Visible
        {
            get =>
                GameData.Snapshot.GameState.Status == GameStatus.LIVE
                &&
                GameData.Snapshot.GameState.IsSetupMenuVisible == false;
        }

        protected override void OnRender(System.Drawing.Graphics g)
        {
            if (!Visible) return;

            if (g.IsVisibleClipEmpty)
            {
                return;
            }

            base.OnRender(g);

            DrawWeatherSVG(g);

            // Set up font and brush for rendering text
            //Font font = _Fonts["arial"];
            //Brush brush = _Brushes["white"];

            //// Define starting position for drawing
            //float x = 10;
            //float y = 50;
            //float lineHeight = 25;

            //// Write each property value from the Environment class to the graphics object
            //g.DrawString("Air Temperature: " + GameData.Snapshot.Weather.AirTemperature, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Air Density: " + GameData.Snapshot.Weather.AirDensity, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Road Temperature: " + GameData.Snapshot.Weather.RoadTemperature, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Wind Direction: " + GameData.Snapshot.Weather.WindDirection, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Wind Speed: " + GameData.Snapshot.Weather.WindSpeed, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Rain Intensity: " + GameData.Snapshot.Weather.RainIntensity, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Rain in 10 Minutes: " + GameData.Snapshot.Weather.RainIn10Minutes, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Rain in 30 Minutes: " + GameData.Snapshot.Weather.RainIn30Minutes, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("Cloudy: " + GameData.Snapshot.Weather.Cloudy, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("InPits: " + GameData.Snapshot.GameState.InPit, font, brush, x, y);
            //y += lineHeight;
            //g.DrawString("InPitLane: " + GameData.Snapshot.GameState.InPitLane, font, brush, x, y);
        }

        private void DrawWeatherSVG(System.Drawing.Graphics g)
        {
            // Get weather icons based on current conditions
            var weather = GetSVGForWeather(GameData.Snapshot.Weather.RainIntensity);
            var weatherMin10 = GetSVGForWeather(GameData.Snapshot.Weather.RainIn10Minutes);
            var weatherMin30 = GetSVGForWeather(GameData.Snapshot.Weather.RainIn30Minutes);

            // Draw current weather icon
            _renderer.DrawSvg(
                g,
                weather,
                50, 10, 32, 32,
                element =>
                {
                    // Process any dynamic elements if needed
                    if (element is SvgText textElement)
                    {
                        // Handle any text elements in the SVG
                        if (textElement.CustomAttributes.TryGetValue("class", out string className))
                        {
                            switch (className)
                            {
                                case "temperature":
                                    textElement.Text = $"{GameData.Snapshot.Weather.AirTemperature}°C";
                                    break;
                                case "wind":
                                    textElement.Text = $"{GameData.Snapshot.Weather.WindSpeed} km/h";
                                    break;
                            }
                        }
                    }
                    return true; // Always process child elements
                });

            // Draw 10-minute forecast icon
            _renderer.DrawSvg(
                g,
                weatherMin10,
                90, 10, 32, 32,
                element =>
                {
                    // Process any dynamic elements if needed
                    if (element is SvgText textElement)
                    {
                        if (textElement.CustomAttributes.TryGetValue("class", out string className) && className == "time")
                        {
                            textElement.Text = "10m";
                        }
                    }
                    return true;
                });

            // Draw 30-minute forecast icon
            _renderer.DrawSvg(
                g,
                weatherMin30,
                130, 10, 32, 32,
                element =>
                {
                    // Process any dynamic elements if needed
                    if (element is SvgText textElement)
                    {
                        if (textElement.CustomAttributes.TryGetValue("class", out string className) && className == "time")
                        {
                            textElement.Text = "30m";
                        }
                    }
                    return true;
                });
        }

        //returns the index of the List<string>
        public int GetSVGForWeather(RainIntensity weather)
        {
            switch (weather)
            {
                case RainIntensity.NO_RAIN:
                    if (GameData.Snapshot.Weather.Cloudy > 0.5)
                    {
                        return 1;
                    }
                    return 0;
                case RainIntensity.DRIZZLE:
                    return 3;
                case RainIntensity.LIGHT_RAIN:
                    return 3;
                case RainIntensity.MEDIUM_RAIN:
                    return 3;
                case RainIntensity.HEAVY_RAIN:
                    return 4;
                case RainIntensity.THUNDERSTORM:
                    return 5;
            }
            return 0;
        }

        protected override void OnGraphicsSetup(System.Drawing.Graphics g)
        {
            _renderer.LoadSvgFiles(new List<string>
            {
                "Assets/Sunny.svg",
                "Assets/PartlyCloudy.svg",
                "Assets/Cloudy.svg",
                "Assets/CloudyRain.svg",
                "Assets/CloudyRainShowers.svg",
                "Assets/ThunderStorm.svg",
            });

            base.OnGraphicsSetup(g);
        }

        protected override void OnGraphicsDestroyed(System.Drawing.Graphics g)
        {
            base.OnGraphicsDestroyed(g);
        }
    }
}
