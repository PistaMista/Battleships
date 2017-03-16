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
}
