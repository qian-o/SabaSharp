using System.Runtime.InteropServices;

namespace Saba;

public readonly unsafe struct Vector<T> : IDisposable where T : unmanaged
{
    private readonly int _size;
    private readonly T* _buffer;

    public readonly int Size => _size;

    public readonly T* Buffer => _buffer;

    public readonly ref T this[int index] => ref _buffer[index];

    public Vector(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        _size = size;
        _buffer = (T*)Marshal.AllocHGlobal(size * sizeof(T));
    }

    public void Fill(T value)
    {
        for (int i = 0; i < _size; i++)
            _buffer[i] = value;
    }

    public readonly void Dispose()
    {
        Marshal.FreeHGlobal((nint)_buffer);

        GC.SuppressFinalize(this);
    }
}
