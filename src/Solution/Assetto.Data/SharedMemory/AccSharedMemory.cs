using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


public enum ConnectionState {
    Disconnected,
    Connecting,
    Connected
}

/// <summary>
/// API to interface with AC and ACC
/// </summary>
public sealed class AccSharedMemory : IDisposable {
    private bool _disposed;

    private readonly PeriodicSharedMemoryPoller<Physics> _physics;
    private readonly PeriodicSharedMemoryPoller<Graphics> _graphics;
    private readonly PeriodicSharedMemoryPoller<StaticInfo> _staticInfo;
    private readonly PeriodicSharedMemoryPoller<CrewInfo> _crewInfo;


    public ConnectionState Status { get; private set; } = ConnectionState.Disconnected;

    public double PhysicsUpdateInterval {
        get => _physics.Interval;
        set => _physics.Interval = value;
    }


    public double GraphicsUpdateInterval {
        get => _graphics.Interval;
        set => _graphics.Interval = value;
    }


    public double StaticInfoUpdateInterval {
        get => _staticInfo.Interval;
        set => _staticInfo.Interval = value;
    }
    public double CrewInfoUpdateInterval
    {
        get => _crewInfo.Interval;
        set => _crewInfo.Interval = value;
    }



    public event UpdatedHandler<Physics> PhysicsUpdated {
        add => _physics.Updated += value;
        remove => _physics.Updated -= value;
    }

    public event UpdatedHandler<Graphics> GraphicsUpdated {
        add => _graphics.Updated += value;
        remove => _graphics.Updated -= value;
    }


    public event UpdatedHandler<StaticInfo> StaticInfoUpdated {
        add => _staticInfo.Updated += value;
        remove => _staticInfo.Updated -= value;
    }

    public event UpdatedHandler<CrewInfo> CrewInfoUpdated
    {
        add => _crewInfo.Updated += value;
        remove => _crewInfo.Updated -= value;
    }

    public AccSharedMemory() {
        _physics = new PeriodicSharedMemoryPoller<Physics>("Local\\acpmf_physics");
        _graphics = new PeriodicSharedMemoryPoller<Graphics>("Local\\acpmf_graphics");
        _staticInfo = new PeriodicSharedMemoryPoller<StaticInfo>("Local\\acpmf_static");

        _crewInfo = new PeriodicSharedMemoryPoller<CrewInfo>("Local\\acpmf_crewchief");
    }


    public AccSharedMemory(double physicsUpdateInterval, double graphicsUpdateInterval, double staticInfoUpdateInterval, double crewInfoUpdateInterval) : this()
    {
        PhysicsUpdateInterval = physicsUpdateInterval;
        GraphicsUpdateInterval = graphicsUpdateInterval;
        StaticInfoUpdateInterval = staticInfoUpdateInterval;
        CrewInfoUpdateInterval = crewInfoUpdateInterval;
    }

    public async Task ConnectAsync(CancellationToken token) {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AccSharedMemory));
        }

        if (Status == ConnectionState.Connected) {
            return;
        }

        if (Status == ConnectionState.Connecting) {
            return;
        }

        Status = ConnectionState.Connecting;

        while (Status != ConnectionState.Connected) {
            try {
                _physics.Connect();
                _graphics.Connect();
                _staticInfo.Connect();
                _crewInfo.Connect();

                Status = ConnectionState.Connected;
            } catch (FileNotFoundException) {
                try {
                    await Task.Delay(100, token);
                } catch (OperationCanceledException) {
                    return;
                }
            }
        }
    }


    public void Disconnect() {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AccSharedMemory));
        }

        if (Status == ConnectionState.Disconnected) {
            return;
        }

        if (Status == ConnectionState.Connecting) {
            throw new InvalidOperationException("Can't disconnect while connecting.");
        }

        Status = ConnectionState.Disconnected;

        _physics.Disconnect();
        _graphics.Disconnect();
        _staticInfo.Disconnect();
        _crewInfo.Disconnect();
    }


    public void Dispose() {
        if (!_disposed) {
            if (Status == ConnectionState.Connected) {
                Disconnect();
            }

            _physics.Dispose();
            _graphics.Dispose();
            _staticInfo.Dispose();
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
