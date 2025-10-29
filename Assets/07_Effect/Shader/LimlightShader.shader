Shader "Unlit/LimlightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _RimColor ("Rim Color", Color) = (1, 1, 1, 1) // 림 라이트 색상
        _RimPower ("Rim Power", Range(0.0, 10.0)) = 3.0 // 림 라이트 강도/두께
        _UseTex ("Use Texture", float) = 1 // 텍스처 사용 여부
        _UseHolo ("Use Holo", float) = 0      // 홀로그램 사용 여부
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {

            // 알파 블렌딩 활성화
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // useTexture properties
            float _UseTex;
            float _UseHolo;

            // limlight properties
            fixed4 _RimColor;
            float _RimPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD2;
                float3 worldViewDir : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                // 월드 공간 위치 계산
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 월드 공간 노멀 계산
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // 월드 공간 뷰 디렉션 계산
                o.worldViewDir = UnityWorldSpaceViewDir(worldPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // ------------------ 림 라이트 로직 (개선) ------------------
                
                // 1. 노멀 벡터와 뷰 디렉션을 정규화합니다.
                // Surface Shader가 아니므로 직접 정규화해야 합니다.
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(i.worldViewDir);
                
                // 2. 림 팩터 계산: 1.0 - (N dot V)
                // 가장자리(직각)일 때 1, 정면일 때 0에 가까움
                float NdotV = 1.0 - saturate(dot(N, V));
                
                // 3. Pow 함수를 사용하여 림의 두께(강도)를 조절합니다.
                float rim = pow(NdotV, _RimPower);
                
                // 4. 림 라이트의 색상 계산: 림 계수 * 림 컬러
                fixed3 rimLight = rim * _RimColor.rgb;
                
                // 5. 최종 색상 혼합: 기본 텍스처 색상 + 림 라이트 색상
                // 텍스처 색상에 림 라이트를 더합니다.
                fixed3 finalColor = col.rgb + rimLight;

                float alpha = col.a;
                // 최종 색상에 림 라이트 적용
                if (_UseTex == 0)
                {
                    alpha = rim;
                }

                if (_UseHolo == 1)
                {
                    alpha = alpha * abs(sin(_Time.y * 3.0));
                }

                fixed4 finalCol = fixed4(finalColor, alpha);
                // ------------------ 림 라이트 로직 끝 ------------------

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalCol);
                return finalCol;
            }
            ENDCG
        }
    }
}
