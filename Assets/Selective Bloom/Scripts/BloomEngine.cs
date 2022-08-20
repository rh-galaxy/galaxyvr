using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

///<summary>
/// Event type executed when adding or removing a new object from the glow object list. Useful to update the rendering.
///</summary>
public delegate void OnGlowObjectsChanged();

///<summary>
/// A script designed to be attached to the camera that will render the bloom effect on the specified object.
///</summary>
public class BloomEngine : MonoBehaviour
{
    private Material _material;
    private CommandBuffer _buffer;
    private Camera _camera;
    public float Radius = 4.0f;
    public float Step = 1.0f;

    ///<summary>
    /// Adds an object from the list of objects to render with glow.
    ///</summary>
    public static void Add(SelectiveBloom Glow)
    {
        BloomRegistry.Instance.Add(Glow);
    }

    ///<summary>
    /// Removes an object from the list of objects to render with glow.
    ///</summary>
    public static void Remove(SelectiveBloom Glow)
    {
        if(BloomRegistry.Instance != null)
            BloomRegistry.Instance.Remove(Glow);
    }

    void OnEnable()
    {
        BloomRegistry.Instance.GlowObjectsChanged += OnGlowObjectsChanged;
        SetupIfNecessary();
    }

    void OnDisable()
    {
        if(BloomRegistry.Instance != null)
            BloomRegistry.Instance.GlowObjectsChanged -= OnGlowObjectsChanged;
        Reset();
    }

    ///<summary>
    /// Executed when something is added or removed from the bloom registry.
    ///</summary>
    private void OnGlowObjectsChanged()
    {
        Reset();
        SetupIfNecessary();
    }

    ///<summary>
    /// Disable the effect by removing the command buffer and disposing it, so as to avoid any memory leaks.
    ///</summary>
    private void Reset()
    {
        _camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _buffer);
        _buffer.Dispose();
        _buffer = null;
    }

    ///<summary>
    /// Lazily find the camera component and recreate the command buffer if needed.
    ///</summary>
    private void SetupIfNecessary()
    {
        if(_camera == null)
            _camera = GetComponent<Camera>();
        if(_buffer == null)
            BuildCommand();
        SetupMaterial();
    }

    ///<summary>
    ///
    ///</summary>
    private void SetupMaterial()
    {
        if(_material == null)
            _material = new Material(Shader.Find("Custom/Bloom"));
    }

    ///<summary>
    /// Every frame pass the bloom shader through the result image as a post processing effect.
    ///</summary>
	void OnRenderImage(RenderTexture Source, RenderTexture Destination)
    {
        SetupMaterial();
        _material.SetFloat("_Radius", Radius);
        _material.SetFloat("_Step", Step);
        const int downsample = 4;
        var rt1 = RenderTexture.GetTemporary(Source.width / downsample, Source.height / downsample);
        var rt2 = RenderTexture.GetTemporary(Source.width / downsample, Source.height / downsample);
        Graphics.Blit(Source, Destination, _material, 2);
        Graphics.Blit(Source, rt1, _material, 0);
        Graphics.Blit(rt1, rt2, _material, 1);
        Graphics.Blit(rt2, Destination, _material, 3);
		RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
    }

    ///<summary>
    /// Build a CommandBuffer that renders all the "glow objects" with the emission shader, so it can be later used in the postprocessing stage.
    /// It is updated every time a new object is added or removed to the list or parameters changed.
    ///</summary>
    private void BuildCommand()
    {
        _buffer = new CommandBuffer();
        _buffer.name = "Selective Bloom";
        var id = Shader.PropertyToID("_GlowMap");
        _buffer.GetTemporaryRT(id, -1, -1, 24, FilterMode.Bilinear);
        _buffer.SetRenderTarget(id);
        _buffer.ClearRenderTarget(true, true, Color.black);

        var objs = BloomRegistry.Instance.Objects;
        for(var i = 0; i < objs.Length; ++i)
        {
            var renderer = objs[i].GetComponent<Renderer>();
            var material = new Material(Shader.Find("Custom/Emission"));
            material.SetColor("_BloomColor", objs[i].Color);
            material.SetFloat("_Strength", objs[i].Strength);
            if(renderer != null && material != null)
                _buffer.DrawRenderer(renderer, material);
        }
        _buffer.SetGlobalTexture("_GlowMap", id);
        _camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _buffer);
        _camera.depthTextureMode |= DepthTextureMode.Depth;
    }
}

/// <summary> 
/// A singleton that stores references to all the objects that need to be rendered with glow.
/// </summary>
public class BloomRegistry : MonoBehaviour
{
    private static bool _shuttingDown;
    private static object _lock = new object();
    private static BloomRegistry _instance;
    private readonly HashSet<SelectiveBloom> _glowObjects;
    public event OnGlowObjectsChanged GlowObjectsChanged;

    public BloomRegistry()
    {
        _glowObjects = new HashSet<SelectiveBloom>();
    }

    ///<summary>
    /// Add an object to the glow registry. It also updates the rendering state.
    ///</summary>
    public void Add(SelectiveBloom Glow)
    {
        _glowObjects.Add(Glow);
        if(GlowObjectsChanged != null) GlowObjectsChanged.Invoke();
    }

    ///<summary>
    /// Remove an object from the glow registry. It also updates the rendering state.
    ///</summary>
    public void Remove(SelectiveBloom Glow)
    {
        _glowObjects.Remove(Glow);
        if(GlowObjectsChanged != null) GlowObjectsChanged.Invoke();
    }

    ///<summary>
    /// Returns an array of references to the objects that need to be renderer with glow.
    ///</summary>
    public SelectiveBloom[] Objects
    {
        get
        {
            var arr = new SelectiveBloom[_glowObjects.Count];
            var i = 0;
            foreach(var obj in _glowObjects)
            {
                arr[i] = obj;
                i++;
            }
            return arr;
        }
    }

    ///<summary>
    /// returns the current (and only) alive instace of the BloomRegistry or null if its shutting down.
    ///</summary>
    public static BloomRegistry Instance
    {
        get
        {
            if (_shuttingDown) return null;
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (BloomRegistry)FindObjectOfType(typeof(BloomRegistry));
                    if (_instance == null)
                    {
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<BloomRegistry>();
                        singletonObject.name = typeof(BloomRegistry).ToString() + " (Singleton)";
 
                        DontDestroyOnLoad(singletonObject);
                    }
                } 
                return _instance;
            }
        }
    }
 
 
    private void OnApplicationQuit()
    {
        _shuttingDown = true;
    }
 
    private void OnDestroy()
    {
        _shuttingDown = true;
    }
}