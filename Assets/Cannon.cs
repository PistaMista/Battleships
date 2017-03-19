using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{

    // Use this for initialization
    public GameObject barrel;

    bool firing = false;

    float recoilDistance;
    float defaultBarrelPosition;
    float recoverySpeed = 0.3f;

    void Start()
    {
        recoilDistance = barrel.transform.localScale.z * 0.45f;
        defaultBarrelPosition = barrel.transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
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

    public void Fire()
    {
        barrel.transform.localPosition = new Vector3(0f, 0f, defaultBarrelPosition - recoilDistance * Mathf.Sign(barrel.transform.localPosition.z));
        firing = true;
    }
}
