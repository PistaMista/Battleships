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
        if (InputController.GetTap(63) && !FieldInterface.battle.attackingPlayer.AI)
        {
            if (FieldInterface.battle.switchTime <= -0.25f)
            {
                Player candidatePlayer = PlayerBeingSelected();
                if (candidatePlayer)
                {
                    if (candidatePlayer != FieldInterface.battle.attackingPlayer)
                    {
                        if (FieldInterface.battle.SelectTarget(candidatePlayer))
                        {
                            //Interface.SwitchMenu("Attack Screen");
                            FieldInterface.battle.ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 1f);
                        }
                    }
                    else
                    {
                        FieldInterface.battle.ChangeState(BattleState.FRIENDLY_SHIP_PREVIEW, 1f);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Gets the player, who should be selected considering input position.
    /// </summary>
    /// <returns>The player, who should be selected.</returns>
    static Player PlayerBeingSelected()
    {
        foreach (Player player in FieldInterface.battle.players)
        {
            Vector3 corner1 = player.board.transform.position - new Vector3(player.board.dimensions / 2f, 0f, player.board.dimensions / 2f);
            Vector3 corner2 = player.board.transform.position + new Vector3(player.board.dimensions / 2f, 0f, player.board.dimensions / 2f);
            Vector3 checkedPosition = InputController.currentInputPosition;

            if (checkedPosition.x > corner1.x && checkedPosition.x < corner2.x && checkedPosition.z > corner1.z && checkedPosition.z < corner2.z)
            {
                return player;
            }
        }

        return null;
    }

    /// <summary>
    /// Shows a player's board.
    /// </summary>
    /// <param name="player">The player, who's board to show.</param>
    static void ViewPlayer(Player player)
    {
        if (player == FieldInterface.battle.attackingPlayer || (GameController.humanPlayers == 1 && !player.AI) || GameController.humanPlayers == 0)
        {
            player.board.Set(BoardState.FRIENDLY);
        }
        else if (!FieldInterface.battle.attackingPlayer.AI || GameController.humanPlayers == 0)
        {
            player.board.Set(BoardState.ENEMY);
        }
        else
        {
            player.board.Set(BoardState.OVERHEAD);
        }
    }
}
