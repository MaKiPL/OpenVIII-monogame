  float4x4 World;
  float4x4 View;
  float4x4 Projection;
  texture ModelTexture;

float Transparency = 1;
  
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
        float4 Position : SV_POSITION;
        float4 TexCoord : TEXCOORD0;
  };

  struct VertexShaderOutput
  {
        float4 Position : SV_POSITION;
        float2 TextureCoordinate : TEXCOORD0;
  };

  VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
  {
        VertexShaderOutput output;
        float4 worldPosition = mul(input.Position, World);
        float4 viewPosition = mul(worldPosition, View);

        output.Position = mul(viewPosition, Projection);
        output.TextureCoordinate = input.TexCoord;
        return output;
  }

  float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
  {      
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	textureColor.a = Transparency;
    return textureColor;
  }

  technique Ambient
  {

         pass Pass1
        {
        AlphaBlendEnable = TRUE;
              VertexShader = compile vs_4_0 VertexShaderFunction();
              PixelShader = compile ps_4_0 PixelShaderFunction();
        }
  }