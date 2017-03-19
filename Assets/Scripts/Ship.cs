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

    void Awake()
    {
        tiles = new Vector2[length];
        lengthRemaining = length;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Hit()
    {
        lengthRemaining--;

        if (lengthRemaining == 0)
        {
            sunk = true;
            owner.ShipSunk(this);
        }
    }

    public float FireAt(Vector3 worldPosition)
    {
        float highestTravelTime = 0f;
        foreach (Turret turret in turrets)
        {
            float travelTime = turret.FireAt(worldPosition);

            if (travelTime > highestTravelTime)
            {
                highestTravelTime = travelTime;
            }
        }

        return highestTravelTime;
    }
}
