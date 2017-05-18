using System.Collections;
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
    /// The weapon currently selected by the player.
    /// </summary>
    static AttackType selectedWeapon;

    /// <summary>
    /// The awake function.
    /// </summary>
    void Awake()
    {
        dummyTorpedo = defaultDummyTorpedo;
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
                                        SelectWeapon(AttackType.ARTILLERY);
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

            UpdateTorpedoOption();
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

                    battle.ArtilleryAttack(tileToShoot);
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
        ResetTargetingUI();
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
        SelectWeapon(AttackType.ARTILLERY);

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
                ResetTargetingUI();
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
                //DEPRECATED
                ViewPlayer(battle.defendingPlayer);
                // recentlyShotTileIndicator = Instantiate(recentlyShotTileMarker);
                // recentlyShotTileIndicator.transform.position = battle.recentAttackInfo.hitTiles + Vector3.up * 0.12f;
                // SquarePulserEffect effect = recentlyShotTileIndicator.GetComponent<SquarePulserEffect>();
                // effect.pulseInterval = 0.45f;
                // effect.insideLength = 0.9f;
                // effect.maxDistance = 2f;
                // effect.pulseSpeed = 7f;
                // effect.squareWidth = 0.35f;
                // effect.color = (battle.recentAttackInfo.hitShips != null) ? Color.red : Color.black;
                //DEPRECATED
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
                    battle.ChangeState(BattleState.SHOWING_HIT_TILE, 2f);
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

    /// <summary>
    /// Selects a weapon to use in the current attack.
    /// </summary>
    /// <param name="weapon">The type of weapon to select.</param>
    static void SelectWeapon(AttackType weapon)
    {
        if (!battle.attackingPlayer.AI)
        {
            Debug.Log("Selected weapon: " + weapon.ToString());
            ResetTargetingUI();
            switch (weapon)
            {
                case AttackType.ARTILLERY:
                    if (battle.state == BattleState.CHOOSING_TILE_TO_SHOOT)
                    {
                        battle.defendingPlayer.board.Set(BoardState.ENEMY);
                    }
                    break;
                case AttackType.TORPEDO:
                    if (selectedWeapon != AttackType.TORPEDO)
                    {
                        dummyTorpedo.SetActive(true);
                        Vector3 relativePosition = battle.defendingPlayer.board.transform.position - battle.GetTorpedoLaunchPosition();
                        dummyTorpedo.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0));
                        dummyTorpedo.transform.position = battle.GetTorpedoLaunchPosition() + Vector3.up * battle.defendingPlayer.board.transform.position.y;
                    }
                    else
                    {
                        battle.TorpedoAttack(torpedoFiringDirection);
                    }
                    break;
            }
        }
        selectedWeapon = weapon;
    }

    /// <summary>
    /// The line used to show torpedo firing direction.
    /// </summary>
    public GameObject defaultDummyTorpedo;
    static GameObject dummyTorpedo;

    /// <summary>
    /// The direction the torpedoes are currently aimed in.
    /// </summary>
    static Vector3 torpedoFiringDirection;

    /// <summary>
    /// Whether the dummy torpedo is set up.
    /// </summary>
    static bool dummySetUp = false;

    /// <summary>
    /// Value used to determine whether the targeted tiles have changed.
    /// </summary>
    static Vector2 refreshDecisionTemplate;

    /// <summary>
    /// Selects a weapon, using the UI.
    /// </summary>
    /// <param name="weapon"></param>
    public void SelectUIWeapon(string weapon)
    {
        switch (weapon)
        {
            case "artillery":
                SelectWeapon(AttackType.ARTILLERY);
                break;
            case "torpedoes":
                SelectWeapon(AttackType.TORPEDO);
                break;
        }
    }

    /// <summary>
    /// Updates the torpedo targeting line.
    /// </summary>
    static void UpdateTorpedoOption()
    {
        // Vector3 relativePosition = InputController.currentInputPosition - dummyTorpedo.transform.position;
        // dummyTorpedo.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0));
        // battle.defendingPlayer.board.Set(BoardState.ENEMY);
        // torpedoFiringDirection = relativePosition.normalized;

        // BoardTile[] hits = battle.GetTorpedoHits(battle.GetTorpedoLaunchPosition(), torpedoFiringDirection * 30f);
        // for (int i = 0; i < hits.Length; i++)
        // {
        //     BoardTile hit = hits[i];
        //     Debug.Log("Hit #: " + i + " Pos: " + hit.boardCoordinates);
        //     hit.SetMarker(Color.yellow, battle.defendingPlayer.board.grid.transform);
        // }

        if (battle.state == BattleState.CHOOSING_TILE_TO_SHOOT && battle.switchTime < -Time.deltaTime && !battle.attackingPlayer.AI && battle.attackingPlayer.torpedoRecharge == 0)
        {
            Vector3 launchPosition = battle.GetTorpedoLaunchPosition();
            Vector3 targetPosition = Vector3.zero;
            Vector3 targetRotation = Vector3.zero;
            if (InputController.IsDragging(63) && dummySetUp)
            {
                targetPosition = InputController.currentInputPosition;
                targetPosition.y = battle.defendingPlayer.board.transform.position.y;
                Vector3 relativePosition = InputController.currentInputPosition - launchPosition;
                //dummyTorpedo.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0));
                //battle.defendingPlayer.board.Set(BoardState.ENEMY);
                torpedoFiringDirection = relativePosition.normalized;
                targetRotation = new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0);

                Vector2 deterministic = Vector2.zero;
                BoardTile[] hits = battle.GetTorpedoHits(launchPosition, torpedoFiringDirection * 30f);
                for (int i = 0; i < hits.Length; i++)
                {
                    BoardTile hit = hits[i];
                    //Debug.Log("Hit #: " + i + " Pos: " + hit.boardCoordinates);
                    deterministic += hit.boardCoordinates;
                }

                if (deterministic != refreshDecisionTemplate)
                {
                    battle.defendingPlayer.board.Set(BoardState.ENEMY);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        BoardTile hit = hits[i];
                        hit.SetMarker(Color.yellow, battle.defendingPlayer.board.grid.transform);
                    }
                    Debug.Log("Refresh: " + deterministic);
                    refreshDecisionTemplate = deterministic;
                }

            }
            else
            {
                BoardTile[] hits = battle.GetTorpedoHits(launchPosition, torpedoFiringDirection * 30f);
                if (InputController.GetEndPress(63) && hits.Length > 0)
                {
                    Debug.Log("Fire!");
                    //ResetTargetingUI();
                    battle.TorpedoAttack(torpedoFiringDirection);
                }
                targetPosition = battle.defendingPlayer.board.transform.position + Vector3.right * ((battle.defendingPlayer.board.dimensions / 2f + 1) + battle.defendingPlayer.board.dimensions * 0.075f);
                targetRotation = Vector3.zero;
            }

            if (!dummySetUp)
            {
                dummyTorpedo.transform.position = targetPosition;
                dummyTorpedo.transform.rotation = Quaternion.Euler(targetRotation);
                dummyTorpedo.transform.localScale = Vector3.one * battle.defendingPlayer.board.dimensions;
                dummyTorpedo.SetActive(true);
                dummySetUp = true;
            }

            dummyTorpedo.transform.position = targetPosition;
            dummyTorpedo.transform.rotation = Quaternion.Euler(targetRotation);
        }
        else
        {
            if (dummySetUp)
            {
                dummyTorpedo.SetActive(false);
                dummySetUp = false;
            }
        }
    }

    /// <summary>
    /// Resets the targeting interface.
    /// </summary>
    static void ResetTargetingUI()
    {
        if (battle.state == BattleState.CHOOSING_TILE_TO_SHOOT)
        {
            battle.defendingPlayer.board.Set(BoardState.ENEMY);
        }
    }
}
