#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// --- MATRICES ---
float4x4 World;
float4x4 WorldViewProjection;
float4x4 InverseTransposeWorld;
float4x4 LightViewProjection;

// --- ILUMINACIÓN Y MATERIAL ---
float3 lightPosition;
float3 eyePosition;
float3 ambientColor = float3(0.3, 0.3, 0.3);
float3 diffuseColor = float3(1, 1, 1); // Color base si no hay textura
float3 specularColor = float3(1, 1, 1);
float shininess = 32.0;

// --- TEXTURAS Y SOMBRAS ---
float useTexture = 0.0; // 0 = usar diffuseColor, 1 = usar baseTexture
texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture shadowMap;
sampler2D shadowMapSampler = sampler_state
{
    Texture = <shadowMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float2 shadowMapSize;
static const float modulatedEpsilon = 0.00008;
static const float maxEpsilon = 0.00009;

// --- STRUCTS ---

struct DepthPassVertexShaderInput
{
    float4 Position : POSITION0;
};

struct DepthPassVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ScreenSpacePosition : TEXCOORD1;
};

struct ShadowedVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};

struct ShadowedVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldSpacePosition : TEXCOORD1;
    float4 LightSpacePosition : TEXCOORD2;
    float3 Normal : TEXCOORD3; // Normal en mundo
};

// --- DEPTH PASS (Generación de Sombras) ---

DepthPassVertexShaderOutput DepthVS(in DepthPassVertexShaderInput input)
{
    DepthPassVertexShaderOutput output;
    // WorldViewProjection aquí debe ser (World * LightView * LightProjection)
    output.Position = mul(input.Position, WorldViewProjection);
    output.ScreenSpacePosition = output.Position;
    return output;
}

float4 DepthPS(in DepthPassVertexShaderOutput input) : COLOR
{
    float depth = input.ScreenSpacePosition.z / input.ScreenSpacePosition.w;
    return float4(depth, depth, depth, 1.0);
}

// --- MAIN PASS (Dibujado de Escena) ---

ShadowedVertexShaderOutput MainVS(in ShadowedVertexShaderInput input)
{
    ShadowedVertexShaderOutput output;
    // WorldViewProjection aquí es (World * CameraView * CameraProjection)
    output.Position = mul(input.Position, WorldViewProjection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.WorldSpacePosition = mul(input.Position, World);
    output.LightSpacePosition = mul(output.WorldSpacePosition, LightViewProjection);
    // Transformar normal al espacio mundo correctamente
    output.Normal = normalize(mul(input.Normal, (float3x3)InverseTransposeWorld)); // Transpuesta inversa para normales!
    return output;
}

float4 ShadowedPCFPS(in ShadowedVertexShaderOutput input) : COLOR
{
    // 1. Coordenadas de Sombra
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowTexCoord = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowTexCoord.y = 1.0f - shadowTexCoord.y;

    // 2. Vectores de Iluminación
    float3 N = normalize(input.Normal);
    float3 L = normalize(lightPosition - input.WorldSpacePosition.xyz);
    float3 V = normalize(eyePosition - input.WorldSpacePosition.xyz);
    float3 H = normalize(L + V);

    // 3. Cálculo de Sombra (PCF)
    float shadowVisibility = 0.0;
    float2 texelSize = 1.0 / shadowMapSize;
    float bias = max(modulatedEpsilon * (1.0 - dot(N, L)), maxEpsilon);

    // Check si estamos fuera del mapa de sombras
    if (shadowTexCoord.x < 0 || shadowTexCoord.x > 1 || shadowTexCoord.y < 0 || shadowTexCoord.y > 1)
    {
        shadowVisibility = 1.0; // Fuera del mapa = iluminado
    }
    else
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                float pcfDepth = tex2D(shadowMapSampler, shadowTexCoord + float2(x, y) * texelSize).r;
                // Si la profundidad en el mapa es menor que la nuestra (menos bias), hay obstáculo -> sombra
                if (lightSpacePosition.z - bias < pcfDepth)
                {
                    shadowVisibility += 1.0;
                }
            }
        }
        shadowVisibility /= 9.0;
    }

    // 4. Color Base (Textura o Sólido)
    float4 baseColor;
    if (useTexture > 0.5)
    {
        baseColor = tex2D(textureSampler, input.TextureCoordinates);
    }
    else
    {
        baseColor = float4(1, 1, 1, 1);
    }
    
    // Si el diffuseColor viene de C#, lo multiplicamos. Si no hay textura, diffuseColor da el color.
    // Si hay textura, diffuseColor debería ser blanco (o teñir la textura).
    baseColor.rgb *= diffuseColor;

    // 5. Iluminación Blinn-Phong
    float NdotL = max(dot(N, L), 0.0);
    float3 diffuse = NdotL * float3(1,1,1); // Luz blanca

    float NdotH = max(dot(N, H), 0.0);
    float3 specular = pow(NdotH, shininess) * specularColor * (NdotL > 0.0);

    // 6. Combinación: (Ambiente + (Difusa + Especular) * Sombra) * ColorBase
    float3 lighting = ambientColor + (diffuse + specular) * shadowVisibility;
    float3 finalColor = lighting * baseColor.rgb;

    return float4(finalColor, baseColor.a);
}

// --- TÉCNICAS ---

technique DepthPass
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DepthVS();
        PixelShader = compile PS_SHADERMODEL DepthPS();
    }
};

technique DrawShadowedPCF
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL ShadowedPCFPS();
    }
};