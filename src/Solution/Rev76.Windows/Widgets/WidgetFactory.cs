using Rev86.Core.Config;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;


namespace Rev76.Windows.Widgets
{
    public static class WidgetFactory
    {

        public static void LoadWidgets(Icon icon)
        {

            foreach (var widgetConfig in RevConfig.Instance.Widgets)
            {
                if (widgetConfig.Enable)
                {
                    Task.Run(() =>
                    {
                       
                        var widget = CreateWidget(widgetConfig, icon);
                        widget.FPS = widgetConfig.FPS; // Set FPS for the widget
                        widget.Show();
                    });

                    Trace.WriteLine($"{widgetConfig.Name} shown.");
                }
            }
        }

        public static OverlayWindow CreateWidget(WidgetConfig config, Icon icon)
        {
            switch (config.Name)
            {
                //case "WeatherWidget":
                //    return new WeatherWidget(config.X, config.Y, config.Width, config.Height, icon) { FPS = config.FPS };
                //case "SessionClockWidget":
                //    return new SessionClockWidget(config.X, config.Y, config.Width, config.Height, icon) { FPS = config.FPS };
                //case "FuelTankWidget":
                //    return new FuelTankWidget(config.X, config.Y, config.Width, config.Height, icon) { FPS = config.FPS };
                case "FlagWidget":
                    return new FlagWidget(config.X, config.Y, config.Width, config.Height, icon) { FPS = config.FPS };
                case "TyreWidgetEx":
                    return new TyreWidgetEx(config.X, config.Y, config.Width, config.Height, icon) { FPS = config.FPS, Settings = config.Settings };
                case "PurpleWidget":
                    return new PurpleWidget(config.X, config.Y, config.Width, config.Height, icon) { FPS = config.FPS, Settings = config.Settings };
                case "Rev76Widget":
                    return new Rev76Widget(config.X, config.Y, config.Width, config.Height,icon) { FPS = config.FPS, Settings = config.Settings };
                    //case "GameStateWidget":
                    //return new GameStateWidget(config.X, config.Y, config.Width, config.Height) { FPS = config.FPS };
                    // Add more cases for other widget types
                    // default:
                    //throw new ArgumentException($"Unknown widget name: {config.Name}");
            }
            return null;
        }
    }
}

