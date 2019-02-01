using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MsgType { MOVEMENT, KEY_CHANGE, BULLETE_NEW, BULLETP_NEW, BULLET_REMOVE, PLAYER_KILL }; //(DOOR_ACTION, ...)

public struct ReplayMessage
{
    public int iTimeSlot;

    public byte iType;
    public int iID; //id of bullet, id of enemy, or 0 for player

    //MOVEMENT, set every time for movable objects
    public Vector2 vPos;
    public Vector2 vVel;
    //public float fAcceleration;
    public float fDirection;

    //KEY_CHANGE
    public byte iKeyFlag;  //player keyboardstate at sendtime

    public byte iGeneralByte1;
    public byte iGeneralByte2;
    public byte iGeneralByte3;
}

public class Replay
{
    int iCurTimeSlot;
    //int iCurGetPos;

    List<ReplayMessage> oReplayMessages;
    int[] aiCurGetPosForID = new int[256];

    public Replay()
    {
        oReplayMessages = new List<ReplayMessage>();
        Reset();
    }

    public void Reset()
    {
        oReplayMessages.Clear();
        ResetBeforePlay();
    }
    public void ResetBeforePlay()
    {
        iCurTimeSlot = 0;
        //iCurGetPos = 0;
        for(int i=0; i< aiCurGetPosForID.Length; i++) aiCurGetPosForID[i] = 0;
    }
    public void IncTimeSlot() //used both in save and replay
    {
        iCurTimeSlot++;
    }
    public void Add(ReplayMessage i_stAction) //must be added in timeslot order (uses iCurTimeSlot as timeslot), save only
    {
        i_stAction.iTimeSlot = iCurTimeSlot;
        oReplayMessages.Add(i_stAction);
    }

    public bool Get(out ReplayMessage o_stAction, int i_iID) //returns the first action not yet returned, if any (based on iCurTimeSlot), replay only
    {
        int iSize = oReplayMessages.Count;
        if (aiCurGetPosForID[i_iID] < iSize)
        {
            ReplayMessage stAction = oReplayMessages[aiCurGetPosForID[i_iID]];
            while (stAction.iTimeSlot <= iCurTimeSlot)
            {
                if (stAction.iID == i_iID)
                {
                    //do it as fast as possible if less than current (never skip actions)
                    aiCurGetPosForID[i_iID]++;
                    o_stAction = stAction;
                    return true;
                }
                aiCurGetPosForID[i_iID]++;
                if (aiCurGetPosForID[i_iID] >= iSize) break;
                stAction = oReplayMessages[aiCurGetPosForID[i_iID]];
            }
        }
        o_stAction = new ReplayMessage(); //this will never be used
        return false;
    }

/*
    public bool LoadFromMem(byte[] i_pMem)
    {

    }
    public byte[] SaveToMem()
    {

    }
    */
}
