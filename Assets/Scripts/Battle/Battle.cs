using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BattleState { NONE, ALL, CHOOSING_TARGET, CHOOSING_TILE_TO_SHOOT, FIRING, TURN_FINISHED, FRIENDLY_SHIP_PREVIEW, SHOWING_HIT_TILE }

public class Battle : MonoBehaviour
{

    /// <summary>
    /// Whether the battle is in progress.
    /// </summary>
    public bool battling = false;
    /// <summary>
    /// Whether this battle is the main battle of the game.
    /// </summary>
    public bool isMainBattle = false;
    /// <summary>
    /// The players competing in this battle.
    /// </summary>
    public Player[] players;
    /// <summary>
    /// All the ships competing in this battle.
    /// </summary>
    public List<Ship> ships;
    /// <summary>
    /// The number of players, who are still alive.
    /// </summary>
    public int playersAlive;
    /// <summary>
    /// The ID of the player on the turn.
    /// </summary>
    public int attackingPlayerID;
    /// <summary>
    /// The player on the turn.
    /// </summary>
    public Player attackingPlayer;
    /// <summary>
    /// The player targeted by the attacking player.
    /// </summary>
    public Player defendingPlayer;
    /// <summary>
    /// The current state of the battle.
    /// </summary>
    public BattleState currentState;
    /// <summary>
    /// The delay before switching to the next state.
    /// </summary>
    public float switchTime;
    /// <summary>
    /// The state to switch to after switch time is over.
    /// </summary>
    public BattleState nextState;
    /// <summary>
    /// Modifies the state switch times.
    /// </summary>
    public float switchTimeModifier;

