// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:34120,y:32455,varname:node_9361,prsc:2|custl-861-OUT;n:type:ShaderForge.SFN_Fresnel,id:7233,x:32473,y:32978,varname:node_7233,prsc:2|EXP-5958-OUT;n:type:ShaderForge.SFN_Add,id:1001,x:33721,y:32745,varname:node_1001,prsc:2|A-6035-OUT,B-7774-OUT;n:type:ShaderForge.SFN_Tex2d,id:6271,x:32382,y:32690,ptovrint:False,ptlb:tex02,ptin:_tex02,varname:node_6271,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:133d88b5cfea3b64088499df3843c31e,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:5958,x:32135,y:32982,ptovrint:False,ptlb:fresnel_daxiao,ptin:_fresnel_daxiao,varname:node_5958,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3.478632,max:10;n:type:ShaderForge.SFN_Multiply,id:4387,x:32607,y:33093,varname:node_4387,prsc:2|A-7233-OUT,B-9770-OUT;n:type:ShaderForge.SFN_Slider,id:9770,x:32179,y:33163,ptovrint:False,ptlb:fresnel_qiangdu,ptin:_fresnel_qiangdu,varname:_fresnel_daxiao_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.9401736,max:10;n:type:ShaderForge.SFN_Color,id:2161,x:32607,y:33281,ptovrint:False,ptlb:fresnel_color,ptin:_fresnel_color,varname:node_2161,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:7774,x:32790,y:33159,varname:node_7774,prsc:2|A-4387-OUT,B-2161-RGB;n:type:ShaderForge.SFN_Slider,id:5489,x:32263,y:33110,ptovrint:False,ptlb:fresnel_daxiao_copy,ptin:_fresnel_daxiao_copy,varname:_fresnel_daxiao_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3.478632,max:10;n:type:ShaderForge.SFN_Tex2d,id:5436,x:32383,y:32428,ptovrint:False,ptlb:tex01,ptin:_tex01,varname:node_5436,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:133d88b5cfea3b64088499df3843c31e,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Add,id:827,x:32591,y:32556,varname:node_827,prsc:2|A-5436-RGB,B-6271-RGB;n:type:ShaderForge.SFN_Multiply,id:6035,x:33510,y:32537,varname:node_6035,prsc:2|A-7332-OUT,B-5221-OUT;n:type:ShaderForge.SFN_Slider,id:7332,x:33035,y:32487,ptovrint:False,ptlb:ditu_liangdu,ptin:_ditu_liangdu,varname:node_7332,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6068376,max:1;n:type:ShaderForge.SFN_Power,id:2150,x:33289,y:32230,varname:node_2150,prsc:2|VAL-5078-OUT,EXP-7865-OUT;n:type:ShaderForge.SFN_Multiply,id:4163,x:33456,y:32337,varname:node_4163,prsc:2|A-2150-OUT,B-3708-OUT;n:type:ShaderForge.SFN_Slider,id:3708,x:33013,y:32377,ptovrint:False,ptlb:xingxing_liangdu,ptin:_xingxing_liangdu,varname:node_3708,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:10;n:type:ShaderForge.SFN_Vector1,id:7865,x:32985,y:32239,varname:node_7865,prsc:2,v1:5;n:type:ShaderForge.SFN_Add,id:861,x:33866,y:32564,varname:node_861,prsc:2|A-8530-OUT,B-1001-OUT;n:type:ShaderForge.SFN_Multiply,id:5221,x:33291,y:32620,varname:node_5221,prsc:2|A-5078-OUT,B-8755-RGB;n:type:ShaderForge.SFN_Color,id:8755,x:33024,y:32789,ptovrint:False,ptlb:ditu_yanse,ptin:_ditu_yanse,varname:node_8755,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Desaturate,id:5078,x:32972,y:32584,varname:node_5078,prsc:2|COL-827-OUT,DES-6042-OUT;n:type:ShaderForge.SFN_Desaturate,id:8530,x:33646,y:32367,varname:node_8530,prsc:2|COL-4163-OUT;n:type:ShaderForge.SFN_Slider,id:6042,x:32602,y:32757,ptovrint:False,ptlb:qushe_bili,ptin:_qushe_bili,varname:node_6042,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;proporder:6271-5958-9770-2161-5436-7332-3708-8755-6042;pass:END;sub:END;*/

Shader "Shader Forge/shoubi" {
    Properties {
        _tex02 ("tex02", 2D) = "white" {}
        _fresnel_daxiao ("fresnel_daxiao", Range(0, 10)) = 3.478632
        _fresnel_qiangdu ("fresnel_qiangdu", Range(0, 10)) = 0.9401736
        _fresnel_color ("fresnel_color", Color) = (1,1,1,1)
        _tex01 ("tex01", 2D) = "white" {}
        _ditu_liangdu ("ditu_liangdu", Range(0, 1)) = 0.6068376
        _xingxing_liangdu ("xingxing_liangdu", Range(0, 10)) = 0
        _ditu_yanse ("ditu_yanse", Color) = (1,1,1,1)
        _qushe_bili ("qushe_bili", Range(0, 1)) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _tex02; uniform float4 _tex02_ST;
            uniform float _fresnel_daxiao;
            uniform float _fresnel_qiangdu;
            uniform float4 _fresnel_color;
            uniform sampler2D _tex01; uniform float4 _tex01_ST;
            uniform float _ditu_liangdu;
            uniform float _xingxing_liangdu;
            uniform float4 _ditu_yanse;
            uniform float _qushe_bili;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
                float4 _tex01_var = tex2D(_tex01,TRANSFORM_TEX(i.uv0, _tex01));
                float4 _tex02_var = tex2D(_tex02,TRANSFORM_TEX(i.uv0, _tex02));
                float3 node_5078 = lerp((_tex01_var.rgb+_tex02_var.rgb),dot((_tex01_var.rgb+_tex02_var.rgb),float3(0.3,0.59,0.11)),_qushe_bili);
                float3 finalColor = (dot((pow(node_5078,5.0)*_xingxing_liangdu),float3(0.3,0.59,0.11))+((_ditu_liangdu*(node_5078*_ditu_yanse.rgb))+((pow(1.0-max(0,dot(normalDirection, viewDirection)),_fresnel_daxiao)*_fresnel_qiangdu)*_fresnel_color.rgb)));
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
