using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    // Use this for initialization
    /// <summary>
    /// The weapon that fired this projectile.
    /// </summary>
    public Weapon weapon;
    /// <summary>
    /// The time it will take for the projectile to reach the target.
    /// </summary>
    float travelTime;
    public float TravelTime
    {
        get { return travelTime; }
        set { travelTime = value; travelTimeLeft = value; }
    }
    /// <summary>
    /// The time left until reaching the target.
    /// </summary>
    public float travelTimeLeft;
    /// <summary>
    /// The velocity of this projectile.
    /// </summary>
    public Vector3 velocity;
    /// <summary>
    /// The type of the projectile.
    /// </summary>
    public TurnType type;
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
        if (travelTimeLeft > 0)
        {
            travelTimeLeft -= Time.deltaTime;
            if (travelTimeLeft <= 0)
            {
                if (onHit != null)
                {
                    onHit(this);
                    onHit = null;
                }
            }
        }
        else
        {
            travelTimeLeft -= Time.deltaTime;
        }
    }
}
