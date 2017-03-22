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
    //The fire effects on this ship
    List<GameObject> fires;

    //How much time it will take this ship to sink
    public float sinkTime;
    float sinkTimeLeft;
    void Awake()
    {
        tiles = new Vector2[length];
        fires = new List<GameObject>();
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


                FixFireRotation();
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
        fires.Add(effect);

        for (int i = 0; i <= length; i++)
        {
            Vector3 localPosition = new Vector3(0f, 0f, Random.Range(-length / 2f, length / 2f));
            effect = Instantiate(GameController.shipFire);
            effect.transform.parent = this.transform;
            effect.transform.localPosition = localPosition;

            fires.Add(effect);
        }
    }

    void FixFireRotation()
    {
        foreach (GameObject fire in fires)
        {
            fire.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}
