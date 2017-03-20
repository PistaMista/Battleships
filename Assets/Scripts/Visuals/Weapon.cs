using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{


    protected bool firing = false;
    public Turret turret;

    protected float currentElevationAngle;
    // Use this for initialization
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    public virtual void Fire()
    {
        firing = true;

    }

    public virtual void SetElevation(float elevation)
    {
        currentElevationAngle = elevation;
        elevation = (turret.ship.reverseTurrets) ? -elevation : elevation;
        transform.localRotation = Quaternion.Euler(elevation, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
    }

    public virtual float GetTimeToTarget(float distance)
    {
        return 1f;
    }

    public virtual void PrepareForFiring()
    {

    }
}
