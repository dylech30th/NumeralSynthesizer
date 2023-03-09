using System.Threading.Channels;

namespace NumeralSynthesizer;

public sealed class InputBuffer
{
    private readonly Channel<string> _queue;

    private bool _isActive;

    private bool _suspendRead;

    public bool CanRead => !_suspendRead;
    
    public InputBuffer()
    {
        _queue = Channel.CreateUnbounded<string>();
    }
    
    public void Start()
    {
        _isActive = true;
        Task.Run(async () =>
        {
            while (Volatile.Read(ref _isActive))
            {
                if (_suspendRead)
                {
                    Thread.SpinWait(20);
                    continue;
                }
                var input = Console.ReadLine();
                if (input?.Trim() is { Length: > 0 } trimmed)
                {
                    await _queue.Writer.WriteAsync(trimmed);
                }
            }
        });
    }

    public ValueTask<bool> WaitToReadAsync()
    {
        return _queue.Reader.WaitToReadAsync();
    }
    
    public void Stop()
    {
        Volatile.Write(ref _isActive, false);
        _queue.Writer.Complete();
    }

    public void ReadBarrier()
    {
        Volatile.Write(ref _suspendRead, true);
    }
    
    public void ReadBarrierRelease()
    {
        Volatile.Write(ref _suspendRead, false);
    }
    
    public ValueTask<string> ReadAsync()
    {
        return _queue.Reader.ReadAsync();
    }
}