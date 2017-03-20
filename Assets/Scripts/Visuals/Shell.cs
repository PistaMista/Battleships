using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{

    // Use this for initialization

    Vector3 velocity;

    // Update is called once per frame
    void Update()
    {
        velocity += Vector3.down * GameController.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (transform.position.y < -2f)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector3 velocity)
    {
        this.velocity = velocity;
    }
}
