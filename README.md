# galaxyvr
Galaxy Forces VR

- Steam:        Using unity 2021.3.8f1 (2019.2.10f1 before 20220827) (the default branch)
- Multiplayer:  Using unity 2021.3.8f1 (2019.2.10f1 before 20220827)
- Oculus Rift:  Using unity 2021.3.8f1 (2021.1.6f1 before 20220827, 2020.1.13f1 before 20210526)
- Oculus Quest: Using unity 2021.3.8f1 (2021.1.6f1 before 20220827, 2020.1.13f1 before 20210526)
- WebReplay:    Using unity 2021.3.8f1 (2021.1.6f1 before 20220829)

Version differences

Steam        URP, Postprocessing, Glow settings*, Use steamvr input with openvr, Single pass Instanced rendering, D3D11
Multiplayer  URP, Postprocessing, Glow settings*, Use steamvr input with openvr, Single pass Instanced rendering, D3D11
Oculus Rift  URP, Postprocessing, Normal glow settings, Use UnityEngine.XR.InputDevice, Single pass Instanced rendering, D3D11
Oculus Quest SRP, No postprocessing, Explosion has no Shockwave, Use UnityEngine.XR.InputDevice, Multiview rendering, OpenGLES3
WebReplay    URP, Postprocessing, Normal glow settings, WebGL2


* Glow settings - Hard to get it to the same as Oculus Rift and WebReplay,
 includes more objects than just the LZ materials, but might be ok now, keep under observation