    //Delegates for external modules to tap into
    /// <summary>
    /// On next player switch.
    /// </summary>
    /// <param name="switchingFrom">The player, who was on turn before.</param>
    /// <param name="switchingTo">The player, who is on turn now.</param>
    public delegate void OnPlayerSwitch(Player switchingFrom, Player switchingTo);
    /// <summary>
    /// On next player switch.
    /// </summary>
    public OnPlayerSwitch onPlayerSwitch;
    /// <summary>
    /// On battle state change.
    /// </summary>
    /// <param name="switchingFrom">The last battle state.</param>
    /// <param name="switchingTo">The current battle state.</param>
    public delegate void OnBattleStateChange(BattleState switchingFrom, BattleState switchingTo);
    /// <summary>
    /// On battle state change.
    /// </summary>
    public OnBattleStateChange onBattleStateChange;
    /// <summary>
    /// On firing guns.
    /// </summary>
    public delegate void OnAttack();
    /// <summary>
    /// On firing guns.
    /// </summary>
    public OnAttack onAttack;
    /// <summary>
    /// Stores information about the most recent attack.
    /// </summary>
    public MoveInformator recentTurnInformation;


    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        if (battling)
        {
            if (switchTime <= 0)
            {
                if (currentState != nextState)
                {
                    ChangeState(nextState);
                }
            }
            else
            {
                if (GameController.switchTimesNill)
                {
                    switchTime = 0;
                }
            }

            switchTime -= Time.deltaTime;

            //if (!isMainBattle)
            //{
            //   AIPlayerActions();
            //}
        }
    }

    /// <summary>
    /// Initializes the battle.
    /// </summary>
    /// <param name="competitors">Players to compete in this battle.</param>
    public void Initialize(Player[] competitors)
    {
        players = competitors;
        ships = new List<Ship>();
        isMainBattle = GameController.mainBattle == this;
        for (int i = 0; i < players.Length; i++)
        {
            players[i].gameObject.transform.parent = transform;
            players[i].battle = this;
            if (isMainBattle)
            {
                players[i].board.Set(BoardState.OVERHEAD);
            }
            else
            {
                players[i].AI = true;
            }
        }

        playersAlive = players.Length;
        ShipPlacer.HandleShipsForBattle(this);
    }

    /// <summary>
    /// Starts the battle.
    /// </summary>
    public void StartBattle()
    {
        battling = true;



        attackingPlayerID = -1;
        NextPlayer();
        recentTurnInformation = (MoveInformator)ScriptableObject.CreateInstance("MoveInformator");
        recentTurnInformation.Reset();

        if (isMainBattle)
        {
            GameController.ChangeState(GameState.BATTLING);
            //BattleInterface.Attach(this); //OUTDATED
        }
        else
        {
            foreach (Player player in players)
            {
                player.ShipsShown(true, false);
            }
        }

        ChangeState(BattleState.CHOOSING_TARGET);
    }

    /// <summary>
    /// Passes the turn to the next player.
    /// </summary>    
    void NextPlayer()
    {
        Player originalAttacker = attackingPlayer;

        attackingPlayerID++;
        if (attackingPlayerID > players.Length - 1)
        {
            attackingPlayerID = 0;
        }

        if (originalAttacker != null)
        {
            foreach (ActiveAircraft squadron in originalAttacker.overheadSquadrons)
            {
                squadron.Spot();
            }
        }

        attackingPlayer = players[attackingPlayerID];
        attackingPlayer.torpedoRecharge = (attackingPlayer.torpedoRecharge == 0) ? 0 : attackingPlayer.torpedoRecharge - 1;

        if (originalAttacker != null)
        {
            originalAttacker.OnTurnEnd();
        }
        attackingPlayer.OnTurnBegin();



        while (!attackingPlayer.alive)
        {
            attackingPlayerID++;
            if (attackingPlayerID > players.Length - 1)
            {
                attackingPlayerID = 0;
            }



            attackingPlayer = players[attackingPlayerID];
        }

        if (onPlayerSwitch != null)
        {
            onPlayerSwitch(originalAttacker, attackingPlayer);
        }
    }

    /// <summary>
    /// Selects a target for the attacking player.
    /// </summary>
    /// <param name="target">The player to target.</param>
    /// <returns>Target validity.</returns>
    public bool SelectTarget(Player target)
    {
        if (target != attackingPlayer && target.alive)
        {
            defendingPlayer = target;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Executes an artillery attack on the target tile of the defending player's board.
    /// </summary>
    /// <param name="tile">Position of the tile to hit.</param>
    /// <returns>Hit successful.</returns>
    public bool ArtilleryAttack(BoardTile tile)
    {

        if (tile != null)
        {
            if (!attackingPlayer.hits[defendingPlayer.ID].Contains(tile) && !attackingPlayer.misses[defendingPlayer.ID].Contains(tile))
            {
                recentTurnInformation.Reset();
                recentTurnInformation.target = tile.boardCoordinates;
                //recentTurnInformation.attackedTileWorldPosition = tile.transform.position;
                recentTurnInformation.type = AttackType.ARTILLERY;
                recentTurnInformation.attacker = attackingPlayer;

                nextState = BattleState.TURN_FINISHED;
                switchTime = 0.5f;

                if (tile.containedShip && Random.Range(0, 10) == 0 && tile.containedShip.type != ShipType.AIRCRAFT_CARRIER)
                {
                    if (!tile.containedShip.eliminated)
                    {
                        foreach (BoardTile t in tile.containedShip.tiles)
                        {
                            RegisterHitOnTile(t);
                        }
                        tile.containedShip.RevealTo(attackingPlayer);
                    }
                }
                else
                {
                    RegisterHitOnTile(tile);
                }

                if (onAttack != null)
                {
                    onAttack();
                }

                if (!defendingPlayer.alive)
                {
                    playersAlive--;
                }

                if (!isMainBattle)
                {
                    FireGunsAtTargetTile(tile);
                    //ChangeState(BattleState.FIRING);
                }

                return true;
            }
        }


        Debug.LogWarning("There was an attempt to shoot an invalid tile: " + tile + ". Things may break.");
        return false;
    }

    /// <summary>
    /// Executes a torpedo attack in the direction from the launch point.
    /// </summary>
    /// <param name="direction"></param>
    public void TorpedoAttack(Vector3 direction)
    {
        recentTurnInformation.Reset();
        recentTurnInformation.target = new Vector2(direction.x, direction.z).normalized;
        //recentTurnInformation.attackedTileWorldPosition = tile.transform.position;
        recentTurnInformation.type = AttackType.TORPEDO;
        recentTurnInformation.attacker = attackingPlayer;

        nextState = BattleState.TURN_FINISHED;
        switchTime = 0.5f;

        int destroyers = 0;
        foreach (Ship ship in attackingPlayer.livingShips)
        {
            if (ship.type == ShipType.DESTROYER && ship.lengthRemaining == ship.length)
            {
                destroyers++;
            }
        }

        switch (destroyers)
        {
            case 2:
                attackingPlayer.torpedoRecharge = 3;
                break;
            case 1:
                attackingPlayer.torpedoRecharge = 5;
                break;
        }

        int torpedoes = 5;
        Vector3 launchPosition = GetTorpedoLaunchPosition();
        BoardTile[] hits = GetTorpedoHits(launchPosition, launchPosition + direction.normalized * 30f);

        for (int i = 0; i < hits.Length; i++)
        {
            BoardTile inspectedTile = hits[i];
            if (inspectedTile.containedShip)
            {
                if (!inspectedTile.containedShip.eliminated && Random.Range(1, 100) > inspectedTile.containedShip.torpedoEvasionChance)
                {
                    foreach (BoardTile tile in inspectedTile.containedShip.tiles)
                    {
                        int distance = (int)Vector2.Distance(tile.boardCoordinates, inspectedTile.boardCoordinates);
                        if (distance <= Random.Range(1, 3))
                        {
                            RegisterHitOnTile(tile);
                            tile.RevealTo(attackingPlayer);
                        }
                    }

                    recentTurnInformation.torpedoInfo.impacts.Add(inspectedTile);

                    if (inspectedTile.containedShip.eliminated)
                    {
                        torpedoes--;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (torpedoes == 0)
            {
                break;
            }
        }

        foreach (BoardTile hit in recentTurnInformation.torpedoInfo.impacts)
        {
            Debug.Log("Torpedo Hit: " + hit.transform.position);
        }

        if (onAttack != null)
        {
            onAttack();
        }
    }

    /// <summary>
    /// Registers that a tile has been hit.
    /// </summary>
    /// <param name="tile">The tile that has been hit.</param>
    public void RegisterHitOnTile(BoardTile tile)
    {
        if (tile.containedShip)
        {
            if (!tile.containedShip.eliminated)
            {
                attackingPlayer.hits[defendingPlayer.ID].Add(tile);
                if (!recentTurnInformation.hitShips.Contains(tile.containedShip))
                {
                    recentTurnInformation.hitShips.Add(tile.containedShip);
                }
                if (!tile.hit)
                {
                    tile.containedShip.RegisterHit();
                    if (tile.containedShip.eliminated)
                    {
                        recentTurnInformation.sunkShips.Add(tile.containedShip);
                    }
                }
            }
            else
            {
                attackingPlayer.misses[defendingPlayer.ID].Add(tile);
            }
        }
        else
        {
            attackingPlayer.misses[defendingPlayer.ID].Add(tile);
        }

        tile.hit = true;
        recentTurnInformation.hitTiles.Add(tile);
    }

    /// <summary>
    /// Changes the state of the battle immediately.
    /// </summary>
    /// <param name="state">The state to change to.</param>
    public void ChangeState(BattleState state)
    {
        BattleState lastState = this.currentState;

        this.nextState = state;
        this.currentState = state;
        switch (state)
        {
            case BattleState.TURN_FINISHED:
                ChangeState(BattleState.CHOOSING_TARGET, 1f);
                NextPlayer();
                break;
        }

        if (onBattleStateChange != null)
        {
            onBattleStateChange(lastState, state);
        }
    }

    /// <summary>
    /// Changes the state of the battle with a delay.
    /// </summary>
    /// <param name="state">The state to change to.</param>
    /// <param name="switchTime">The delay before changing.</param>    
    public void ChangeState(BattleState state, float switchTime)
    {
        nextState = state;
        this.switchTime = switchTime * switchTimeModifier;
    }

    /// <summary>
    /// Ends the battle.
    /// </summary>
    public void End()
    {
        foreach (Player player in players)
        {
            player.SetMacroMarker(-1);
            player.board.Set(BoardState.DISABLED);
        }

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Checks if a ship has been sunk and disables it, if it was.
    /// </summary>
    /// <param name="ship">The ship to check.</param>
    /// <returns>Ship sunk.</returns>
    public bool DisableSunkShip(Ship ship)
    {
        if (ship.eliminated)
        {
            ship.gameObject.SetActive(false);
        }

        return ship.eliminated;
    }

    /// <summary>
    /// Fires the guns of all living ships of the attacking player at the target tile.
    /// </summary>
    /// <param name="targetTile">The position of the tile to target.</param>
    /// <returns>The time it will take for the shells to arrive.</returns>
    public float FireGunsAtTargetTile(BoardTile targetTile)
    {
        float highestTravelTime = 0f;
        foreach (Ship ship in attackingPlayer.livingShips)
        {
            Vector3 targetPosition = targetTile.transform.position;
            targetPosition.y = 0f;
            float travelTime = ship.PrepareToFireAt(targetPosition, targetTile.containedShip);
            ship.Fire();

            if (travelTime > highestTravelTime)
            {
                highestTravelTime = travelTime;
            }

            //ship.FireAt(defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].worldPosition + Vector3.down * (GameController.playerBoardElevation - 0.4f));
        }

        // if (defendingPlayer.board.tiles[(int)targetTile.x, (int)targetTile.y].containedShip)
        // {
        //     defendingPlayer.board.tiles[(int)targetTile.x, (int)targetTile.y].containedShip.InformAboutIncomingProjectile(highestTravelTime, ProjectileType.SHELL);
        // }

        return highestTravelTime;
    }

    /// <summary>
    /// Returns the position from which torpedoes will be launched if the player decides to use them.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTorpedoLaunchPosition()
    {
        Vector3 position = Vector2.zero;

        if (Mathf.Abs(defendingPlayer.board.transform.position.z) < Mathf.Abs(defendingPlayer.board.transform.position.x))
        {
            position = defendingPlayer.board.transform.position - Vector3.right * GameController.playerBoardDistanceFromCenter * Mathf.Sign(defendingPlayer.board.transform.position.x);
        }
        else
        {
            position = defendingPlayer.board.transform.position - Vector3.forward * GameController.playerBoardDistanceFromCenter * Mathf.Sign(defendingPlayer.board.transform.position.z);
        }

        position.y = 0;
        return position;
    }

    /// <summary>
    /// Gets all the tiles in the line between launchPoint and stoppingPoint.
    /// </summary>
    /// <param name="launchPoint"></param>
    /// <param name="stoppingPoint"></param>
    /// <returns></returns>
    public BoardTile[] GetTorpedoHits(Vector3 launchPoint, Vector3 stoppingPoint)
    {
        List<BoardTile> hits = new List<BoardTile>();

        launchPoint.y = 0;
        stoppingPoint.y = 0;

        int steps = Mathf.CeilToInt(Vector3.Distance(launchPoint, stoppingPoint));

        for (int i = 0; i < steps; i++)
        {
            Vector3 inspectedWorldPosition = Vector3.Lerp(launchPoint, stoppingPoint, (float)i / (float)steps);
            BoardTile inspectedTile = defendingPlayer.board.GetTileAtWorldPosition(inspectedWorldPosition);

            if (inspectedTile != null && !hits.Contains(inspectedTile))
            {
                hits.Add(inspectedTile);
            }
        }

        return hits.ToArray();
    }

    /// <summary>
    /// Checks if a torpedo attack is available this turn.
    /// </summary>
    /// <returns></returns>
    public bool TorpedoAttackAvailable()
    {
        if (this != GameController.mainBattle)
        {
            return false;
        }

        int destroyers = 0;
        foreach (Ship ship in attackingPlayer.livingShips)
        {
            if (ship.type == ShipType.DESTROYER && ship.lengthRemaining == ship.length)
            {
                destroyers++;
            }
        }

        if (destroyers > 0 && attackingPlayer.torpedoRecharge == 0)
        {
            return true;
        }

        if (destroyers == 0)
        {
            attackingPlayer.torpedoRecharge = 9999999;
        }

        return false;
    }


}
