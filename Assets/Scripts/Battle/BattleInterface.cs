using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInterface : MonoBehaviour
{
    //The battle currently managed by the interface 
    static Battle battle;

    void Update()
    {
        if (battle)
        {
            if (InputController.beginPress && !battle.attackingPlayer.AI)
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
                        break;
                    case BattleState.CHOOSING_TILE_TO_SHOOT:
                        Vector2 candidateTargetPosition = battle.defendingPlayer.board.WorldToTilePosition(InputController.currentInputPosition);
                        bool shotSuccessful = battle.ShootAtTile(candidateTargetPosition);

                        if (shotSuccessful)
                        {
                            battle.defendingPlayer.board.ShowToEnemy(battle.attackingPlayer);
                        }

                        break;
                }
            }
            else if (battle.attackingPlayer.AI)
            {
                AIPlayerActions();
            }
        }
    }

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
                        battle.ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 1.8f);

                        ViewPlayer(battle.defendingPlayer);
                    }
                    break;
                case BattleState.CHOOSING_TILE_TO_SHOOT:
                    Vector2 positionToShoot = battle.ChooseTileToAttackForAIPlayer();
                    while (!battle.ShootAtTile(positionToShoot))
                    {
                        positionToShoot = battle.ChooseTileToAttackForAIPlayer(); ;
                    }

                    if (GameController.singleplayer && !battle.defendingPlayer.AI)
                    {
                        battle.defendingPlayer.board.ShowToFriendly();
                    }
                    else
                    {
                        battle.defendingPlayer.board.ShowToEnemy(battle.attackingPlayer);
                    }
                    break;
            }
        }
    }

    public static void Attach(Battle battle)
    {
        BattleInterface.battle = battle;
        battle.onBattleStateChange += OnBattleStateChange;
        battle.onPlayerSwitch += OnPlayerSwitch;
        battle.onFire += OnFire;
        SetUpOverhead();
    }

    public static void Dettach()
    {
        battle = null;
    }

    public static void Disable()
    {
        battle = null;
    }

    static Player PlayerBeingSelected()
    {
        foreach (Player player in battle.players)
        {
            Vector3 corner1 = player.board.position - new Vector3(player.board.dimensions / 2f, 0f, player.board.dimensions / 2f);
            Vector3 corner2 = player.board.position + new Vector3(player.board.dimensions / 2f, 0f, player.board.dimensions / 2f);
            Vector3 checkedPosition = InputController.currentInputPosition;

            if (checkedPosition.x > corner1.x && checkedPosition.x < corner2.x && checkedPosition.z > corner1.z && checkedPosition.z < corner2.z)
            {
                return player;
            }
        }

        return null;
    }

    static void SetUpOverhead()
    {
        foreach (Player player in battle.players)
        {
            player.board.CamouflageBoard();
            if (player == battle.attackingPlayer)
            {
                player.SetMacroMarker(0);
            }

            if (!player.alive)
            {
                player.SetMacroMarker(1);
            }
        }
    }

    static void ViewPlayer(Player player)
    {
        player.board.SetGridEnabled(true);
        player.SetMacroMarker(-1);

        if (player == battle.attackingPlayer || (GameController.singleplayer && !player.AI))
        {
            player.board.ShowToFriendly();
        }
        else
        {
            player.board.ShowToEnemy(battle.attackingPlayer);
        }
        Cameraman.TakePosition("Board " + (player.ID + 1), 0.6f);
    }

    static void OnPlayerSwitch(Player switchingFrom, Player switchingTo)
    {
        switchingFrom.SetMacroMarker(-1);
        switchingTo.SetMacroMarker(0);

        battle.ChangeState(BattleState.CHOOSING_TARGET, 1f);
    }

    static void OnBattleStateChange(BattleState switchingFrom, BattleState switchingTo)
    {
        switch (switchingTo)
        {
            case BattleState.TURN_FINISHED:
                SetUpOverhead();
                Cameraman.TakePosition("Overhead View");
                Interface.SwitchMenu("Overhead");
                break;
        }
    }

    static void OnFire()
    {

    }

    public void BackToOverhead()
    {
        SetUpOverhead();
        Cameraman.TakePosition("Overhead View");
        Interface.SwitchMenu("Overhead");
        battle.ChangeState(BattleState.CHOOSING_TARGET, 1f);
    }
}
