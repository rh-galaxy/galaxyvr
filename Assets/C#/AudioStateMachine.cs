using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using FMODUnity;

public class AudioStateMachine : MonoBehaviour
{
    public static AudioStateMachine instance;
    public Player player;

    [Header("Config")]

    [Range(0.0f, 2f)]
    public float masterVolume;

    [Range(0, 1)]
    public float enemyVol;
    private float activeEnemyVol;

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
    public EventReference EventMain;
    
    FMOD.Studio.EventInstance main;

    [Header("Debug/Testing")]
    public float transitionVal;
    public float lifeVal;
    public float flowVal;
    public float cargoVal;
    public float enemiesNear;
    public float bulletsNear;

    [Range(25f, 2f)]
    public float nearbyEnemyDiv = 10;

    void Awake()
    {
        //singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            /**///Init();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public bool Init()
    {
        if (!main.isValid())
        {
            // Spatialize audio
            main = FMODUnity.RuntimeManager.CreateInstance(EventMain);
            main.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));

            main.start();
            return main.isValid();
        }
        return false;
    }

    //sets the output to # i in the device list
    // device 0 is always default OS device
    public void SetOutput(int i = 0) 
    {
        print("Using device " + i);
        FMODUnity.RuntimeManager.CoreSystem.setOutput(FMOD.OUTPUTTYPE.AUTODETECT);
        FMODUnity.RuntimeManager.CoreSystem.setDriver(i);
    }

    public void LevelTransition(float f)
    {
        SetParam(transition, f);
        transitionVal = f;
    }

    public void SetLife(float f)
    {
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

    bool bLifeFadeOut = false;
    float fLifeBeforeZero = 0.0f;
    //fading code
    float fFadeFinishTime = 1.0f;
    float fFadeTimer = 1.0f; //init so fade done is true at the start
    float fFadeDelay = 0.0f;
    float fFadeRange, fFadeStart, fFadeStop;
    bool UpdateFade()
    {
        //set progress
        fFadeTimer += Time.deltaTime;
        float fProgress = (fFadeTimer - fFadeDelay) / fFadeFinishTime;
        if (fProgress > 0.999f)
            return true; //fade done

        if (fFadeTimer < fFadeDelay)
            return false; //delay not reached

        //calculate new value fcrom progress
        float fFadeCurVol = fFadeStart + fProgress * fFadeRange;
        //hard limits
        if (fFadeCurVol < 0.0f) fFadeCurVol = 0.0f;
        if (fFadeCurVol > 1.0f) fFadeCurVol = 1.0f;
        //set value
        SetLife(fFadeCurVol);
        return false;
    }
    public void StartFade(float fTime, float fDelay, float fStart, float fStop)
    {
        fFadeFinishTime = fTime;
        fFadeTimer = 0.0f;
        fFadeDelay = fDelay;
        fFadeStart = fStart;
        fFadeStop = fStop;
        fFadeRange = fFadeStop - fFadeStart;
        //UpdateFade();
    }

    private void Update()
    {
        bool bFadeDone = UpdateFade();
        if (bLifeFadeOut && bFadeDone)
        {
            //fade out just done (currently not active)
            bLifeFadeOut = false;
            StartFade(2.3f, 0.15f, 0.0f, 1.00f); //begin fade in again, done over 2.45 sec
            SetLife(0); //ensure 0 value during the delay part 0.15 sec
            bFadeDone = false;
            //print("StartFade 0 -> 1");
        }

        //player is now to be set to a valid player (or null) before transitioning to in-game music
        // in the scene switching code in the end of GameManager.cs
        if (player != null && player.oMap != null)
        {
            float fClipped = player.fShipHealth;
            if (fClipped <= 0) fClipped = 0;
            else fLifeBeforeZero = fClipped / player.FULL_HEALTH;
            if (bFadeDone /**//*&& fClipped!=0*/)
                SetLife(fClipped / player.FULL_HEALTH);
            SetFlow(player.fMeanSpeed / 10);
            fClipped = ((float)player.iCargoNumUsed / (float)Player.MAXSPACEINHOLDUNITS) - 0.15f;
            if (fClipped <= 0) fClipped = 0;
            SetCargo(fClipped);
            fClipped = (float)player.iNumEnemiesNear / 5.0f;
            if (fClipped > 1.0f) fClipped = 1.0f;
            enemiesNear = fClipped;
            fClipped = (float)player.iNumBulletsNear / 10.0f;
            if (fClipped > 1.0f) fClipped = 1.0f;
            bulletsNear = fClipped;

            if (player.oMap.iLevelType == (int)LevelType.MAP_RACE)
            {
                SetParam("Mission", 0);
                //print("starting race audio");
            }
            else if (player.oMap.iLevelType == (int)LevelType.MAP_MISSION)
            {
                SetParam("Mission", 1);
                //print("starting mission audio");
            }

            fClipped = (float)(player.iNumBulletsNear + player.iNumEnemiesNear) / nearbyEnemyDiv;
            if (fClipped > 1.0f) fClipped = 1.0f;
            SetParam("Enemy", fClipped);
            /*fClipped = (float)((player.iNumBulletsNear + player.iNumEnemiesNear) - nearbyEnemyDiv) / 50.0f;
            if (fClipped < 0) fClipped = 0;
            if (fClipped > 0.20f) fClipped = 0.20f;
            enemyVol = 0.5f + fClipped;*/

            if (Math.Abs(enemyVol - activeEnemyVol) > 0.01f) 
            { 
                SetParam("EnemyVol", enemyVol);
                activeEnemyVol = enemyVol;
            }
        }

        SetVolume();
    }

    void SetParam(string s, float val)
    {
        // do not send out of range values, breaks playback
        float eventValue = Mathf.Clamp01(val);
        main.setParameterByName(s, eventValue);
    }

    void SetVolume()
    {
        main.setVolume(masterVolume);
    }

    public void ResetLife()
    {
        bLifeFadeOut = true;
        StartFade(1.2f, 0.0f, fLifeBeforeZero, 0.0f); //begin at current health, go to 0

        //print("ResetLife, "+ fLifeBeforeZero.ToString()+" -> 0");
    }

}
