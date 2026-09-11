Shader "Skybox/PanoramicCustom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (0.5, 0.5, 0.5, 1)
        _Exposure ("Exposure", Range(0, 2)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Tint;
            half _Exposure;
            float _Rotation;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;

                float rot = _Rotation * 3.14159265 / 180.0;
                float s = sin(rot);
                float c = cos(rot);
                float3 coords = o.texcoord;
                o.texcoord = float3(
                    coords.x * c - coords.z * s,
                    coords.y,
                    coords.x * s + coords.z * c
                );

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                float2 uv = float2(atan2(dir.x, dir.z) / (2 * 3.14159265) + 0.5, asin(dir.y) / 3.14159265 + 0.5);
                fixed4 col = tex2D(_MainTex, uv) * _Tint * _Exposure;
                return col;
            }
            ENDCG
        }
    }
}
