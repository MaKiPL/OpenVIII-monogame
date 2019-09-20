#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

  float4x4 World;
  float4x4 View;
  float4x4 Projection;
  texture ModelTexture;
  
  sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Point; //Controls sampling. None, Linear, or Point.
    MagFilter = Point; //Controls sampling. None, Linear, or Point.
    MipFilter = Point; //Controls how the mips are generated. None, Linear, or Point.
    AddressU = Wrap;
    AddressV = Wrap;
};

  struct VertexShaderInput
  {
        float4 TexCoord : TEXCOORD0;
        float4 Position : POSITION0;
        float4 Normal : NORMAL;
        float4 Color :COLOR0;
  };

  struct VertexShaderOutput
  {
        float4 Position : POSITION0;
        float4 Color : COLOR0;
        float2 TextureCoordinate : TEXCOORD0;
  };

  VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
  {
        VertexShaderOutput output;
        float4 worldPosition = mul(input.Position, World);
        float4 viewPosition = mul(worldPosition, View);

        output.Position = mul(viewPosition, Projection);
        output.Color = input.Color;
        output.TextureCoordinate = input.TexCoord;
        return output;
  }

  float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
  {      
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	textureColor.a = 1;
    return textureColor;
  }

  technique Ambient
  {

         pass Pass1
        {

              VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
              PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
        }
  }