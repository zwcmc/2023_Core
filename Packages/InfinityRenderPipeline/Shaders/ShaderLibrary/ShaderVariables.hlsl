#ifndef _ShaderVariableInclude
#define _ShaderVariableInclude

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

float4 _Time; // (t/20, t, t*2, t*3)
float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)
float4 _CosTime; // cos(t/8), cos(t/4), cos(t/2), cos(t)
float4 unity_DeltaTime; // dt, 1/dt, smoothdt, 1/smoothdt
float4 _TimeParameters; // t, sin(t), cos(t)

float3 _WorldSpaceCameraPos;
float4 _ScreenParams;
float4 _ZBufferParams;
float4 _ProjectionParams;

CBUFFER_START(ViewUnifrom)
    int FrameIndex;
    int Prev_FrameIndex;
    float4 TAAJitter;
    float4x4 Matrix_WorldToView;
    float4x4 Matrix_ViewToWorld;
    float4x4 Matrix_Proj;
    float4x4 Matrix_InvProj;
    float4x4 Matrix_JitterProj;
    float4x4 Matrix_InvJitterProj;
    float4x4 Matrix_FlipYProj;
    float4x4 Matrix_InvFlipYProj;
    float4x4 Matrix_FlipYJitterProj;
    float4x4 Matrix_InvFlipYJitterProj;
    float4x4 Matrix_ViewProj;
    float4x4 Matrix_InvViewProj;
    float4x4 Matrix_ViewFlipYProj;
    float4x4 Matrix_InvViewFlipYProj;
    float4x4 Matrix_ViewJitterProj;
    float4x4 Matrix_InvViewJitterProj;
    float4x4 Matrix_ViewFlipYJitterProj;
    float4x4 Matrix_InvViewFlipYJitterProj;
    float4x4 Matrix_LastViewProj;
    float4x4 Matrix_LastViewFlipYProj;
    float4x4 Matrix_LastViewJitterProj;
    float4x4 Matrix_LastViewFlipYJitterProj;
CBUFFER_END

CBUFFER_START(UnityPerCamera)
    float4x4 unity_CameraToWorld;
    float4x4 unity_MatrixV;
    float4x4 unity_MatrixInvV;
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
    float4 unity_LODFade;
    float4 unity_WorldTransformParams;

    float4 unity_LightmapST;
    float4 unity_DynamicLightmapST;

    float4 unity_RenderingLayer;

    float4 unity_SHAr;
    float4 unity_SHAg;
    float4 unity_SHAb;
    float4 unity_SHBr;
    float4 unity_SHBg;
    float4 unity_SHBb;
    float4 unity_SHC;

    float4 unity_ProbeVolumeParams;
    float4x4 unity_ProbeVolumeWorldToObject;
    float4 unity_ProbeVolumeSizeInv; 
    float4 unity_ProbeVolumeMin; 

    float4 unity_ProbesOcclusion;

    float4x4 unity_MatrixPreviousM;
    float4x4 unity_MatrixPreviousMI;
    float4 unity_MotionVectorsParams;

    float4 unity_LightData;
    float4 unity_LightIndices[2];

    float4 unity_SpecCube0_HDR;
    float4 unity_SpecCube1_HDR;
CBUFFER_END

#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_I_V   unity_MatrixInvV
#define UNITY_MATRIX_P     Matrix_Proj
#define UNITY_MATRIX_I_P   Matrix_InvProj
//#define UNITY_MATRIX_VP    Matrix_ViewProj
#define UNITY_MATRIX_VP    Matrix_ViewJitterProj
#define UNITY_MATRIX_I_VP  _InvCameraViewProj
#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)
#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)
#define UNITY_MATRIX_IT_MV transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)
//#define UNITY_PREV_MATRIX_M unity_MatrixPreviousM;
//#define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI;
#endif