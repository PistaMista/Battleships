using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : Projectile
{

    // Use this for initialization

    // Update is called once per frame
    /// <summary>
    /// The update function.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        velocity += Vector3.down * GameController.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (travelTimeLeft < 0f)
        {
            bool condition = weapon.turret.ship.owner.battle.recentAttackInfo.hitShip == null;


            if (condition)
            {
                GameObject tmp = Instantiate(GameController.waterSplashEffect);
                Vector3 position = gameObject.transform.position;
                position.y = GameController.seaLevel;

                tmp.transform.position = position;
                tmp.transform.parent = weapon.turret.ship.owner.battle.gameObject.transform;
            }
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
    }
}
