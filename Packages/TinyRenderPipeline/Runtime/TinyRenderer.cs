#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Rendering;

public partial class TinyRenderer
{
    private static class Profiling
    {
        public static readonly ProfilingSampler drawOpaque = new ProfilingSampler($"{nameof(DrawOpaque)}");
        public static readonly ProfilingSampler drawTransparent = new ProfilingSampler($"{nameof(DrawTransparent)}");
        public static readonly ProfilingSampler drawGizmos = new ProfilingSampler($"{nameof(DrawGizmos)}");
    }

    private ForwardLights m_ForwardLights;

    public TinyRenderer()
    {
        m_ForwardLights = new ForwardLights();
    }

    public void Execute(ref RenderingData renderingData)
    {
        var context = renderingData.renderContext;
        var camera = renderingData.camera;
        var cmd = renderingData.commandBuffer;

        SetCameraProperties(context, camera);
        ClearRenderTarget(cmd, camera);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        m_ForwardLights.Setup(context, ref renderingData);

        DrawOpaque(context, ref renderingData);

        DrawSkybox(context, cmd, camera);

        DrawTransparent(context, ref renderingData);

        DrawGizmos(context, cmd, camera);
    }

    public void Dispose(bool disposing)
    {

    }

    private void SetCameraProperties(ScriptableRenderContext context, Camera camera)
    {
        context.SetupCameraProperties(camera);
    }

    private void ClearRenderTarget(CommandBuffer cmd, Camera camera)
    {
        CameraClearFlags flags = camera.clearFlags;
        cmd.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
    }

    private void DrawGizmos(ScriptableRenderContext context, CommandBuffer cmd, Camera camera)
    {
#if UNITY_EDITOR
        if (!Handles.ShouldRenderGizmos())
            return;

        using (new ProfilingScope(cmd, Profiling.drawGizmos))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
#endif
    }
}
