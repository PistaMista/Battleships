using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (battle.attackingPlayer == this)
        {
            Think();
        }
    }

    void Think()
    {
        if (aircraftCarrier.activeSquadron != null)
        {
            Aircraft();
        }

        if (battle.switchTime <= -0.1f && battle.currentState == battle.nextState)
        {
            switch (battle.currentState)
            {
                case BattleState.CHOOSING_TARGET:
                    Target();
                    break;
                case BattleState.CHOOSING_TILE_TO_SHOOT:
                    Attack();
                    break;
            }
        }
    }

    /// <summary>
    /// Decides where to send the aircraft.
    /// </summary>
    void Aircraft()
    {
        int[] playerBias = new int[battle.players.Length];
        for (int i = 0; i < battle.players.Length; i++)
        {
            playerBias[i] = 0;
            Player player = battle.players[i];
            if (aircraftCarrier.activeSquadron.Target == player)
            {
                playerBias[i] += aircraftCarrier.activeSquadron.travelTime * 3;
            }

            if (player == this)
            {
                int totalAircraft = 0;
                foreach (ActiveAircraft squadron in overheadSquadrons)
                {
                    totalAircraft += squadron.aircraft.Count;
                }

                playerBias[i] += totalAircraft * overheadSquadrons.Count;
            }
            else
            {
                int attackBias = 0;

                attackBias += (int)((27f - hits[player.ID].Count) / 4f);
            }
        }

        int highestID = 0;
        for (int i = 0; i < playerBias.Length; i++)
        {
            highestID = playerBias[highestID] >= playerBias[i] ? highestID : i;
        }

        aircraftCarrier.activeSquadron.NextTarget = battle.players[highestID];
    }

    /// <summary>
    /// Decides which player to attack.
    /// </summary>
    void Target()
    {
        int randomTargetID = Random.Range(0, battle.players.Length);
        while (randomTargetID == battle.attackingPlayerID)
        {
            randomTargetID = Random.Range(0, battle.players.Length);
        }

        if (battle.SelectTarget(battle.players[randomTargetID]))
        {
            battle.ChangeState(BattleState.CHOOSING_TILE_TO_SHOOT, 1.2f);
        }
    }
    /// <summary>
    /// Decides the attacking move.
    /// </summary>
    void Attack()
    {
        BoardTile tileToShoot = ChooseTileToAttack();
        if (battle.TorpedoAttackAvailable())
        {
            battle.TorpedoAttack(tileToShoot.transform.position - battle.GetTorpedoLaunchPosition());
        }
        else
        {
            battle.ArtilleryAttack(tileToShoot);
        }
    }


    /// <summary>
    /// Calculates the optimal tile to attack.
    /// </summary>
    /// <returns>Optimal tile to target.</returns>
    public BoardTile ChooseTileToAttack()
    {


        List<BoardTile> hits = this.hits[battle.defendingPlayer.ID];
        List<BoardTile> misses = this.misses[battle.defendingPlayer.ID];

        foreach (BoardTile tile in battle.defendingPlayer.board.tiles)
        {
            if (tile.revealedTo.Contains(this) && !(hits.Contains(tile) || misses.Contains(tile)))
            {
                return tile;
            }
        }

        List<BoardTile> processedTiles = new List<BoardTile>();
        Dictionary<int, List<BoardTile>> rankedTiles = new Dictionary<int, List<BoardTile>>();

        int highestRank = 0;

        for (int i = 1; i <= 10; i++)
        {
            rankedTiles.Add(i, new List<BoardTile>());
        }

        Vector2[] cardinalDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        //Eliminate the misses
        foreach (BoardTile miss in misses)
        {
            processedTiles.Add(miss);
        }

        //Analyze hits
        foreach (BoardTile hit in hits)
        {
            if (!processedTiles.Contains(hit))
            {
                processedTiles.Add(hit);


                Vector2 examinedDirection = Vector2.zero;
                foreach (Vector2 direction in cardinalDirections)
                {
                    Vector2 checkedPosition = hit.boardCoordinates + direction;
                    if (battle.defendingPlayer.board.IsPositionValid(checkedPosition))
                    {
                        BoardTile checkedTile = battle.defendingPlayer.board.tiles[(int)checkedPosition.x, (int)checkedPosition.y];
                        if (hits.Contains(checkedTile))
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
                        for (int i = 1; i < battle.defendingPlayer.board.dimensions; i++)
                        {
                            Vector2 checkedPosition = hit.boardCoordinates + examinedDirection * i * direction;
                            if (battle.defendingPlayer.board.IsPositionValid(checkedPosition))
                            {

                                BoardTile checkedTile = battle.defendingPlayer.board.tiles[(int)checkedPosition.x, (int)checkedPosition.y];
                                processedTiles.Add(checkedTile);
                                if (!hits.Contains(checkedTile) && !misses.Contains(checkedTile))
                                {
                                    rankedTiles[10].Add(checkedTile);
                                    if (10 > highestRank)
                                    {
                                        highestRank = 10;
                                    }
                                    break;
                                }
                                else if (misses.Contains(checkedTile))
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
                        Vector2 checkedPosition = hit.boardCoordinates + direction;
                        if (battle.defendingPlayer.board.IsPositionValid(checkedPosition))
                        {
                            BoardTile checkedTile = battle.defendingPlayer.board.tiles[(int)checkedPosition.x, (int)checkedPosition.y];
                            if (!processedTiles.Contains(checkedTile))
                            {
                                processedTiles.Add(checkedTile);
                                rankedTiles[10].Add(checkedTile);
                                if (10 > highestRank)
                                {
                                    highestRank = 10;
                                }
                            }
                        }
                    }
                }
            }
        }



        //Add the other tiles
        foreach (BoardTile candidateTile in battle.defendingPlayer.board.tiles)
        {
            if (!processedTiles.Contains(candidateTile))
            {
                rankedTiles[1].Add(candidateTile);
                if (1 > highestRank)
                {
                    highestRank = 1;
                }
            }
        }

        BoardTile result = null;
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

        //Debug.Log(result);
        return result;
    }
}
