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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
            #pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            float4      _Color;
            sampler2D   _MainTex;
            
            struct Grid_str_ins
            {
                float2 position; // strating from the bottom left corner of the grid
                int2 MassTPossition;
                int mass;
                int jailerID;
            
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
            
            int Level;
            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                
                float sensitivity = 10;
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                static const uint GridSideCellLenght[6] = {2048, 1024, 512, 256, 128, 64};
                static const float CellLenght[6] = {0.5, 1.0, 2.0, 4.0, 8.0, 16.0};
                static const int FloatIntScaler = 1000000; // 10^6
                
                float x = float(instanceID) % float(GridSideCellLenght[Level]);
                float y = float(instanceID) / float(GridSideCellLenght[Level]);
                
                float2 pos = float2(x, y) * CellLenght[Level] + float2(CellLenght[Level]/2, CellLenght[Level]/2);
                //pos = float2(instanceID, 0) * CellLenght[Level];
                float3 worldPos = float3(v.vertex.xy + pos,0);
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));

                float mass01 = saturate(float(GridBuff[instanceID].mass) / FloatIntScaler);

                //float4(pos.x/1024, 0, pos.y/1024, 1);    float4(float(instanceID)/4194304,0,0,1);
                o.color = float4(mass01,
                                 mass01,
                                 mass01,
                                 1);//_Color;
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
