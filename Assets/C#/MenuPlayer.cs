using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlayer : MonoBehaviour
{
    public GameObject oShip;
    public ParticleSystem oThruster;
    ParticleSystem.EmissionModule oThrusterEmission;

    //movement
    Rigidbody2D oRb;
    ConstantForce2D oCustomGravity;
    float fAcceleration = 0.0f;
    float fDirection = 90.0f;
    internal const float fGravityScale = 0.045f;

    //ship properties
    const float SHIP_MASS = 4.8f;
    const float SHIP_STEERSPEED = 235.0f; //degree/second
    const float SHIP_THRUST = 1.40f;

    bool bInited = false;
    private void Awake()
    {
        oThrusterEmission = oThruster.emission;
        oThrusterEmission.enabled = false;

        bInited = true;

        oCustomGravity = GetComponent<ConstantForce2D>();
        oRb = GetComponent<Rigidbody2D>();
        oRb.rotation = fDirection; //happens after next FixedUpdate
        //therefore we need to set the transform immediately so that it
        // is in the start position pointing correctly after init
        //all objects must be handled like this
        oRb.transform.eulerAngles = new Vector3(0, 0, fDirection);
        oRb.transform.localPosition = vStartPos;
        oRb.position = vStartPos / 10.0f;
        iCurTarget = UnityEngine.Random.Range(0, TARGETS.Length);

        //Physics2D gravity is set to 0,0 because the ship is the only object affected
        // by gravity, so we set a constant force here instead of having it global
        oRb.drag = 0.68f * 0.85f;
        oRb.mass = SHIP_MASS;
        oCustomGravity.force = new Vector2(0, -7.6f) * oRb.mass * fGravityScale;
    }

    Vector3 vStartPos = new Vector3(-120.0f, -120.0f, 32.0f);
    int iCurTarget = 0;
    Vector2[] TARGETS = {
        new Vector2(-12.0f, -12.0f), new Vector2(12.0f, 12.0f), new Vector2(-12.0f, 12.0f), new Vector2(12.0f, -12.0f),
        new Vector2(-12.0f, 0.0f), new Vector2(12.0f, 0.0f), new Vector2(0.0f, 12.0f), new Vector2(0.0f, -12.0f)};

    bool bThrottle, bLeft, bRight;
    float fMovementTimer = 0.0f;

    ////////////////////////////////////////////////////////////////////////
    //code used in point movement
    struct S_CurState
    {
        public Vector2 vPos;
        public Vector2 vVel;
        public float fPointing;

        public Vector2 l0; //original pos
        public Vector2 l1; //goal

        public int iLastMove;
        public float fTimestep;
    }
    struct S_CurMove
    {
        public S_CurState stValue;
        public float a;
        public float b;
        public float c;
        public float d;
        public float e;
    }

    struct S_Move
    {
        public S_Move(bool i_bLeft, bool i_bRight, bool i_bThrottle)
        {
            bLeft = i_bLeft;
            bRight = i_bRight;
            bThrottle = i_bThrottle;
        }
        public bool bLeft, bRight, bThrottle;
    }
    S_Move[] ALLMOVES =
        {new S_Move(false, false, false), new S_Move(false, false, true), new S_Move(true, false, false),
        new S_Move(true, false, true), new S_Move(false, true, false), new S_Move(false, true, true) };

    int[,] MOVE_TO_MOVE_SCORE = {
        { 0, 100, 100, 100, 100, 100}, //from 0 to 
        { 100, 0, 100, 100, 100, 100}, //from 1 to 
        { 100, 100, 0, 100, 400, 400},
        { 100, 100, 100, 0, 400, 400},
        { 100, 100, 400, 400, 0, 100},
        { 100, 100, 400, 400, 100, 0}
    };

    void GenerateMoves(S_CurState i_stCurrent, float i_fTime, out S_CurMove[] o_stMoves)
    {
        int i;
        o_stMoves = new S_CurMove[6];
        for (i = 0; i < 6; i++)
        {
            S_CurState stNew = i_stCurrent;

            stNew.fPointing -= ALLMOVES[i].bRight ? SHIP_STEERSPEED * i_fTime : 0;
            stNew.fPointing += ALLMOVES[i].bLeft ? SHIP_STEERSPEED * i_fTime : 0;
            if (stNew.fPointing < 0) stNew.fPointing += 360;
            if (stNew.fPointing > 360) stNew.fPointing -= 360;

            float fThrust = ALLMOVES[i].bThrottle ? SHIP_THRUST : 0;

            Vector2 a = new Vector2(Mathf.Cos(stNew.fPointing * (Mathf.PI / 180)) * fThrust, Mathf.Sin(stNew.fPointing * (Mathf.PI / 180)) * fThrust);
            a += oCustomGravity.force;
            a -= stNew.vVel * oRb.drag;

            stNew.vPos += (stNew.vVel * i_fTime) + (0.5f * a * i_fTime * i_fTime);
            stNew.vVel += a * i_fTime;

            o_stMoves[i].stValue = stNew;
            o_stMoves[i].a = Strategy5(stNew);
            o_stMoves[i].b = Strategy1(stNew);
            o_stMoves[i].c = Strategy2(stNew);
            o_stMoves[i].d = Strategy4(stNew);
            o_stMoves[i].e = MOVE_TO_MOVE_SCORE[stNew.iLastMove, i]; // * (1.0f / Strategy5(stNew));
        }
    }

    float[] TIME_FACTOR = { 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 3.0f, 5.0f, 5.0f, 5.0f, 5.0f };
    int EvaluateMoves(S_CurState i_stCurrent, int i_iLevel, bool i_bShallow)
    {
        int i, iMove = 0;
        float fBest = 1000000000, fWorst = -1000000000;
        float[] dTest = new float[6];
        float a, b, c, d, e;
        S_CurMove[] aMoves;
        int iWorst;

        S_CurState stCurrent = i_stCurrent;
        stCurrent.fTimestep = 0.07f * TIME_FACTOR[i_iLevel];
        GenerateMoves(stCurrent, 0.07f * TIME_FACTOR[i_iLevel], out aMoves);

        for (i = 0; i < 6; i++)
        {
            a = aMoves[i].a;
            b = aMoves[i].b;
            c = aMoves[i].c;
            d = aMoves[i].d;
            e = aMoves[i].e;
            //dTest[i] = a * 0.0f + b * 1000.0f + c * 1.0f + d * 10.0f + e * 0.05f;
            dTest[i] = a * 0.0f + b * 10000.0f + c * 2.0f + d * 10.0f + e * 0.1f;

            if (dTest[i] > fWorst)
            {
                fWorst = dTest[i];
                iWorst = i;
            }
        }

        { //full search
            for (i = 0; i < 6; i++)
            {
                //recurse...
                stCurrent = aMoves[i].stValue;
                stCurrent.iLastMove = i;
                if (i_iLevel < /**/1)
                {
                    dTest[i] += EvaluateMoves(stCurrent, i_iLevel + 1, true);
                }

                if (dTest[i] < fBest)
                {
                    fBest = dTest[i];
                    iMove = i;
                }
            }
        }

        if (i_iLevel > 0) return (int)fBest;
        else return iMove;
    }

    //score for how far from the line
    float Strategy0(S_CurState stValues)
    {
        return CheckPoint.PointDistanceToLineSeg(stValues.vPos, stValues.l0, stValues.l1);
    }
    //score for how much closer to goal
    float Strategy1(S_CurState stValues)
    {
        float fDistNow = (stValues.vPos - stValues.l1).magnitude;
        float fDistBefore = ((stValues.vPos - (stValues.vVel * stValues.fTimestep)) - stValues.l1).magnitude;

        float fScore = (fDistNow - fDistBefore); //negative value is closer
        return fScore;
    }
    float CalcLineAngle(Vector2 a, Vector2 b)
    {
        float difx = b.x - a.x;
        float dify = b.y - a.y;

        float fResult = 360.0f - (Mathf.Atan2(-dify, difx) * (180.0f / Mathf.PI));
        if (fResult >= 360.0f) fResult -= 360.0f;
        else if (fResult < 0.0f) fResult += 360.0f;

        return fResult;
    }
    //score for pointing toward goal (more important further from goal)
    float Strategy2(S_CurState stValues)
    {
        float fGoalAngle = CalcLineAngle(stValues.vPos, stValues.l1);
        float fDiff = fGoalAngle - stValues.fPointing;
        if (fDiff < 0) fDiff += 360;
        if (fDiff > 360) fDiff -= 360;
        if (fDiff > 180) fDiff = 360 - fDiff; //the difference of two angles can be max 180.

        float fDistNow = (stValues.vPos - stValues.l1).magnitude;
        if (fDistNow > 1.0f) fDistNow = 1.0f;

        float fScore = fDiff / fDistNow;
        return fScore;
    }
    //score for speed
    float Strategy3(S_CurState stValues)
    {
        float fVel = stValues.vVel.magnitude;

        float fScore = -fVel;
        return fScore;
    }
    //score for speed vs dist to goal
    float Strategy4(S_CurState stValues)
    {
        float fVel = stValues.vVel.magnitude;

        float fDistNow = (stValues.vPos - stValues.l1).magnitude;
        if (fDistNow > 1.0f) fDistNow = 1.0f;

        float fScore = -fVel / (1.0f / fDistNow);
        return fScore;
    }
    //score for dist to goal
    float Strategy5(S_CurState stValues)
    {
        float fDistNow = (stValues.vPos - stValues.l1).magnitude;

        float fScore = fDistNow;
        return fScore;
    }
    ////////////////////////////////////////////////////////////////////////


    Vector2 vForceDir = new Vector2(0, 0);
    int iBestMove = 0;

    void FixedUpdate()
    {
        if (!bInited)
            return;

        //////get input, either from replay or from human player
        {
            bLeft = bRight = false;
            bThrottle = true;

            fMovementTimer += Time.fixedDeltaTime;

            //new move decision
            if (fMovementTimer > 0.07f)
            {
                fMovementTimer = 0;

                if((TARGETS[iCurTarget] - oRb.position).magnitude <0.5f)
                {
                    iCurTarget = UnityEngine.Random.Range(0, TARGETS.Length);
                }

                S_CurState stCurrent;
                stCurrent.vPos = oRb.position;
                stCurrent.vVel = oRb.velocity;
                stCurrent.fPointing = fDirection;
                stCurrent.fTimestep = 0.1f;
                stCurrent.l0 = oRb.position;
                stCurrent.l1 = TARGETS[iCurTarget];
                stCurrent.iLastMove = iBestMove;
                iBestMove = EvaluateMoves(stCurrent, 0, true);

                bThrottle = ALLMOVES[iBestMove].bThrottle;
                bLeft = ALLMOVES[iBestMove].bLeft;
                bRight = ALLMOVES[iBestMove].bRight;
            }
            //keep old move
            else
            {
                bThrottle = ALLMOVES[iBestMove].bThrottle;
                bLeft = ALLMOVES[iBestMove].bLeft;
                bRight = ALLMOVES[iBestMove].bRight;
            }
        }
        //////end of get input

        //////react to input
        {
            //get rotation
            fDirection = oRb.rotation;

            //clipping, we cannot trust that m_oRb.rotation is 0..360 that we rely on
            int iNumCounts = 0;
            if (fDirection < 0)
                iNumCounts = (int)(fDirection / 360.0f) - 1;
            else if (fDirection > 360.0f)
                iNumCounts = (int)(fDirection / 360.0f);
            fDirection -= iNumCounts * 360.0f;

            float fTemp = (fDirection * (Mathf.PI / 180.0f));
            float fSin = Mathf.Sin(fTemp);
            float fCos = Mathf.Cos(fTemp);

            fTemp = (bThrottle) ? SHIP_THRUST : 0.0f;
            if (fTemp != fAcceleration)
            {
                if (fTemp != 0)
                {
                    oThrusterEmission.enabled = true;
                }
                else
                {
                    oThrusterEmission.enabled = false;
                }
            }

            fAcceleration = fTemp;
            vForceDir.x = fCos;
            vForceDir.y = fSin;

            oRb.AddForce(vForceDir * fAcceleration * 3.6f, ForceMode2D.Force);

            //steering
            {
                { //normal steering
                    fDirection -= bRight ? SHIP_STEERSPEED * Time.fixedDeltaTime : 0.0f;
                    fDirection += bLeft ? SHIP_STEERSPEED * Time.fixedDeltaTime : 0.0f;
                }
                //clipping, to try and keep m_oRb.rotation 0..360
                iNumCounts = 0;
                if (fDirection < 0)
                    iNumCounts = (int)(fDirection / 360.0f) - 1;
                else if (fDirection > 360.0f)
                    iNumCounts = (int)(fDirection / 360.0f);
                fDirection -= iNumCounts * 360.0f;

                oRb.MoveRotation(fDirection);
            }
        }
        //////end react to input
    }

}
