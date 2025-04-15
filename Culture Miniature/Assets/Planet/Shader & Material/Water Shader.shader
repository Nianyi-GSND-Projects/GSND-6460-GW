Shader "Culture Miniature/Planet Water"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.2, 0.5, 0.8, 0.5)
        _MainTex ("Main Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        sampler2D _MainTex;
        sampler2D _NormalMap;

        fixed4 _Color;
        float _WaveSpeed;
        float _Transparency;
        float _FresnelPower;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float3 viewDir;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float wave = sin(_Time.y * _WaveSpeed + IN.worldPos.x * 4.0 + IN.worldPos.z * 4.0) * 0.05;
            float2 disturbedUV = IN.uv_MainTex + float2(wave, wave);

            fixed4 tex = tex2D(_MainTex, disturbedUV);

            fixed3 normalTex = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap + float2(_Time.y * 0.1, _Time.y * 0.1)));
            o.Normal = normalTex;

            // edge
            float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), _FresnelPower);

            o.Albedo = tex.rgb * _Color.rgb + fresnel * 0.3;

            o.Alpha = _Transparency;
            o.Smoothness = 0.9;
            o.Metallic = 0.1;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
