using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Saba;

public readonly unsafe struct FixedArray<T> : IDisposable where T : unmanaged
{
    private readonly int _size;
    private readonly int _length;
    private readonly T* _buffer;

    public readonly int Size => _size;

    public readonly int Length => _length;

    public readonly T* Buffer => _buffer;

    public readonly ref T this[int index] => ref _buffer[index];

    public FixedArray(int length, uint alignment = 4096)
    {
        _size = length * sizeof(T);
        _length = length;
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
        NativeMemory.AlignedFree(_buffer);

        GC.SuppressFinalize(this);
    }
}
