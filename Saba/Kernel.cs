﻿using System.Text;
using Silk.NET.OpenCL;

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

    public static bool UseOpenCL { get; set; } = true;

    public double Version { get; }

    public uint Alignment { get; }

    public uint[] MaxWorkItemSizes { get; }

    public bool UseCoarseBuffer { get; }

    internal Kernel(CL cl, nint platform, nint device, nint context, nint queue, nint program, nint kernel)
    {
        _cl = cl;
        _platform = platform;
        _device = device;
        _context = context;
        _queue = queue;
        _program = program;
        _kernel = kernel;
        _buffers = [];

        Version = GetVersion(cl, device);
        Alignment = GetAlignment(cl, device);
        MaxWorkItemSizes = GetMaxWorkItemSizes(cl, device);

        // 检测支持的 SVM 类型。
        // https://www.intel.cn/content/www/cn/zh/developer/articles/technical/opencl-20-shared-virtual-memory-overview.html
        if (Version >= 2.0)
        {
            DeviceSvmCapabilities svm_capabilities;
            cl.GetDeviceInfo(device, DeviceInfo.SvmCapabilities, sizeof(DeviceSvmCapabilities), &svm_capabilities, null);

            if (svm_capabilities.HasFlag(DeviceSvmCapabilities.CoarseGrainBuffer))
            {
                UseCoarseBuffer = true;
            }
        }
    }

    /// <summary>
    /// Create a buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="length">length</param>
    /// <param name="flags">flags</param>
    /// <param name="host">host</param>
    /// <returns></returns>
    public nint CreateBuffer<T>(int length, T* host = null, MemFlags flags = MemFlags.None) where T : unmanaged
    {
        nint buffer_id = _cl.CreateBuffer(_context, flags, (uint)(length * sizeof(T)), host, null);

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
    /// Allocate a SVM buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="length">length</param>
    /// <param name="flags">flags</param>
    /// <returns></returns>
    public T* SvmAlloc<T>(int length, MemFlags flags = MemFlags.None) where T : unmanaged
    {
        return (T*)_cl.Svmalloc(_context, (SvmMemFlags)flags, (uint)(length * sizeof(T)), 0);
    }

    /// <summary>
    /// Free a SVM buffer.
    /// </summary>
    /// <param name="ptr">ptr</param>
    public void FreeSvm(void* ptr)
    {
        _cl.Svmfree(_context, ptr);
    }

    /// <summary>
    /// Get a pointer to a buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="buffer">buffer</param>
    /// <param name="length">length</param>
    /// <param name="flags">flags</param>
    /// <returns></returns>
    public T* MapBuffer<T>(nint buffer, int length, MapFlags flags = MapFlags.None) where T : unmanaged
    {
        return (T*)_cl.EnqueueMapBuffer(_queue, buffer, false, flags, 0, (uint)(length * sizeof(T)), 0, null, null, null);
    }

    /// <summary>
    /// Delete a pointer to a buffer.
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="ptr">ptr</param>
    public void UnmapBuffer(nint buffer, void* ptr)
    {
        _cl.EnqueueUnmapMemObject(_queue, buffer, ptr, 0, null, null);
    }

    /// <summary>
    /// Map a SVM buffer.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="ptr">ptr</param>
    /// <param name="length">length</param>
    /// <param name="flags">flags</param>
    public void MapSvm<T>(T* ptr, int length, MapFlags flags = MapFlags.None) where T : unmanaged
    {
        _cl.EnqueueSvmmap(_queue, false, flags, ptr, (uint)(length * sizeof(T)), 0, null, null);
    }

    /// <summary>
    /// Unmap a SVM buffer.
    /// </summary>
    /// <param name="ptr">ptr</param>
    public void UnmapSvm(void* ptr)
    {
        _cl.EnqueueSvmunmap(_queue, ptr, 0, null, null);
    }

    /// <summary>
    /// Flush
    /// </summary>
    public void Flush()
    {
        State(_cl.Flush(_queue));
    }

    /// <summary>
    /// Finish
    /// </summary>
    public void Finish()
    {
        State(_cl.Finish(_queue));
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
    /// Set an SVM argument.
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="ptr">ptr</param>
    public void SetSvmArgument(uint index, void* ptr)
    {
        int state = _cl.SetKernelArgSvmpointer(_kernel, index, ptr);

        State(state);
    }

    /// <summary>
    /// Run the kernel by 1 dimension.
    /// </summary>
    /// <param name="size">size</param>
    public void Run1(int size)
    {
        uint maxWorkItemSize = MaxWorkItemSizes[0] / 2;

        nuint* globalWorkSize = stackalloc nuint[1];
        globalWorkSize[0] = (nuint)(size / maxWorkItemSize + 1) * maxWorkItemSize;

        nuint* localWorkSize = stackalloc nuint[1];
        localWorkSize[0] = maxWorkItemSize;

        int state = _cl.EnqueueNdrangeKernel(_queue,
                                             _kernel,
                                             1,
                                             null,
                                             globalWorkSize,
                                             localWorkSize,
                                             0,
                                             null,
                                             null);

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
        if (!UseOpenCL)
        {
            return null;
        }

        CL cl = CL.GetApi();

        nint platform;
        nint device;
        nint context;
        nint queue;
        nint program;
        nint kernel;

        (platform, device) = GetHighestVersion(cl);

        if (platform == 0)
            return null;

        if (device == 0)
            return null;

        context = cl.CreateContext(null, 1, in device, null, null, null);
        if (context == 0)
            return null;

        queue = cl.CreateCommandQueue(context, device, CommandQueueProperties.None, null);
        if (queue == 0)
            return null;

        options ??= ["-cl-opt-disable"];

        program = cl.CreateProgramWithSource(context, 1, [code], null, null);

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
        if (kernel == 0)
            return null;

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

    /// <summary>
    /// Get the highest version platform and device.
    /// </summary>
    /// <param name="cl">cl</param>
    /// <returns></returns>
    private static (nint Platform, nint Device) GetHighestVersion(CL cl)
    {
        uint* num_platforms = stackalloc uint[1];
        cl.GetPlatformIDs(0, null, num_platforms);

        nint* platform_ids = stackalloc nint[(int)*num_platforms];
        cl.GetPlatformIDs(*num_platforms, platform_ids, null);

        nint highest_platform = 0;
        nint highest_device = 0;
        double highest_version = 0.0;
        for (int i = 0; i < *num_platforms; i++)
        {
            nint platform = platform_ids[i];

            uint num_devices = GetMaxDevices(cl, platform);

            nint[] device_ids = GetDevices(cl, platform, num_devices);

            for (int j = 0; j < num_devices; j++)
            {
                nint device = device_ids[j];

                double version = GetVersion(cl, device);

                if (version > highest_version)
                {
                    if (highest_version > 0.0)
                    {
                        cl.ReleaseDevice(highest_device);
                    }

                    highest_platform = platform;
                    highest_device = device;
                    highest_version = version;
                }
                else
                {
                    cl.ReleaseDevice(device);
                }
            }
        }

        return (highest_platform, highest_device);
    }

    /// <summary>
    /// Get the maximum number of devices.
    /// </summary>
    /// <param name="cl">cl</param>
    /// <param name="platform">platform</param>
    /// <returns></returns>
    private static uint GetMaxDevices(CL cl, nint platform)
    {
        uint* num_devices = stackalloc uint[1];
        cl.GetDeviceIDs(platform, DeviceType.Gpu, 0, null, num_devices);

        return *num_devices;
    }

    /// <summary>
    /// Get the devices.
    /// </summary>
    /// <param name="cl">cl</param>
    /// <param name="platform">platform</param>
    /// <param name="num_devices">num_devices</param>
    /// <returns></returns>
    private static nint[] GetDevices(CL cl, nint platform, uint num_devices)
    {
        nint[] device_ids = new nint[(int)num_devices];

        fixed (nint* ptr = device_ids)
        {
            cl.GetDeviceIDs(platform, DeviceType.Gpu, num_devices, ptr, null);
        }

        return device_ids;
    }

    /// <summary>
    /// Get the version.
    /// </summary>
    /// <param name="cl">cl</param>
    /// <param name="device">device</param>
    /// <returns></returns>
    private static double GetVersion(CL cl, nint device)
    {
        byte* version = stackalloc byte[1024];
        cl.GetDeviceInfo(device, DeviceInfo.Version, 1024, version, null);

        string version_string = new((sbyte*)version);

        int index = version_string.IndexOf(' ');
        version_string = version_string.Substring(index + 1, 3);

        return Convert.ToDouble(version_string);
    }

    /// <summary>
    /// Get the alignment.
    /// </summary>
    /// <param name="cl">cl</param>
    /// <param name="device">device</param>
    /// <returns></returns>
    private static uint GetAlignment(CL cl, nint device)
    {
        uint* alignment = stackalloc uint[1];
        cl.GetDeviceInfo(device, DeviceInfo.MemBaseAddrAlign, sizeof(uint), alignment, null);

        return *alignment;
    }

    /// <summary>
    /// Get the maximum work item sizes.
    /// </summary>
    /// <param name="cl">cl</param>
    /// <param name="device">device</param>
    /// <returns></returns>
    private static uint[] GetMaxWorkItemSizes(CL cl, nint device)
    {
        uint maxWorkItemDimensions = 0;
        cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemDimensions, sizeof(uint), &maxWorkItemDimensions, null);

        nuint* maxWorkItemSizes = stackalloc nuint[(int)maxWorkItemDimensions];
        cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemSizes, (nuint)(sizeof(nuint) * maxWorkItemDimensions), maxWorkItemSizes, null);

        uint[] sizes = new uint[(int)maxWorkItemDimensions];

        for (int i = 0; i < maxWorkItemDimensions; i++)
        {
            sizes[i] = (uint)maxWorkItemSizes[i];
        }

        return sizes;
    }
}