# galaxyvr
Galaxy Forces VR

- Steam:       Using unity 2021.3.25f1 (2021.3.8f1 before 20230525, 2019.2.10f1 before 20220827) (the default branch)
- Multiplayer: Using unity 2021.3.25f1 (2021.3.8f1 before 20230525, 2019.2.10f1 before 20220827)
- Meta PC:     Using unity 2021.3.25f1 (2021.3.8f1 before 20230525, 2021.1.6f1 before 20220827, 2020.1.13f1 before 20210526) (Use this on macOS)
- Meta Quest:  Using unity 2021.3.8f1 (2021.1.6f1 before 20220827, 2020.1.13f1 before 20210526)
- WebReplay:   Using unity 2021.3.8f1 (2021.1.6f1 before 20220829)

Version differences

```
- Steam:       URP, Postprocessing, Use UnityEngine.XR.InputDevice, Single pass Instanced rendering, D3D11, OpenXR
- Multiplayer: URP, Postprocessing, Use UnityEngine.XR.InputDevice, Single pass Instanced rendering, D3D11, OpenXR
- Meta PC:     URP, Postprocessing, Use UnityEngine.XR.InputDevice, Single pass Instanced rendering, D3D11, Oculus
- Meta Quest:  SRP, No postprocessing, Explosion has no Shockwave, Use UnityEngine.XR.InputDevice, Multiview rendering, OpenGLES3
- WebReplay:   URP, Postprocessing, WebGL2
```

Development on macOS or for Win/Linux without VR
Define NOOCULUS, delete Oculus folder

TODO

* Quest: Instead of Glow with postprocessing, do it with a colored stretched transparent billboard.
 Tested but no good.

* Maybe reduce transparency on status bar.

* Planets are rendered differently on Quest then the rest URP projects.
 Update to the same tree later.

Dev screenshot

![Current dev screenshot](/gfx_dev/bloom_editor.jpg)