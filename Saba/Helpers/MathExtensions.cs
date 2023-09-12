using Silk.NET.Maths;
using System.Numerics;

namespace Saba.Helpers;

public static class MathExtensions
{
    public static Vector3D<float> ToVector3D(this Vector4D<float> vector)
    {
        return new Vector3D<float>(vector.X, vector.Y, vector.Z);
    }

    public static Vector4 ToSystem(this Vector4D<float> value)
    {
        return new Vector4(value.X, value.Y, value.Z, value.W);
    }

    public static Vector4D<float> ToGeneric(this Vector4 value)
    {
        return new Vector4D<float>(value.X, value.Y, value.Z, value.W);
    }
}
