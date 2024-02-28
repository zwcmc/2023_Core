using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ForwardLights
{
    private static class LightDefaultValue
    {
        public static Vector4 DefaultLightPosition = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        public static Vector4 DefaultLightColor = Color.black;
    }

    private static class LightConstantBuffer
    {
        public static int _MainLightPosition;
        public static int _MainLightColor;

        public static int _AdditionalLightsCount;
        public static int _AdditionalLightsPosition;
        public static int _AdditionalLightsColor;
    }

    private Vector4[] m_AdditionalLightPositions;
    private Vector4[] m_AdditionalLightColors;

    public ForwardLights()
    {
        LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
        LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
        LightConstantBuffer._AdditionalLightsCount = Shader.PropertyToID("_AdditionalLightsCount");
        LightConstantBuffer._AdditionalLightsPosition = Shader.PropertyToID("_AdditionalLightsPosition");
        LightConstantBuffer._AdditionalLightsColor = Shader.PropertyToID("_AdditionalLightsColor");

        int maxAdditionalLights = TinyRenderPipeline.maxVisibleAdditionalLights;
        m_AdditionalLightPositions = new Vector4[maxAdditionalLights];
        m_AdditionalLightColors = new Vector4[maxAdditionalLights];
    }

    public void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = renderingData.commandBuffer;

        SetupLights(cmd, ref renderingData);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    private void SetupLights(CommandBuffer cmd, ref RenderingData renderingData)
    {
        SetupShaderLightConstants(cmd, ref renderingData);
    }

    private void SetupShaderLightConstants(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Main light data
        SetupMainLightConstants(cmd, ref renderingData);
        // Additional lights data
        SetupAdditionalLightConstants(cmd, ref renderingData);
    }

    private void SetupMainLightConstants(CommandBuffer cmd, ref RenderingData renderingData)
    {
        Vector4 lightPos, lightColor;
        InitializeLightConstants(renderingData.cullResults.visibleLights, renderingData.mainLightIndex, out lightPos, out lightColor);

        cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, lightPos);
        cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, lightColor);
    }

    private void SetupAdditionalLightConstants(CommandBuffer cmd, ref RenderingData renderingData)
    {
        int additionalLightsCount = SetupPerObjectLightIndices(ref renderingData);
        if (additionalLightsCount > 0)
        {
            var visibleLights = renderingData.cullResults.visibleLights;
            int maxAdditionalLightsCount = TinyRenderPipeline.maxVisibleAdditionalLights;
            for (int i = 0, lightIter = 0; i < visibleLights.Length && lightIter < maxAdditionalLightsCount; ++i)
            {
                if (renderingData.mainLightIndex != i)
                {
                    InitializeLightConstants(visibleLights, i, out m_AdditionalLightPositions[lightIter], out m_AdditionalLightColors[lightIter]);
                    lightIter++;
                }
            }

            cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition, m_AdditionalLightPositions);
            cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor, m_AdditionalLightColors);

            cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, new Vector4(TinyRenderPipeline.maxVisibleAdditionalLights, 0.0f, 0.0f, 0.0f));
        }
        else
        {
            cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, Vector4.zero);
        }
    }

    private void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex, out Vector4 lightPos, out Vector4 lightColor)
    {
        lightPos = LightDefaultValue.DefaultLightPosition;
        lightColor = LightDefaultValue.DefaultLightColor;

        if (lightIndex < 0)
            return;

        VisibleLight visibleLight = lights[lightIndex];
        var lightLocalToWorld = visibleLight.localToWorldMatrix;
        var lightType = visibleLight.lightType;

        if (lightType == LightType.Directional)
        {
            Vector4 dir = -lightLocalToWorld.GetColumn(2);
            lightPos = new Vector4(dir.x, dir.y, dir.z, 0.0f);
        }
        else
        {
            Vector4 pos = lightLocalToWorld.GetColumn(3);
            lightPos = new Vector4(pos.x, pos.y, pos.z, 1.0f);
        }

        // VisibleLight.finalColor already returns color in active color space
        lightColor = visibleLight.finalColor;
    }

    private int SetupPerObjectLightIndices(ref RenderingData renderingData)
    {
        if (renderingData.additionalLightsCount == 0)
            return renderingData.additionalLightsCount;

        var cullResults = renderingData.cullResults;
        var perObjectLightIndexMap = cullResults.GetLightIndexMap(Allocator.Temp);
        int globalDirectionalLightsCount = 0;
        int additionalLightsCount = 0;

        int maxVisibleAdditionalLightsCount = TinyRenderPipeline.maxVisibleAdditionalLights;
        int len = cullResults.visibleLights.Length;
        for (int i = 0; i < len; ++i)
        {
            if (additionalLightsCount >= maxVisibleAdditionalLightsCount)
                break;

            if (i == renderingData.mainLightIndex)
            {
                // Disable main light
                perObjectLightIndexMap[i] = -1;
                ++globalDirectionalLightsCount;
            }
            else
            {
                // Support additional directional light, spot light, and point light
                if (cullResults.visibleLights[i].lightType == LightType.Directional ||
                    cullResults.visibleLights[i].lightType == LightType.Spot ||
                    cullResults.visibleLights[i].lightType == LightType.Point)
                {
                    perObjectLightIndexMap[i] -= globalDirectionalLightsCount;
                }
                else
                {
                    // Disable unsupported lights
                    perObjectLightIndexMap[i] = -1;
                }

                ++additionalLightsCount;
            }
        }

        // Disable all remaining lights we cannot fit into the global light buffer
        for (int i = globalDirectionalLightsCount + additionalLightsCount; i < perObjectLightIndexMap.Length; ++i)
            perObjectLightIndexMap[i] = -1;

        cullResults.SetLightIndexMap(perObjectLightIndexMap);

        perObjectLightIndexMap.Dispose();

        return additionalLightsCount;
    }
}
