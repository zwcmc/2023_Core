#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class PostProcessingData : ScriptableObject
{
    /// <summary>
    /// This controls the size of the bloom texture.
    /// </summary>
    public enum BloomDownscaleMode
    {
        /// <summary>
        /// Use this to select half size as the starting resolution.
        /// </summary>
        Half,

        /// <summary>
        /// Use this to select quarter size as the starting resolution.
        /// </summary>
        Quarter
    }

    /// <summary>
    /// Tonemapping algorithms
    /// </summary>
    public enum TonemappingMode
    {
        /// <summary>
        /// Do not apply tonemapping.
        /// </summary>
        None,

        /// <summary>
        /// Neutral tonemapper
        /// </summary>
        Neutral,

        /// <summary>
        /// ACES Filmic reference tonemapper
        /// </summary>
        ACES
    }

    public enum AntialiasingMode
    {
        [InspectorName("No Anti-aliasing")]
        None,
        [InspectorName("Fast Approximate Anti-aliasing (FXAA)")]
        FastApproximateAntialiasing
    }

#if UNITY_EDITOR
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
    private class CreatePostProcessingDataAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = CreateInstance<PostProcessingData>();
            AssetDatabase.CreateAsset(instance, pathName);
            ResourceReloader.ReloadAllNullIn(instance, TinyRenderPipelineAsset.packagePath);
            Selection.activeObject = instance;
        }
    }

    [MenuItem("Assets/Create/Rendering/Post Processing Data")]
    private static void CreatePostProcessingData()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreatePostProcessingDataAsset>(), "New Post Processing Data.asset", null, null);
    }
#endif

    [Serializable, ReloadGroup]
    public class ShaderResources
    {
        [Reload("Shaders/PostProcessing/UberPost.shader")]
        public Shader uberPostShader;

        [Reload("Shaders/PostProcessing/Bloom.shader")]
        public Shader bloomShader;

        [Reload("Shaders/PostProcessing/LutBuilder.shader")]
        public Shader lutBuilderShader;

        [Reload("Shaders/PostProcessing/FXAA.shader")]
        public Shader fxaaShader;
    }

    [Serializable]
    public class Bloom
    {
        [Min(0f)]
        public float threshold = 0.9f;

        [Min(0f)]
        public float intensity = 0f;

        [Range(0f, 1f)]
        public float scatter = 0.7f;

        [Min(0f)]
        public float clamp = 65472f;

        [Range(2, 8)]
        public int maxIterations = 6;

        public BloomDownscaleMode downscale = BloomDownscaleMode.Half;

        public bool highQualityFiltering = false;

        public bool IsActive() => intensity > 0f;
    }

    [Serializable]
    public class Tonemapping
    {
        public TonemappingMode mode = TonemappingMode.None;
    }

    [Serializable]
    public class ColorAdjustments
    {
        public float postExposure = 0f;

        [Range(-100f, 100f)]
        public float contrast = 0f;

        [ColorUsage(false, true)]
        public Color colorFilter = Color.white;

        [Range(-180f, 180f)]
        public float hueShift = 0f;

        [Range(-100f, 100f)]
        public float saturation = 0f;
    }

    [Serializable]
    public class WhiteBalance
    {
        [Range(-100f, 100f)]
        public float temperature = 0f;

        [Range(-100f, 100f)]
        public float tint = 0f;
    }

    public ShaderResources shaders = default;

    public Bloom bloom = default;

    public Tonemapping tonemapping = default;

    public ColorAdjustments colorAdjustments = default;

    public WhiteBalance whiteBalance = default;

    public AntialiasingMode antialiasingMode = AntialiasingMode.None;
}
