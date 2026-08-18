Shader "WorldLocator/Unlit"
{
    // 単一バリアントの自作Unlitシェーダー。keyword/multi_compileを一切持たないため
    // ビルド時のバリアントストリッピングで消える要素が無く、il2cpp/iOS実機でも
    // Shader.Find依存の組み込みシェーダー("Unlit/Color")のように未検出→マゼンタ化しない。
    // プロジェクト内アセット(この.shader)を .mat が参照し、その .mat をシーンが参照するため
    // ビルドへ確実に含まれる。Cull Off で裏面も描画し、視線角度による「消える」誤解も排除する。
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        Lighting Off
        ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
