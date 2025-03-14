﻿using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;


internal class PeriodicSharedMemoryPoller<T> : IDisposable where T : struct 
{
  
    private bool _disposed;

    private MemoryMappedFile _memory;
    private readonly System.Timers.Timer _timer = new System.Timers.Timer(1000);
    private readonly object _lock = new object();

    private readonly string _mapName;

    public double Interval {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public event UpdatedHandler<T> Updated;

    public PeriodicSharedMemoryPoller(string mapeName) => _mapName = mapeName;

    public PeriodicSharedMemoryPoller(string mapName, double interval) =>
        (_mapName, Interval) = (mapName, interval);

    public void Connect() {
        lock (_lock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AccSharedMemory));
            }

            _memory = MemoryMappedFile.OpenExisting(_mapName);

            T data = ReadMemory(_memory);
            OnUpdated(data);

            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }
    }

    public void Disconnect() {
        lock (_lock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AccSharedMemory));
            }
            _timer.Stop();
            _timer.Elapsed -= Timer_Elapsed;
           

            _memory?.Dispose();
        }
    }

    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {

        lock (_lock)
        {
            if (_memory is null)
            {
                return;
            }
            if (_memory.SafeMemoryMappedFileHandle.IsClosed)
            {
                return;
            }
            else
            {
                T data = ReadMemory(_memory);
                OnUpdated(data);
            }
        }
    }

    private void OnUpdated(T data) {
        Updated?.Invoke(this, new UpdatedEventArgs<T>(data));
    }

    private static T ReadMemory(MemoryMappedFile memory) {

       
        using (MemoryMappedViewStream stream = memory.CreateViewStream())
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int size = Marshal.SizeOf(typeof(T));
                byte[] bytes = reader.ReadBytes(size);
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                try
                {
                    T? data = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());

                    if (data is null)
                    {
                        throw new InvalidOperationException("Failed to read data from memory");
                    }

                    return data.Value;
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }

    public void Dispose() {
        lock (_lock)
        {
            if (!_disposed)
            {
                _timer.Dispose();
                _memory?.Dispose();
            }

            _disposed = true;

        }
        GC.SuppressFinalize(this);
    }
}
