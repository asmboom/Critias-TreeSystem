Shader "Critias/Nature/SpeedTree Billboard Master" 
{
	Properties {		
		_MainTex("Main Texture", 2D) = "black" {}
		_BumpMap("Normal Map", 2D) = "black" {}
		_Cutoff("Cutoff", Range(0, 1)) = 0.33		
		master_LODFade("LOD Fade", Vector) = (0, 1, 0, 0)
		_Size("Billboard Sizes", Vector) = (0, 0, 0, 1)
	}
	SubShader {
		Tags 
		{ 
			"IgnoreProjector" = "True"
			"Queue" = "Geometry"
			"RenderType" = "TransparentCutout" 

			"DisableBatching" = "True"
		}
		LOD 200

		Cull Off
		
		CGPROGRAM

		// #pragma surface surf Standard vertex:TreeVertex fullforwardshadows
		#pragma surface surf Lambert vertex:TreeVertex nolightmap addshadow

		#pragma target 3.0

		struct appdata_t
		{
			float4 vertex		: POSITION;

			float2 texcoord		: TEXCOORD0;
			float2 texcoord1	: TEXCOORD1;
			float2 texcoord2	: TEXCOORD2;

			float4 tangent		: TANGENT;
			float3 normal		: NORMAL;
		};

		struct Input 
		{			
			float2 computedUv;

			float3 screenPos;
		};

		// Global tree system distance
		// float _TreeSystemDistance;

		float4 _UVVert_U[8];
		float4 _UVVert_V[8];

		float4 _UVHorz_U;
		float4 _UVHorz_V;

		float3 _Size;

		float4 _InstanceScaleRotation;
		
		void TreeVertex(inout appdata_t IN, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);

			// Get corners
			float x = IN.vertex.x;
			float y = IN.vertex.y;

			int idx;
			if (x < 0 && y < 0) idx = 0;
			else if (x < 0 && y > 0) idx = 3;
			else if (x > 0 && y < 0) idx = 1;
			else idx = 2;

			float3 campos = _WorldSpaceCameraPos;
			float3 pos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);

			int angleIdx, angleIdxNext;

			float3 v2 = float3(0, 0, 1);
			float3 v1 = campos - pos;

			v1 = normalize(v1);

			float dotA, detA;

			dotA = v1.x * v2.x + v1.z * v2.z;
			detA = v1.x * v2.z - v1.z * v2.x;

			// TODO: add tree instance's rotation too
			// Map to 0 - 360
			// float angle = (atan2(det, dot)) + 180.0f;
			float angle = (atan2(detA, dotA)) + UNITY_PI;

			v2 = float3(0, 1, 0);
			float F = dot(v1.yz, v2.yz);

			// TODO: see if we should have here an 'F' value snap or something so that we 
			// don't tranzition at once into a vertical billboard
			if (F > .9)
			{
				// Make it a horizontal billboard						
				IN.vertex.xzy = IN.vertex.xyz;				
				OUT.computedUv = float2(_UVHorz_U[idx], _UVHorz_V[idx]);
			}
			else
			{				
				v2 = v1;
				v1 = float3(-1, 0, 0);

				dotA = v1.x * v2.x + v1.z * v2.z;
				detA = v1.x * v2.z - v1.z * v2.x;

				float angle360 = (atan2(detA, dotA) + 3.141592632) + _InstanceScaleRotation.w;
				if (angle360 < 0) angle360 = 6.283185264 + angle360;

				// 1.27 is inverse of 45' in rad				
				angleIdx = fmod(floor((angle360) * 1.273239553 + 0.5), 8);
				
				OUT.computedUv = float2(_UVVert_U[angleIdx][idx], _UVVert_V[angleIdx][idx]);
			}

			// Rotate vert, tangent and normal
			float cosO = cos(angle);
			float sinO = sin(angle);

			float2 vert;

			vert.x = cosO * IN.vertex.x + sinO * IN.vertex.z;
			vert.y = -sinO * IN.vertex.x + cosO * IN.vertex.z;

			IN.vertex.x = vert.x;
			IN.vertex.z = vert.y;

			vert.x = cosO * IN.normal.x + sinO * IN.normal.z;
			vert.y = -sinO * IN.normal.x + cosO * IN.normal.z;

			IN.normal.x = vert.x;
			IN.normal.z = vert.y;

			vert.x = cosO * IN.tangent.x + sinO * IN.tangent.z;
			vert.y = -sinO * IN.tangent.x + cosO * IN.tangent.z;

			IN.tangent.x = vert.x;
			IN.tangent.z = vert.y;			

			// Offset by .5 so that we sit on the ground			
			IN.vertex += float4(0.0, 0.5, 0, 0);
			IN.vertex *= float4(_Size.xyx, 1.0) * float4(_InstanceScaleRotation.xyz, 1.0);
			IN.vertex.y += _Size.z;
		}

		sampler2D _MainTex;
		sampler2D _BumpMap;
		half _Cutoff;
		
		float4 master_LODFade;

		// SurfaceOutputStandard for Standard lighting model
		void surf (Input IN, inout SurfaceOutput o)
		{					
			{
				float4x4 thresholdMatrix =
				{ 
					 1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
					13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
					 4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
					16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
				};
				float4x4 _RowAccess = 
				{ 
					1,0,0,0, 
					0,1,0,0, 
					0,0,1,0, 
					0,0,0,1 
				};

				float2 pos = IN.screenPos.xy / IN.screenPos.z;
				pos *= _ScreenParams.xy;
				clip(master_LODFade.y - thresholdMatrix[fmod(pos.x, 4)] * _RowAccess[fmod(pos.y, 4)]);
			}

			float4 c = tex2D(_MainTex, IN.computedUv);
			clip(c.a - _Cutoff);

			o.Albedo = c;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.computedUv));
		}

		ENDCG

		// Disable shadowstuff at the moment
		/*
		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			struct v2f
			{
				V2F_SHADOW_CASTER;
				#ifdef SPEEDTREE_ALPHATEST
				half2 uv : TEXCOORD1;
				#endif
				UNITY_DITHER_CROSSFADE_COORDS_IDX(2)
			};

			v2f vert(SpeedTreeVB v)
			{
				v2f o;
				#ifdef SPEEDTREE_ALPHATEST
				o.uv = v.texcoord.xy;
				#endif

				OffsetSpeedTreeVertex(v, master_LODFade);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				UNITY_TRANSFER_DITHER_CROSSFADE_HPOS(o, o.pos)

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				#ifdef SPEEDTREE_ALPHATEST
				clip(tex2D(_MainTex, i.uv).a * _Color.a - _Cutoff);
				#endif

				UNITY_APPLY_DITHER_CROSSFADE(i)
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		} // END PASS
		*/
	}
	FallBack "Diffuse"
}
