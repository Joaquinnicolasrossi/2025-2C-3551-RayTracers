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
float4x4 LightViewProj; // View * Projection de la luz

struct VSIn
{
    float4 Position : POSITION0;
};

struct VSOut
{
    float4 Position : SV_POSITION;
    float4 ShadowPos : TEXCOORD0; // pos en espacio de la luz (clip)
};

VSOut VSMain(VSIn input)
{
    VSOut o;
    float4 worldPos = mul(input.Position, World);
    o.ShadowPos = mul(worldPos, LightViewProj);
    // La posicion para rasterizer la calculamos con la misma matriz (necesaria para z)
    o.Position = mul(worldPos, LightViewProj);
    return o;
}

// Escribimos la profundidad en el render target (SurfaceFormat.Single)
float4 PSMain(VSOut input) : COLOR
{
    // Normalized device coords:
    float3 proj = input.ShadowPos.xyz / input.ShadowPos.w;

#if NEED_Z_REMAP
    // en OpenGL NDC z está en [-1,1], lo mapeamos a [0,1]
    proj.z = proj.z * 0.5 + 0.5;
#endif

    float depth = proj.z;
    return float4(depth, depth, depth, 1.0f);
}

technique ShadowGen
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VSMain();
        PixelShader  = compile PS_SHADERMODEL PSMain();
    }
}
