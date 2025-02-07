using Assetto.Data.Broadcasting.Structs;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Assetto.Data.Broadcasting.BroadcastingNetworkProtocol;

namespace Assetto.Data.Broadcasting
{
    public class ACCUdpRemoteClient : IDisposable
    {
        private UdpClient _client;
        private Task _listenerTask;
        // Expose an event at the client level rather than via MessageHandler.
        //public event Action<int, bool, bool, string> OnConnectionStateChanged;
        
        public event ConnectionStateChangedDelegate OnConnectionStateChanged;
        public event TrackDataUpdateDelegate OnTrackDataUpdate;
        public event EntryListUpdateDelegate OnEntrylistUpdate;
        public event RealtimeUpdateDelegate OnRealtimeUpdate;
        public event RealtimeCarUpdateDelegate OnRealtimeCarUpdate;
        public event BroadcastingEventDelegate OnBroadcastingEvent;

        // Now MessageHandler is read-only externally.
        public BroadcastingNetworkProtocol MessageHandler { get; private set; }
        public string IpPort { get; }
        public string DisplayName { get; }
        public string ConnectionPassword { get; }
        public string CommandPassword { get; }
        public int MsRealtimeUpdateInterval { get; }
        public string LastError { get; set; }

        private readonly string _ip;
        private readonly int _port;

        public ACCUdpRemoteClient(string ip, int port, string displayName, string connectionPassword, string commandPassword, int msRealtimeUpdateInterval)
        {
            _ip = ip;
            _port = port;
            IpPort = $"{ip}:{port}";
            DisplayName = displayName;
            ConnectionPassword = connectionPassword;
            CommandPassword = commandPassword;
            MsRealtimeUpdateInterval = msRealtimeUpdateInterval;

            InitializeMessageHandler();
            _client = new UdpClient();
            _client.EnableBroadcast = true;
            _client.ExclusiveAddressUse = false;
            _client.Connect(ip, port);

            _listenerTask = Task.Run(ConnectAndRunAsync);
        }

        // Creates a new MessageHandler instance and attaches the event.
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
                if (payload != null && _client != null)
                    _client.Send(payload, payload.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public async Task ShutdownAsync()
        {
            if (_listenerTask != null && !_listenerTask.IsCompleted)
            {
                MessageHandler.Disconnect();
                _client?.Close();
                await _listenerTask;
            }
        }

        public struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }

        private async Task ConnectAndRunAsync()
        {
            while (true)
            {
                try
                {
                    if (_client == null)
                    {
                        // When resetting, reinitialize the MessageHandler so its events get wired.
                        InitializeMessageHandler();
                        _client = new UdpClient();
                        _client.EnableBroadcast = true;
                        _client.ExclusiveAddressUse = false;
                        _client.Connect(_ip, _port);
                    }

                    var name = $"{DisplayName}. {DateTime.Now.Second}";
                    MessageHandler.RequestConnection(name, ConnectionPassword, MsRealtimeUpdateInterval, CommandPassword);

                    var udpState = new UdpState { u = _client, e = new IPEndPoint(IPAddress.Any, 0) };
                    while (_client != null)
                    {
                        // Wait up to 5 seconds for a message.
                        var receiveTask = _client.ReceiveAsync();
                        var timeoutTask = Task.Delay(5000);
                        var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException("Receive timeout after 5 seconds.");
                        }

                        var result = await receiveTask;
                        using (var ms = new System.IO.MemoryStream(result.Buffer))
                        using (var reader = new System.IO.BinaryReader(ms))
                        {
                            MessageHandler.ProcessMessage(reader);
                        }
                    }
                }
                catch (TimeoutException ex)
                {
                    LastError = ex.Message;
                }
                catch (ObjectDisposedException ex)
                {
                    LastError = ex.Message;
                    break; // Shutdown occurred.
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    LastError = ex.Message;
                    await Task.Delay(5000); // wait before retrying
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    await Task.Delay(5000);
                }
                finally
                {
                    if (_client != null)
                    {
                        OnConnectionStateChanged?.Invoke(MessageHandler.ConnectionId, false, false, "Connection failed");
                        MessageHandler?.Disconnect();
                        _client.Close();
                        _client = null;
                    }
                }
            }
            var x = 1;
        }

        public void RequestTrackData() => MessageHandler.RequestTrackData();
        public void RequestEntryList() => MessageHandler.RequestEntryList();

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        _client?.Close();
                        _client?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            MessageHandler?.Disconnect();
            _client?.Close();
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
