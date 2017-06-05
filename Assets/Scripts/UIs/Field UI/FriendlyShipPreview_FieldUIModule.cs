using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyShipPreview_FieldUIModule : FieldUIModule
{
    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
        FieldInterface.battle.attackingPlayer.board.Set(BoardState.FRIENDLY);
        FieldInterface.battle.attackingPlayer.SetMacroMarker(-1);
        Cameraman.TakePosition("Board " + (FieldInterface.battle.attackingPlayer.ID + 1), 0.3f);
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
        if (InputController.GetTap(63))
        {
            FieldInterface.battle.ChangeState(BattleState.CHOOSING_TARGET, 0.3f);
        }
    }
}
