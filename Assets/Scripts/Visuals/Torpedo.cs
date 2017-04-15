using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : Projectile
{
    /// <summary>
    /// The propeller on the back of the torpedo.
    /// </summary>
    public GameObject propeller;

    /// <summary>
    /// The rotation speed of the propeller. (revolutions per second)
    /// </summary>
    float currentPropellerSpeed;
    /// <summary>
    /// The maximum rotation speed of the propeller. (revolutions per second)
    /// </summary>
    public float maxPropellerSpeed;
    /// <summary>
    /// The time it will take to spin up the propeller.
    /// </summary>
    public float propellerSpinUpTime;
    /// <summary>
    /// The current acceleration of the propeller.
    /// </summary>
    float propellerAcceleration;

    protected override void Start()
    {
        base.Start();

    }

    protected override void Update()
    {
        base.Update();
        if (travelTimeLeft != 0)
        {
            currentPropellerSpeed = Mathf.SmoothDamp(currentPropellerSpeed, maxPropellerSpeed, ref propellerAcceleration, propellerSpinUpTime);
            propeller.transform.Rotate(Vector3.forward * currentPropellerSpeed * Time.deltaTime);
        }
    }
}
