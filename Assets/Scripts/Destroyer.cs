using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : Ship
{
    /// <summary>
    /// All the torpedo launchers mounted on this ship.
    /// </summary>
    public Turret[] torpedoLaunchers;

    /// <summary>
    /// Prepares the torpedo launcher for firing.
    /// </summary>
    public void PrepareTorpedoLaunchers(Vector3 targetPosition)
    {
        foreach (Turret launcher in torpedoLaunchers)
        {
            launcher.PrepareToFireAt(targetPosition);
        }
    }
}
