using System.Numerics;
using BtMatrix4x4 = Evergine.Mathematics.Matrix4x4;

namespace Saba.Helpers;

public static class MathExtensions
{
    public static Vector3 ToVector3(this Vector4 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector4 GetRow(this Matrix4x4 matrix, int row)
    {
        row -= 1;
        return new Vector4(matrix[row, 0], matrix[row, 1], matrix[row, 2], matrix[row, 3]);
    }

    public static Matrix4x4 Invert(this Matrix4x4 matrix)
    {
        Matrix4x4.Invert(matrix, out Matrix4x4 result);

        return result;
    }

    public static Matrix4x4 InvZ(this Matrix4x4 matrix)
    {
        Matrix4x4 invZ = Matrix4x4.CreateScale(new Vector3(1.0f, 1.0f, -1.0f));

        return invZ * matrix * invZ;
    }

    public static BtMatrix4x4 ToBtMatrix4x4(this Matrix4x4 matrix)
    {
        return new BtMatrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }

    public static Matrix4x4 ToMatrix4x4(this BtMatrix4x4 transform)
    {
        Matrix4x4 matrix = new(transform.M11, transform.M12, transform.M13, transform.M14, transform.M21, transform.M22, transform.M23, transform.M24, transform.M31, transform.M32, transform.M33, transform.M34, transform.M41, transform.M42, transform.M43, transform.M44);

        return matrix;
    }
}
