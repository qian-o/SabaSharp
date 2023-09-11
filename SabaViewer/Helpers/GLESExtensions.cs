using Silk.NET.OpenGLES;

namespace SabaViewer.Helpers;

public static class GLESExtensions
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
}
