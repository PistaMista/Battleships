using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionShotModule : ScriptableObject
{
    /// <summary>
    /// All the ships participating in the attack.
    /// </summary>
    protected List<Ship> attackers;
    /// <summary>
    /// All the destroyers participating in the attack.
    /// </summary>
    protected List<Ship> destroyers;
    /// <summary>
    /// All the cruisers participating in the attack.
    /// </summary>
    protected List<Ship> cruisers;
    /// <summary>
    /// All the battleships participating in the attack.
    /// </summary>
    protected List<Ship> battleships;
    /// <summary>
    /// The stage of the action shot.
    /// </summary>
    public int stage = 0;
    /// <summary>
    /// The time this module is running for.
    /// </summary>
    public float lifetime = 0f;
    /// <summary>
    /// Prepares the action shot.
    /// </summary>
    public virtual void Prepare()
    {
        attackers = new List<Ship>();
        destroyers = new List<Ship>();
        cruisers = new List<Ship>();
        battleships = new List<Ship>();
        //Prepares the interface
        Interface.SwitchMenu("Firing Screen");

        foreach (Player player in BattleInterface.battle.players)
        {
            player.board.Set(BoardState.DISABLED);
            player.SetMacroMarker(-1);
        }

        if (!BattleInterface.battle.attackingPlayer.AI || (GameController.humanPlayers < 2 && !BattleInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
        {
            BattleInterface.battle.ChangeState(BattleState.SHOWING_HIT_TILE, 0.5f);
        }
        else
        {
            BattleInterface.battle.ChangeState(BattleState.TURN_FINISHED, 1f);
        }

        BattleInterface.battle.switchTime = Mathf.Infinity;
    }
    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    public virtual void Refresh()
    {
        lifetime += Time.deltaTime;
    }
}
