Shader "Custom/BlinkShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _BlinkColor("Blink Color", Color) = (1,1,1,1)
        _Blink("Blink", Range(0, 1)) = 0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 100

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            Lighting Off
            ZWrite Off

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
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
                fixed4 _BlinkColor;
                float _Blink;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 texColor = tex2D(_MainTex, i.uv);
                    texColor.rgb = lerp(texColor.rgb, _BlinkColor.rgb, _Blink);
                    return texColor;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}