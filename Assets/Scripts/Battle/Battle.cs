using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BattleState { CHOOSING_TARGET, CHOOSING_TILE_TO_SHOOT, FIRING, TURN_FINISHED, FRIENDLY_SHIP_PREVIEW, SHOWING_HIT_TILE }

public class Battle : MonoBehaviour
{

    //Is the battle in progress?
    public bool battling = false;
    //Is this the main battle of the game?
    public bool isMainBattle = false;
    //The players competing in this battle
    public Player[] players;
    //All the ships competing in this battle
    public List<Ship> ships;
    //How many players are still alive
    public int playersAlive;
    //The ID of the player on the turn
    public int attackingPlayerID;
    //The player on the turn
    public Player attackingPlayer;
    //The player targeted by the attackingPlayer
    public Player defendingPlayer;
    //The state of the battle
    public BattleState state;
    //The delay before switching to the next state
    public float switchTime;
    //The state to switch to after switchTime is over
    public BattleState targetState;

    //Delegates for external modules to tap into
    //On next player
    public delegate void OnPlayerSwitch(Player switchingFrom, Player switchingTo);
    public OnPlayerSwitch onPlayerSwitch;
    //On battle state switch
    public delegate void OnBattleStateChange(BattleState switchingFrom, BattleState switchingTo);
    public OnBattleStateChange onBattleStateChange;
    //On gun fire
    public delegate void OnFire();
    public OnFire onFire;


    void Start()
    {

    }

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

                    while (!ShootAtTile(positionToShoot))
                    {
                        positionToShoot = ChooseTileToAttackForAIPlayer();
                    }

                    ChangeState(BattleState.TURN_FINISHED, 0.2f);
                    break;
            }
        }
    }

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

    public bool ShootAtTile(Vector2 tile)
    {
        if (defendingPlayer)
        {
            if (defendingPlayer.board.IsPositionValid(tile))
            {
                if (!attackingPlayer.hits[defendingPlayer.ID].Contains(tile) && !attackingPlayer.misses[defendingPlayer.ID].Contains(tile))
                {
                    targetState = BattleState.TURN_FINISHED;
                    switchTime = 0.5f;


                    defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].hit = true;
                    if (defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].containedShip)
                    {
                        if (!defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].containedShip.sunk)
                        {
                            attackingPlayer.hits[defendingPlayer.ID].Add(tile);
                            defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].containedShip.Hit();
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
                        foreach (Ship ship in attackingPlayer.ships)
                        {
                            Vector3 targetPosition = defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].worldPosition;
                            targetPosition.y = 0f;
                            ship.FireAt(targetPosition);
                            //ship.FireAt(defendingPlayer.board.tiles[(int)tile.x, (int)tile.y].worldPosition + Vector3.down * (GameController.playerBoardElevation - 0.4f));
                        }
                    }

                    ChangeState(BattleState.FIRING);
                    return true;
                }
            }
        }

        return false;
    }

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

    public void ChangeState(BattleState state, float switchTime)
    {
        targetState = state;
        this.switchTime = switchTime;
    }



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

    public void End()
    {
        foreach (Player player in players)
        {
            player.SetMacroMarker(-1);
            player.board.Set(BoardState.DISABLED);
        }

        foreach (Ship ship in ships)
        {
            Destroy(ship.gameObject);
        }

        Destroy(this.gameObject);
    }

    public bool DestroySunkShip(Ship ship)
    {
        if (ship.sunk)
        {
            ships.Remove(ship);
            Destroy(ship.gameObject);
        }

        return ship.sunk;
    }
}
