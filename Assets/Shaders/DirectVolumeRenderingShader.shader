Shader "VolumeRendering/DirectVolumeRenderingShader"
{
   
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
        _Density("Density", Range(256, 5000)) = 512
        _LightIntensity("Light Intensity", Range(0.0, 5)) = 0.2
        _Opacity("Opactiy", Range(0.00, 1.00)) = 1.00
        _MaxDepth("Max Depth", Range(0.0, 1.5)) = 1
        _MinDepth("Min Depth", Range(-1.0, 1.0)) = 0
        _DecreaseOpacityBasedOnDepth("OpacityBasedOnDepth", int) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma multi_compile __ CUTOUT_PLANE CUTOUT_BOX_INCL CUTOUT_BOX_EXCL
            #pragma multi_compile __ LIGHTING_ON
            #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
            #pragma multi_compile __ DVR_BACKWARD_ON
            #pragma multi_compile __ RAY_TERMINATE_ON
            #pragma multi_compile __ OPACITY_BASED_ON_DEPTH

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            //#include "UnityCustomRenderTexture.cginc"
      
            #define CUTOUT_ON CUTOUT_PLANE || CUTOUT_BOX_INCL || CUTOUT_BOX_EXCL
            
            struct vert_in { fixed4 vertex : POSITION; fixed4 normal : NORMAL;  fixed2 uv : TEXCOORD0; };

            struct frag_in  {  fixed4 vertex : SV_POSITION; fixed2 uv : TEXCOORD0; fixed3 vertexLocal : TEXCOORD1; fixed3 normal : NORMAL; };

            struct frag_out { fixed4 colour : SV_TARGET;
#if DEPTHWRITE_ON
                fixed depth : SV_DEPTH; 
#endif
            };

            sampler3D _DataTex;
            sampler3D _GradientTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;

            
            fixed _MinVal;
            fixed _MaxVal;
            int _Density;
            fixed _LightIntensity;
            fixed _Opacity;
            fixed _MinDepth;
            fixed _MaxDepth;
            fixed4 _SurfaceRenderingColour;
            fixed _SurfaceRenderingTDepth;
            int _DecreaseOpacityBasedOnDepth;
            
            fixed4 col = fixed4(0,0,0,0);


            static const fixed3 Zerofixed3 = fixed3 (0,0,0);
            static const fixed3 fixedfixed3 = fixed3 (0.5,0.5,0.5);
            static const fixed3 Onefixed3 = fixed3 (1,1,1);
            static const fixed4 Zerofixed4 = fixed4 (0,0,0,0);
#if CUTOUT_ON
            fixed4x4 _CrossSectionMatrix;
#endif

            struct RayInfo {  fixed3 startPos; fixed3 endPos; fixed3 direction; fixed2 aabbInters; };

            struct RaymarchInfo { RayInfo ray;  int numSteps; fixed numStepsRecip; fixed stepSize; };

            fixed3 getViewRayDir(fixed3 vertexLocal)  {
                if (unity_OrthoParams.w == 0)  {    
                    return normalize(ObjSpaceViewDir(fixed4(vertexLocal, 0.0f))); // Perspective
                }
                else {
                    // Orthographic
                    fixed3 camfwd = mul((fixed3x3)unity_CameraToWorld, fixed3(0,0,-1));
                    fixed4 camfwdobjspace = mul(unity_WorldToObject, camfwd);
                    return normalize(camfwdobjspace);
                }
            }

            // Find ray intersection points with axis aligned bounding box
            fixed2 intersectAABB(fixed3 rayOrigin, fixed3 rayDir, fixed3 boxMin, fixed3 boxMax)
            {
                fixed3 tMin = (boxMin - rayOrigin) / rayDir;
                fixed3 tMax = (boxMax - rayOrigin) / rayDir;
                fixed3 t1 = min(tMin, tMax);
                fixed3 t2 = max(tMin, tMax);
                fixed tNear = max(max(t1.x, t1.y), t1.z);
                fixed tFar = min(min(t2.x, t2.y), t2.z);
                return fixed2(tNear, tFar);
            };

            // Get a ray for the specified fragment (back-to-front)
            RayInfo getRayBack2Front(fixed3 vertexLocal)
            {
                RayInfo ray;
                ray.direction = getViewRayDir(vertexLocal);
                ray.startPos = vertexLocal + fixed3(0.5f, 0.5f, 0.5f);
                // Find intersections with axis aligned boundinng box (the volume)
                ray.aabbInters = intersectAABB(ray.startPos, ray.direction, Zerofixed3, Onefixed3);

                // Check if camera is inside AABB
                const fixed3 farPos = ray.startPos + ray.direction * ray.aabbInters.y - fixedfixed3;
                fixed4 clipPos = UnityObjectToClipPos(fixed4(farPos, 1.0f));
                ray.aabbInters += min(clipPos.w, 0.0);

                ray.endPos = ray.startPos + ray.direction * ray.aabbInters.y;
                return ray;
            }

            // Get a ray for the specified fragment (front-to-back)
            RayInfo getRayFront2Back(fixed3 vertexLocal)
            {
                RayInfo ray = getRayBack2Front(vertexLocal);
                ray.direction = -ray.direction;
                fixed3 tmp = ray.startPos;
                ray.startPos = ray.endPos;
                ray.endPos = tmp;
                return ray;
            }

            RaymarchInfo initRaymarch(RayInfo ray, int maxNumSteps)
            {
                RaymarchInfo raymarchInfo;
                raymarchInfo.stepSize = 0.0005f;/*1.732fgreatest distance in box*/// / maxNumSteps;
                raymarchInfo.numSteps = (int)clamp(abs(ray.aabbInters.x - ray.aabbInters.y) / raymarchInfo.stepSize, 1, maxNumSteps);
                raymarchInfo.numStepsRecip = 1.0 / raymarchInfo.numSteps;
                return raymarchInfo;
            }

            // Gets the colour from a 1D Transfer Function (x = density)
            fixed4 getTF1DColour(fixed density) {  return tex2Dlod(_TFTex, fixed4(density, 0.0f, 0.0f, 0.0f)); }

            // Gets the colour from a 2D Transfer Function (x = density, y = gradient magnitude)
            fixed4 getTF2DColour(fixed density, fixed gradientMagnitude) {  return tex2Dlod(_TFTex, fixed4(density, gradientMagnitude, 0.0f, 0.0f)); }

            // Gets the density at the specified position
            fixed getDensity(fixed3 pos) {  return tex3Dlod(_DataTex, fixed4(pos.x, pos.y, pos.z, 0.0f)); }

            // Gets the gradient at the specified position
            fixed3 getGradient(fixed3 pos) {  return tex3Dlod(_GradientTex, fixed4(pos.x, pos.y, pos.z, 0.0f)).rgb; }

            // Performs lighting calculations, and returns a modified colour.
            fixed3 calculateLighting(fixed3 col, fixed3 normal, fixed3 lightDir, fixed3 eyeDir, fixed specularIntensity)
            {
                fixed ndotl = max(lerp(0.0, _LightIntensity, dot(normal, lightDir)), 0.0); // modified, to avoid volume becoming too dark
                fixed3 diffuse = ndotl * col;
                fixed3 v = eyeDir;
                fixed3 r = normalize(reflect(-lightDir, normal));
                fixed rdotv = max( dot( r, v ), 0.0 );
                fixed3 specular = pow(rdotv, 32.0f) /* * fixed3(1.0f, 1.0f, 1.0f) doesnt make any visual changes -Alex */ * specularIntensity;
                return diffuse + specular;
            }

            // Converts local position to depth value
            fixed localToDepth(fixed3 localPos)  {

                fixed4 clipPos = UnityObjectToClipPos(fixed4(localPos, 1.0f));

                #if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                return (clipPos.z / clipPos.w) * 0.5 + 0.5;
                #else
                return clipPos.z / clipPos.w;
                #endif
            }

            bool IsCutout(fixed3 currPos) {

                #if CUTOUT_ON
                // Move the reference in the middle of the mesh, like the pivot
                fixed3 pos = currPos - fixed3(0.5f, 0.5f, 0.5f);

                // Convert from model space to plane's vector space
                fixed3 planeSpacePos = mul(_CrossSectionMatrix, fixed4(pos, 1.0f));
                
                #if CUTOUT_PLANE
                    return planeSpacePos.z > 0.0f;
                #elif CUTOUT_BOX_INCL
                    return !(planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f);
                #elif CUTOUT_BOX_EXCL
                    return planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f && planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f;
                #endif
                #else
                    return false;
                #endif
            }

            frag_in vert_main (vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

/****************************************************************************************************************************************************************************/

            // Direct Volume Rendering
            frag_out frag_dvr(frag_in i)
            {
                #define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)

                #ifdef DVR_BACKWARD_ON
                RayInfo ray = getRayBack2Front(i.vertexLocal);
                #else
                RayInfo ray = getRayFront2Back(i.vertexLocal);
                #endif
                RaymarchInfo raymarchInfo = initRaymarch(ray, _Density);

                fixed3 lightDir = normalize(ObjSpaceViewDir(fixed4(Zerofixed3, 0.0)));

                // Create a small random offset in order to remove artifacts
                ray.startPos += (2 * ray.direction * raymarchInfo.stepSize);// * tex2D(_NoiseTex, fixed2(i.uv.x, i.uv.y)).r;

                col = Zerofixed4;
                #ifdef DVR_BACKWARD_ON
                    fixed tDepth = 0.0f;
                #else
                    fixed tDepth = raymarchInfo.numStepsRecip * (raymarchInfo.numSteps - 1);
                #endif
                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const fixed t = iStep * raymarchInfo.numStepsRecip;
                    const fixed3 currPos = lerp(ray.startPos, ray.endPos, t);

                    // Perform slice culling (cross section plane)
                    if (currPos.y > _MaxDepth)
                        continue;
                    #ifdef CUTOUT_ON
                        if(IsCutout(currPos))
                    	    continue;
                    #endif

                    // Get the dansity/sample value of the current position
                    const fixed density = getDensity(currPos);
                    
                    // Apply visibility window
                    if (density < _MinVal || density > _MaxVal || currPos.y > _MaxDepth || currPos.y < _MinDepth) continue;

                    // Calculate gradient (needed for lighting and 2D transfer functions)
                    #if defined(TF2D_ON) || defined(LIGHTING_ON)
                        fixed3 gradient = getGradient(currPos);
                    #endif

                    // Apply transfer function
                    #if TF2D_ON
                        fixed mag = length(gradient) / 1.75f;
                        fixed4 src = getTF2DColour(density, mag);
                    #else
                        fixed4 src = getTF1DColour(density);
                    #endif

                    // Apply lighting
                    #if defined(LIGHTING_ON) && defined(DVR_BACKWARD_ON)
                        src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, ray.direction, 0.3f);
                    #elif defined(LIGHTING_ON)
                        src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, -ray.direction, 0.3f);
                    #endif

                    #ifdef DVR_BACKWARD_ON
                        const fixed OneMinusAlpha = (1 - src.a);
                        col.rgb = src.a * src.rgb + OneMinusAlpha * col.rgb;
                        col.a = src.a + OneMinusAlpha * col.a;
                    
                        // Optimisation: A branchless version of: if (src.a > 0.15f) tDepth = t;
                        tDepth = max(tDepth, t * step(0.15f, src.a));
                    #else
                        src.rgb *= src.a ;
                        col = (1.0f - col.a) * src + col;

                        if (col.a > 0.1f && t < tDepth) {
                            tDepth = t;
                        }
                    #endif

                     // Early ray termination
                    #if !defined(DVR_BACKWARD_ON) && defined(RAY_TERMINATE_ON)
                        if (col.a > OPACITY_THRESHOLD) {
                            break;
                        }
                    #endif

                    #if OPACITY_BASED_ON_DEPTH
                        col.a = (1 - currPos.y ) * _Opacity;
                    #else    
                        col.a *= _Opacity ; // - ALEX
                    #endif  
                }
                // Write fragment output
                frag_out output;
                
                output.colour = col;
                #if DEPTHWRITE_ON
                    tDepth += (step(col.a, 0.0) * 1000.0); // Write large depth if no hit
                    const fixed3 depthPos = lerp(ray.startPos, ray.endPos, tDepth) - fixed3(0.5f, 0.5f, 0.5f);
                    output.depth = localToDepth(depthPos);
                #endif
                return output;
            }

