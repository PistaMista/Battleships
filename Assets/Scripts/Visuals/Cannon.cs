using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : Weapon
{

    // Use this for initialization
    public GameObject barrel;


    float recoilDistance;
    float defaultBarrelPosition;
    float recoverySpeed = 0.3f;

    protected override void Start()
    {
        recoilDistance = barrel.transform.localScale.z * 0.45f;
        defaultBarrelPosition = barrel.transform.localPosition.z;
    }

    // Update is called once per frame
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

    public override void Fire()
    {
        base.Fire();
        barrel.transform.localPosition = new Vector3(0f, 0f, defaultBarrelPosition - recoilDistance * Mathf.Sign(barrel.transform.localPosition.z));
        LaunchShell();
    }

    public override float GetTimeToTarget(float distanceToTarget)
    {
        float time = (2f * turret.shellVelocity * Mathf.Sin(currentElevationAngle)) / GameController.gravity;
        return 1f;
    }

    public override void PrepareForFiring()
    {
        float angle = (Mathf.Asin((turret.distanceToTarget * GameController.gravity) / (turret.shellVelocity * turret.shellVelocity))) * Mathf.Rad2Deg / 2f;
        SetElevation(angle);
    }

    void LaunchShell()
    {
        Shell shell = Instantiate(GameController.cannonShell).GetComponent<Shell>();
        Vector3 direction = -(barrel.transform.position - transform.position).normalized;
        shell.Launch(direction * turret.shellVelocity);
        shell.transform.position = transform.position + direction * barrel.transform.localScale.z;
    }
}
