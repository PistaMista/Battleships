using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : Weapon
{

    // Use this for initialization
    /// <summary>
    /// The barrel of the cannon.
    /// </summary>
    public GameObject barrel;

    /// <summary>
    /// The recoil distance of this cannon, that is how far the barrel goes back, when firing.
    /// </summary>
    float recoilDistance;
    /// <summary>
    /// The starting position of the barrel.
    /// </summary>
    float defaultBarrelPosition;
    /// <summary>
    /// The speed at which the recoil recovers.
    /// </summary>
    float recoverySpeed = 0.3f;
    /// <summary>
    /// The effect used to show the muzzle flash of the cannon.
    /// </summary>
    public GameObject muzzleFlashEffect;

    /// <summary>
    /// The start function.
    /// </summary>    
    protected override void Start()
    {
        recoilDistance = barrel.transform.localScale.z * 0.45f;
        defaultBarrelPosition = barrel.transform.localPosition.z;
    }
    /// <summary>
    /// The update function.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (firing)
        {
            float position = barrel.transform.localPosition.z;
            position = Mathf.SmoothDamp(position, defaultBarrelPosition, ref recoverySpeed, 0.35f, 2f);

            barrel.transform.localPosition = new Vector3(0, 0, position);
            if (barrel.transform.localScale.z - position < 0.05f)
            {
                firing = false;
            }
        }
    }
    /// <summary>
    /// Fires the cannon.
    /// </summary>
    /// <returns>The shell that was fired.</returns>
    public override Projectile Fire()
    {
        base.Fire();
        barrel.transform.localPosition = new Vector3(0f, 0f, defaultBarrelPosition - recoilDistance * Mathf.Sign(barrel.transform.localPosition.z));
        Shell shell = LaunchShell();
        if (turret.ship.targetedShip != null)
        {
            turret.ship.targetedShip.IncomingProjectile(shell);
        }

        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.GetComponent<ParticleSystem>().Play();
        }

        return shell;
    }
    /// <summary>
    /// Gets the time needed for the shell to reach the target.
    /// </summary>
    /// <param name="distanceToTarget">The distance of the target.</param>
    /// <returns>Time needed for the shell to cross the distance to target.</returns>
    public override float GetTimeToTarget(float distanceToTarget)
    {
        //float time = (2f * turret.projectileVelocity * Mathf.Sin(currentElevationAngle * Mathf.Deg2Rad)) / GameController.gravity;
        float pAngle = currentElevationAngle * Mathf.Deg2Rad;
        float velocitySin = turret.projectileVelocity * Mathf.Sin(pAngle);
        float time = (velocitySin + Mathf.Sqrt(Mathf.Pow(velocitySin, 2f) + 2f * GameController.gravity * (0.55f))) / GameController.gravity;
        return time;
    }
    /// <summary>
    /// Prepares the cannon for firing.
    /// </summary>
    public override void PrepareForFiring()
    {
        float altitudeDifference = (GameController.seaLevel - transform.position.y);
        //float angle = (Mathf.Asin((turret.distanceToTarget * GameController.gravity) / (turret.projectileVelocity * turret.projectileVelocity))) * Mathf.Rad2Deg / 2f;
        float angle = Mathf.Atan((Mathf.Pow(turret.projectileVelocity, 2f) - Mathf.Sqrt(Mathf.Pow(turret.projectileVelocity, 4f) - GameController.gravity * (GameController.gravity * Mathf.Pow(turret.distanceToTarget, 2f) + 2 * altitudeDifference * Mathf.Pow(turret.projectileVelocity, 2f)))) / (GameController.gravity * turret.distanceToTarget)) * Mathf.Rad2Deg;
        SetElevation(angle);
    }

    /// <summary>
    /// Launches a shell from the tip of the barrel.
    /// </summary>
    /// <returns>The shell that was launched.</returns>
    Shell LaunchShell()
    {
        Shell shell = Instantiate(GameController.cannonShell).GetComponent<Shell>();
        Vector3 direction = -(barrel.transform.position - transform.position).normalized;

        float dispersionValue = turret.ship.gunDispersion;

        Vector3 dispersion = new Vector3(Random.Range(0f, dispersionValue), Random.Range(0f, dispersionValue), Random.Range(0f, dispersionValue));
        direction = (direction + dispersion).normalized;
        shell.transform.parent = turret.ship.owner.battle.transform;
        shell.transform.position = transform.position + direction * barrel.transform.localScale.z;
        shell.TravelTime = GetTimeToTarget(turret.distanceToTarget);
        shell.type = AttackType.SHELL;
        shell.weapon = this;
        shell.Launch(direction * turret.projectileVelocity);
        return shell;
    }

    public override void SetElevation(float elevation)
    {
        base.SetElevation(elevation);
        turret.gunDirection = -(transform.position - barrel.transform.position).normalized;
    }
}
