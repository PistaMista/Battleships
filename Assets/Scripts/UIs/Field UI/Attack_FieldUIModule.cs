using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_FieldUIModule : FieldUIModule
{
    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
        if (!FieldInterface.battle.attackingPlayer.AI || GameController.humanPlayers == 0)
        {
            FieldInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
        }
        else if (GameController.humanPlayers == 1 && !FieldInterface.battle.defendingPlayer.AI)
        {
            FieldInterface.battle.defendingPlayer.board.Set(BoardState.FRIENDLY);
        }
        Cameraman.TakePosition("Board " + (FieldInterface.battle.defendingPlayer.ID + 1), 0.3f);
        if (!FieldInterface.battle.attackingPlayer.AI)
        {
            Interface.SwitchMenu("Attack Screen");
        }
        else
        {
            Interface.SwitchMenu("Firing Screen");
        }
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

    /// <summary>
    /// Accepts input from the player.
    /// </summary>
    protected override void UpdateInput()
    {
        base.UpdateInput();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Back()
    {
        if (inputEnabled)
        {
            FieldInterface.battle.ChangeState(BattleState.CHOOSING_TARGET, 0.4f);
        }
    }
}
