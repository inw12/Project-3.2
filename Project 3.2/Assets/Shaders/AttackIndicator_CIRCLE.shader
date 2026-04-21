Shader "Custom/AttackIndicator_CIRCLE"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,0.3)
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Convert UV to centered coordinates (-0.5 to 0.5)
                float2 centered = i.uv - 0.5;

                // Get the distance of this pixel from the center
                float dist = length(centered);

                // Discard anything outside the 0.5 radius boundary
                // This clips the quad's corners into a circle
                clip(0.5 - dist);

                return _Color;
            }
            ENDCG
        }
    }
}
