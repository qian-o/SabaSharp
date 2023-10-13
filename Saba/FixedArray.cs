using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Saba;

public readonly unsafe struct FixedArray<T> : IDisposable where T : unmanaged
{
    private readonly int _length;
    private readonly uint _alignment;
    private readonly T* _buffer;

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

    public void Fill(T value)
    {
        T* buffer = _buffer;

        Parallel.ForEach(Partitioner.Create(0, _length), range =>
        {
            int length = range.Item2 - range.Item1;

            T* temp = buffer + range.Item1;

            for (int i = 0; i < length; i++)
            {
                *temp = value;

                temp++;
            }
        });
    }

    public readonly void Dispose()
    {
        if (_alignment > 0)
        {
            NativeMemory.AlignedFree(_buffer);
        }
        else
        {
            Marshal.FreeHGlobal((IntPtr)_buffer);
        }

        GC.SuppressFinalize(this);
    }
}
