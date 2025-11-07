Shader "Custom/InstancingShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _RENDER_MODE ("RENDER MODE", int) = 0

        /*
        RENDER_MODE:
        0: default Color
        1: speed
        2: mass
        */
    }
    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            

            float4x4 TranslationMatrix(float3 t)
            {
                return float4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    t.x, t.y, t.z, 1
                );
            }

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            struct dotData
            {
                float3 color;
                float2 position;
                float2 velocity;
                float mass;
            };

            uniform StructuredBuffer<dotData> _dotData;
            
            int _RENDER_MODE = 0;

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint instanceID = GetIndirectInstanceID(svInstanceID);

                if (_dotData[instanceID].mass == 0){
                    o.color = float4(0,0,0,0);
                    o.pos = float4(0,0,0,0);
                    return o;
                }
                
                float maxmass = 10;

                if (_RENDER_MODE == 0){ // default Color
                    o.color = float4(_dotData[instanceID].color.x, _dotData[instanceID].color.y, _dotData[instanceID].color.z, 1);
                }
                else if (_RENDER_MODE == 1){ // speed

                    float currAbsVelo = sqrt(pow(_dotData[instanceID].velocity.x, 2) + pow(_dotData[instanceID].velocity.y, 2));
                    o.color = float4(clamp(currAbsVelo / 1, 0, 1), (1 - clamp(currAbsVelo / 1, 0, 1)), 0, 1);

                }
                else if (_RENDER_MODE == 2){ // mass
                    o.color = float4(clamp(_dotData[instanceID].mass / maxmass, 0, 1), clamp(_dotData[instanceID].mass / maxmass, 0, 1), clamp(_dotData[instanceID].mass / maxmass, 0, 1), 1);
                }

                float4 wpos = mul(TranslationMatrix(float3(0,0,0)), v.vertex + float4(_dotData[instanceID].position, 0, 0));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
