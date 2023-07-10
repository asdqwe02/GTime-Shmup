Shader "Custom/TextureScroll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _ScrollXSpeed("X Scroll Speed",Range(0,100)) = 0.1
        _ScrollYSpeed("Y Scroll Speed",Range(0,100)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uvmask : TEXCOORD1;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4 _MaskTex_ST;
            fixed _ScrollXSpeed;
            fixed _ScrollYSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.uvmask = v.uv;
                o.uvmask = TRANSFORM_TEX(v.uv, _MaskTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // half4 sub;
                half2 uv = i.uv;
                half t = _Time.x - floor(_Time.x);
                uv.x = i.uv.x + t * _ScrollXSpeed;
                uv.y = i.uv.y + t * _ScrollYSpeed;
                half opacity = tex2D(_MaskTex, i.uvmask).r;
                fixed4 col = tex2D(_MainTex, uv) * i.color;
                return col * opacity;
            }
            ENDCG
        }
    }
}