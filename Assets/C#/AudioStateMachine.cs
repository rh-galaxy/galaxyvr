﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class AudioStateMachine : MonoBehaviour
{
    public static AudioStateMachine instance;
    public Player player;

    [Header("Config")]

    [Range(0.0f, 2f)]
    public float masterVolume;

    [Range(0.2f, 1f)]
    public float flowEnterLimit;

    [Range(0.0f, 0.8f)]
    public float flowExitLimit;


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
    public float enemiesNear;
    public float bulletsNear;

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

    /// <summary>
    /// Sets the output to # i in the device list
    /// Device 0 is always default OS device
    /// </summary>
    /// <param name="i">The index.</param>
    public void SetOutput(int i = 0) 
    {
        print("Using device " + i);
        FMODUnity.RuntimeManager.LowlevelSystem.setOutput(FMOD.OUTPUTTYPE.AUTODETECT);
        FMODUnity.RuntimeManager.LowlevelSystem.setDriver(i);
    }

    //only to be run when compiled for oculus
    // setting rift headphones or windows default or both depending on config in Oculus Home
    public void SetOutputByRiftSetting()
    {
        FMOD.System sys;
        FMODUnity.RuntimeManager.StudioSystem.getLowLevelSystem(out sys);

        int i, driverCount = 0;
        sys.getNumDrivers(out driverCount);

        string riftId = OVRManager.audioOutId;

        for (i=0; i<driverCount; i++)
        {
            System.Guid guid;
            int rate, channels;
            FMOD.SPEAKERMODE mode;
            sys.getDriverInfo(i, out guid, out rate, out mode, out channels);

            if (guid.ToString() == riftId)
            {
                sys.setDriver(i);
                break;
            }
        }
    }


    public void LevelTransition(float f)
    {
        SetParam(transition, f);
        transitionVal = f;
    }

    public void SetLife(float f, bool overridefade = false)
    {
        if (fading && !overridefade)
            return;
        SetParam(death, 1 - f);
        lifeVal = 1 - f;
    }

    public void SetFlow(float f)
    {
        if (f < flowExitLimit)
        {
            SetParam(flow, 0);
        }
        if (f >= flowEnterLimit)
        {
            SetParam(flow, 1);
        }

        flowVal = f;
    }

    public void SetCargo(float f)
    {
        SetParam(cargo, f);
        cargoVal = f;
    }

    float fadeLen = 1;

    private void Update()
    {
        if (fading)
        {
            fadeVal -= (1 * fadeLen) * (Time.deltaTime);
            SetLife(fadeVal, true);
            fading &= fadeVal > 0;
            return;
        }
        //print("fading done");

        //player is now to be set to a valid player (or null) before transitioning to in-game music
        // in the scene switching code in the end of GameManager.cs
        if (player != null)
        {
            float fClipped = player.fShipHealth;
            if (fClipped < 0) fClipped = 0;
            SetLife(fClipped / Player.FULL_HEALTH);
            SetFlow(player.fMeanSpeed / 10);
            SetCargo((float)player.iCargoNumUsed / (float)Player.MAXSPACEINHOLDUNITS);
            fClipped = (float)player.iNumEnemiesNear / 4.0f;
            if (fClipped > 1.0) fClipped = 1.0f;
            enemiesNear = fClipped;
            fClipped = (float)player.iNumBulletsNear / 10.0f;
            if (fClipped > 1.0) fClipped = 1.0f;
            bulletsNear = fClipped;
        }

        SetVolume();
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

    void SetVolume()
    {
        main.setVolume(masterVolume);
    }

    float target = 0;
    bool fading = false;
    float fadeVal;
    public void ResetLife()
    {
        fading = true;
        fadeVal = 1;
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
        switch (sceneName)
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

