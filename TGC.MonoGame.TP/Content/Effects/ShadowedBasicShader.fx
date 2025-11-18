#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
	#define NEED_Z_REMAP 1
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
	#define NEED_Z_REMAP 0
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 LightViewProj; // view*proj de la luz

float3 DiffuseColor;
float UseTexture = 0;
texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state { Texture = <MainTexture>; };

texture2D ShadowMap;
sampler2D ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float3 LightDirection     = float3(1, -1, 1);
float3 AmbientColor       = float3(0.2, 0.2, 0.2);
float3 SpecularColor      = float3(1, 1, 1);
float  Shininess          = 32.0;
float3 CameraPosition     = float3(0, 10, 0);

float ShadowBias = 0.002;    // ajustar si hay acne o peter-panning
float2 ShadowMapTexelSize = float2(1.0/1024.0, 1.0/1024.0); // setear desde C#

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float3 Normal   : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float4 ShadowPos: TEXCOORD3;
};

VSOutput VSMain(VSInput input)
{
    VSOutput o;
    float4 worldPos = mul(input.Position, World);
    o.Position = mul(mul(worldPos, View), Projection);
    o.Normal = mul(input.Normal, (float3x3)World);
    o.WorldPos = worldPos.xyz;
    o.TexCoord = input.TexCoord;
    o.ShadowPos = mul(worldPos, LightViewProj);
    return o;
}

// PCF 3x3 simple
float SampleShadow(float4 shadowPos)
{
    float3 proj = shadowPos.xyz / shadowPos.w;

#if NEED_Z_REMAP
    proj.z = proj.z * 0.5 + 0.5;
#endif

    // fuera del atlas -> no sombra
    if (proj.x < 0.0 || proj.x > 1.0 || proj.y < 0.0 || proj.y > 1.0)
        return 1.0;

    float visibility = 0.0;
    // 3x3 kernel
    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            float2 offset = float2(dx, dy) * ShadowMapTexelSize;
            float depth = tex2D(ShadowMapSampler, proj.xy + offset).r;
            float current = proj.z - ShadowBias;
            if (current <= depth) visibility += 1.0;
        }
    }
    return visibility / 9.0;
}

float4 PSMain(VSOutput input) : COLOR
{
    float3 baseColor = DiffuseColor;
    if (UseTexture > 0.5)
        baseColor = tex2D(MainTextureSampler, input.TexCoord).rgb;

    float3 N = normalize(input.Normal);
    float3 L = normalize(-LightDirection);
    float NdotL = saturate(dot(N, L));
    float3 diffuse = baseColor * NdotL;

    float3 V = normalize(CameraPosition - input.WorldPos);
    float3 H = normalize(V + L);
    float NdotH = saturate(dot(N, H));
    float3 specular = SpecularColor * pow(NdotH, Shininess) * step(0.0, NdotL);

    // shadow test
    float shadow = SampleShadow(input.ShadowPos);

    float3 color = AmbientColor * baseColor + (diffuse + specular) * shadow;
    return float4(saturate(color), 1.0);
}

technique ShadowedTech
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VSMain();
        PixelShader  = compile PS_SHADERMODEL PSMain();
    }
}
