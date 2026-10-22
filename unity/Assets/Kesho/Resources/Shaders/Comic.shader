Shader "Kesho/Comic"
{
    Properties { _BaseColor("Colour", Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct A { float4 p:POSITION; float3 n:NORMAL; };
            struct V { float4 p:SV_POSITION; float3 n:TEXCOORD0; float3 world:TEXCOORD1; };
            V Vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); o.n=TransformObjectToWorldNormal(a.n); return o; }
            half4 Frag(V i):SV_Target
            {
                Light light=GetMainLight(); half d=dot(normalize(i.n),light.direction);
                half band=d>.48?1.0:(d>-.12?.84:.64);
                half edge=saturate(dot(normalize(i.n),normalize(GetWorldSpaceViewDir(i.world))));
                half3 colour=_BaseColor.rgb*band;
                colour=lerp(colour*.72,colour,smoothstep(.0,.16,edge));
                return half4(colour,1);
            }
            ENDHLSL
        }
    }
}
