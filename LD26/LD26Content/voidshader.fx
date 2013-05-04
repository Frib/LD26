float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
Texture2D Texture;

sampler TextureSampler = sampler_state { texture = <Texture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

float fogStart;
float fogEnd;

uniform const float3	DiffuseColor	: register(c5) = 1;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;

uniform const float3	AmbientLightColor		: register(c10);

uniform const float3	DirLight0Direction		: register(c11);
uniform const float3	DirLight0DiffuseColor	: register(c12);
uniform const float3	DirLight0SpecularColor	: register(c13);

uniform const float3	DirLight1Direction		: register(c14);
uniform const float3	DirLight1DiffuseColor	: register(c15);
uniform const float3	DirLight1SpecularColor	: register(c16);

struct ColorPair
{
    float3 Diffuse;
    float3 Specular;
};

struct CommonVSOutput
{
    float4	Pos_ws;
    float4	Pos_ps;
    float4	Diffuse;
    float3	Specular;
};


float ComputeFogFactor(float d)
{
    return clamp((d - fogStart) / (fogEnd - fogStart), 0, 1);
}

ColorPair ComputeLights(float3 E, float3 N)
{
    ColorPair result;
    
    result.Diffuse = AmbientLightColor;
    result.Specular = 0;

    // Directional Light 0
    float3 L = -DirLight0Direction;
    float3 H = normalize(E + L);
    float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
    result.Diffuse += DirLight0DiffuseColor * ret.x;
    result.Specular += DirLight0SpecularColor * ret.y;
    
    // Directional Light 1
    L = -DirLight1Direction;
    H = normalize(E + L);
    ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
    result.Diffuse += DirLight1DiffuseColor * ret.x;
    result.Specular += DirLight1SpecularColor * ret.y;
        
    result.Diffuse *= DiffuseColor;
    result.Diffuse	+= EmissiveColor;
    result.Specular	*= SpecularColor;
        
    return result;
}
//-----------------------------------------------------------------------------
// Compute per-pixel lighting.
// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
// than doing this directly ourselves, so we don't use the intrinsic.
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputePerPixelLights(float3 E, float3 N)
{
    ColorPair result;
    
    result.Diffuse = AmbientLightColor;
    result.Specular = 0;
    
    // Light0
    float3 L = DirLight0Direction;
    float3 H = normalize(E + L);
    float dt = max(0,dot(L,N));
    result.Diffuse += DirLight0DiffuseColor * dt;
    if (dt != 0)
        result.Specular += DirLight0SpecularColor * pow(max(0.00001f ,dot(H,N)), SpecularPower);

    // Light1
    L = -DirLight1Direction;
    H = normalize(E + L);
    dt = max(0,dot(L,N));
    result.Diffuse += DirLight1DiffuseColor * dt;
    if (dt != 0)
        result.Specular += DirLight1SpecularColor * pow(max(0.00001f ,dot(H,N)), SpecularPower);
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
        
    return result;
}
CommonVSOutput ComputeCommonVSOutputWithLighting(float4 position, float3 normal)
{
    CommonVSOutput vout;
    
    float4 pos_ws = mul(position, World);
    float4 pos_vs = mul(pos_ws, View);
    float4 pos_ps = mul(pos_vs, Projection);
    vout.Pos_ws = pos_ws;
    vout.Pos_ps = pos_ps;
    
    float3 N = normalize(mul(normal, World));
    float3 posToEye = pos_ws - CameraPosition;
    float3 E = normalize(posToEye);
    ColorPair lightResult = ComputeLights(E, N);
    
    vout.Diffuse	= float4(lightResult.Diffuse.rgb, Alpha);
    vout.Specular	= lightResult.Specular;
    
    return vout;
}


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR;
};

struct VertexInputPositionNormalTexture
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexOutputGoggles
{
    float4	PositionPS	: POSITION;		// Position in projection space
    float2	TexCoord	: TEXCOORD0;
    float4	PositionWS	: TEXCOORD1;
    float3	NormalWS	: TEXCOORD2;
    float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

struct WireframeWithFogOutput
{
    float4 Pos : POSITION0;
    float4 Specular : COLOR0;
};

struct PixelWireframeWithFogOutput
{
    float4 Color : COLOR;
};

VertexOutputGoggles VertexGoggle(VertexInputPositionNormalTexture vin)
{
    VertexOutputGoggles vout;
    
    float4 pos_ws = mul(vin.Position, World);
    float4 pos_vs = mul(pos_ws, View);
    float4 pos_ps = mul(pos_vs, Projection);
    
    vout.PositionPS		= pos_ps;
    vout.PositionWS.xyz	= pos_ws.xyz;
    vout.PositionWS.w	= ComputeFogFactor(length(CameraPosition - pos_ws));
    vout.NormalWS		= normalize(mul(vin.Normal, World));
    vout.Diffuse		= float4(1, 1, 1, Alpha);
    vout.TexCoord		= vin.TexCoords;

    return vout;
}

struct PixelLightingPSInputTx
{
    float2	TexCoord	: TEXCOORD0;
    float4	PositionWS	: TEXCOORD1;
    float3	NormalWS	: TEXCOORD2;
    float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

float4 PixelGoggle(PixelLightingPSInputTx pin) : COLOR0
{
    float3 posToEye = CameraPosition - pin.PositionWS.xyz;
    
    float3 N = normalize(pin.NormalWS);
    float3 E = normalize(posToEye);
    
    ColorPair lightResult = ComputePerPixelLights(E, N);
    
    float4 diffuse = tex2D(TextureSampler, pin.TexCoord) * float4(lightResult.Diffuse * pin.Diffuse.rgb, pin.Diffuse.a);
    float4 color = diffuse + float4(lightResult.Specular, 0);
    //color.rgb = lerp(color.rgb, 0, pin.PositionWS.w);
    
    return color;


    //float4 diffuse = tex2D(TextureSampler, input.TexCoords) * float4(input.Diffuse.rgb, 1);	
    //return diffuse;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

WireframeWithFogOutput VertexWireframeWithFog(VertexShaderInput input)
{
    WireframeWithFogOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Pos = mul(viewPosition, Projection);
    output.Specular = float4(0, 0, 0, 1 - ComputeFogFactor(length(CameraPosition - worldPosition)));
    return output;
}

PixelWireframeWithFogOutput PixelWireframeWithFog(WireframeWithFogOutput In)
{
    PixelWireframeWithFogOutput result;

    result.Color = In.Specular.w;
    return result;
}

technique Wireframe
{
    pass Pass1
    {
        // TODO: set wireframe renderstates here.

        VertexShader = compile vs_2_0 VertexWireframeWithFog();
       // PixelShader = compile ps_2_0 PixelShaderFunction();
        PixelShader = compile ps_2_0 PixelWireframeWithFog();
    }
}

technique Goggles
{
    pass Pass1
    {
        // TODO: set goggle renderstates here.

        VertexShader = compile vs_2_0 VertexGoggle();
        PixelShader = compile ps_2_0 PixelGoggle();
    }
}

technique VertexColor
{
    pass Pass1
    {
        // TODO: set vertex color (portal, editor etc) renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
