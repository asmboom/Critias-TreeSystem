Shader "Atlantis Nature/SpeedTree Bilboard Batch" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bump (RGB)", 2D) = "white" {}
		_Cutoff("Cutoff", Range(0, 1)) = 0.33
		_Size("Billboard Sizes", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags
		{ 
			"IgnoreProjector" = "True" 
			"Queue" = "Geometry" 
			"RenderType" = "TransparentCutout" 

			// "DisableBatching" = "True"
		}
		
		LOD 200
		Cull Off

		CGPROGRAM						

		#pragma surface surf Standard vertex:BatchedTreeVertex
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 computedUv;

			float4 screenPos;
			half transparency;
		};

		struct appdata_t
		{
			float4 vertex		: POSITION;
			float4 tangent		: TANGENT;
			float3 normal		: NORMAL;
			float4 texcoord		: TEXCOORD0;
			float4 texcoord1	: TEXCOORD1;
			float4 texcoord2	: TEXCOORD2;
			half4 color			: COLOR;
		};

		fixed4 _Color;
		fixed _Cutoff;

		float4 _UVVert_U[8];
		float4 _UVVert_V[8];

		float4 _UVHorz_U;
		float4 _UVHorz_V;

		float3 _Size;

		void BatchedTreeVertex(inout appdata_t IN, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			
			float x = IN.vertex.x;
			float y = IN.vertex.y;

			int idx;
			if (x < 0 && y < 0) idx = 0;
			else if (x < 0 && y > 0) idx = 3;
			else if (x > 0 && y < 0) idx = 1;
			else idx = 2;

			OUT.computedUv = float2(_UVVert_U[0][idx], _UVVert_V[0][idx]);

			float3 campos = _WorldSpaceCameraPos;
			float3 pos = IN.texcoord1.xyz;

			// Instance rotation
			float rot = IN.texcoord1.w;

			// TODO: this batched stuff we'll only test for scale 0-1 no need for dithering here
			float dist = distance(pos, campos);

			float3 v2 = float3(0, 0, 1);
			float3 v1 = campos - pos;

			int angleIdx;

			v1 = normalize(v1);

			float dotA, detA;

			dotA = v1.x * v2.x + v1.z * v2.z;
			detA = v1.x * v2.z - v1.z * v2.x;

			// TODO: add tree instance's rotation too
			// Map to 0 - 360
			// float angle = (atan2(det, dot)) + 180.0f + instance rotation
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
				// Quarter PI: 0.78539816339
				// Half PI: 1.57079632679
				// Chose the index based on the distance (angle / 45)
				// There's a 90' difference between our bill's and unity's... Ask on forums for this one?
				angleIdx = fmod((angle + rot) / 0.78539816339, 8);
				// angleIdx = fmod((angle) / 0.78539816339, 8);
				OUT.computedUv = float2(_UVVert_U[angleIdx][idx], _UVVert_V[angleIdx][idx]);
			}

#define DISTANCE 300.0
#define THRES 10.0

			// if (dist > DISTANCE - THRES)
			if (dist >= DISTANCE)
			{
				// OUT.transparency = 1.0 - (clamp(DISTANCE - dist, 0.0, THRES) / THRES);

				float2 vert;

				// Rotate vert, tangent and normal
				float cosO = cos(angle);
				float sinO = sin(angle);

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

				// Apply the scale, and move into world space
				IN.vertex += float4(0.0, 0.5, 0, 0);
				float4 scale = float4(IN.texcoord2.xyx, 1.0);
				IN.tangent *= scale;
				IN.vertex *= scale;
				IN.vertex += IN.texcoord1;
			}
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{	
			/*
			{
				// Screen-door transparency: Discard pixel if below threshold.
				float4x4 thresholdMatrix =
				{ 
					1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
					13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
					4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
					16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
				};
				float4x4 _RowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
				float2 pos = IN.screenPos.xy / IN.screenPos.z;
				pos *= _ScreenParams.xy; // pixel position
				clip(IN.transparency - thresholdMatrix[fmod(pos.x, 4)] * _RowAccess[fmod(pos.y, 4)]);
			}
			*/

			// Albedo comes from a texture tinted by color
			// fixed4 c = fixed4(0.2, 0.4, 0.6, 1); // tex2D(_MainTex, IN.computedUv) * _Color;
			fixed4 c = tex2D(_MainTex, IN.computedUv) * _Color;
			clip(c.a - _Cutoff);

			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.computedUv));
			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
