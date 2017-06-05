using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialViewActionShotModule : FleetAttackFormationBaseModule
{
    /// <summary>
    /// The time needed for the fired shells to land.
    /// </summary>
    float timeNeeded = 0f;

    /// <summary>
    /// Prepares the action view.
    /// </summary>
    public override void Prepare()
    {
        base.Prepare();
        Vector3 fleetPosition = Vector3.zero;
        if (FieldInterface.battle.recentTurnInformation.hitShips.Count > 0)
        {
            if ((GameController.humanPlayers == 1 && !FieldInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
            {
                FieldInterface.battle.recentTurnInformation.hitShips[0].gameObject.SetActive(true);
            }
        }

        if (Mathf.Abs(FieldInterface.battle.defendingPlayer.board.transform.position.z) < Mathf.Abs(FieldInterface.battle.defendingPlayer.board.transform.position.x))
        {
            fleetPosition = FieldInterface.battle.defendingPlayer.board.transform.position - Vector3.right * GameController.playerBoardDistanceFromCenter * Mathf.Sign(FieldInterface.battle.defendingPlayer.board.transform.position.x) * 1.5f;
        }
        else
        {
            fleetPosition = FieldInterface.battle.defendingPlayer.board.transform.position - Vector3.forward * GameController.playerBoardDistanceFromCenter * Mathf.Sign(FieldInterface.battle.defendingPlayer.board.transform.position.z) * 1.5f;
        }

        fleetPosition.y = GameController.seaLevel;

        Vector3 targetCameraPosition = Vector3.Lerp(fleetPosition, FieldInterface.battle.recentTurnInformation.hitTiles[0].transform.position, 0.5f);
        targetCameraPosition.y = 35f;

        Cameraman.TakePosition(new Cameraman.CameraPosition(0.45f, targetCameraPosition, Vector3.right * 90f));

        foreach (Ship ship in attackers)
        {
            //ship.PrepareToFireAt(FieldInterface.battle.defendingPlayer.board.tiles[(int)FieldInterface.battle.recentlyShot.x, (int)FieldInterface.battle.recentlyShot.y].worldPosition, FieldInterface.battle.defendingPlayer.board.tiles[(int)FieldInterface.battle.recentlyShot.x, (int)FieldInterface.battle.recentlyShot.y].containedShip);
            float time = ship.PrepareToFireAt(FieldInterface.battle.recentTurnInformation.hitTiles[0].transform.position, FieldInterface.battle.recentTurnInformation.hitShips[0]);
            timeNeeded = (time > timeNeeded) ? time : timeNeeded;
        }

        if (attackers.Count == 0)
        {
            Actionman.EndActionView();
        }
    }

    /// <summary>
    /// Refreshes the action view.
    /// </summary>
    public override void Refresh()
    {
        base.Refresh();
        switch (stage)
        {
            case 0:
                if (Cameraman.transitionProgress > 98f)
                {
                    foreach (Ship ship in attackers)
                    {
                        ship.Fire();
                    }
                    stage = 1;
                }
                break;
            case 1:
                if (lifetime > 3.5f)
                {
                    Actionman.EndActionView();
                }
                break;
        }
    }
}
