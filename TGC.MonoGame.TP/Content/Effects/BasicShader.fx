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

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;

float Time = 0;

// Lighting parameters
float3 LightDirection     = float3(1, -1, 1);  // dirección principal de la luz
float3 AmbientColor       = float3(0.2, 0.2, 0.2); // luz ambiente
float3 SpecularColor      = float3(1, 1, 1);   // color del brillo especular
float  Shininess          = 32.0;             // potencia del brillo
float3 CameraPosition     = float3(0, 10, 0); // posición de la cámara (alterada desde C#)

texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state { Texture = <MainTexture>; };

// 0 = color , 1 = texture
float UseTexture = 0;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal   : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPos = mul(input.Position, World);
    output.Position = mul(mul(worldPos, View), Projection);

    // Transform normal to world space
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    output.WorldPos = worldPos.xyz;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Base color
    float3 baseColor;
    if (UseTexture > 0.5)
    {
        baseColor = tex2D(MainTextureSampler, input.TexCoord).rgb;
    }
    else
    {
        baseColor = DiffuseColor;
    }

    // Normal y luz
    float3 N = normalize(input.Normal);
    float3 L = normalize(-LightDirection); // dirección desde el fragmento hacia la luz

    // Componente difusa
    float NdotL = saturate(dot(N, L));
    float3 diffuse = baseColor * NdotL;

    // Componente especular (Blinn-Phong)
    float3 V = normalize(CameraPosition - input.WorldPos);
    float3 H = normalize(V + L);
    float NdotH = saturate(dot(N, H));
    float3 specular = SpecularColor * pow(NdotH, Shininess) * step(0.0, NdotL);

    // Final color
    float3 color = AmbientColor * baseColor + diffuse + specular;
    return float4(color, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
