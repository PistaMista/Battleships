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
    void Start()
    {
        defaultAngle = gameObject.transform.localRotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float FireAt(Vector3 worldPosition)
    {
        if (!RotateTo(worldPosition))
        {
            return 0f;
        }



        return 1f;
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
