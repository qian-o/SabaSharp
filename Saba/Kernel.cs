using Silk.NET.OpenCL;
using System.Text;

namespace Saba;

public unsafe class Kernel : IDisposable
{
    private readonly CL _cl;
    private readonly nint _platform;
    private readonly nint _device;
    private readonly nint _context;
    private readonly nint _queue;
    private readonly nint _program;
    private readonly nint _kernel;
    private readonly List<nint> _buffers;

    internal Kernel(CL cl, nint platform, nint device, nint context, nint queue, nint program, nint kernel)
    {
        _cl = cl;
        _platform = platform;
        _device = device;
        _context = context;
        _queue = queue;
        _program = program;
        _kernel = kernel;
        _buffers = new();
    }

    /// <summary>
    /// Create a buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="size">size</param>
    /// <param name="flags">flags</param>
    /// <returns></returns>
    public nint CreateBuffer<T>(uint size, MemFlags flags) where T : unmanaged
    {
        nint buffer_id = _cl.CreateBuffer(_context, flags, (uint)(size * sizeof(T)), null, null);

        _buffers.Add(buffer_id);

        return buffer_id;
    }

    /// <summary>
    /// Delete a buffer.
    /// </summary>
    /// <param name="buffer">buffer</param>
    public void DeleteBuffer(nint buffer)
    {
        if (_buffers.Contains(buffer))
        {
            _cl.ReleaseMemObject(buffer);

            _buffers.Remove(buffer);
        }
    }

    /// <summary>
    /// Write a buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="buffer">buffer</param>
    /// <param name="size">size</param>
    /// <param name="ptr">ptr</param>
    public void WriteBuffer<T>(nint buffer, uint size, void* ptr) where T : unmanaged
    {
        int state = _cl.EnqueueWriteBuffer(_queue, buffer, true, 0, (uint)(size * sizeof(T)), ptr, 0, null, null);

        State(state);
    }

    /// <summary>
    /// Read a buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="buffer">buffer</param>
    /// <param name="size">size</param>
    /// <param name="ptr">ptr</param>
    public void ReadBuffer<T>(nint buffer, uint size, void* ptr) where T : unmanaged
    {
        int state = _cl.EnqueueReadBuffer(_queue, buffer, true, 0, (uint)(size * sizeof(T)), ptr, 0, null, null);

        State(state);
    }

    /// <summary>
    /// Set an argument.
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="buffer">buffer</param>
    public void SetArgument(uint index, nint buffer)
    {
        int state = _cl.SetKernelArg(_kernel, index, (uint)sizeof(nint), &buffer);

        State(state);
    }

    /// <summary>
    /// Set an argument.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="index">index</param>
    /// <param name="value">value</param>
    public void SetArgument<T>(uint index, T value) where T : unmanaged
    {
        int state = _cl.SetKernelArg(_kernel, index, (uint)sizeof(T), value);

        State(state);
    }

    /// <summary>
    /// Run the kernel.
    /// </summary>
    /// <param name="dim">dim</param>
    /// <param name="size">size</param>
    public void Run(uint dim, uint size)
    {
        int state = _cl.EnqueueNdrangeKernel(_queue, _kernel, dim, null, size, null, 0, null, null);

        State(state);
    }

    public void Dispose()
    {
        foreach (nint buffer in _buffers)
        {
            _cl.ReleaseMemObject(buffer);
        }
        _buffers.Clear();

        _cl.ReleaseKernel(_kernel);
        _cl.ReleaseProgram(_program);
        _cl.ReleaseCommandQueue(_queue);
        _cl.ReleaseContext(_context);
        _cl.ReleaseDevice(_device);
        _cl.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Create a kernel from a string of code.
    /// </summary>
    /// <param name="code">code</param>
    /// <param name="method">method</param>
    /// <param name="options">options</param>
    /// <returns></returns>
    public static Kernel? Create(string code, string method, string[]? options = null)
    {
        CL cl = CL.GetApi();

        nint platform;
        nint device;
        nint context;
        nint queue;
        nint program;
        nint kernel;

        if (cl.GetPlatformIDs(1, &platform, null) != 0)
            return null;

        if (cl.GetDeviceIDs(platform, DeviceType.Gpu, 1, &device, null) != 0)
            return null;

        context = cl.CreateContext(null, 1, device, null, null, null);
        if (context == 0)
            return null;

        queue = cl.CreateCommandQueue(context, device, CommandQueueProperties.None, null);
        if (queue == 0)
            return null;

        options ??= new string[] { "-cl-opt-disable" };

        program = cl.CreateProgramWithSource(context, 1, new[] { code }, null, null);

        if (cl.BuildProgram(program, 1, &device, string.Join(" ", options), null, null) != 0)
        {
            nuint length;
            cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, 0, null, &length);
            byte* buffer = stackalloc byte[(int)length];
            cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, length, buffer, null);
            Console.WriteLine("Build error: " + Encoding.UTF8.GetString(buffer, (int)length));
            return null;
        }

        kernel = cl.CreateKernel(program, method, null);

        return new Kernel(cl, platform, device, context, queue, program, kernel);
    }

    /// <summary>
    /// Check the state.
    /// </summary>
    /// <param name="errorCode">errorCode</param>
    private static void State(int errorCode)
    {
        if ((ErrorCodes)errorCode != ErrorCodes.Success)
        {
            Console.WriteLine(errorCode);
        }
    }
}
