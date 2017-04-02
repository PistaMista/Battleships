using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInterface : MonoBehaviour
{
    /// <summary>
    /// The marker to show what tile is being shot.
    /// </summary>
    public GameObject defaultRecentlyShotTileMarker;
    /// <summary>
    /// The marker to show what tile is being shot.
    /// </summary>
    static GameObject recentlyShotTileMarker;
    /// <summary>
    /// The speed at which the marker lands on tile being shot.
    /// </summary>
    static float markerDescentSpeed;
    /// <summary>
    /// The battle the interface is attached to.
    /// </summary>
    static Battle battle;
    /// <summary>
    /// The instance of the marker to show the tile being shot.
    /// </summary>
    static GameObject recentlyShotTileIndicator;

    /// <summary>
    /// The awake function.
    /// </summary>
    void Awake()
    {
        recentlyShotTileMarker = defaultRecentlyShotTileMarker;
    }

    /// <summary>
    /// The update function.
    /// </summary>    
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


                        battle.HitTile(candidateTargetPosition);


                        break;
                }
            }
            else if (battle.attackingPlayer.AI)
            {
                AIPlayerActions();
            }

            if (battle.state == BattleState.SHOWING_HIT_TILE)
            {
                recentlyShotTileIndicator.transform.position = new Vector3(recentlyShotTileIndicator.transform.position.x, Mathf.SmoothDamp(recentlyShotTileIndicator.transform.position.y, GameController.playerBoardElevation + 0.1f, ref markerDescentSpeed, 0.2f, Mathf.Infinity), recentlyShotTileIndicator.transform.position.z);
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
                        if (GameController.skipAIvsAIActionShots && battle.attackingPlayer.AI && battle.defendingPlayer.AI)
                        {
                            Debug.Log(battle.HitTile(battle.ChooseTileToAttackForAIPlayer()));
                        }
                        else
                        {
                            battle.ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 1.8f);
                        }

                        ViewPlayer(battle.defendingPlayer);
                    }
                    break;
                case BattleState.CHOOSING_TILE_TO_SHOOT:
                    Vector2 positionToShoot = battle.ChooseTileToAttackForAIPlayer();

                    battle.HitTile(positionToShoot);
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
        battle.onFire += OnFire;
        SetUpOverhead();
    }

    /// <summary>
    /// Dettaches the current battle from the interface.
    /// </summary>
    public static void Dettach()
    {
        battle = null;
        Destroy(recentlyShotTileIndicator);
    }

    /// <summary>
    /// Gets the player, who should be selected considering input position.
    /// </summary>
    /// <returns>The player, who should be selected.</returns>
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
                Destroy(recentlyShotTileIndicator);
                break;
            case BattleState.FIRING:
                foreach (Ship ship in battle.attackingPlayer.livingShips)
                {
                    ship.PositionOnPlayingBoard();
                    ship.gameObject.SetActive(false);
                }
                break;
        }

        switch (switchingTo)
        {
            case BattleState.FIRING:
                PrepareActionView();


                break;
            case BattleState.SHOWING_HIT_TILE:
                battle.ChangeState(BattleState.TURN_FINISHED, 1f);
                ViewPlayer(battle.defendingPlayer);
                recentlyShotTileIndicator = Instantiate(recentlyShotTileMarker);
                recentlyShotTileIndicator.transform.position = battle.defendingPlayer.board.tiles[(int)battle.recentlyShot.x, (int)battle.recentlyShot.y].worldPosition + Vector3.up * 3f;


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
        if (!(GameController.skipAIvsAIActionShots && battle.attackingPlayer.AI && battle.defendingPlayer.AI))
        {
            battle.ChangeState(BattleState.FIRING);
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


    /// <summary>
    /// Prepares the game for an action shot of weapons firing.
    /// </summary>
    static void PrepareActionView()
    {
        //Prepares the interface
        Interface.SwitchMenu("Firing Screen");

        foreach (Player player in battle.players)
        {
            player.board.Set(BoardState.DISABLED);
            player.SetMacroMarker(-1);
        }

        if ((GameController.humanPlayers == 1 && !battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
        {
            battle.defendingPlayer.board.Set(BoardState.SHIPS);
        }

        if (!battle.attackingPlayer.AI || (GameController.humanPlayers < 2 && !battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
        {
            battle.ChangeState(BattleState.SHOWING_HIT_TILE, 0.5f);
        }
        else
        {
            battle.ChangeState(BattleState.TURN_FINISHED, 1f);
        }

        //TEST
        battle.switchTime = 2f;
        //TEST

        //Prepares the attack fleet
        Vector3 fleetPosition = Vector3.zero;
        float fleetRotation = 0f;

        if (Mathf.Abs(battle.defendingPlayer.board.position.z) < Mathf.Abs(battle.defendingPlayer.board.position.x))
        {
            fleetPosition = battle.defendingPlayer.board.position - Vector3.right * GameController.playerBoardDistanceFromCenter * Mathf.Sign(battle.defendingPlayer.board.position.x) * 1.5f;
            fleetRotation = 90f * Mathf.Sign(battle.defendingPlayer.board.position.x);
        }
        else
        {
            fleetPosition = battle.defendingPlayer.board.position - Vector3.forward * GameController.playerBoardDistanceFromCenter * Mathf.Sign(battle.defendingPlayer.board.position.z) * 1.5f;
            fleetRotation = 90f - 90f * Mathf.Sign(battle.defendingPlayer.board.position.z);
        }

        fleetPosition.y = GameController.seaLevel;

        // GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // tmp.transform.position = fleetPosition;
        // tmp.transform.localScale = new Vector3(0.1f, 0.1f, 2f);
        // tmp.transform.localRotation = Quaternion.Euler(new Vector3(0f, fleetRotation, 0f));

        List<Ship> destroyers = new List<Ship>();
        List<Ship> cruisers = new List<Ship>();
        List<Ship> battleships = new List<Ship>();
        foreach (Ship ship in battle.attackingPlayer.livingShips)
        {
            switch (ship.length)
            {
                case 3:
                    destroyers.Add(ship);
                    break;
                case 4:
                    cruisers.Add(ship);
                    break;
                case 5:
                    battleships.Add(ship);
                    break;
            }
        }

        Vector3 position = Vector3.zero;
        //Place the battleships
        for (int i = 0; i < battleships.Count; i++)
        {
            Ship ship = battleships[i];
            position = new Vector3(-(battleships.Count / 2f - 0.5f) * 4f + (i) * 4f, ship.transform.position.y, 0f);
            ship.transform.position = position;
        }

        //Place the destroyers
        for (int i = 0; i < destroyers.Count; i++)
        {
            Ship ship = destroyers[i];
            position = new Vector3(-(destroyers.Count / 2f - 0.5f) * 3f + (i) * 3f, ship.transform.position.y, -5.5f);
            ship.transform.position = position;
        }

        //Place the cruisers
        for (int i = 0; i < cruisers.Count; i++)
        {
            Ship ship = cruisers[i];
            position = new Vector3(-(cruisers.Count / 2f - 0.5f) * 5.5f + (i) * 5.5f, ship.transform.position.y, 5.5f);
            ship.transform.position = position;
        }

        //Rotate the ships
        //Vector3 directionModifier = new Vector3(Mathf.Cos(fleetRotation * Mathf.Deg2Rad), 0f, Mathf.Sin(fleetRotation * Mathf.Deg2Rad)).normalized;

        foreach (Ship ship in battle.attackingPlayer.livingShips)
        {
            ship.gameObject.SetActive(true);
            ship.transform.rotation = Quaternion.Euler(Vector3.up * fleetRotation);
            Vector3 currentDirection = ship.transform.position.normalized;
            float radianAngle = fleetRotation * Mathf.Deg2Rad;
            //Vector3 targetDirection = new Vector3(Mathf.Cos(fleetRotation * Mathf.Deg2Rad), 0f, Mathf.Sin(fleetRotation * Mathf.Deg2Rad)).normalized;
            Vector3 localPosition = new Vector3(ship.transform.position.x * Mathf.Cos(radianAngle) - ship.transform.position.z * Mathf.Sin(radianAngle), 0f, ship.transform.position.z * Mathf.Cos(radianAngle) + ship.transform.position.x * Mathf.Sin(radianAngle));
            ship.transform.position = localPosition + fleetPosition;
        }

        foreach (Ship ship in battle.attackingPlayer.livingShips)
        {
            ship.PrepareToFireAt(battle.defendingPlayer.board.tiles[(int)battle.recentlyShot.x, (int)battle.recentlyShot.y].worldPosition, battle.defendingPlayer.board.tiles[(int)battle.recentlyShot.x, (int)battle.recentlyShot.y].containedShip);
            ship.Fire();
        }

    }

    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    static void RefreshActionView()
    {

    }
}
