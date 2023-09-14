using System.Reflection;

namespace Saba.Helpers;

public static class ObjectExtensions
{
    public static TObject Copy<TObject>(this TObject @object) where TObject : class, new()
    {
        TObject destination = new();

        PropertyInfo[] properties = typeof(TObject).GetProperties();

        foreach (PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                property.SetValue(destination, property.GetValue(@object));
            }
        }

        return destination;
    }

    public static void Copy<TObject>(this IEnumerable<TObject> source, ref TObject[] dest) where TObject : class, new()
    {
        dest = new TObject[source.Count()];

        for (int i = 0; i < source.Count(); i++)
        {
            dest[i] = source.ElementAt(i).Copy();
        }
    }
}
