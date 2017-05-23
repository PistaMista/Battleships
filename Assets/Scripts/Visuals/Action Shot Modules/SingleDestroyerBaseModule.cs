using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleDestroyerBaseModule : ActionShotModule
{
    /// <summary>
    /// The destroyer which was selected for use in this action shot.
    /// </summary>
    protected Destroyer selectedDestroyer;
    /// <summary>
    /// Whether the torpedoes were fired or not.
    /// </summary>
    bool fired;
    /// <summary>
    /// Prepares the action shot.
    /// </summary>
    public override void Prepare()
    {
        base.Prepare();

        foreach (Ship ship in BattleInterface.battle.attackingPlayer.livingShips)
        {
            if (ship.type == ShipType.DESTROYER && ship.lengthRemaining == ship.length)
            {
                selectedDestroyer = (Destroyer)ship;
                break;
            }
        }

        Vector3 position = BattleInterface.battle.GetTorpedoLaunchPosition();
        bool alternateOrientation = (Mathf.Abs(position.z - BattleInterface.battle.defendingPlayer.board.transform.position.z) > 1);
        selectedDestroyer.transform.rotation = Quaternion.Euler(new Vector3(0, alternateOrientation ? 90 : 0, 0));

        Vector3 relativeLauncherPos = selectedDestroyer.torpedoLaunchers[0].transform.position - selectedDestroyer.transform.position;
        Vector3 finalPos = position - relativeLauncherPos;
        finalPos.y = 0;

        selectedDestroyer.transform.position = finalPos;
        selectedDestroyer.PrepareTorpedoLaunchers(new Vector3(BattleInterface.battle.recentTurnInformation.target.x, 0, BattleInterface.battle.recentTurnInformation.target.y) + position);
        selectedDestroyer.gameObject.SetActive(true);
        //selectedDestroyer.FireTorpedoLaunchers();

        Cameraman.TakePosition(new Cameraman.CameraPosition(0.35f, selectedDestroyer.torpedoLaunchers[0].transform.position + Vector3.up * 0.4f - new Vector3(BattleInterface.battle.recentTurnInformation.target.x, 0, BattleInterface.battle.recentTurnInformation.target.y) * 0.5f, new Vector3(20f, Mathf.Atan2(BattleInterface.battle.recentTurnInformation.target.x, BattleInterface.battle.recentTurnInformation.target.y) * Mathf.Rad2Deg, 0f)));

        foreach (BoardTile hit in BattleInterface.battle.recentTurnInformation.torpedoInfo.impacts)
        {
            if (hit.containedShip.eliminated)
            {
                hit.containedShip.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    public override void Refresh()
    {
        base.Refresh();
        if (Cameraman.transitionProgress > 99.7f && !fired)
        {
            fired = true;
            selectedDestroyer.FireTorpedoLaunchers();
        }

        if (lifetime > 12f || (BattleInterface.battle.recentTurnInformation.torpedoInfo.impacts.Count == 0 && lifetime > 4f))
        {
            Actionman.EndActionView();
        }
    }
}
