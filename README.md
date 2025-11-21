# Foundation.OpenCL ⚡

**OpenCL binding for DotNet, idiomatic C#, zero cost abstraction.**

[![NuGet](https://img.shields.io/nuget/v/Foundation.OpenCL.svg)](https://www.nuget.org/packages/Foundation.OpenCL)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

> **Performance Density Matters** - 100 kernel chain executes in **18ms**

Foundation.OpenCL is a high-performance, modern OpenCL binding for .NET that combines the full power of OpenCL 3.0 with idiomatic C# design patterns. Built from the ground up for performance and reliability.

## 🚀 Why Foundation.OpenCL?

### **Zero-Cost Abstractions**
Get type safety and modern C# features without performance penalties:

```csharp
// Traditional bindings: clSetKernelArg(kernel, 0, sizeof(cl_mem), &buffer)
// Foundation: Type-safe, zero-overhead
kernel.SetArgBuffer(0, buffer);  // Compiles to optimal native calls
```

### **Idiomatic C#**
Work with spans, generics, and events instead of raw pointers:

```csharp
// Modern memory patterns
Span<float> data = stackalloc float[1024];
queue.EnqueueWriteBuffer(buffer, 0, data);
```

### **Proven Performance**
```csharp
// 100-kernel chain executes in 18ms
var events = new Event[100];
for (int i = 0; i < 100; i++)
{
    events[i] = queue.EnqueueNdRangeKernel(kernel, globalSize, localSize, 
                 i > 0 ? events[i-1] : default);
}
```

## 💡 Quick Start

### Installation
```bash
dotnet add package Foundation.OpenCL
```

### Your First OpenCL Program in 5 Minutes

```csharp
using Foundation.OpenCL;

// 1. Discover platforms and devices
var platform = Platform.GetPlatforms()[0];
var device = platform.GetDevices(DeviceType.Gpu)[0];

// 2. Create context and command queue
using var context = platform.CreateContext(new[] { device });
using var queue = context.CreateCommandQueue(device);

// 3. Build kernel from source
string kernelSource = """
__kernel void vector_add(__global const float* a, 
                        __global const float* b, 
                        __global float* c) {
    int i = get_global_id(0);
    c[i] = a[i] + b[i];
}
""";

using var program = context.CreateWithSource(kernelSource);
program.Build(new[] { device });

// 4. Create and execute kernel
using var kernel = program.CreateKernel("vector_add");

// 5. Manage memory with type safety
using var aBuffer = context.CreateBuffer(MemFlags.ReadOnly, 1024 * sizeof(float));
using var bBuffer = context.CreateBuffer(MemFlags.ReadOnly, 1024 * sizeof(float)); 
using var cBuffer = context.CreateBuffer(MemFlags.WriteOnly, 1024 * sizeof(float));

// 6. Execute with proper event synchronization
var kernelEvent = queue.EnqueueNdRangeKernel(kernel, 
    globalSize: new[] { (nuint)1024 },
    localSize: new[] { (nuint)64 });

kernelEvent.Wait(); // Blocks until completion
```

## 🎯 Advanced Features

### **Intel USM Extensions**
```csharp
using Foundation.OpenCL.Extensions.Intel;

// Unified Shared Memory allocation
void* deviceMemory = context.AllocateDeviceMemory(device, 1024 * sizeof(float));

// Set kernel arguments directly from USM pointers
kernel.SetArgMemPointer(0, deviceMemory);
```

### **Multi-GPU Ready**
```csharp
var devices = Platform.GetPlatforms().SelectMany(p => p.GetDevices()).ToArray();
var contexts = devices.Select(d => Context.CreateContext(new[] { d })).ToArray();

// Distribute work across all available GPUs
Parallel.For(0, devices.Length, i =>
{
    var queue = contexts[i].CreateCommandQueue(devices[i]);
    queue.EnqueueNdRangeKernel(kernel, globalSize, localSize);
});
```

### **Sophisticated Event System**
```csharp
// Multicast event callbacks
var completionEvent = queue.EnqueueNdRangeKernel(kernel, globalSize, localSize);

completionEvent.OnComplete += () => Console.WriteLine("Kernel completed!");
completionEvent.OnRunning += () => Console.WriteLine("Kernel started execution!");

// Complex dependency chains
var eventA = queue.EnqueueKernel(kernelA, size);
var eventB = queue.EnqueueKernel(kernelB, size, waitFor: eventA); 
var eventC = queue.EnqueueKernel(kernelC, size, waitFor: new[] { eventA, eventB });
```

## 🏗️ Architecture Benefits

### **Type-Safe Handles**
```csharp
// Compile-time safety: no mixing of handle types
Handle<Context> contextHandle = context.Handle;
Handle<Device> deviceHandle = device.Handle;

// This won't compile - caught at build time!
// contextHandle = deviceHandle; // Error!
```

### **Resource Management**
```csharp
// Automatic reference counting and disposal
using var context = Context.CreateContext(devices);
using var queue = context.CreateCommandQueue(device);
using var buffer = context.CreateBuffer(MemFlags.ReadWrite, size);

// Proper cleanup even with complex object graphs
```

### **Performance Optimizations**
- **Stack allocation** for temporary buffers
- **Aggressive inlining** of hot paths  
- **Span-based APIs** for zero-copy operations
- **Generic specialization** for common types

## 📊 Performance Showcase

### Kernel Chaining Benchmark

100 sequential kernels gemm 32x32 with event dependencies
Typical result: 18ms on Intel Arc Pro B60

## 🛠 Installation

### Package Manager
```bash
Install-Package Foundation.OpenCL
```

### .NET CLI
```bash
dotnet add package Foundation.OpenCL
```

### Project Reference
```xml
<PackageReference Include="Foundation.OpenCL" Version="1.0.0" />
```

## 🔧 Requirements

- **.NET 9.0** or later
- **OpenCL 1.2+** compatible drivers, full support for OpenCL 3.0
- **Windows/Linux/macOS** with GPU support

## 🤝 Collaboration

This project was developed through human-AI collaboration:

- **Project Lead & Architect**: Samuel Alexandre Vidal
- **AI Collaborators**: 
  - DeepSeek (implementation, code review, technical expertise)
  - Qwen3-Next-80B-A3B (help with native API mapping)  
  - Gemini 2.5 (initial concept collaboration)

The entire development process featured continuous collaboration between human expertise and multiple AI systems, each contributing unique strengths to create a production-quality library.

## 📄 License

MIT License - see [LICENSE.txt](LICENSE.txt) for details.

## 🚀 Getting Involved

- **Found a bug?** [Open an issue](https://github.com/samuel-vidal/Foundation.OpenCL/issues)
- **Have a feature request?** [Start a discussion](https://github.com/samuel-vidal/Foundation.OpenCL/discussions)
- **Want to contribute?** Check our contribution guidelines

---

**Ready to accelerate your GPU computing using idiomatic C#?** Get started with Foundation.OpenCL today! ⚡
