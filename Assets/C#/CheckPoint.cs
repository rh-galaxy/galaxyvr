using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public GameObject oCP1, oCP2;
    public GameObject oCP1Text, oCP2Text;

    Vector2 vPos1;
    Vector2 vPos2;
    int iCPNum;

    bool bBlinkState;
    double dBlinkTimer;


    void Start()
    {

    }

    public void Init(Vector2 i_vPos1, Vector2 i_vPos2, int i_iCPNum)
    {
        iCPNum = i_iCPNum;
        vPos1 = i_vPos1;
        vPos2 = i_vPos2;

        dBlinkTimer = 0;
        bBlinkState = false;

        oCP1.transform.position = new Vector3(vPos1.x, vPos1.y, -1.2f);
        oCP2.transform.position = new Vector3(vPos2.x, vPos2.y, -1.2f);
        oCP1.transform.localScale = new Vector3(20.0f / 32.0f * 1.0f, 16.0f / 32.0f * 1.0f, 1.0f);
        oCP2.transform.localScale = new Vector3(20.0f / 32.0f * 1.0f, 16.0f / 32.0f * 1.0f, 1.0f);

        oCP1Text.transform.position = new Vector3(vPos1.x, vPos1.y, -1.7f);
        oCP2Text.transform.position = new Vector3(vPos2.x, vPos2.y, -1.7f);
        if (iCPNum >= 10)
        {
            oCP1Text.transform.localScale = new Vector3(12.0f / 32.0f * 0.40f, 16.0f / 32.0f * 0.3f, 1.0f);
            oCP2Text.transform.localScale = new Vector3(12.0f / 32.0f * 0.40f, 16.0f / 32.0f * 0.3f, 1.0f);
        }
        else
        {
            oCP1Text.transform.localScale = new Vector3(14.0f / 32.0f * 0.4f, 16.0f / 32.0f * 0.3f, 1.0f);
            oCP2Text.transform.localScale = new Vector3(14.0f / 32.0f * 0.4f, 16.0f / 32.0f * 0.3f, 1.0f);
        }

        oCP1Text.GetComponent<TextMesh>().text = iCPNum.ToString();
        oCP2Text.GetComponent<TextMesh>().text = iCPNum.ToString();

        gameObject.SetActive(true);
    }

    public void FixedUpdate()
    {
        dBlinkTimer += Time.deltaTime;

        //enable/disable text for blinking
        bool bActive = (!bBlinkState || (bBlinkState && ((int)(dBlinkTimer * 1000) % 450 < 300)));

        oCP1.SetActive(bActive);
        oCP2.SetActive(bActive);
        oCP1Text.SetActive(bActive);
        oCP2Text.SetActive(bActive);
    }

    float PointDistance(Vector2 i_vPos1, Vector2 i_vPos2)
    {
        Vector2 vDist = i_vPos1 - i_vPos2;
        return (float)Mathf.Sqrt(vDist.x * vDist.x + vDist.y * vDist.y);
    }
    float PointDistanceToLineSeg(Vector2 p, Vector2 l0, Vector2 l1/*, out Vector2 o_vClosest*/)
    {
        Vector2 l0_l1;
        float dist;

        l0_l1 = l1 - l0;

        float l2 = (l0_l1.x * l0_l1.x + l0_l1.y * l0_l1.y); // i.e. |l0-l1|^2 -  avoid a sqrt

        if (l2 < 0.01)
        {
            //just take l0, the line is too short
            dist = PointDistance(l0, p);
            //if (o_vClosest!=null) o_vClosest = l0;
        }
        else
        {
            //consider the line extending the segment, parameterized as l0 + t (l1 - l0).
            //we find projection of point p onto the line. 
            //it falls where t = [(p-l0) . (l1-l0)] / |l1-l0|^2
            float t = Vector2.Dot(p - l0, l0_l1) / l2;
            if (t < 0.0)
            {
                //beyond the 'l0' end of the segment
                dist = PointDistance(l0, p);
                //if (o_vClosest!=null) o_vClosest = l0;
            }
            else if (t > 1.0f)
            {
                //beyond the 'l0' end of the segment
                dist = PointDistance(l1, p);
                //if (o_vClosest!=null) o_vClosest = l1;
            }
            else
            {
                //projection falls on the segment
                Vector2 projection = l0 + (l0_l1 * t);
                dist = PointDistance(p, projection);
                //if (o_vClosest!=null) o_vClosest = projection;
            }
        }

        return dist;
    }

    /**/const float CP_RANGE = 0.6f;
    Vector2 p = Vector2.zero;
    Vector2 l0 = Vector2.zero;
    Vector2 l1 = Vector2.zero;
    public bool AtCP(Vector2 i_vPos)
    {
        //calculate shortest distance from player to checkpoint line (all points are midpoints)
        //(avoid creating garbage with new)
        p.x = i_vPos.x;
        p.y = i_vPos.y;
        l0.x = vPos1.x;
        l0.y = vPos1.y;
        l1.x = vPos2.x;
        l1.y = vPos2.y;
        double dDist = PointDistanceToLineSeg(p, l0, l1);

        if (dDist < CP_RANGE)
        {
            return true;
        }
        return false;
    }

    public void SetBlinkState(bool i_bBlinkState)
    {
        if (i_bBlinkState && !bBlinkState) dBlinkTimer = 0;
        bBlinkState = i_bBlinkState;
    }

}
