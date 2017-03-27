using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    // Use this for initialization
    /// <summary>
    /// The time it will take for the projectile to reach the target.
    /// </summary>
    public float travelTime;
    /// <summary>
    /// The type of the projectile.
    /// </summary>
    public ProjectileType type;
    /// <summary>
    /// Executed when the projectile hits the target.
    /// </summary>
    /// <param name="projectile">The projectile that hit.</param>
    public delegate void OnHit(Projectile projectile);

    /// <summary>
    /// Executed when the projectile hits the target.
    /// </summary>
    public OnHit onHit;
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
        if (travelTime >= 0)
        {
            travelTime -= Time.deltaTime;
            if (travelTime < 0)
            {
                if (onHit != null)
                {
                    onHit(this);
                }
            }
        }
    }
}
