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
        Vector3 launchPoint = owner.battle.GetTorpedoLaunchPosition();
        foreach (Turret launcher in torpedoLaunchers)
        {
            launcher.PrepareToFireAt(targetPosition);
            int id = 0;
            foreach (TorpedoLauncher tube in launcher.weapons)
            {
                BoardTile hitTile = owner.battle.recentTurnInformation.torpedoInfo.impacts[id];
                Vector3 hitPosition = hitTile.transform.position;
                hitPosition.y = 0;
                if (id + 1 < owner.battle.recentTurnInformation.torpedoInfo.impacts.Count)
                {
                    id++;
                }

                tube.torpedo.targetShip = hitTile.containedShip;
                tube.torpedo.launchDirection = new Vector3(owner.battle.recentTurnInformation.target.x, 0, owner.battle.recentTurnInformation.target.y);
                tube.torpedo.targetDistance = Vector3.Distance(launchPoint, hitPosition);
            }
        }
    }

    /// <summary>
    /// Fires the torpedo launchers.
    /// </summary>
    public void FireTorpedoLaunchers()
    {
        foreach (Turret launcher in torpedoLaunchers)
        {
            launcher.Fire(0f);
        }
    }
}
