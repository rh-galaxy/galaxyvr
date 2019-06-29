using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class AudioStateMachine : MonoBehaviour
{
    public static AudioStateMachine instance;
    public Player player;
    private int sceneState = 0;

    [Header("Parameter name in Studio")]
    public string transition = "Transition";
    public string death = "Death";
    public string flow = "Flow";
    public string cargo = "Cargo";


    [Header("Global Events")]
    [FMODUnity.EventRef]
    public string mainEvent;
    FMOD.Studio.EventInstance main;

    [Header("Debug/Testing")]
    public float transitionVal;

    public float lifeVal;
    public float flowVal;
    public float cargoVal;


    void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            StartSound(main, mainEvent);

        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void LevelTransition(float f)
    {
        SetParam(transition, f);
        transitionVal = f;
    }

    public void SetLife(float f)
    {
        SetParam(death, f - 1);
        lifeVal = f - 1;
    }

    public void SetFlow(float f)
    {
        SetParam(flow, f);
        flowVal = f;
    }

    public void SetCargo(float f)
    {
        SetParam(cargo, f);
        cargoVal = f; 
    }


    private void Update()
    {
        //if(fading)
        //{
        //    SetParam(death, Mathf.Lerp(1, 0, 0.1f));
        //}
        FindPlayer();
        if(player!=null)
        {
            SetLife(player.fShipHealth / Player.FULL_HEALTH);
            SetFlow(player.fMeanSpeed / 10);
            SetCargo(player.iCargoNumUsed / Player.MAXSPACEINHOLDUNITS);
            //SetEnemies()
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.Find("Player");
        if(p!=null) player = player ?? p.GetComponent<Player>();
    }

    void StartSound(FMOD.Studio.EventInstance eventInstance, string eventRef, GameObject sender = null)
    {
        sender = sender ?? gameObject;

        // Spatialize audio
        eventInstance = FMODUnity.RuntimeManager.CreateInstance(eventRef);
        eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(sender));

        main = eventInstance;
        main.start();
    }

    /// <summary>
    /// Sets the parameter s to val the current instance
    /// </summary>
    /// <param name="s">S.</param>
    /// <param name="val">Value.</param>
    void SetParam(string s, float val)
    {
        // do not send out of range values, breaks playback
        float eventValue = Mathf.Clamp01(val);

        main.setParameterValue(s, eventValue);
    }

    float target = 0;
    bool fading = false;
    public void ResetLife()
    {
        fading = true;
    }

    ///// <summary>
    ///// Initiates a parameter fade 
    ///// </summary>
    ///// <param name="param">Parameter.</param>
    ///// <param name="startVal">Start value.</param>
    ///// <param name="targetVal">Target value.</param>
    ///// <param name="time">Time.</param>
    //void SetParamFade(string param, float startVal, float targetVal, float time)
    //{
    //    Fade f = new Fade(param, startVal, targetVal, 0, time, time / Fade.resolution);
    //    FadeCall(f);
    //}

    //IEnumerator StepFade(Fade f)
    //{
    //    yield return new WaitForSeconds(f.timestep);
    //    FadeCall(f);
    //    yield return null;
    //}
     
    //void FadeCall(Fade f) 
    //{ 
    //    if(f.currentTime < f.time)
    //    {
    //        StartCoroutine(StepFade(f));
    //        f.currentVal = Mathf.Lerp(f.currentVal, f.targetVal, f.timestep / f.time);
    //        SetParam(f.name, f.currentVal);
    //        f.currentTime += f.timestep;
    //    }

    //    SetParam(f.name, f.targetVal);
    //}

    public void Transition(string sceneName)
    {
        switch(sceneName)
        {
            case "Scenes/GameStart":
                LevelTransition(0.0f);
                break;

            case "Scenes/PlayGame":
                LevelTransition(1.0f);
                break;
        }
    }
}

//class Fade
//{
//    public static int resolution = 128;
//    public string name;
//    public float currentVal;
//    public float targetVal;

//    public float currentTime;
//    public float time;
//    public float timestep;

//    public Fade(string name, float currentVal, float targetVal, float currentTime, float time, float timestep)
//    {
//        this.name = name;
//        this.currentVal = currentVal;
//        this.targetVal = targetVal;
//        this.currentTime = currentTime;
//        this.time = time;
//        this.timestep = timestep;
//    }
//}