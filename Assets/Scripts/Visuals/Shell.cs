using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : Projectile
{

    // Use this for initialization
    /// <summary>
    /// Whether this shell missed.
    /// </summary>
    bool miss = false;
    /// <summary>
    /// Whether the projectile landed in the water.
    /// </summary>
    bool splashed;
    // Update is called once per frame
    /// <summary>
    /// The update function.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        velocity += Vector3.down * GameController.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (transform.position.y < 0f && !splashed && miss)
        {
            GameObject tmp = Instantiate(GameController.waterSplashEffect);
            Vector3 position = gameObject.transform.position;
            position.y = GameController.seaLevel;

            tmp.transform.position = position;
            tmp.transform.parent = weapon.turret.ship.owner.battle.gameObject.transform;

            splashed = true;
        }

        if (travelTimeLeft < -2f)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Sets this shell's velocity.
    /// </summary>
    /// <param name="velocity">Velocity to set.</param>
    public void Launch(Vector3 velocity)
    {
        this.velocity = velocity;
        miss = weapon.turret.ship.owner.battle.recentTurnInformation.hitShips.Count == 0;
        if (!miss)
        {
            miss = weapon.turret.ship.owner.battle.recentTurnInformation.hitShips[0].eliminated && !weapon.turret.ship.owner.battle.recentTurnInformation.sunkShips.Contains(weapon.turret.ship.targetedShip);
        }
    }
}
