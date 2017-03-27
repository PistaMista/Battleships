using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : Projectile
{

    // Use this for initialization
    /// <summary>
    /// The velocity of the shell.
    /// </summary>
    Vector3 velocity;

    // Update is called once per frame
    /// <summary>
    /// The update function.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        velocity += Vector3.down * GameController.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (transform.position.y < -2f)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Sets this shell's velocity.
    /// </summary>
    /// <param name="velocity">Velocity to set.</param>
    public void Launch(Vector3 velocity)
    {
        this.velocity = velocity;
    }
}
