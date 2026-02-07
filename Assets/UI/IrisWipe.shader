Shader "UI/IrisWipe"
{
    Properties
    {
        // --- TOTO JE TA OPRAVA ---
        // Unity UI Image vyžaduje tuto property, jinak hází error.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        _Color ("Main Color", Color) = (0,0,0,1)
        _Radius ("Hole Radius", Range(0, 1.5)) = 0 
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.1 
        
        // Nutné pro maskování v UI (Stencil), aby to neházelo warningy
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        // Nastavení pro správné fungování UI maskování
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
        Lighting Off 
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc" // Knihovna pro UI pomocné funkce

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;     // Originální UV (0..1) pro texturu
                float2 uvCentered : TEXCOORD1; // Posunuté UV (-0.5..0.5) pro kruh
                fixed4 color : COLOR;
            };

            fixed4 _Color;
            float _Radius;
            float _Softness;
            sampler2D _MainTex; // Deklarace promìnné pro texturu
            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Uložíme si normální UV pro pøípadnou texturu
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                // Vytvoøíme si centrované UV pro výpoèet kruhu (støed je 0,0)
                o.uvCentered = v.texcoord - float2(0.5, 0.5);
                
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Vzdálenost pixelu od støedu (použijeme centrované UV)
                float dist = length(i.uvCentered);
                
                // Výpoèet kruhové díry (Alpha)
                float circleAlpha = smoothstep(_Radius, _Radius + _Softness, dist);
                
                // Naèteme barvu (èernou)
                // (Technicky tady násobíme i texturou, aby Unity UI neøvalo, ale texture je bílá)
                fixed4 texColor = tex2D(_MainTex, i.uv) * i.color;
                
                // Výsledná alfa je kombinace barvy a našeho kruhu
                texColor.a *= circleAlpha;
                
                return texColor;
            }
            ENDCG
        }
    }
}