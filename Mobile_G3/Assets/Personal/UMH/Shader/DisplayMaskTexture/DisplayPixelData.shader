Shader "Unlit/DisplayPixelData"
{
    Properties
    {
        _PixelValues ("Pixel Values", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelValues[256];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the index of the current pixel in the _PixelValues array
                float2 pixelIndex = i.uv * _MainTex_ST.xy;
                int index = (int)(pixelIndex.x + pixelIndex.y * _MainTex_ST.x);
             
                // Get the pixel color from the _PixelValues array
                //float pixelValue = _PixelValues[index];
                float pixelValue = _PixelValues[75];
             
                // Apply the pixel color to the output
                return fixed4(pixelValue, 0.0 , 0.0, 1.0);
            }
            ENDCG
        }
    }
}
