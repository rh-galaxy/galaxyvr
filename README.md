# galaxyvr
Galaxy Forces VR

- Steam:        Using unity 2021.3.8f1 (2019.2.10f1 before 20220827) (the default branch)
- Multiplayer:  Using unity 2021.3.8f1 (2019.2.10f1 before 20220827)
- Oculus PC:    Using unity 2021.3.8f1 (2021.1.6f1 before 20220827, 2020.1.13f1 before 20210526)
- Oculus Quest: Using unity 2021.3.8f1 (2021.1.6f1 before 20220827, 2020.1.13f1 before 20210526)
- WebReplay:    Using unity 2021.3.8f1 (2021.1.6f1 before 20220829)

Version differences

```
- Steam:       URP, Postprocessing, Use steamvr input with openvr, Single pass Instanced rendering, D3D11
- Multiplayer: URP, Postprocessing, Use steamvr input with openvr, Single pass Instanced rendering, D3D11
- Meta PC:     URP, Postprocessing, Use UnityEngine.XR.InputDevice, Single pass Instanced rendering, D3D11
- Meta Quest:  SRP, No postprocessing, Explosion has no Shockwave, Use UnityEngine.XR.InputDevice, Multiview rendering, OpenGLES3
- WebReplay:   URP, Postprocessing, WebGL2
```

TODO

* Quest: Instead of Glow with postprocessing, do it with a colored stretched transparent billboard.
 Tested but no good.

* Maybe reduce transparency on status bar.

* Planets are rendered differently on Quest then the rest URP projects.
 Update to the same tree later.

* Add a new mode to existing mission maps, no hiscore - just for fun and something different:
 Have the cargo dangle on a chain (as one box, even when loading more cargo) below the ship and affect the ships physics accordingly.
 Perhaps a breakable chain where cargo will be lost if broken, and cargo may be damaged for less score.

Dev screenshot

![Current dev screenshot](/gfx_dev/bloom_editor.jpg)