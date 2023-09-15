using Silk.NET.Maths;
using System.Numerics;
using BtTransform3x3 = Evergine.Mathematics.Matrix3x3;
using BtTransform4x4 = Evergine.Mathematics.Matrix4x4;

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

    public static Matrix4X4<float> InvZ(this Matrix4X4<float> matrix)
    {
        Matrix4X4<float> invZ = Matrix4X4.CreateScale(new Vector3D<float>(1.0f, 1.0f, -1.0f));

        return invZ * matrix * invZ;
    }

    public static BtTransform3x3 ToBtTransform(this Matrix3X3<float> matrix)
    {
        matrix = Matrix3X3.Transpose(matrix);

        return new BtTransform3x3(matrix.M11, matrix.M12, matrix.M13, matrix.M21, matrix.M22, matrix.M23, matrix.M31, matrix.M32, matrix.M33);
    }

    public static Matrix3X3<float> ToMatrix3X3(this BtTransform3x3 transform)
    {
        Matrix3X3<float> matrix = new(transform.M11, transform.M12, transform.M13, transform.M21, transform.M22, transform.M23, transform.M31, transform.M32, transform.M33);

        return Matrix3X3.Transpose(matrix);
    }

    public static BtTransform4x4 ToBtTransform(this Matrix4X4<float> matrix)
    {
        // matrix = Matrix4X4.Transpose(matrix);

        return new BtTransform4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }

    public static Matrix4X4<float> ToMatrix4X4(this BtTransform4x4 transform)
    {
        Matrix4X4<float> matrix = new(transform.M11, transform.M12, transform.M13, transform.M14, transform.M21, transform.M22, transform.M23, transform.M24, transform.M31, transform.M32, transform.M33, transform.M34, transform.M41, transform.M42, transform.M43, transform.M44);

        return matrix;
        return Matrix4X4.Transpose(matrix);
    }
}
