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

    public FixedArray(int length)
    {
        _size = length * sizeof(T);
        _length = length;
        _buffer = (T*)Marshal.AllocHGlobal(length * sizeof(T));
    }

    public void Fill(T value)
    {
        T* buffer = _buffer;
        for (int i = 0; i < Length; i++)
        {
            *buffer = value;

            buffer++;
        }
    }

    public readonly void Dispose()
    {
        Marshal.FreeHGlobal((nint)_buffer);

        GC.SuppressFinalize(this);
    }
}
