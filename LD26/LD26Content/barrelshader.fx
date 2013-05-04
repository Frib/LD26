Texture2D Texture : register(t0);
SamplerState Linear : register(s0);
float2 LensCenter;
float2 ScreenCenter;
float2 Scale;
float2 ScaleIn;
float4 HmdWarpParam;
float4x4 View : register(c4);
float4x4 Texm : register(c8);

sampler colorSampler = sampler_state
{
    Texture = Texture;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0_centroid;
};

// Scales input texture coordinates for distortion.
float2 HmdWarp(float2 in01)
{
	float2 theta = (in01 - LensCenter) * ScaleIn; // Scales to [-1, 1]
	float rSq = theta.x * theta.x + theta.y * theta.y;
	float2 rvector = theta * (HmdWarpParam.x + HmdWarpParam.y * rSq + HmdWarpParam.z * rSq * rSq + HmdWarpParam.w * rSq * rSq * rSq);
	return LensCenter + Scale * rvector;
}

float4 Stuff(in float4 oPosition : POSITION0, in float2 oTexCoord : TEXCOORD0) : COLOR
{
	//return Texture.Sample(Linear, oTexCoord);

	float2 tc = HmdWarp(oTexCoord);
	if (any(clamp(tc, ScreenCenter-float2(0.25,0.5), ScreenCenter+float2(0.25, 0.5)) - tc))
		return 0;
	return Texture.Sample(Linear, oTexCoord);
}

float2 halfPixel;

VertexShaderOutput Nothing(VertexShaderInput input)
{
	VertexShaderOutput outp;


	   outp.Position = mul(View, input.Position);
	   outp.TexCoord = mul(Texm, float4(input.TexCoord,0,1));

	return outp;
}

technique Barrel
{
    pass Pass1
    {
        // TODO: set goggle renderstates here.

        VertexShader = compile vs_2_0 Nothing();
        PixelShader = compile ps_2_0 Stuff();
    }
}