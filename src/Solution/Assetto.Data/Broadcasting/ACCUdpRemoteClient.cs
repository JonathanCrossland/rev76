using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Assetto.Data.Broadcasting.BroadcastingNetworkProtocol;

namespace Assetto.Data.Broadcasting
{
    public class ACCUdpRemoteClient : IDisposable
    {
        private UdpClient _Client;
        private Task _ListenerTask;
        private readonly object _Lock = new object();
        private bool _Disposing = false;

        public event ConnectionStateChangedDelegate OnConnectionStateChanged;
        public event TrackDataUpdateDelegate OnTrackDataUpdate;
        public event EntryListUpdateDelegate OnEntrylistUpdate;
        public event RealtimeUpdateDelegate OnRealtimeUpdate;
        public event RealtimeCarUpdateDelegate OnRealtimeCarUpdate;
        public event BroadcastingEventDelegate OnBroadcastingEvent;

        public BroadcastingNetworkProtocol MessageHandler { get; private set; }
        public string IpPort { get; }
        public string DisplayName { get; }
        public string ConnectionPassword { get; }
        public string CommandPassword { get; }
        public int MsRealtimeUpdateInterval { get; }
        public string LastError { get; set; }

        private readonly string _ip;
        private readonly int _port;


        public void RequestTrackData() => MessageHandler.RequestTrackData();
        public void RequestEntryList() => MessageHandler.RequestEntryList();

        public ACCUdpRemoteClient(string ip, int port, string displayName, string connectionPassword, string commandPassword, int msRealtimeUpdateInterval)
        {
            _ip = ip;
            _port = port;
            IpPort = $"{ip}:{port}";
            DisplayName = displayName;
            ConnectionPassword = connectionPassword;
            CommandPassword = commandPassword;
            MsRealtimeUpdateInterval = msRealtimeUpdateInterval;

            _ListenerTask = Task.Run(ConnectAndRunAsync);
        }

        private void InitializeMessageHandler()
        {
            MessageHandler = new BroadcastingNetworkProtocol(IpPort, Send);
            MessageHandler.OnConnectionStateChanged += (id, success, readOnly, error) =>
                OnConnectionStateChanged?.Invoke(id, success, readOnly, error);
            MessageHandler.OnTrackDataUpdate += (sender, e) => OnTrackDataUpdate?.Invoke(sender, e);
            MessageHandler.OnEntrylistUpdate += (sender, e) => OnEntrylistUpdate?.Invoke(sender, e);
            MessageHandler.OnBroadcastingEvent += (sender, e) => OnBroadcastingEvent?.Invoke(sender, e);
            MessageHandler.OnRealtimeUpdate += (sender, e) => OnRealtimeUpdate?.Invoke(sender, e);
            MessageHandler.OnRealtimeCarUpdate += (sender, e) => OnRealtimeCarUpdate?.Invoke(sender, e);
           
        }

