using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace SabaViewer.Helpers;

public static unsafe class GLESExtensions
{
    public static uint CreateShader(this GL gl, GLEnum type, string shaderSource)
    {
        uint shader = gl.CreateShader(type);

        gl.ShaderSource(shader, shaderSource);
        gl.CompileShader(shader);

        string error = gl.GetShaderInfoLog(shader);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"{type}: {error}");
        }

        return shader;
    }

    public static uint CreateShaderProgram(this GL gl, string vsFile, string fsFile)
    {
        string vsSource = File.ReadAllText(vsFile);
        string fsSource = File.ReadAllText(fsFile);

        uint vs = gl.CreateShader(GLEnum.VertexShader, vsSource);
        uint fs = gl.CreateShader(GLEnum.FragmentShader, fsSource);

        uint program = gl.CreateProgram();

        gl.AttachShader(program, vs);
        gl.AttachShader(program, fs);

        gl.LinkProgram(program);

        string error = gl.GetProgramInfoLog(program);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        gl.DetachShader(program, vs);
        gl.DetachShader(program, fs);

        gl.DeleteShader(vs);
        gl.DeleteShader(fs);

        return program;
    }

    public static void SetUniform(this GL gl, int name, int data)
    {
        gl.Uniform1(name, data);
    }

    public static void SetUniform(this GL gl, int name, float data)
    {
        gl.Uniform1(name, data);
    }

    public static void SetUniform(this GL gl, int name, double data)
    {
        gl.Uniform1(name, Convert.ToSingle(data));
    }

    public static void SetUniform(this GL gl, int name, Vector2 data)
    {
        gl.Uniform2(name, 1, (float*)&data);
    }

    public static void SetUniform(this GL gl, int name, Vector3 data)
    {
        gl.Uniform3(name, 1, (float*)&data);
    }

    public static void SetUniform(this GL gl, int name, Vector4 data)
    {
        gl.Uniform4(name, 1, (float*)&data);
    }

    public static void SetUniform(this GL gl, int name, Matrix2X2<float> data)
    {
        gl.UniformMatrix2(name, 1, false, (float*)&data);
    }

    public static void SetUniform(this GL gl, int name, Matrix3X3<float> data)
    {
        gl.UniformMatrix3(name, 1, false, (float*)&data);
    }

    public static void SetUniform(this GL gl, int name, Matrix4x4 data)
    {
        gl.UniformMatrix4(name, 1, false, (float*)&data);
    }

    public static void SetUniform(this GL gl, int name, IEnumerable<Matrix4x4> data)
    {
        Matrix4x4[] matrixArray = data.ToArray();

        fixed (Matrix4x4* matrix = matrixArray)
        {
            gl.UniformMatrix4(name, (uint)matrixArray.Length, false, (float*)matrix);
        }
    }
}
