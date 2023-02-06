using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo : MonoBehaviour
{ 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;
        if (szOtherObject.StartsWith("LandingZone"))
        {
            Physics2D.IgnoreLayerCollision(15, 16, true); //off
        }

        //must exist one impact point

        int iNum = collision.contactCount;
        ContactPoint2D c = collision.GetContact(0);
        float fImpulse = c.normalImpulse * 100.0f;

        Player p = GameLevel.theMap.player;

        if (szOtherObject.CompareTo("Map") == 0 || szOtherObject.StartsWith("Slider") ||
            szOtherObject.CompareTo("Balk") == 0 || szOtherObject.StartsWith("Knapp") ||
            szOtherObject.CompareTo("Barrels") == 0 || szOtherObject.StartsWith("Tree") ||
            szOtherObject.StartsWith("House") || szOtherObject.CompareTo("RadioTower") == 0 ||
            szOtherObject.StartsWith("Enemy") || szOtherObject.StartsWith("Player"))
        {
            if (szOtherObject.StartsWith("Slider") || szOtherObject.StartsWith("Enemy4") || szOtherObject.StartsWith("Enemy5"))
            {
                //loss of cargo (or ship)
                if (p.bNoUnloading)
                {
                    p.fShipHealth -= 8.0f; //instant kill
                }
                else
                {
                    //respawn cargo on landingzone taken from
                    for (int i = p.iCargoNumUsed - 1; i >= 0; i--)
                    {
                        LandingZone oZone = GameLevel.theMap.GetLandingZone(p.aHoldZoneId[i]);
                        oZone.PushCargo(p.aHold[i]);
                    }
                    //reset cargo
                    p.cargo0rb.mass = 0.1f;
                    p.cargo0cf.force = GameLevel.theMap.vGravity * p.cargo0rb.mass * Player.fGravityScale;
                    p.iCargoNumUsed = p.iCargoSpaceUsed = 0;
                }
            }
            else
            {
                //damage to cargo

                //minimum impulse to damage
                fImpulse -= 24.0f;
                if (fImpulse <= 0) fImpulse = 0.0f;

                for (int i = p.iCargoNumUsed - 1; i >= 0; i--)
                {
                    p.aHoldHealth[i] -= (fImpulse / 80.0f) * 0.06f;
                    if (p.aHoldHealth[i] < 0.5f) p.aHoldHealth[i] = 0.5f; //limit to 50%
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;
        if (szOtherObject.StartsWith("LandingZone"))
        {
            Physics2D.IgnoreLayerCollision(15, 16, false); //on
        }
    }

    void FixedUpdate()
    {
    }
    
}
