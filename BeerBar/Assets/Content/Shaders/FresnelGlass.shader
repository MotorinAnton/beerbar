Shader "VFX/FresnelGlass"
{
    Properties
    {
        //Режимы смешивания
        [Header(Blend and Offset)]
        [Enum(One, 1, SrcAlpha, 5)] _Blend1 ("Blend1 mode subset", Float) = 1
        [Enum(One, 1, OneMinusSrcAlpha, 10)] _Blend2 ("Blend2 mode subset", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2

        _CamOffset ("Camera offset", Range(-100, 100)) = 0
        
        _Mask ("Mask", 2D) = "white" { }

        [Header(Reflection settings)]
        [Normal]_NormalRefl ("Reflection Normal", 2D) = "bump" { }
        [NoScaleOffset] _CubemapTex("Cubemap", Cube) = "grey" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _NormalScale ("Normal Scale", Range(0, 2)) = 1
        _Roughness ("Roughness", Range(0, 7)) = 1
        _DissolveMask2("Dissolve Mask 2", 2D) = "white" {}
        // _DissolveMask("Dissolve Mask 2", 2D) = "white" {}

        _Mult ("Multiply", Range(0, 10)) = 1

        _Fresnel("Fresnel", Range(0,10)) = 1
        _FresnelPower("Fresnel Power", Range(0,5)) = 1
        _ColorFill("Color Fill", Color) = (1,1,1,1)

        _Dissolve("Dissolve", Range(-1,1)) = 1
        _Max("Smoothing", Range(0,1)) = 1

        [HideInInspector]
        _Transparent ("transparency Factor", Range(0, 1)) = 1
        [HideInInspector]
        _DissolveEdgeColor ("Dissolve Color", Color) = (1, 1, 1, 1)
        [HideInInspector]
        _SfDissolveBlendValue("SF smoothness",         Range(0,1)) = 1
        [HideInInspector]
        _DissolveBlendValue("smoothness",         Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "IgnoreProjector" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend [_Blend1][_Blend2]
        ZWrite Off
        Cull[_Cull]

        Pass
        {
            CGPROGRAM

            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ SF_USE_DISSOLVE

            #include "UnityStandardUtils.cginc"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex: POSITION;
                half4 color: COLOR;
                half2 uv: TEXCOORD0;
                half3 normal: NORMAL;
                half4 tangent: TANGENT;
                half dissolve : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex: SV_POSITION;
                half4 color: COLOR;
                half3 normal: NORMAL;
                half2 uv: TEXCOORD0;
                half2 uv2: TEXCOORD6;
                half3 positionWS: TEXCOORD7;
                half3 normalWS: TEXCOORD2;
                half3 tangentWS: TEXCOORD3;
                half3 binormalWS: TEXCOORD4;
                half3 viewDir: TEXCOORD5;
                half dissolve : TEXCOORD1;
            };

            sampler2D_half _NormalRefl;
            sampler2D_half _Mask;
            sampler2D _DissolveMask2;
            samplerCUBE _CubemapTex;
            sampler2D_half _DissolveMask;


            half4 _Mask_ST;
            float4 _DissolveMask2_ST;
            half4 _Color;
            half _CamOffset;
            half _NormalScale;
            half _Mult;
            half _Roughness;
            half _FresnelEmission;
            half _FresnelExponent;
            half _Transparent;
            half _Fresnel;
            half _FresnelPower;
            half4 _ColorFill;
            half _Dissolve;
            half _Max;
            half _SfDissolveBlendValue;
            half _DissolveBlendValue;
            half4 _DissolveEdgeColor;


            v2f vert(appdata v)
            {
                v2f o;

                float3 camVtx = UnityObjectToViewPos(v.vertex);
                camVtx.xyz -= normalize(camVtx.xyz) * _CamOffset;
                o.vertex = mul(UNITY_MATRIX_P, float4(camVtx, 1.0));

                

                o.color = v.color * _Color * _Transparent;
                o.normal = v.normal;
                o.uv = TRANSFORM_TEX(v.uv, _Mask);
                o.uv2 = TRANSFORM_TEX(v.uv, _DissolveMask2);

                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.positionWS = mul(unity_ObjectToWorld, v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.tangentWS = UnityObjectToWorldDir(v.tangent.xyz);
                o.binormalWS = cross(o.normalWS, o.tangentWS) * v.tangent.w * unity_WorldTransformParams.w;

                o.dissolve = v.dissolve + _Dissolve;

                return o;
            }


            half4 frag(v2f i): SV_Target
            {

                half3 viewWS = normalize(_WorldSpaceCameraPos - i.positionWS.xyz);
                half3 tgNormal = UnpackScaleNormal(tex2D(_NormalRefl, i.uv), _NormalScale).xyz;
                half  dissolveMask = tex2D(_DissolveMask2, i.uv2).r;

                i.normalWS = normalize(tgNormal.x * i.tangentWS
                + tgNormal.y * i.binormalWS
                + tgNormal.z * i.normalWS);


                half3 reflectionDir = reflect(-viewWS, i.normalWS);
                // half4 refl = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectionDir, _Roughness * UNITY_SPECCUBE_LOD_STEPS);

                half4 refl = texCUBElod(_CubemapTex, half4(reflectionDir,_Roughness));

                //Френель с заливкой цветом
                half fresnelFill = abs(dot(i.viewDir, i.normal));
                half powFresnelFill = 1 - saturate(pow(fresnelFill, _FresnelPower) * _Fresnel);


                half mask = tex2D(_Mask, i.uv).r;

                half4 col = refl * i.color * _Mult * mask;

                col.rgb += _ColorFill.rgb * powFresnelFill;

                col -= smoothstep(dissolveMask, dissolveMask - _Max, i.dissolve);

                half edge = tex2Dlod(_DissolveMask, float4(i.uv,0,0)).r - _DissolveBlendValue;
                if(edge > 0.1)
                {
                    discard;
                }
                col.a *= smoothstep(0.1, 0.0, edge);
                col.rgb += smoothstep(0.04, 0.0, abs(edge - 0.04)) * _DissolveEdgeColor.rgb;

                return col;
            }
            ENDCG

        }
    }
}
