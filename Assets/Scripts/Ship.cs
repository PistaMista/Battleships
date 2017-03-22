using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    //The owner of this Ship
    public Player owner;
    //The length of this Ship
    public int length = 3;
    //The positions on the board that this ship occupies
    public Vector2[] tiles;
    //Has this ship been eliminated?
    public bool eliminated = false;
    //Is this ship sinking?
    bool sinking = false;
    //The amount of ship segments still intact
    public int lengthRemaining;
    //The artillery turrets mounted on this ship
    public Turret[] turrets;
    //Are the turrets placed in reverse?
    public bool reverseTurrets;
    public float turretFiringDelay;

    //How much time it will take this ship to sink
    public float sinkTime;
    float sinkTimeLeft;
    void Awake()
    {
        tiles = new Vector2[length];
        lengthRemaining = length;
    }


    // Update is called once per frame
    void Update()
    {
        if (eliminated)
        {
            if (sinkTimeLeft < sinkTime)
            {
                if (!sinking)
                {
                    Explode();
                }

                sinking = true;


            }
            sinkTimeLeft -= Time.deltaTime;
            if (sinkTimeLeft < 0)
            {
                owner.battle.DisableSunkShip(this);
            }
        }
    }

    public void Hit()
    {
        lengthRemaining--;

        if (lengthRemaining == 0)
        {
            eliminated = true;
            sinkTimeLeft = Mathf.Infinity;
            owner.ShipSunk(this);
        }
    }

    public float FireAt(Vector3 worldPosition)
    {
        float highestTravelTime = 0f;
        float currentDelay = 0f;

        foreach (Turret turret in turrets)
        {
            float travelTime = turret.FireAt(worldPosition, currentDelay);

            if (travelTime > highestTravelTime)
            {
                highestTravelTime = travelTime;
            }

            currentDelay += turretFiringDelay;
        }

        return highestTravelTime;
    }

    public void InformAboutShellTravelTime(float travelTime)
    {
        if (eliminated)
        {
            sinkTimeLeft = sinkTime + travelTime;
        }
    }

    void Explode()
    {
        GameObject effect = Instantiate(GameController.shipExplosion);
        effect.transform.parent = this.transform;
        effect.transform.localPosition = Vector3.zero;

    }
}
