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
    //The amount of ship segments still intact
    public int lengthRemaining;
    //The artillery turrets mounted on this ship
    public Turret[] turrets;
    //Are the turrets placed in reverse?
    public bool reverseTurrets;
    public float turretFiringDelay;
    //The effects on this ship
    List<GameObject> effects;
    //The gun dispersion of this ship
    public float gunDispersion;

    //How much time it will take this ship to sink
    public float sinkTime;
    float sinkTimeLeft;

    //The time it will take for an incoming shell to get here
    float incomingShellTravelTime = -1f;

    void Awake()
    {
        tiles = new Vector2[length];
        effects = new List<GameObject>();
        lengthRemaining = length;
    }


    // Update is called once per frame
    void Update()
    {
        if (eliminated)
        {
            if (sinkTimeLeft < sinkTime)
            {



                FixFireRotation();
            }
            sinkTimeLeft -= Time.deltaTime;
            if (sinkTimeLeft < 0)
            {
                owner.battle.DisableSunkShip(this);
            }
        }

        if (incomingShellTravelTime >= 0)
        {
            incomingShellTravelTime -= Time.deltaTime;
            if (incomingShellTravelTime < 0)
            {
                OnShellHit();
            }
        }

    }

    public void RegisterHit()
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
        incomingShellTravelTime = travelTime;
    }

    public void BeginSinking()
    {
        GameObject effect = Instantiate(GameController.shipExplosion);
        effect.transform.parent = this.transform;
        effect.transform.localPosition = Vector3.zero;
        sinkTimeLeft = sinkTime;
        effects.Add(effect);
    }

    void FixFireRotation()
    {
        foreach (GameObject fire in effects)
        {
            fire.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    void AddFire()
    {
        Vector3 localPosition = new Vector3(0f, 0f, Random.Range(-length / 2f, length / 2f));
        GameObject effect = Instantiate(GameController.shipFire);
        effect.transform.parent = this.transform;
        effect.transform.localPosition = localPosition;

        effects.Add(effect);
    }

    void OnShellHit()
    {
        if (eliminated)
        {
            BeginSinking();
        }

        AddFire();
    }
}
