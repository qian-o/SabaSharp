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

    public static Matrix4X4<T> Invert<T>(this Matrix4X4<T> matrix) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        Matrix4X4.Invert(matrix, out Matrix4X4<T> result);

        return result;
    }

    public static Matrix3X3<float> InvZ(this Matrix3X3<float> matrix)
    {
        Matrix3X3<float> invZ = Matrix3X3.CreateScale(new Vector3D<float>(1.0f, 1.0f, -1.0f));
        
        return invZ * matrix * invZ;
    }
}
