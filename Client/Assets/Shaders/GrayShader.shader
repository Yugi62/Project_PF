Shader "Custom/GrayShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _GrayAmount("GrayAmount", Range(0,1)) = 0
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha
                Cull Off
                ZWrite Off 
                ZTest Off
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
                float4 _MainTex_ST;
                float _GrayAmount;

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
                    float gray = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                    fixed4 grayColor = float4(gray, gray, gray, texColor.a);
                    return lerp(texColor, grayColor, _GrayAmount);
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}