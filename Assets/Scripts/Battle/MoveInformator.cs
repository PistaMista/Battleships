using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInformator : ScriptableObject
{
    /// <summary>
    /// The attacker this turn.
    /// </summary>
    public Player attacker;
    /// <summary>
    /// The position of the tile which was shot or the direction of launched torpedoes.
    /// </summary>
    public Vector2 target;
    /// <summary>
    /// The weapon used for the attack.
    /// </summary>
    public AttackType type;
    /// <summary>
    /// The ships which were sunk.
    /// </summary>
    public List<Ship> sunkShips;
    /// <summary>
    /// The ships which were hit.
    /// </summary>
    public List<Ship> hitShips;
    /// <summary>
    /// The tiles which were hit.
    /// </summary>
    public List<BoardTile> hitTiles;

    /// <summary>
    /// Resets the informator.
    /// </summary>
    public void Reset()
    {
        attacker = null;
        target = Vector2.zero;
        type = AttackType.ARTILLERY;
        sunkShips = new List<Ship>();
        hitShips = new List<Ship>();
        hitTiles = new List<BoardTile>();
    }
}