        private void Send(byte[] payload)
        {
            try
            {
                if (payload != null && _Client?.Client != null && _Client.Client.Connected)
                    _Client.Send(payload, payload.Length);
            }
            catch (ObjectDisposedException)
            {
                Trace.TraceError("Udp: Send failed: UDP client was disposed.");
            }
            catch (SocketException ex)
            {
                Trace.TraceError($"Udp: Send failed: {ex.SocketErrorCode}");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Udp: Unexpected Send error: {e.Message}");
            }
        }
        private async Task ConnectAndRunAsync()
        {
            int adaptiveUpdateInterval = MsRealtimeUpdateInterval; // Start with provided interval
            Stopwatch stopwatch = new Stopwatch();
            DateTime lastReRegisterTime = DateTime.Now;
             
            while (!_Disposing)
            {
                Trace.TraceWarning("Udp:  Loop Start");
                try
                {
                    lock (_Lock)
                    {
                        if (_Client == null)
                        {
                            InitializeMessageHandler();
                            _Client = new UdpClient();
                            _Client.EnableBroadcast = true;
                            _Client.ExclusiveAddressUse = false;
                            _Client.Connect(_ip, _port);
                            Trace.TraceWarning("Udp: Client is set");
                        }
                    }

                    
                    // **Register with ACC using the current adaptive interval**
                    MessageHandler.RequestConnection($"{DisplayName}.{DateTime.Now.Millisecond}", ConnectionPassword, adaptiveUpdateInterval, CommandPassword);

                    while (!_Disposing)
                    {
                        stopwatch.Restart();

                        var receiveTask = _Client.ReceiveAsync();
                        var timeoutTask = Task.Delay(20000);

                        var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
                        stopwatch.Stop();

                        if (_Disposing) break;

                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException("Udp: Receive timeout after 10 seconds.");
                        }
                        if (_Client.Available > 0) { 

                            using (var ms = new System.IO.MemoryStream(receiveTask.Result.Buffer))
                            using (var reader = new System.IO.BinaryReader(ms))
                            {
                                MessageHandler.ProcessMessage(reader);
                            }
                    }
                        int elapsedTime = (int)stopwatch.ElapsedMilliseconds;

                        if (elapsedTime > 250) 
                        {
                            adaptiveUpdateInterval = Math.Min(adaptiveUpdateInterval + 5, 250); 
                        }
                        else if (elapsedTime < 250)
                        {
                            adaptiveUpdateInterval = Math.Max(adaptiveUpdateInterval - 5, 20); 
                        }

                        if (adaptiveUpdateInterval >= 350 && (DateTime.Now - lastReRegisterTime).TotalSeconds >= 30)
                        {
                           
                            //MessageHandler.Disconnect();
                          
                            //MessageHandler.RequestConnection($"{DisplayName}.{DateTime.Now.Millisecond}", ConnectionPassword, adaptiveUpdateInterval, CommandPassword);
                            adaptiveUpdateInterval = 200;
                            lastReRegisterTime = DateTime.Now;
                            Trace.TraceWarning($"Re-registering with ACC using update interval: {adaptiveUpdateInterval}ms");
                        }
                    }
                }
                catch (TimeoutException ex)
                {
                    Trace.TraceError("Udp: Timeout on UDP Connection");
                    LastError = ex.Message;
                }
                catch (ObjectDisposedException ex)
                {
                    Trace.TraceError("Udp: Disposed Object on UDP Connection");
                    LastError = ex.Message;
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    Trace.TraceError("Udp: Socket Exception on UDP Connection");
                    Trace.TraceError(ex.Message);
                    LastError = ex.Message;
                    OnConnectionStateChanged?.Invoke(0, false, true, "Connection refused");
                    await Task.Delay(5000); // wait before retrying
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Udp: Generic Exception on UDP Connection");
                    Trace.TraceError(ex.Message);
                    LastError = ex.Message;
                     await Task.Delay(5000);
                }
                finally
                {
                    lock (_Lock)
                    {
                        if (_Client != null)
                        {
                           
                            MessageHandler?.Disconnect(); // Sometimes the disconnect did not land. Important!
                            MessageHandler?.Disconnect(); // Sometimes the disconnect did not land. Send again. Important!
                            MessageHandler?.Disconnect(); // Sometimes the disconnect did not land. 2nd time seems to work, this one is extra. Important!
                            _Client.Close(); // Close the UDP Client. Important!
                            _Client = null; // Set Client to null. Important!
                            OnConnectionStateChanged?.Invoke(MessageHandler.ConnectionId, false, false, "Connection failed");
                            Trace.TraceWarning("Udp: Client reset");
                        }
                    }
                }
            }

            Trace.TraceWarning("Udp: Loop End");
        }


        #region IDisposable Support
        public async Task ShutdownAsync()
        {

            if (_ListenerTask != null && !_ListenerTask.IsCompleted)
            {
                await _ListenerTask; // Wait for the loop to exit
            }

            MessageHandler.OnConnectionStateChanged -= OnConnectionStateChanged;
            MessageHandler.OnTrackDataUpdate -= OnTrackDataUpdate;
            MessageHandler.OnEntrylistUpdate -= OnEntrylistUpdate;
            MessageHandler.OnBroadcastingEvent -= OnBroadcastingEvent;
            MessageHandler.OnRealtimeUpdate -= OnRealtimeUpdate;
            MessageHandler.OnRealtimeCarUpdate -= OnRealtimeCarUpdate;

            MessageHandler?.Disconnect();
            MessageHandler?.Disconnect();

            Task.Delay(1000).Wait();
            MessageHandler?.Disconnect();
            MessageHandler?.Disconnect();

            lock (_Lock)
            {
                if (_Client != null)
                {
                    _Client?.Close();
                    Task.Delay(1000);
                    _Client?.Dispose();
                    _Client = null;
                }
            }
        }

        public void Dispose()
        {
            _Disposing = true; // Signal loop to stop

            try
            {
                Task.Run(async () => await ShutdownAsync()).Wait(TimeSpan.FromSeconds(30)); // Example timeout
            }
            catch (AggregateException ex)
            {
               Trace.TraceError($"Udp: Dispose {ex.Message}");
            }

        }
        #endregion
    }
}
