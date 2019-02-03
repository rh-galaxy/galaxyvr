using System;
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
    public float fDirection;

    //KEY_CHANGE
    public byte iKeyFlag; //player keyboardstate at sendtime

    public byte iGeneralByte1;
    public byte iGeneralByte2;
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

    public bool LoadFromMem(byte[] i_pMem)
    {
        Reset();

        if (i_pMem.Length % 32 != 0) return false; //not an array containing elements with size 32 bytes
        int iOffset = 0;

        for (int i = 0; i < i_pMem.Length/32; i++)
        {
            ReplayMessage stAction = new ReplayMessage();

            stAction.iTimeSlot = BitConverter.ToInt32(i_pMem, iOffset + 0);
            stAction.iType = (byte)BitConverter.ToChar(i_pMem, iOffset + 4);
            stAction.iKeyFlag = (byte)BitConverter.ToChar(i_pMem, iOffset + 5);
            stAction.iGeneralByte1 = (byte)BitConverter.ToChar(i_pMem, iOffset + 6);
            stAction.iGeneralByte2 = (byte)BitConverter.ToChar(i_pMem, iOffset + 7);
            stAction.iID = BitConverter.ToInt32(i_pMem, iOffset + 8);

            //scale down the ints by 2^18 to get floats again,
            // we never have a float bigger that +-8192.0 so
            stAction.vPos.x = BitConverter.ToInt32(i_pMem, iOffset + 12) / (65536 * 4);
            stAction.vPos.y = BitConverter.ToInt32(i_pMem, iOffset + 16) / (65536 * 4);
            stAction.vVel.x = BitConverter.ToInt32(i_pMem, iOffset + 20) / (65536 * 4);
            stAction.vVel.y = BitConverter.ToInt32(i_pMem, iOffset + 24) / (65536 * 4);
            stAction.fDirection = BitConverter.ToInt32(i_pMem, iOffset + 28) / (65536 * 4);

            iOffset += 32;
            oReplayMessages.Add(stAction);
        }
        return true;
    }
    public byte[] SaveToMem()
    {
        int iCount = oReplayMessages.Count;
        byte[] data = new byte[iCount*32];

        int iOffs = 0;
        for (int i=0; i< iCount; i++)
        {
            ReplayMessage stAction = oReplayMessages[i];

            byte[] b = BitConverter.GetBytes(stAction.iTimeSlot);
            System.Array.Copy(BitConverter.GetBytes(stAction.iTimeSlot), 0, data, iOffs + 0, 4);
            System.Array.Copy(BitConverter.GetBytes(stAction.iType), 0, data, iOffs + 4, 1);
            System.Array.Copy(BitConverter.GetBytes(stAction.iKeyFlag), 0, data, iOffs + 5, 1);
            System.Array.Copy(BitConverter.GetBytes(stAction.iGeneralByte1), 0, data, iOffs + 6, 1);
            System.Array.Copy(BitConverter.GetBytes(stAction.iGeneralByte2), 0, data, iOffs + 7, 1);
            System.Array.Copy(BitConverter.GetBytes(stAction.iID), 0, data, iOffs + 8, 4);

            //scale floats with 2^18 to get ints that will fit in 32 bit,
            // we never have a float bigger that +-8192.0 so
            System.Array.Copy(BitConverter.GetBytes(stAction.vPos.x * (65536 * 4)), 0, data, iOffs + 12, 4);
            System.Array.Copy(BitConverter.GetBytes(stAction.vPos.y * (65536 * 4)), 0, data, iOffs + 16, 4);
            System.Array.Copy(BitConverter.GetBytes(stAction.vVel.x * (65536 * 4)), 0, data, iOffs + 20, 4);
            System.Array.Copy(BitConverter.GetBytes(stAction.vVel.y * (65536 * 4)), 0, data, iOffs + 24, 4);
            System.Array.Copy(BitConverter.GetBytes(stAction.fDirection * (65536 * 4)), 0, data, iOffs + 28, 4);
            iOffs += 32;
        }
        return data;
    }
}