/****************************************************************************************************************************************************************************/
            // Maximum Intensity Projection mode
            frag_out frag_mip(frag_in i)
            {

                RayInfo ray = getRayBack2Front(i.vertexLocal);
                RaymarchInfo raymarchInfo = initRaymarch(ray, _Density);

                fixed maxDensity = 0.0f;
                fixed3 maxDensityPos = ray.startPos;
                for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                {
                    const fixed t = iStep * raymarchInfo.numStepsRecip;
                    const fixed3 currPos = lerp(ray.startPos, ray.endPos, t);
                    
                    #ifdef CUTOUT_ON
                        if (IsCutout(currPos))
                            continue;
                    #endif

                    const fixed density = getDensity(currPos);
                    if (density > maxDensity && density > _MinVal && density < _MaxVal)
                    {
                        maxDensity = density;
                        maxDensityPos = currPos;
                    }
                }

                // Write fragment output
                frag_out output;
                output.colour = fixed4(1.0f, 1.0f, 1.0f, maxDensity); // maximum intensity
                #if DEPTHWRITE_ON
                    output.depth = localToDepth(maxDensityPos - fixed3(0.5f, 0.5f, 0.5f));
                #endif
                return output;
            }
/****************************************************************************************************************************************************************************/

            // Surface rendering mode
            // Draws the first point (closest to camera) with a density within the user-defined thresholds.
            frag_out frag_surf(frag_in i)
            {
                    RayInfo ray = getRayFront2Back(i.vertexLocal);
                    RaymarchInfo raymarchInfo = initRaymarch(ray, _Density);

                    // Create a small random offset in order to remove artifacts
                    // ray.startPos = ray.startPos + (2.0 * ray.direction * raymarchInfo.stepSize) * tex2D(_NoiseTex, fixed2(i.uv.x, i.uv.y)).r;

                    //_SurfaceRenderingColour = fixed4(0,0,0,0);

                    for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
                        {
                        const fixed t = iStep * raymarchInfo.numStepsRecip;
                        const fixed3 currPos = lerp(ray.startPos, ray.endPos, t);

                        #ifdef CUTOUT_ON
                            if (IsCutout(currPos))
                                continue;
                        #endif

                        const fixed density = getDensity(currPos);
                        if (density > _MinVal && density < _MaxVal && currPos.y < _MaxDepth && currPos.y > _MinDepth)
                        {
                            fixed3 normal = normalize(getGradient(currPos));
      
                            _SurfaceRenderingColour = getTF2DColour(density, normal);                         
                            _SurfaceRenderingColour.rgb = calculateLighting(_SurfaceRenderingColour.rgb, normal, -ray.direction, -ray.direction, 0.25);
                      
                            // 1.732f 
                            #if OPACITY_BASED_ON_DEPTH
                                _SurfaceRenderingColour = (1 - currPos.y - density) * _Opacity;
                            #else
                                _SurfaceRenderingColour.a *= _Opacity;
                            #endif
                            break;
                        }
                    }
                    #if DEPTHWRITE_ON            
                        const fixed tDepth = iStep * raymarchInfo.numStepsRecip + (step(_SurfaceRenderingColour.a, 0.0) * 1000.0); // Write large depth if no hit
                        _SurfaceRenderingTDepth = localToDepth(lerp(ray.startPos, ray.endPos, tDepth) + fixed3(0.5f, 0.5f, 0.5f));
                    #endif

                // Write fragment output
                frag_out output;
                output.colour = _SurfaceRenderingColour;
                #if DEPTHWRITE_ON   
                    output.depth = _SurfaceRenderingTDepth;
                #endif
                return output;
            }
/****************************************************************************************************************************************************************************/

            frag_in vert(vert_in v)
            {
                return vert_main(v);
            }

            frag_out frag(frag_in i)
            {
#if MODE_DVR
                return frag_dvr(i);
#elif MODE_MIP
                return frag_mip(i);
#elif MODE_SURF
                return frag_surf(i);
#endif
            }

            ENDCG
        }
    }
}
