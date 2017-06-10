using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overhead_FieldUIModule : FieldUIModule
{

    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
        foreach (Player player in FieldInterface.battle.players)
        {
            player.SetMacroMarker(-1);
            player.board.Set(BoardState.OVERHEAD);
            if (player == FieldInterface.battle.attackingPlayer)
            {
                player.SetMacroMarker(0);
            }

            if (!player.alive)
            {
                player.SetMacroMarker(1);
            }
        }
        Cameraman.TakePosition("Overhead View", 0.45f);
        Interface.SwitchMenu("Overhead");
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
}
