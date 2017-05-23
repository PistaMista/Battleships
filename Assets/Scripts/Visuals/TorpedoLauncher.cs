using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoLauncher : Weapon
{
    /// <summary>
    /// The point at which the torpedo should be spawned when reloading the tube.
    /// </summary>
    public Transform torpedoSummonPoint;
    /// <summary>
    /// The torpedo currently loaded in the tube.
    /// </summary>
    public Torpedo torpedo;

    /// <summary>
    /// Prepares the launcher for firing.
    /// </summary>
    public override void PrepareForFiring()
    {
        base.PrepareForFiring();
        if (torpedo == null)
        {
            torpedo = Instantiate(GameController.torpedo).GetComponent<Torpedo>();
            torpedo.transform.position = torpedoSummonPoint.position;
            torpedo.transform.rotation = torpedoSummonPoint.rotation;
            torpedo.transform.Rotate(Vector3.up * 90f);
            torpedo.transform.parent = turret.ship.owner.battle.transform;
            torpedo.weapon = this;
        }
    }

    /// <summary>
    /// Fires the torpedo launcher.
    /// </summary>
    public override Projectile Fire()
    {
        if (torpedo.targetShip)
        {
            torpedo.targetShip.IncomingProjectile(torpedo);
        }
        else
        {
            torpedo.targetDistance = 30f;
        }
        torpedo.Launch();


        Torpedo firedTorpedo = torpedo;
        torpedo = null;
        return firedTorpedo;
    }
}
