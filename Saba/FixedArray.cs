using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Saba;

public readonly unsafe struct FixedArray<T> : IDisposable where T : unmanaged
{
    private readonly bool _isExternal;
    private readonly int _length;
    private readonly uint _alignment;
    private readonly T* _buffer;
    private readonly Action<nint>? _destroy;

    public readonly int Length => _length;

    public readonly T* Buffer => _buffer;

    public readonly ref T this[int index] => ref _buffer[index];

    public FixedArray(int length)
    {
        _length = length;
        _buffer = (T*)Marshal.AllocHGlobal(length * sizeof(T));
    }

    public FixedArray(int length, uint alignment)
    {
        _length = length;
        _alignment = alignment;
        _buffer = (T*)NativeMemory.AlignedAlloc((uint)(length * sizeof(T)), alignment);
    }

    public FixedArray(T* ptr, int length, Action<nint> destroy)
    {
        _isExternal = true;
        _length = length;
        _buffer = ptr;
        _destroy = destroy;
    }

    public void Fill(T value)
    {
        T* buffer = _buffer;

        Parallel.ForEach(Partitioner.Create(0, _length), range =>
        {
            int length = range.Item2 - range.Item1;

            Span<T> values = new(buffer + range.Item1, length);
            values.Fill(value);
        });
    }

    public readonly void Dispose()
    {
        if (_isExternal)
        {
            _destroy?.Invoke((nint)_buffer);
        }
        else
        {
            if (_alignment > 0)
            {
                NativeMemory.AlignedFree(_buffer);
            }
            else
            {
                Marshal.FreeHGlobal((IntPtr)_buffer);
            }
        }

        GC.SuppressFinalize(this);
    }
}
