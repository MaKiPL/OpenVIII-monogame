

// ==DIRECTX

    float4x4 World;
    float4x4 View;
    float4x4 Projection;
    float3 camWorld;
    texture ModelTexture;

float bendValue = 1.4;
float3 bendOrigin = { 0, 0, 0 };
float bendDistance = 350.0;
float3 bendVector = { 0, -0.01, 0 };



    //pre-set;
    float Transparency = 1;
float4 fogColor = { 
    0.39215,
    0.58431,
    0.92941,
    0
    };
  
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
    float4 vertPos : TEXCOORD1;
};

  VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
  {
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);

    //curve test
    float4 world = worldPosition;
    float3 bendOrigin = camWorld;
    float dist = length(world.xyz - bendOrigin);
    float newDist = dist - bendDistance;
    dist = max(0.0, newDist);
    dist = pow(dist, bendValue);
    world.xyz += dist * bendVector;


    output.vertPos = worldPosition; //that will be our position in world- however we need to get the camera position too
    worldPosition = world;
    float4 viewPosition = mul(worldPosition, View);
    float4 projectionPosition = mul(viewPosition, Projection);
    output.Position = projectionPosition;
    output.TextureCoordinate = input.TexCoord;
    return output;
}

float4 ApplyFog(float4 textureColor, float3 vertPosition, float3 camPosition)
{
    float4 fogDist = distance(vertPosition, camPosition);
    fogDist /= 800.0; //you can change that 800f to find best value
    fogDist = clamp(fogDist, 0, 1); //this prevents inversion of colours when at high distance;
    textureColor.xyz = lerp(textureColor.xyz, fogColor.xyz, fogDist.xyz); //we lerp the texture color to fogColor (changeable) with fogDistancw
    return textureColor;
}

void ApplyAlphaMasking(float4 textureColor)
{
    clip(textureColor.a - 0.01); //0.01 - I'm not really sure how does it work, why 0.01 magically makes it work perfectly ;-;
}

  float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
  {      
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    float3 vertPosition = input.vertPos;
    float3 camPosition = camWorld;
    textureColor = ApplyFog(textureColor, vertPosition, camPosition);
    ApplyAlphaMasking(textureColor);
    return textureColor;
}

float4 PixelShaderFunction_Water(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    float3 vertPosition = input.vertPos;
    float3 camPosition = camWorld;
    textureColor = ApplyFog(textureColor, vertPosition, camPosition);
    ApplyAlphaMasking(textureColor);
    //TODO water anims- maybe param with UV or something?
    return textureColor;
}



  technique Texture_fog_bend
  {
    pass Pass1
    {
        AlphaBlendEnable = TRUE;
        SrcBlend = SRCALPHA;
        DestBlend = INVSRCALPHA;
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
  }

technique Texture_fog_bend_waterAnim
{
    pass Pass1
    {
        AlphaBlendEnable = TRUE;
        SrcBlend = SRCALPHA;
        DestBlend = INVSRCALPHA;
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction_Water();
    }
}