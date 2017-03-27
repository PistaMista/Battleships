using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    /// <summary>
    /// The owner of this ship.
    /// </summary>
    public Player owner;
    /// <summary>
    /// The length of this ship.
    /// </summary>
    public int length = 3;
    /// <summary>
    /// The positions on the board that this ship occupies.
    /// </summary>
    public Vector2[] tiles;
    /// <summary>
    /// Whether this ship has been eliminated.
    /// </summary>
    public bool eliminated = false;
    /// <summary>
    /// The number of ship segments still intact.
    /// </summary>
    public int lengthRemaining;
    /// <summary>
    /// The weapon turrets mounted on this ship.
    /// </summary>
    public Turret[] turrets;
    /// <summary>
    /// Whether the turrets are placed in reverse.
    /// </summary>
    public bool reverseTurrets;
    /// <summary>
    /// Delay between each of the turrets firing.
    /// </summary>
    public float turretFiringDelay;
    /// <summary>
    /// The effects on this ship.
    /// </summary>
    List<GameObject> effects;
    /// <summary>
    /// The dispersion of this ship's guns.
    /// </summary>
    public float gunDispersion;

    /// <summary>
    /// The time it will take for this ship to sink.
    /// </summary>
    public float sinkTime;
    /// <summary>
    /// The sinking time left.
    /// </summary>
    float sinkTimeLeft;

    /// <summary>
    /// The time it will take for incoming projectiles to arrive. Deprecated.
    /// </summary>
    float incomingProjectileTravelTime = -1f;
    /// <summary>
    /// The type of incoming projectile. Deprecated.
    /// </summary>
    ProjectileType incomingProjectileDamageType;

    /// <summary>
    /// The first projectile which is going to hit the ship.
    /// </summary>
    Projectile incomingProjectile;

    /// <summary>
    /// The world position of this ship's place on the playing board.
    /// </summary>
    public Vector3 boardPosition;
    /// <summary>
    /// The world rotation of this ship's place on the playing board.
    /// </summary>
    public Vector3 boardRotation;
    /// <summary>
    /// The ship, that is currently getting shot at by this ship.
    /// </summary>
    public Ship targetedShip;
    /// <summary>
    /// The awake function.
    /// </summary> 
    void Awake()
    {
        tiles = new Vector2[length];
        effects = new List<GameObject>();
        lengthRemaining = length;
    }

    /// <summary>
    /// The update function.
    /// </summary>
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
    }
    /// <summary>
    /// Registers a hit on this ship.
    /// </summary>
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
    /// <summary>
    /// Fires the ship's guns at the target world position.
    /// </summary>
    /// <param name="worldPosition">The world position to target.</param>
    /// <returns>The time it will take for the shells to arrive at the target position.</returns>
    public float FireAt(Vector3 worldPosition, Ship targetedShip)
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

        this.targetedShip = targetedShip;
        return highestTravelTime;
    }
    /// <summary>
    /// Informs this ship about incoming projectiles.
    /// </summary>
    /// <param name="projectile">The incoming projectile.</param> 
    public void IncomingProjectile(Projectile projectile)
    {
        if (incomingProjectile == null)
        {
            incomingProjectile = projectile;
            projectile.onHit += OnProjectileHit;
        }
    }

    /// <summary>
    /// Begins the sinking effect.
    /// </summary>
    public void BeginSinking()
    {
        GameObject effect = Instantiate(GameController.shipExplosion);
        effect.transform.parent = this.transform;
        effect.transform.localPosition = Vector3.zero;
        sinkTimeLeft = sinkTime;
        effects.Add(effect);
    }
    /// <summary>
    /// Fixes the rotation of fires during sinking.
    /// </summary>
    void FixFireRotation()
    {
        foreach (GameObject fire in effects)
        {
            fire.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
    /// <summary>
    /// Randomly starts a fire on the ship.
    /// </summary>
    void AddFire()
    {
        Vector3 localPosition = new Vector3(0f, 0f, Random.Range(-length / 2f, length / 2f));
        GameObject effect = Instantiate(GameController.shipFire);
        effect.transform.parent = this.transform;
        effect.transform.localPosition = localPosition;

        effects.Add(effect);
    }
    /// <summary>
    /// Executed when a shell hits the ship.
    /// </summary>
    void OnProjectileHit(Projectile projectile)
    {
        switch (projectile.type)
        {
            case ProjectileType.SHELL:
                if (eliminated)
                {
                    BeginSinking();
                }

                AddFire();
                break;
        }
    }

    /// <summary>
    /// Positions the ship on the playing board.
    /// </summary>
    public void PositionOnPlayingBoard()
    {
        transform.position = boardPosition;
        transform.rotation = Quaternion.Euler(boardRotation);
    }
}
