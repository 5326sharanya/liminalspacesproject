// Skybox shader for a PANORAMA PHOTO (not a full 360 sphere image).
//
// A normal panorama photo only covers a slice of the sky, so feeding it to Unity's
// "Skybox/Panoramic" shader stretches it over the whole sphere and the mountains end up
// towering walls. This shader instead keeps the photo at its real size in a band around
// the horizon, paints sky above it and ground below it, and softens the joins.
Shader "LiminalSpace/Skybox Panorama Band"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Panorama Photo", 2D) = "white" {}
        _Exposure ("Exposure", Range(0, 4)) = 1
        _Rotation ("Rotation (degrees)", Range(0, 360)) = 0
        _VerticalFov ("Photo Vertical FOV (degrees)", Range(10, 120)) = 55
        _EdgeFade ("Edge Softness (degrees)", Range(0.1, 15)) = 3
        [Toggle] _MirrorTiling ("Mirror Tiling (hides the seam)", Float) = 1
        _SkyHorizon ("Sky At Horizon", Color) = (0.74, 0.84, 0.93, 1)
        _SkyZenith ("Sky At Zenith", Color) = (0.16, 0.38, 0.72, 1)
        _GroundColor ("Ground", Color) = (0.21, 0.20, 0.18, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _Exposure;
                float _Rotation;
                float _VerticalFov;
                float _EdgeFade;
                float _MirrorTiling;
                float4 _SkyHorizon;
                float4 _SkyZenith;
                float4 _GroundColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            Varyings Vert (Attributes input)
            {
                Varyings output;
                float4 clip = TransformObjectToHClip(input.positionOS.xyz);
                output.positionCS = clip.xyww; // xyww = "push this to the far plane"
                output.direction = input.positionOS.xyz;
                return output;
            }

            float3 Frag (Varyings input) : SV_Target
            {
                float3 dir = normalize(input.direction);

                // Spin the whole panorama around the vertical axis.
                float angle = radians(_Rotation);
                float spin = sin(angle);
                float spinCos = cos(angle);
                dir = float3(dir.x * spinCos - dir.z * spin, dir.y, dir.x * spin + dir.z * spin);

                // Horizontal position around the horizon, 0..1.
                float u = saturate(atan2(dir.x, dir.z) / (2.0 * PI) + 0.5);

                // A photo is not 360 degrees wide, so tiling it once leaves a visible seam
                // where the two ends meet. Bouncing it back and forth removes the seam and
                // is invisible for organic shapes like mountains.
                if (_MirrorTiling > 0.5)
                {
                    u = 1.0 - abs(frac(u * 2.0) - 1.0);
                }

                // Vertical: how far above/below the horizon this pixel is, in degrees.
                float pitch = degrees(asin(clamp(dir.y, -1.0, 1.0)));
                float halfBand = _VerticalFov * 0.5;
                float v = (pitch + halfBand) / _VerticalFov; // 0 = photo bottom, 1 = photo top

                float3 photo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(u, saturate(v))).rgb * _Exposure;

                // Fade the photo out as it reaches the top and bottom of the band so the
                // cut into the plain sky and ground does not look like a hard edge.
                float fadeWidth = _EdgeFade / _VerticalFov;
                float fade = smoothstep(0.0, fadeWidth, v) * smoothstep(0.0, fadeWidth, 1.0 - v);

                // Plain sky above the photo, plain ground below it.
                float3 sky = lerp(_SkyHorizon.rgb, _SkyZenith.rgb, saturate(pitch / 70.0));
                float3 fill = pitch >= 0.0 ? sky : _GroundColor.rgb;

                return lerp(fill, photo, saturate(fade));
            }
            ENDHLSL
        }
    }

    Fallback Off
}
