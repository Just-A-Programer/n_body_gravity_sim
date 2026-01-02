Shader "Custom/InstancingGrids"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _RENDER_GRID_MODE ("RENDER GRID MODE", int) = 0

        /*
        RENDER MODE:
        0: no render
        1: default Color 
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

            float4      _Color;
            sampler2D   _MainTex;
            
            float4x4 TranslationMatrix(float3 t)
            {
                return float4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    t.x, t.y, t.z, 1
                );
            }
            
            struct Grid_str_ins
            {
                int2 position; // strating from the bottom left corner of the grid
                float2 localCenterOfMass;
                float mass;
            
                /*
                Grid Sizes (6):
                0: 0.5x0.5
                1: 1x1     (x2)
                2: 2x2     (x2)
                3: 4x4     (x2)
                4: 8x8     (x2)
                5: 16x16   (x2)
                */
            
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };
            
            uniform StructuredBuffer<Grid_str_ins> GridBuff;
            
            float sensitivity = 10;
            
            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                
                
                float4 wpos = mul(TranslationMatrix(float3(0,0,0)), v.vertex + float4(float(GridBuff[instanceID].position.x), float(GridBuff[instanceID].position.y) , 0, 0));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                //o.color = float4(clamp(float(GridBuff[instanceID].position.x) / sensitivity, 0, 1), 0, clamp(float(GridBuff[instanceID].position.y) / sensitivity, 0, 1), 1);
                o.color = _Color;
                
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
