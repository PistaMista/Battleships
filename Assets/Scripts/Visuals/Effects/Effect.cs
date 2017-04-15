using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    /// <summary>
    /// The time before destroying the effect object.
    /// </summary>
    public float dissipationTime = 1f;
    /// <summary>
    /// The time this object was alive for.
    /// </summary>
    float lifetime = 0f;
    // Use this for initialization
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime > dissipationTime)
        {
            Destroy(gameObject);
        }
        lifetime += Time.deltaTime;
    }
}
