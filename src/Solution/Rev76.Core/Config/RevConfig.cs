using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rev86.Core.Config
{
    public class WidgetConfig
    {
        public string Name { get; set; } = "Widget";
        public int FPS { get; set; } = 10;
        public int X { get; set; } = 1;
        public int Y { get; set; } = 1;
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public bool Enable { get; set; } = false;
        public float Scale { get; set; } = 1;
        public bool ShowInTaskBar { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

    }

    public class RevConfig
    {
        private static readonly string _configFilePath = "RevConfig.json";
        private static RevConfig _instance;
        private static readonly object _lock = new object();
        private bool _isDirty = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public List<WidgetConfig> Widgets { get; set; }

        private RevConfig() { }

        public static RevConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = Load();
                        }
                    }
                }
                return _instance;
            }
        }

        private static RevConfig Load()
        {


            if (!File.Exists(_configFilePath))
            {
                throw new FileNotFoundException("Configuration file not found.", _configFilePath);
            }

            string json = File.ReadAllText(_configFilePath);
            return JsonConvert.DeserializeObject<RevConfig>(json);
        }

        private async void Save()
        {
            lock (_lock)
            {
                if (_isDirty)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    var token = _cancellationTokenSource.Token;

                    Task.Run(async () =>
                    {
                        await Task.Delay(100, token); // Delay to allow for batching
                        if (!token.IsCancellationRequested)
                        {
                            lock (_lock)
                            {
                                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                                File.WriteAllText(_configFilePath, json);
                                _isDirty = false;
                            }
                        }
                    }, token);
                }
            }
        }

        public void UpdateWidget(WidgetConfig widgetConfig)
        {
            int idcx = Widgets.FindIndex(w => w.Name == widgetConfig.Name);
            Widgets[idcx] = widgetConfig;
            _isDirty = true;
            Save();
        }


        public void UpdateWidgetPosition(string widgetName, int x, int y)
        {
            var widget = Widgets.Find(w => w.Name == widgetName);
            if (widget != null)
            {
                if (widget.X != x || widget.Y != y)
                {
                    widget.X = x;
                    widget.Y = y;
                    _isDirty = true;
                    Save();
                }
            }
        }
    }
}
