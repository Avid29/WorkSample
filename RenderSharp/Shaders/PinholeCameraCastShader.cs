// Adam Dernis 2023

//
//                                    Excerpt Summary
// ----------------------------------------------------------------------------
// 
//      This is an excerpt of a Ray Tracer I made as part of Global Illuminations
// course. This file is a pinhole camera ray caster written in C# to compile to HLSL.
//
// ----------------------------------------------------------------------------
// 
// This is an excerpt from my project RenderSharp on GitHub.

// The full project can be found here:
// https://github.com/Avid29/RenderSharp
//
// A link to the active file is available here:
// https://github.com/Avid29/RenderSharp/blob/main/src/Renderers/RenderSharp.RayTracing/Shaders/Pipeline/RayCasting/Camera/PinholeCameraCastShader.cs
//
// And a permalink to when this excerpt was taken is available here:
// https://github.com/Avid29/RenderSharp/blob/853aecc4bfaab94f1efca91ef4269d0a06c91e8a/src/Renderers/RenderSharp.RayTracing/Shaders/Pipeline/RayCasting/Camera/PinholeCameraCastShader.cs
// 

namespace WorkSample.RenderSharp;

/// <summary>
/// A shader that casts rays from a camera.
/// </summary>
[AutoConstructor]
[EmbeddedBytecode(DispatchAxis.XY)]
public readonly partial struct PinholeCameraCastShader : ICameraCastShader
{
    private readonly Tile tile;
    private readonly int2 imageSize;
    private readonly PinholeCamera camera;
    private readonly ReadWriteBuffer<Ray> rayBuffer;

    /// <inheritdoc/>
    public void Execute()
    {
        // Get the index of resources managed by the current thread
        // in both 2D textures and flat buffers
        int2 index2D = ThreadIds.XY;
        int fIndex = (index2D.Y * DispatchSize.X) + index2D.X;
        int2 imageIndex = index2D + tile.offset;

        // Calculate the camera u and v normalized pixel coordinates.
        float u = (imageIndex.X + 0.5f) / imageSize.X;
        float v = 1 - (imageIndex.Y + 0.5f) / imageSize.Y;

        // Create a ray from the camera and store it in the ray buffer.
        var ray = PinholeCamera.CreateRay(camera, u, v);
        rayBuffer[fIndex] = ray;
    }
}