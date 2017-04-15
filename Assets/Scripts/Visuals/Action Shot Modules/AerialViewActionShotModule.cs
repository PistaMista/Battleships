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
        if (BattleInterface.battle.recentAttackInfo.hitShip != null)
        {
            if ((GameController.humanPlayers == 1 && !BattleInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
            {
                BattleInterface.battle.recentAttackInfo.hitShip.gameObject.SetActive(true);
            }
        }

        if (Mathf.Abs(BattleInterface.battle.defendingPlayer.board.position.z) < Mathf.Abs(BattleInterface.battle.defendingPlayer.board.position.x))
        {
            fleetPosition = BattleInterface.battle.defendingPlayer.board.position - Vector3.right * GameController.playerBoardDistanceFromCenter * Mathf.Sign(BattleInterface.battle.defendingPlayer.board.position.x) * 1.5f;
        }
        else
        {
            fleetPosition = BattleInterface.battle.defendingPlayer.board.position - Vector3.forward * GameController.playerBoardDistanceFromCenter * Mathf.Sign(BattleInterface.battle.defendingPlayer.board.position.z) * 1.5f;
        }

        fleetPosition.y = GameController.seaLevel;

        Vector3 targetCameraPosition = Vector3.Lerp(fleetPosition, BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition, 0.5f);
        targetCameraPosition.y = 35f;

        Cameraman.TakePosition(new Cameraman.CameraPosition(0.45f, targetCameraPosition, Vector3.right * 90f));

        foreach (Ship ship in attackers)
        {
            //ship.PrepareToFireAt(BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition, BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].containedShip);
            float time = ship.PrepareToFireAt(BattleInterface.battle.recentAttackInfo.attackedTileWorldPosition, BattleInterface.battle.recentAttackInfo.hitShip);
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
