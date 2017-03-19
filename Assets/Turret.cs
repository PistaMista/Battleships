using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{

    // Use this for initialization
    public Cannon[] cannons;
    public Ship ship;
    public int leftTraverseLimit;
    public int rightTraverseLimit;
    public float shellVelocity;
    public float defaultAngle;
    int currentlyFiring = 999;
    public float firingDelay;
    bool doneFiring;
    void Start()
    {
        defaultAngle = gameObject.transform.localRotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (firingDelay <= 0)
        {
            if (currentlyFiring < cannons.Length)
            {
                cannons[currentlyFiring].Fire();

                currentlyFiring++;
                firingDelay = 0.5f;
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

    public float FireAt(Vector3 worldPosition, float firingDelay)
    {
        if (RotateTo(worldPosition))
        {
            this.firingDelay = firingDelay;
            doneFiring = false;
            return 1f;
        }
        else
        {
            return 0f;
        }
    }

    bool RotateTo(Vector3 targetPosition)
    {
        Vector3 relativeTargetPosition = targetPosition - transform.position;
        float relativeTargetAngle = Mathf.Atan2(relativeTargetPosition.z, relativeTargetPosition.x) * Mathf.Rad2Deg + ship.transform.rotation.eulerAngles.y - defaultAngle + 90f;

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
            transform.rotation = Quaternion.Euler(0f, defaultAngle - relativeTargetAngle + ship.transform.eulerAngles.y, 0f);
            return true;
        }
        else
        {
            return false;
        }
    }
}
