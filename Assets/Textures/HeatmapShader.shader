// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "HeatmapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HeatData("Texture", 3D) = "" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldDirection : TEXCOORD3;
            };

            //float4x4 _ViewProjectInverse;

            float4x4 clipToWorld;

            float startX;
            float startY;
            float startZ;

            float stepX;
            float stepY;
            float stepZ;

            int sizeX;
            int sizeY;
            int sizeZ;

            int rangeX;
            int rangeY;
            int rangeZ;

            float affectingRadius;
            float smoothness;

            float _MinVal;
            float _MaxVal;

            float enableHeatmap;

            v2f vert (appdata v)
            {
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float4 clip = float4(o.vertex.xy, 0.0, 1.0);
                o.worldDirection = mul(clipToWorld, clip) - _WorldSpaceCameraPos;

                return o;
            }

            sampler2D _MainTex;
            sampler3D _HeatData;
            //sampler2D _HeatData;
            sampler2D_float _CameraDepthTexture;
            float4 _CameraDepthTexture_ST;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                //col.rgb = 1 - col.rgb;

                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
                depth = LinearEyeDepth(depth);
                float3 worldspace = i.worldDirection * depth + _WorldSpaceCameraPos;

                // range affected 
                //int rangeX = int(ceil(affectingRadius / stepX)) * 2;
                //int rangeY = int(ceil(affectingRadius / stepY)) * 2;
                //int rangeZ = int(ceil(affectingRadius / stepZ)) * 2;

                // starting xyz in index
                int X0 = int(floor((worldspace.x - startX) / stepX));
                int Y0 = int(floor((worldspace.y - startY) / stepY));
                int Z0 = int(floor((worldspace.z - startZ) / stepZ));

                // Loops over all the points
                half h = 0;
                half scale = _MaxVal - _MinVal;
                int size = sizeX * sizeY * sizeZ;
                half count = 0;

                [loop]
                for (int i = X0 - rangeX; i < X0 + rangeX; i++)
                {
                  [loop]
                  for (int j = Y0 - rangeY; j < Y0 + rangeY; j++)
                  {
                    [loop]
                    for (int k = Z0 - rangeZ; k < Z0 + rangeZ; k++)
                    {
                      // look up in the texture
                      //int index = k * sizeY * sizeX + j * sizeX + i;
                      //int3 ind = int3(index, 1, 0);
                      float3 uvw = float3((i + 0.5) / sizeX, (j+0.5)/sizeY, (k+0.5)/sizeZ);
                      float4 color = tex3D(_HeatData, uvw);//_HeatData.Load(ind);
                      float property = color.r;
                      //property = 1;

                      float3 gridPos = float3(float(i) * stepX + startX,
                        float(j) * stepY + startY,
                          float(k) * stepZ + startZ);

                      half dist = distance(worldspace, gridPos);
                      half hi = 1.0 - saturate(dist * dist * dist / affectingRadius);
                      h += hi * (property) / smoothness; //
                      //h += property;

                      /*float4 index = int3(i, j, k, 0);
                      float4 uvw = tex3Dfetch()
                      tex3D*/
                      count = 1;
                    }
                  }
                }

                h /= scale;

                half4 colorLow = half4(41 / 255, 199 / 255, 46 / 255, 0);
                half4 colorMid = half4(1, 1, 0, 0.09);
                half4 colorHigh = half4(1, 0, 0, 0.5);

                // Converts (0-1) according to the heat texture
                h = saturate(h);

                // 0 - 0.5: low - mid 

                half4 color;
                if (h <= 0.2) {
                  color = lerp(colorLow, colorMid, h / 0.2);
                }
                else {
                  color = lerp(colorMid, colorHigh, (h - 0.2) / 0.8);
                }

                // Calculates the contribution of each point
                //half di = distance(worldspace, );

                //half hi = 1 - saturate(di / affectingRadius);

                //h += hi * (_Properties[i].w - _MinVal) / scale;
                //h += hi * (_Properties[i].w) / smoothness;

                //(worldspace.x - startX-5)/(stepX*sizeX)
                //if (enableHeatmap > 0) {
                  col += color;
                //}
                //col.rgb += float3(0, worldspace.g, worldspace.b);
                return col;
            }
            ENDCG
        }
    }
}
