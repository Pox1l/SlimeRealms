Shader "UI/IrisWipe"
{
    Properties
    {
        _Color ("Main Color", Color) = (0,0,0,1)
        _Radius ("Hole Radius", Range(0, 1.5)) = 0
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _Radius;
            float _Softness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Posuneme UV souøadnice tak, aby støed (0,0) byl uprostøed obrázku
                o.uv = v.texcoord - float2(0.5, 0.5);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Vypoèítáme vzdálenost pixelu od støedu
                float dist = length(i.uv);
                
                // Vytvoøíme díru: Pokud je vzdálenost menší než Radius, bude prùhledná
                float alpha = smoothstep(_Radius, _Radius + _Softness, dist);
                
                return float4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}