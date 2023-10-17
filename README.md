# SabaSharp
这是一款基于 .NET 7 的跨平台 MikuMikuDance 渲染器，支持 Windows、Linux、macOS 等系统。

## 技术实现
### 项目参考：[saba](https://github.com/benikabocha/saba)、[Coocoo3D](https://github.com/sselecirPyM/Coocoo3D)
这套代码绝大部分实现都是仿照的 [saba](https://github.com/benikabocha/saba)，我将它核心代码迁移到c#中并依照语言特点做了些许调整。

### 技术栈：[OpenGL ES](https://github.com/dotnet/Silk.NET)、[OpenCL](https://github.com/dotnet/Silk.NET)、[bullet3](https://github.com/bulletphysics/bullet3)
bullet3 在.NET框架下有许多优秀的绑定库，我这边选择的 [Evergine.Bullet](https://evergine.com/)。<br>
OpenCL 的存在是为了计算蒙皮动画，因其大量并行计算使用 GPU 会存在优势，原 saba 项目使用的cpu并行，该项目也是支持的。<br>

未来该项目会根据我学习进度不断扩展 Vulkan、WebGPU、光线追踪、卡通渲染等主流技术，目前来说这些都是零进展（太懒啦）。

## 效果图
![image](https://github.com/qian-o/SabaSharp/assets/84434846/01df2a13-9ff6-4dd7-8e26-ee855c7b9e32)
![image](https://github.com/qian-o/SabaSharp/assets/84434846/131bdb3b-07af-4792-97e8-99d848e96d5a)
