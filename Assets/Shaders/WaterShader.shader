Shader "Unlit/WaterShader" {
Properties {
	_TexAtlas ("WaterAtlas", 2D) = "white" {}
    _TexSize("Single Texture Size", Float) = 0
    _AnimSpeed("Animation Speed", Float) = 0
	_Opacity("Opacity", Float) = 1
    _Tint("Tint", Color) = (1, 1, 1, 1)
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _TexAtlas;
            float4 _TexAtlas_ST;
            float _AnimSpeed;
            float _TexSize;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _TexAtlas);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float _Opacity;
            float4 _Tint;
			
			fixed4 frag (v2f i) : SV_Target
			{
				float t =_Time.x * _AnimSpeed;
                float2 uv = i.texcoord + float2(0, _TexSize * floor(t));

				fixed4 col = tex2D(_TexAtlas, uv) * fixed4(1, 1, 1, _Opacity) * _Tint;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
}

}