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
    //Has this ship been sunk?
    public bool sunk = false;
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

    //Testing just 4fun
    float rotationVelocity = 0f;
    float rotationAcceleration = 30f;


    // Update is called once per frame
    void Update()
    {
        if (sunk)
        {
            if (sinkTimeLeft < sinkTime)
            {
                rotationVelocity += rotationAcceleration * Time.deltaTime;
                transform.Rotate(new Vector3(rotationVelocity * Time.deltaTime, rotationVelocity * Time.deltaTime, rotationVelocity * Time.deltaTime));
            }
            sinkTimeLeft -= Time.deltaTime;
            if (sinkTimeLeft < 0)
            {
                owner.battle.DestroySunkShip(this);
            }
        }
    }

    public void Hit()
    {
        lengthRemaining--;

        if (lengthRemaining == 0)
        {
            sunk = true;
            sinkTimeLeft = sinkTime;
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
}
