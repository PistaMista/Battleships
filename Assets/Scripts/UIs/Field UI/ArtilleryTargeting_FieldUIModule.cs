using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryTargeting_FieldUIModule : FieldUIModule
{

    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
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
            BoardTile candidateTargetTile = FieldInterface.battle.defendingPlayer.board.GetTileAtWorldPosition(InputController.currentInputPosition);
            if (FieldInterface.battle.ArtilleryAttack(candidateTargetTile))
            {
                FieldInterface.battle.ChangeState(BattleState.FIRING);
            }
        }
    }
}
