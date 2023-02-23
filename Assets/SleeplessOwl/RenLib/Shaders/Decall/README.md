﻿

## Source

https://github.com/ColinLeung-NiloCat/UnityURPUnlitScreenSpaceDecalShader



When should I use this shader?
-------------------

if you need to render bullet holes, dirt/logo on wall, 3D projected UI, explosion dirt mark, blood splat,  projected texture fake shadow(blob shadow) ..... and the receiver surface is not flat(can't use a flat transparent quad to finish the job), try using this shader.

How to use this shader in my project?
-------------------
0. clone the shader to your project
1. create a new material using that shader
2. assign any texture to material's Texture slot
3. create a new unity default cube GameObject in scene (in Hierarchy window, click +/3D Object/Cube)
4. apply that material to Cube Gameobject's MeshRenderer component's material slot
5. edit the GameObject's transform so the local forward vector (blue Z arrow) is pointing to scene objects, and the cube is intersecting scene objects
6. you should now see your new decal cube is rendering correctly(projecting alpha blending texture to scene objects correctly)
7. (optional)edit _Color / BlendingOption, according to your needs
8. (optional)finally make the cube as thin/small as possible to improve GPU rendering performance

Requirement
-------------------
- Forward rendering (only tested on URP, it may work in built-in RP)
- Perspective camera
- _CameraDepthTexture is already rendering by unity (toggle on DepthTexture in your Universal Render Pipeline Asset)
- For mobile, you need at least OpenGLES3.0 (#pragma target 3.0 due to ddx() & ddy())

Is this shader optimized for mobile?
-------------------
This screen space decal shader is SRP batcher compatible, so you can put lots of decals in scene without hurting CPU performance too much(even all decals use different materials).

Also, this shader moved all matrix mul() inside the fragment shader to vertex shader, so you can put lots of decals in scene without hurting GPU performance too much, as long as they are thin, small and don't overlap(overdraw).

I need LOTs of decals in my game, is there performance best practice?
-------------------
- make all decal cube as thin/small as possible
- don't overlap decals(overdraw)
- Set ZTest to LessEqual, and Cull to Back in the material inspector, if your camera never goes into decal's cube volume, doing this will improve GPU performance a lot! (due to effective early-Z, GPU only need to render visible decals)
- disable _ProjectionAngleDiscardEnable, doing this will improve GPU performance a lot!
- enable "generate mipmap" for your decal texture, else a high resolution decal texture will make your game slow due to cache miss in GPU memory

if you do every optimzations listed above, and your game is still slow due to this decal shader, please send me an issue, I will treat it as bug.

Editor System Requirements
-------------------
- Unity 2019.1 or later (due to "shader_feature_local"). But you can replace to "shader_feature" if you want to use this shader in older unity versions

Implementation Reference
-------------------
Low Complexity, High Fidelity: The Rendering of INSIDE's optimized decal shader

https://youtu.be/RdN06E6Xn9E?t=2153

Screen Space Decals in Warhammer 40,000: Space Marine

https://www.slideshare.net/blindrenderer/screen-space-decals-in-warhammer-40000-space-marine-14699854?fbclid=IwAR2X6yYeWmDiz1Ho4labx3zA3GATpC7fi5qNkzjEj-MYTOBpXnkIsnA3T-A

