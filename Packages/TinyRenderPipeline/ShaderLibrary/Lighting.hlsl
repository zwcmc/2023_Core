#ifndef TINY_RP_LIGHTING_INCLUDED
#define TINY_RP_LIGHTING_INCLUDED

#include "Packages/com.tiny.render-pipeline/ShaderLibrary/RealtimeLights.hlsl"

half3 LightingPBR(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS)
{
    half3 lightDirectionWS = light.direction;
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half3 radiance = light.color * NdotL;

    half3 brdf = brdfData.diffuse;
    brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

    return brdf * radiance;
}

half4 FragmentPBR(InputData inputData, SurfaceData surfaceData)
{
    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);

    half3 lightingColor = 0.0;
    Light mainLight = GetMainLight();

    half3 mainLightColor = LightingPBR(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS);
    lightingColor += mainLightColor;

    return half4(lightingColor, surfaceData.alpha);
}

#endif
