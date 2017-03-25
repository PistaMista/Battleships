using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    /// <summary>
    /// Whether the weapon is in the process of firing.
    /// </summary>
    protected bool firing = false;
    /// <summary>
    /// The turret this weapon is mounted in.
    /// </summary>
    public Turret turret;
    /// <summary>
    /// The current elevation angle of this weapon - some weapons may not use this property.
    /// </summary>
    protected float currentElevationAngle;
    /// <summary>
    /// The start function.
    /// </summary>
    protected virtual void Start()
    {

    }

    /// <summary>
    /// The update function.
    /// </summary>
    protected virtual void Update()
    {

    }
    /// <summary>
    /// Fires this weapon.
    /// </summary>
    public virtual void Fire()
    {
        firing = true;

    }
    /// <summary>
    /// Sets the elevation angle of this weapon.
    /// </summary>
    /// <param name="elevation">The elevation to set.</param>
    public virtual void SetElevation(float elevation)
    {
        currentElevationAngle = elevation;
        elevation = (turret.ship.reverseTurrets) ? -elevation : elevation;
        transform.localRotation = Quaternion.Euler(elevation, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
    }
    /// <summary>
    /// Gets the time required for the projectile to cross distance.
    /// </summary>
    /// <param name="distance">Distance to cross.</param>
    /// <returns>Time it will take for the projectile to cross the distance.</returns>
    public virtual float GetTimeToTarget(float distance)
    {
        return 1f;
    }
    /// <summary>
    /// Prepares this weapon for firing.
    /// </summary>
    public virtual void PrepareForFiring()
    {

    }
}
