using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSetup_FieldUIModule : FieldUIModule
{

    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
        if (!(FieldInterface.battle.attackingPlayer.AI && FieldInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
        {
            FieldInterface.battle.ChangeState(BattleState.SHOWING_HIT_TILE, 1f);
            Actionman.ActionView();
        }
    }

    /// <summary>
    /// Disables the UI module.
    /// </summary>
    protected override void Disable()
    {
        base.Disable();
        if (FieldInterface.battle != null)
        {
            foreach (Ship ship in FieldInterface.battle.attackingPlayer.livingShips)
            {
                ship.PositionOnPlayingBoard();
                ship.gameObject.SetActive(false);
            }
        }
    }
}
