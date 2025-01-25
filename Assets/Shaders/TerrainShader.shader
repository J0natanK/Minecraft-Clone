Shader "Unlit/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _ShadowUp("Shadow Strength Up", Float) = 0
        _ShadowDown("Shadow Strength Down", Float) = 0
        _ShadowLeft("Shadow Strength Left", Float) = 0
        _ShadowRight("Shadow Strength Right", Float) = 0
        _ShadowForward("Shadow Strength Forward", Float) = 0
        _ShadowBack("Shadow Strength Back", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            float _ShadowUp;
            float _ShadowDown;
            float _ShadowRight;
            float _ShadowLeft;
            float _ShadowForward;
            float _ShadowBack;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = i.normal;
                fixed4 color = tex2D(_MainTex, i.uv);

                if (all(n == float3(0, 1, 0)))       // Up
                    color *= _ShadowUp;
                else if (all(n == float3(0, -1, 0))) // Down
                    color *= _ShadowDown;
                else if (all(n == float3(1, 0, 0)))  // Right
                    color *= _ShadowRight;
                else if (all(n == float3(-1, 0, 0))) // Left
                    color *= _ShadowLeft;
                else if (all(n == float3(0, 0, 1)))  // Forward
                    color *= _ShadowForward;
                else if (all(n == float3(0, 0, -1))) // Backward
                    color *= _ShadowBack;
                else
                    color *= fixed4(1, 1, 1, 1); // Default White
                
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;


            }
            ENDCG
        }
    }
}
