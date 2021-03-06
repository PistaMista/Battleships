﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInterface : MonoBehaviour
{
    /// <summary>
    /// The marker to show what tile is being shot.
    /// </summary>
    static GameObject recentlyShotTileMarker;
    /// <summary>
    /// The battle the interface is attached to.
    /// </summary>
    public static Battle battle;

    /// <summary>
    /// The awake function.
    /// </summary>
    void Awake()
    {
    }

    /// <summary>
    /// The update function.
    /// </summary>    
    void Update()
    {
        if (battle)
        {
            if (InputController.GetTap(63) && !battle.attackingPlayer.AI)
            {
                switch (battle.state)
                {
                    case BattleState.CHOOSING_TARGET:
                        if (battle.switchTime <= -0.25f)
                        {
                            Player candidatePlayer = PlayerBeingSelected();
                            if (candidatePlayer)
                            {
                                if (candidatePlayer != battle.attackingPlayer)
                                {
                                    if (battle.SelectTarget(candidatePlayer))
                                    {
                                        Interface.SwitchMenu("Attack Screen");

                                        battle.ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 1f);
                                        ViewPlayer(candidatePlayer);
                                    }
                                }
                                else
                                {
                                    battle.ChangeState(BattleState.FRIENDLY_SHIP_PREVIEW, 1f);
                                    ViewPlayer(candidatePlayer);
                                }

                            }
                        }
                        break;
                    case BattleState.FRIENDLY_SHIP_PREVIEW:
                        BackToOverhead();
                        battle.switchTime = 0f;
                        break;
                    case BattleState.CHOOSING_TILE_TO_SHOOT:

                        BoardTile candidateTargetTile = battle.defendingPlayer.board.GetTileAtWorldPosition(InputController.currentInputPosition);


                        battle.ArtilleryAttack(candidateTargetTile);
                        break;
                }
            }
            else if (battle.attackingPlayer.AI)
            {
                AIPlayerActions();
            }
        }
    }

    /// <summary>
    /// Manages the actions for AI players.
    /// </summary>    
    static void AIPlayerActions()
    {
        if (battle.switchTime <= -0.1f)
        {
            switch (battle.state)
            {
                case BattleState.CHOOSING_TARGET:
                    int randomTargetID = Random.Range(0, battle.players.Length);
                    while (randomTargetID == battle.attackingPlayerID)
                    {
                        randomTargetID = Random.Range(0, battle.players.Length);
                    }

                    if (battle.SelectTarget(battle.players[randomTargetID]))
                    {
                        if (GameController.skipAIvsAIActionShots && battle.attackingPlayer.AI && battle.defendingPlayer.AI && GameController.humanPlayers > 0)
                        {
                            Debug.Log(battle.ArtilleryAttack(battle.ChooseTileToAttackForAIPlayer()));
                        }
                        else
                        {
                            battle.ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 1.8f);
                        }

                        ViewPlayer(battle.defendingPlayer);
                    }
                    break;
                case BattleState.CHOOSING_TILE_TO_SHOOT:
                    BoardTile tileToShoot = battle.ChooseTileToAttackForAIPlayer();
                    if (battle.TorpedoAttackAvailable())
                    {
                        battle.TorpedoAttack(tileToShoot.transform.position - battle.GetTorpedoLaunchPosition());
                    }
                    else
                    {
                        battle.ArtilleryAttack(tileToShoot);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Attaches a battle to the interface.
    /// </summary>
    /// <param name="battle">The battle to attach.</param>
    public static void Attach(Battle battle)
    {
        BattleInterface.battle = battle;
        battle.onBattleStateChange += OnBattleStateChange;
        battle.onPlayerSwitch += OnPlayerSwitch;
        battle.onAttack += OnFire;
        SetUpOverhead();
    }

    /// <summary>
    /// Dettaches the current battle from the interface.
    /// </summary>
    public static void Dettach()
    {
        battle = null;
    }

    /// <summary>
    /// Gets the player, who should be selected considering input position.
    /// </summary>
    /// <returns>The player, who should be selected.</returns>
    static Player PlayerBeingSelected()
    {
        foreach (Player player in battle.players)
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
    /// Sets up the overhead view.
    /// </summary>
    static void SetUpOverhead()
    {
        foreach (Player player in battle.players)
        {
            player.board.Set(BoardState.OVERHEAD);
            if (player == battle.attackingPlayer)
            {
                player.SetMacroMarker(0);
            }

            if (!player.alive)
            {
                player.SetMacroMarker(1);
            }
        }
        TorpedoTargetingBattleUIModule.Disable();
    }


    /// <summary>
    /// Shows a player's board.
    /// </summary>
    /// <param name="player">The player, who's board to show.</param>
    static void ViewPlayer(Player player)
    {

        player.SetMacroMarker(-1);

        if (player == battle.attackingPlayer || (GameController.humanPlayers == 1 && !player.AI) || GameController.humanPlayers == 0)
        {
            player.board.Set(BoardState.FRIENDLY);
        }
        else if (!battle.attackingPlayer.AI || GameController.humanPlayers == 0)
        {
            player.board.Set(BoardState.ENEMY);
            TorpedoTargetingBattleUIModule.Enable();
        }
        else
        {
            player.board.Set(BoardState.OVERHEAD);
        }
        Cameraman.TakePosition("Board " + (player.ID + 1), 0.3f);
    }

    /// <summary>
    /// On player switch.
    /// </summary>
    /// <param name="switchingFrom">...</param>
    /// <param name="switchingTo">...</param>
    static void OnPlayerSwitch(Player switchingFrom, Player switchingTo)
    {
        switchingFrom.SetMacroMarker(-1);
        switchingTo.SetMacroMarker(0);

        foreach (Ship ship in switchingFrom.allShips)
        {
            battle.DisableSunkShip(ship);
        }

        battle.ChangeState(BattleState.CHOOSING_TARGET, 1f);
    }

    /// <summary>
    /// On battle state change.
    /// </summary>
    /// <param name="switchingFrom">...</param>
    /// <param name="switchingTo">...</param>
    static void OnBattleStateChange(BattleState switchingFrom, BattleState switchingTo)
    {
        switch (switchingFrom)
        {
            case BattleState.SHOWING_HIT_TILE:

                break;
            case BattleState.FIRING:
                foreach (Ship ship in battle.attackingPlayer.livingShips)
                {
                    ship.PositionOnPlayingBoard();
                    ship.gameObject.SetActive(false);
                }


                break;
            case BattleState.CHOOSING_TILE_TO_SHOOT:
                TorpedoTargetingBattleUIModule.Disable();
                break;
        }

        switch (switchingTo)
        {
            case BattleState.FIRING:
                battle.ChangeState(BattleState.SHOWING_HIT_TILE, 1f);
                Actionman.ActionView();

                break;
            case BattleState.SHOWING_HIT_TILE:
                battle.ChangeState(BattleState.TURN_FINISHED, 1.5f);
                ViewPlayer(battle.defendingPlayer);
                break;
            case BattleState.TURN_FINISHED:
                SetUpOverhead();
                Cameraman.TakePosition("Overhead View", 0.45f);
                Interface.SwitchMenu("Overhead");
                break;
            case BattleState.CHOOSING_TILE_TO_SHOOT:
                break;
        }
    }

    /// <summary>
    /// On guns firing. Sets up the action camera and attack fleet.
    /// </summary>
    static void OnFire()
    {
        if (!(GameController.skipAIvsAIActionShots && battle.attackingPlayer.AI && battle.defendingPlayer.AI && GameController.humanPlayers > 0))
        {
            switch (battle.recentTurnInformation.type)
            {
                case AttackType.ARTILLERY:
                    battle.ChangeState(BattleState.FIRING);
                    break;
                case AttackType.TORPEDO:
                    //battle.ChangeState(BattleState.SHOWING_HIT_TILE, 2f);
                    battle.ChangeState(BattleState.FIRING);
                    break;
            }
        }
    }



    /// <summary>
    /// Used by UI elements to return back to overhead view.
    /// </summary>
    public void BackToOverhead()
    {
        SetUpOverhead();
        Cameraman.TakePosition("Overhead View", 0.45f);
        Interface.SwitchMenu("Overhead");
        battle.ChangeState(BattleState.CHOOSING_TARGET, 1f);
    }
}
