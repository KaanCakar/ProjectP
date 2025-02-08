Shader "Unlit/ChromaticAberrationShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RedOffset ("Red Channel Offset", Vector) = (0.01, 0, 0, 0)
        _GreenOffset ("Green Channel Offset", Vector) = (0, 0, 0, 0)
        _BlueOffset ("Blue Channel Offset", Vector) = (-0.01, 0, 0, 0)
        _Intensity ("Effect Intensity", Range(0, 1)) = 1
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
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _RedOffset;
            float2 _GreenOffset;
            float2 _BlueOffset;
            float _Intensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 redUV = i.uv + _RedOffset * _Intensity;
                float2 greenUV = i.uv + _GreenOffset * _Intensity;
                float2 blueUV = i.uv + _BlueOffset * _Intensity;

                fixed4 col;
                col.r = tex2D(_MainTex, redUV).r;
                col.g = tex2D(_MainTex, greenUV).g;
                col.b = tex2D(_MainTex, blueUV).b;
                col.a = 1;
                
                return col;
            }
            ENDCG
        }
    }
}