namespace Saba.Helpers;

public static unsafe class ArrayExtensions
{
    public static T* GetData<T>(this T[] array) where T : unmanaged
    {
        fixed (T* ptr = array)
        {
            return ptr;
        }
    }
}
