using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{

    /// <summary>
    /// The weapons mounted in this turret.
    /// </summary>
    public Weapon[] weapons;
    /// <summary>
    /// The ship this turret is mounted on.
    /// </summary>
    public Ship ship;
    /// <summary>
    /// The angular distance the turret is capable of traversing to the left.
    /// </summary>
    public int leftTraverseLimit;
    /// <summary>
    /// The angular distance the turret is capable of traversing to the right.
    /// </summary>
    public int rightTraverseLimit;
    /// <summary>
    /// The velocity of the projectiles launched by the weapons of this turret.
    /// </summary>
    public float projectileVelocity;
    /// <summary>
    /// The default rotation of this turret.
    /// </summary>
    public float defaultRotation;
    /// <summary>
    /// The number of the weapon, that is currently being fired.
    /// </summary>
    int currentlyFiring = 999;
    /// <summary>
    /// The delay between each weapon firing.
    /// </summary>
    public float firingDelay;
    /// <summary>
    /// Whether the turret is done firing.
    /// </summary>
    bool doneFiring = true;

    //The info about the trajectory
    /// <summary>
    /// The time it will take for all projectiles to reach the target.
    /// </summary>
    public float projectileTravelTime;
    /// <summary>
    /// The distance of the target.
    /// </summary>
    public float distanceToTarget;

    /// <summary>
    /// Whether this turret can fire at the currently targeted location.
    /// </summary>
    public bool canFire;

    /// <summary>
    /// The start function.
    /// </summary>
    void Start()
    {
        defaultRotation = gameObject.transform.localRotation.eulerAngles.y;
    }

    // Update is called once per frame
    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        if (firingDelay <= 0)
        {
            if (currentlyFiring < weapons.Length)
            {
                weapons[currentlyFiring].Fire();

                currentlyFiring++;
                firingDelay = 0.2f;
            }
            else if (!doneFiring)
            {
                currentlyFiring = 0;
                firingDelay = 0.1f;
                doneFiring = true;
            }
        }

        firingDelay -= Time.deltaTime;
    }

    /// <summary>
    /// Fires all weapons at worldPosition.
    /// </summary>
    /// <param name="worldPosition">The position to fire at.</param>
    /// <param name="firingDelay">The delay before starting to fire.</param>
    /// <returns>The time it will take for projectiles to arrive at the target position.</returns>
    public float FireAt(Vector3 worldPosition, float firingDelay)
    {
        distanceToTarget = Vector3.Distance(ship.transform.position, worldPosition);
        if (RotateTo(worldPosition))
        {
            this.firingDelay = firingDelay;
            doneFiring = false;


            foreach (Weapon weapon in weapons)
            {
                weapon.PrepareForFiring();
            }

            projectileTravelTime = weapons[0].GetTimeToTarget(distanceToTarget);
            canFire = true;
            return projectileTravelTime + firingDelay;
        }
        else
        {
            canFire = false;
            return 0f;
        }
    }
    /// <summary>
    /// Rotates the turret to point at the target world position.
    /// </summary>
    /// <param name="targetPosition">The position to rotate towards.</param>
    /// <returns>Whether the turret is able to point at the target position.</returns>    
    bool RotateTo(Vector3 targetPosition)
    {
        Vector3 relativeTargetPosition = targetPosition - transform.position;
        float relativeTargetAngle = Mathf.Atan2(relativeTargetPosition.z, relativeTargetPosition.x) * Mathf.Rad2Deg + ship.transform.rotation.eulerAngles.y - defaultRotation + 90f;

        if (ship.reverseTurrets)
        {
            relativeTargetAngle += 180f;
        }

        if (Mathf.Abs(relativeTargetAngle) > 180f)
        {
            relativeTargetAngle -= 360f * Mathf.Sign(relativeTargetAngle);
        }
        if ((relativeTargetAngle <= 0 && Mathf.Abs(relativeTargetAngle) <= rightTraverseLimit) || (relativeTargetAngle >= 0 && Mathf.Abs(relativeTargetAngle) <= leftTraverseLimit))
        {
            transform.rotation = Quaternion.Euler(0f, defaultRotation - relativeTargetAngle + ship.transform.eulerAngles.y, 0f);
            return true;
        }
        else
        {
            return false;
        }
    }
}
