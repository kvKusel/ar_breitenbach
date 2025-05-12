Shader "Custom/SplineShader"
{
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _GlowTex ("Border Glow (Edges of path)", 2D) = "white" {}
    _GlowColor ("Glow Color", Color) = (1,1,1,1)
    _StripeTex ("Stripes", 2D) = "white" {}
    _StripeColor ("Stripe Color", Color) = (1,1,1,1)
    _StripeSize ("Stripe Size", Range (0.00, 2)) = 1
    _StripeOffset ("Stripe Offset", Float) = 0
    _BGTex ("Background", 2D) = "white" {}
    _BGColor ("Background Color", Color) = (1,1,1,1)
    
    [HideInInspector] _endNodePosition ("End Node Position", Vector) = (0,0,0,0)
    [HideInInspector] _startNodePosition ("Start Node Position", Vector) = (0,0,0,0)
    
    [Header(Animation Parameters)]
    [Space]
    [Space]
    [Range] _FadeOut ("Fade Out", Range (0.01, 1)) = 0.08
    [Range] _FadeIn ("Fade In", Range (0.01, 1)) = 0.08
}
    
    CGINCLUDE
    #include "UnityCG.cginc"
    ENDCG
    
SubShader {

    //but changes the look slightly (probably not noticable when not doing a 1-1 comparison ?)
    Tags {"Queue"="Transparent" "RenderType" = "Transparent" "IgnoreProjector"="True"}
    ZWrite Off
    Cull off
    Blend One One
 
    Pass {
        CGPROGRAM
        #pragma vertex UnlitVertex
        #pragma fragment UnlitFragment

        CBUFFER_START(UnityPerMaterial)
        //Main Color
        float4 _Color;
        
        //Glow texture and UV-Scale Translate and Color
        sampler2D _GlowTex;
        float4 _GlowTex_ST;
        float4 _GlowColor;
        
        //Stripe texture, UV-Scale Translate and Color
        sampler2D _StripeTex;
        float4 _StripeTex_ST;
        float4 _StripeColor;
        float _StripeSize;
        float _StripeOffset;
        
        //Background texture, UV-Scale Translate and Color
        sampler2D _BGTex;
        float4 _BGColor;
        
        //Start-End Position Vectors
        float4 _endNodePosition; 
        float4 _startNodePosition;
        
        //Fade Parameters
        float _FadeOut;
        float _FadeIn;
        CBUFFER_END

        struct Attributes
        {
            float4 pos : POSITION;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 vert : TEXCOORD2;
            float4 color    : COLOR0;
            float3 worldPos : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 pos : SV_POSITION;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 vert : TEXCOORD2;
            float4 color    : COLOR0;
            float3 worldPos : TEXCOORD3;
            UNITY_VERTEX_OUTPUT_STEREO
        };
 
        //Vertex program
        Varyings UnlitVertex(Attributes v) {
            Varyings o = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(attributes);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.pos = UnityObjectToClipPos(v.pos);
            o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _GlowTex);
            o.texcoord1.xy = TRANSFORM_TEX(v.texcoord1, _StripeTex);
            o.worldPos = mul(unity_ObjectToWorld, v.pos);
            o.color = _Color;
            o.vert = v.pos;
            return o;
        }
        
        //Fragment program
        float4 UnlitFragment(Varyings i) : SV_Target
        {
            _FadeIn = clamp(0, 1, _FadeIn);
            _FadeOut = clamp(0, 1, _FadeOut);
            
            //Near fading / falloff
            float startEndDistance = length(_startNodePosition - _endNodePosition);
            
            float3 viewDirW = _startNodePosition - i.worldPos;
            float viewDist = length(viewDirW);
            float nearFalloff = saturate((viewDist + 0.3) / (startEndDistance * _FadeIn));
            i.color.a *= (1.0f - nearFalloff);
            
            //Make stripes use world-coordinates for Y to avoid UV tiling artifacts
            float3 worldSpaceCoordinates = i.worldPos.xyz;
            worldSpaceCoordinates += 75 * viewDirW;
            
            //Offset the center of the world coordinates slightly further than the End Position ((1 * viewDirection)) to avoid UV wrap-around artifacts near the target.
            float2 correctedStripeUV = float2(i.texcoord1.y, dot(worldSpaceCoordinates, viewDirW + (1 * _StripeSize * viewDirW)) * 0.005);
            float clampedDist = (startEndDistance)/(20);
            correctedStripeUV *= float2(1, clamp(clampedDist, 0.75, 35 / startEndDistance * 0.5));
            //Scroll stripes
            correctedStripeUV -= float2(0, _StripeOffset);
            
            viewDirW = _endNodePosition - i.worldPos;
            viewDist = length(viewDirW);
            float farFalloff = saturate((viewDist + 0.5) / (startEndDistance * (1.0f - _FadeOut)) );
            i.color.a *= (1.0f - farFalloff);
            
            float4 c = tex2D(_StripeTex, correctedStripeUV);
            c = lerp(_StripeColor, c, c - 0.25);
            float4 main_tex = tex2D(_GlowTex, i.texcoord.xy);
            //main_tex = clamp(c + main_tex, 0, 1);
            float4 bgTex = tex2D(_BGTex, i.texcoord.xy) * _BGColor;
            float4 bgTexColor = lerp(bgTex, _BGColor * 0.5f, c);
            //Blend the Glow, Stripes and Background textures / colors together
            return (bgTexColor * (bgTex.a * 2 * _StripeColor * 2) * (i.color.a * 5)) + (main_tex * _GlowColor * (i.color.a * 3)) * 2;
        }
        ENDCG
        }
    }
}
