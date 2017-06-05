using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackReport_FieldUIModule : FieldUIModule
{

    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
        FieldInterface.battle.ChangeState(BattleState.TURN_FINISHED, 1.5f);
        if (GameController.humanPlayers == 0 || (GameController.humanPlayers == 1 && !FieldInterface.battle.defendingPlayer.AI))
        {
            FieldInterface.battle.defendingPlayer.board.Set(BoardState.FRIENDLY);
        }
        else if (!FieldInterface.battle.attackingPlayer.AI)
        {
            FieldInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
        }
        Cameraman.TakePosition("Board " + (FieldInterface.battle.defendingPlayer.ID + 1), 0.3f);
    }

    /// <summary>
    /// Disables the UI module.
    /// </summary>
    protected override void Disable()
    {
        base.Disable();

    }

    /// <summary>
    /// Updates the visuals of the module.
    /// </summary>
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
    }
}
