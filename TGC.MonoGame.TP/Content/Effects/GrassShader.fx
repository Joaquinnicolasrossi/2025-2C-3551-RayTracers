#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

// GrassWithTexture.fx
float4x4 World;
float4x4 View;
float4x4 Projection;

float Time;            // segundos desde C#
float WindSpeed = 1;   // velocidad animación
float WindScale = 0.15;// escala espacial del patrón
float WindStrength = 0.7;
float Exposure = 1.4;

float Tiling = 8.0;           // cuantas repeticiones de la textura en el mesh
float ScrollSpeed = 0.02;     // desplazamiento de la textura
float TextureInfluence = 0.6; // [0..1] cuánto pesa la textura sobre el color procedural

float3 Ambient = float3(0.25, 0.45, 0.18);
float3 BaseColor = float3(0.18, 0.80, 0.25);
float3 Highlight = float3(0.45, 0.95, 0.55);

// Texture + sampler
texture GrassTexture;
sampler2D GrassSampler = sampler_state
{
    Texture = <GrassTexture>;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VSInput
{
    float4 Position : POSITION;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position  : POSITION;
    float3 Normal    : TEXCOORD0;
    float3 WorldPos  : TEXCOORD1;
    float2 TexCoord  : TEXCOORD2;
};

VSOutput VSMain(VSInput input)
{
    VSOutput o;
    float4 worldPos = mul(input.Position, World);
    o.Position = mul(mul(worldPos, View), Projection);
    o.Normal = mul(input.Normal, (float3x3)World);
    o.WorldPos = worldPos.xyz;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 PSMain(VSOutput i) : COLOR
{
    // patrón animado
    float sx = sin(i.WorldPos.x * WindScale + Time * WindSpeed);
    float cz = cos(i.WorldPos.z * (WindScale * 0.8) + Time * (WindSpeed * 0.9));
    float pattern = sx * cz;
    pattern = (pattern * 0.5) + 0.5; // -> [0,1]

    float nDotUp = saturate(dot(normalize(i.Normal), float3(0,1,0)));

    // color procedural
    float3 grassBase = lerp(BaseColor, Highlight, saturate(pattern * WindStrength));
    float3 lit = Ambient + grassBase * saturate(0.6 + 0.6 * nDotUp);
    lit += 0.08 * pow(pattern, 2.0) * nDotUp * Highlight;
    lit *= Exposure;
    lit = saturate(lit);

    // sample textura (tiling + scroll)
    float2 uv = i.TexCoord * Tiling + float2(Time * ScrollSpeed, 0);
    float4 tex = tex2D(GrassSampler, uv);

    // mezcla: lerp entre el procedural y la textura
    // si la textura tiene mucha iluminación propia, podés multiplicar en vez de lerp
    float3 final = lerp(lit, tex.rgb, saturate(TextureInfluence));

    // opcional: combinar detalle multiplicativo para dar más contraste
    // final *= lerp(1.0, tex.rgb, 0.25); 

    return float4(saturate(final), 1.0);
}

technique GrassTech
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader  = compile ps_3_0 PSMain();
    }
}