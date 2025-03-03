Shader "Custom/HexagonGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridColor ("Grid Color", Color) = (0.0, 0.7, 1.0, 1.0) // Light blue default
        _BorderThickness ("Border Thickness", Range(0.0, 0.5)) = 0.05
        _GridScale ("Grid Scale", Range(0.1, 10.0)) = 1.0
        _FlowDirectionX ("Flow X Direction", Range(-1.0, 1.0)) = 0.0
        _FlowDirectionY ("Flow Y Direction", Range(-1.0, 1.0)) = 0.0
        _FlowSpeed ("Flow Speed", Range(0.0, 2.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GridColor;
            float _BorderThickness;
            float _GridScale;
            float _FlowDirectionX;
            float _FlowDirectionY;
            float _FlowSpeed;
            
            // Helper function to calculate distance to nearest hexagon edge
            float hexDist(float2 p) {
                // Convert to pointy-top hexagon coordinates
                p = abs(p);
                
                // Compute distance to edge for regular hexagon
                float c = dot(p, normalize(float2(1, 1.732)));
                c = max(c, p.x);
                return c;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            // Convert from axial to pixel coordinates
            float2 axialToPixel(float2 hex) {
                float x = hex.x * 1.5;
                float y = hex.x * 0.866 + hex.y * 1.732;
                return float2(x, y);
            }
            
            // Find the nearest hexagon center in axial coordinates
            float2 nearestHexCenter(float2 p) {
                // Convert from pixel to axial coordinates (approximately)
                float q = p.x * 2.0/3.0;
                float r = (-p.x + sqrt(3) * p.y) / 3.0;
                
                // Round to nearest hexagon
                float x = round(q);
                float z = round(r);
                float y = round(-x-z);
                
                // If we broke the constraint x+y+z=0, fix it
                float dx = abs(x - q);
                float dy = abs(y - (-q-r));
                float dz = abs(z - r);
                
                if (dx > dy && dx > dz) {
                    x = -y-z;
                } else if (dy > dz) {
                    y = -x-z;
                } else {
                    z = -x-y;
                }
                
                return float2(x, z);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Apply flow animation by offsetting UV
                float2 flowOffset = float2(_FlowDirectionX, _FlowDirectionY) * _FlowSpeed * _Time.y;
                float2 uv = i.uv + flowOffset;
                
                // Scale the grid
                uv *= _GridScale;
                
                // Find nearest hexagon center
                float2 nearest = nearestHexCenter(uv);
                
                // Convert center back to pixel coordinates
                float2 centerPixel = axialToPixel(nearest);
                
                // Vector from center to current point
                float2 toCenter = uv - centerPixel;
                
                // Calculate distance to hexagon edge (normalized)
                float d = hexDist(toCenter) / 0.866; // Normalize to [0,1] range inside a hex
                
                // Create the border effect - only show points near the edge
                float borderMask = smoothstep(1.0 - _BorderThickness - 0.01, 1.0 - _BorderThickness + 0.01, d);
                float innerMask = smoothstep(d, d + 0.01, 1.0);
                float hexMask = borderMask * innerMask;
                
                // Apply the color
                fixed4 col = _GridColor * hexMask;
                
                return col;
            }
            ENDCG
        }
    }
}