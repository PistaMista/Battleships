using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BattleState { CHOOSING_TARGET, CHOOSING_TILE_TO_SHOOT, FIRING, TURN_FINISHED, FRIENDLY_SHIP_PREVIEW, SHOWING_HIT_TILE }

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
    public BattleState state;
    /// <summary>
    /// The delay before switching to the next state.
    /// </summary>
    public float switchTime;
    /// <summary>
    /// The state to switch to after switch time is over.
    /// </summary>
    public BattleState targetState;

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
    public delegate void OnFire();
    /// <summary>
    /// On firing guns.
    /// </summary>
    public OnFire onFire;
    /// <summary>
    /// The tile which was targeted in this frame.
    /// </summary>
    public Vector2 recentlyShot;


    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        if (battling)
        {
            if (switchTime <= 0)
            {
                if (state != targetState)
                {
                    ChangeState(targetState);
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

            if (!isMainBattle)
            {
                AIPlayerActions();
            }
        }
    }

    /// <summary>
    /// Processes actions for AI players.
    /// </summary>    
    void AIPlayerActions()
    {
        if (switchTime <= -0.1f)
        {
            switch (state)
            {
                case BattleState.CHOOSING_TARGET:
                    int randomTargetID = Random.Range(0, players.Length);
                    while (randomTargetID == attackingPlayerID)
                    {
                        randomTargetID = Random.Range(0, players.Length);
                    }

                    if (SelectTarget(players[randomTargetID]))
                    {
                        ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 0.2f);
                    }
                    break;
                case BattleState.CHOOSING_TILE_TO_SHOOT:
                    Vector2 positionToShoot = ChooseTileToAttackForAIPlayer();

                    while (!HitTile(positionToShoot))
                    {
                        positionToShoot = ChooseTileToAttackForAIPlayer();
                    }

                    ChangeState(BattleState.TURN_FINISHED, 0.2f);
                    break;
            }
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

        if (isMainBattle)
        {
            GameController.ChangeState(GameState.BATTLING);
            BattleInterface.Attach(this);
        }
        else
        {
            foreach (Player player in players)
            {
                player.ShipsShown(true);
            }
        }
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

        attackingPlayer = players[attackingPlayerID];
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
    /// Registers a hit on the targeted player's tile.
    /// </summary>
    /// <param name="tile">Position of the tile to hit.</param>
    /// <returns>Hit successful.</returns>
    public bool HitTile(Vector2 tile)
    {
        if (defendingPlayer)
        {
            if (defendingPlayer.board.IsPositionValid(tile))
            {
                if (!attackingPlayer.hits[defendingPlayer.ID].Contains(tile) && !attackingPlayer.misses[defendingPlayer.ID].Contains(tile))
                {
                    recentlyShot = tile;

                    targetState = BattleState.TURN_FINISHED;
                    switchTime = 0.5f;



                    if (defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].containedShip)
                    {
                        if (!defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].containedShip.eliminated)
                        {
                            attackingPlayer.hits[defendingPlayer.ID].Add(tile);

                            if (!defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].hit)
                            {
                                defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].containedShip.RegisterHit();
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

                    defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].hit = true;

                    if (onFire != null)
                    {
                        onFire();
                    }

                    if (!defendingPlayer.alive)
                    {
                        playersAlive--;
                    }

                    if (!isMainBattle)
                    {
                        FireGunsAtTargetTile(tile);
                    }

                    ChangeState(BattleState.FIRING);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Changes the state of the battle immediately.
    /// </summary>
    /// <param name="state">The state to change to.</param>
    public void ChangeState(BattleState state)
    {
        BattleState lastState = this.state;

        this.targetState = state;
        this.state = state;
        switch (state)
        {
            case BattleState.TURN_FINISHED:
                if (!isMainBattle)
                {
                    ChangeState(BattleState.CHOOSING_TARGET, 1f);
                }
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
        targetState = state;
        this.switchTime = switchTime;
    }

    /// <summary>
    /// Calculates the optimal position of tile to attack for AI players.
    /// </summary>
    /// <returns>Position of optimal tile to target.</returns>
    public Vector2 ChooseTileToAttackForAIPlayer()
    {
        List<Vector2> hits = attackingPlayer.hits[defendingPlayer.ID];
        List<Vector2> misses = attackingPlayer.misses[defendingPlayer.ID];

        List<Vector2> processedTiles = new List<Vector2>();
        Dictionary<int, List<Vector2>> rankedTiles = new Dictionary<int, List<Vector2>>();

        int highestRank = 0;

        for (int i = 1; i <= 10; i++)
        {
            rankedTiles.Add(i, new List<Vector2>());
        }

        Vector2[] cardinalDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        //Analyze hits
        foreach (Vector2 hit in hits)
        {
            if (!processedTiles.Contains(hit))
            {
                processedTiles.Add(hit);


                Vector2 examinedDirection = Vector2.zero;
                foreach (Vector2 direction in cardinalDirections)
                {
                    Vector2 checkedPosition = hit + direction;
                    if (defendingPlayer.board.IsPositionValid(checkedPosition))
                    {
                        if (hits.Contains(checkedPosition))
                        {
                            examinedDirection = direction;
                            break;
                        }
                    }
                }

                if (examinedDirection != Vector2.zero)
                {
                    for (int direction = -1; direction <= 1; direction += 2)
                    {
                        for (int i = 1; i < defendingPlayer.board.dimensions; i++)
                        {
                            Vector2 checkedPosition = hit + examinedDirection * i * direction;
                            if (defendingPlayer.board.IsPositionValid(checkedPosition))
                            {
                                //if (!processedTiles.Contains(checkedPosition))
                                //{
                                processedTiles.Add(checkedPosition);
                                if (!hits.Contains(checkedPosition) && !misses.Contains(checkedPosition))
                                {
                                    rankedTiles[10].Add(checkedPosition);
                                    if (10 > highestRank)
                                    {
                                        highestRank = 10;
                                    }
                                    break;
                                }
                                else if (misses.Contains(checkedPosition))
                                {
                                    break;
                                }
                                //}
                                //else
                                //{
                                //   break;
                                //}
                            }
                        }
                    }
                }
                else
                {
                    foreach (Vector2 direction in cardinalDirections)
                    {
                        Vector2 checkedPosition = hit + direction;
                        if (defendingPlayer.board.IsPositionValid(checkedPosition))
                        {
                            processedTiles.Add(checkedPosition);
                            rankedTiles[10].Add(checkedPosition);
                            if (10 > highestRank)
                            {
                                highestRank = 10;
                            }
                        }
                    }
                }
            }
        }

        //Add the other tiles
        for (int x = 0; x < defendingPlayer.board.dimensions; x++)
        {
            for (int y = 0; y < defendingPlayer.board.dimensions; y++)
            {
                Vector2 candidatePosition = new Vector2(x, y);
                if (!processedTiles.Contains(candidatePosition))
                {
                    rankedTiles[1].Add(candidatePosition);
                    if (1 > highestRank)
                    {
                        highestRank = 1;
                    }
                }
            }
        }

        Vector2 result = Vector2.zero;
        //Choose a tile to attack
        int targetRank = Random.Range(0, highestRank - 1);
        //Choose a rank to pick a tile from
        for (int i = 1; i <= 10; i++)
        {
            if (targetRank < i && rankedTiles[i].Count > 0)
            {
                targetRank = i;
                break;
            }
        }

        result = rankedTiles[targetRank][Random.Range(0, rankedTiles[targetRank].Count - 1)];

        return result;
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
    public float FireGunsAtTargetTile(Vector2 targetTile)
    {
        float highestTravelTime = 0f;
        foreach (Ship ship in attackingPlayer.livingShips)
        {
            Vector3 targetPosition = defendingPlayer.board.tiles[(int)targetTile.x, (int)targetTile.y].worldPosition;
            targetPosition.y = 0f;
            float travelTime = ship.FireAt(targetPosition);
            if (travelTime > highestTravelTime)
            {
                highestTravelTime = travelTime;
            }

            //ship.FireAt(defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].worldPosition + Vector3.down * (GameController.playerBoardElevation - 0.4f));
        }

        if (defendingPlayer.board.tiles[(int)targetTile.x, (int)targetTile.y].containedShip)
        {
            defendingPlayer.board.tiles[(int)targetTile.x, (int)targetTile.y].containedShip.InformAboutShellTravelTime(highestTravelTime);
        }

        return highestTravelTime;
    }
}
