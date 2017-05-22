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
    /// How fast the torpedo slows down after being launched.
    /// </summary>
    public float drag;
    /// <summary>
    /// The speed of the torpedo underwater.
    /// </summary>
    public float underWaterVelocity;
    /// <summary>
    /// The speed of the torpedo after being launched.
    /// </summary>
    public float initialVelocity;
    /// <summary>
    /// The current acceleration of the propeller.
    /// </summary>
    float propellerAcceleration;
    /// <summary>
    /// The distance this torpedo should travel before detonating.
    /// </summary> 
    public float targetDistance;
    /// <summary>
    /// The ship this torpedo is going to hit.
    /// </summary>
    public Ship targetShip;
    /// <summary>
    /// The direction the torpedo was launched in.
    /// </summary> 
    public Vector3 launchDirection;
    /// <summary>
    /// Whether this torpedo is underwater.
    /// </summary>
    public bool underwater;

    protected override void Start()
    {
        base.Start();

    }

    protected override void Update()
    {
        base.Update();
        if (travelTimeLeft >= 0)
        {
            currentPropellerSpeed = Mathf.SmoothDamp(currentPropellerSpeed, maxPropellerSpeed, ref propellerAcceleration, propellerSpinUpTime);
            propeller.transform.Rotate(Vector3.forward * currentPropellerSpeed * Time.deltaTime);

            if (transform.position.y > -0.1f)
            {
                velocity -= (launchDirection * drag + Vector3.up * GameController.gravity / 12f) * Time.deltaTime;
                underwater = false;
            }
            else
            {
                velocity = launchDirection * underWaterVelocity;
                if (!underwater)
                {
                    GameObject tmp = Instantiate(GameController.waterSplashEffect);
                    Vector3 position = gameObject.transform.position;
                    position.y = GameController.seaLevel;

                    tmp.transform.position = position;
                    tmp.transform.parent = weapon.turret.ship.owner.battle.gameObject.transform;
                }
                underwater = true;
            }
            transform.position += velocity * Time.deltaTime;
        }
        else
        {
            if (underwater)
            {
                Detonate();
            }
        }
    }

    /// <summary>
    /// Launches the torpedo.
    /// </summary>
    public void Launch()
    {
        float dropTime = Mathf.Sqrt((transform.position.y + 0.1f) / (GameController.gravity / 6f));
        float horizontalTravel = dropTime * (initialVelocity - dropTime * drag / 2f);
        targetDistance -= horizontalTravel - 1;
        travelTimeLeft = dropTime + targetDistance / underWaterVelocity;
        velocity = launchDirection * initialVelocity;
    }

    /// <summary>
    /// Detonates the torpedo.
    /// </summary>
    void Detonate()
    {
        GameObject tmp = Instantiate(GameController.torpedoDetonation);
        Vector3 position = gameObject.transform.position;
        position.y = GameController.seaLevel;

        tmp.transform.position = position;
        tmp.transform.parent = weapon.turret.ship.owner.battle.gameObject.transform;
        Destroy(this.gameObject);
    }
}
