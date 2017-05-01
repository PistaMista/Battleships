using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the possible board states.
/// </summary>
public enum BoardState
{
    /// <summary>
    /// State to be used in overhead battle view.
    /// </summary>
    OVERHEAD,
    /// <summary>
    /// Shows only the board grid.
    /// </summary>
    GRID_ONLY,
    /// <summary>
    /// Shows, whatever should be shown to an enemy.
    /// </summary>
    ENEMY,
    /// <summary>
    /// Shows, whatever should be shown to a friendly.
    /// </summary>
    FRIENDLY,
    /// <summary>
    /// Shows only the ships.
    /// </summary>
    SHIPS,
    /// <summary>
    /// Mode to be used while placing ships.
    /// </summary>
    PLACING,
    /// <summary>
    /// Disables all graphical features of the board.
    /// </summary>
    DISABLED
}

public class Board : MonoBehaviour
{
    //     /// <summary>
    //     /// The tiles the board is made up of.
    //     /// </summary>
    //     public struct BoardTile
    //     {
    //         /// <summary>
    //         /// The ship which occupies this tile.
    //         /// </summary>
    //         public Ship containedShip;
    //         /// <summary>
    //         /// Whether this tile was hit by a shell.
    //         /// </summary>
    //         public bool hit;
    //         /// <summary>
    //         /// The position in the world of this tile.
    //         /// </summary>
    //         public Vector3 worldPosition;
    //         /// <summary>
    //         /// The marker object of this tile.
    //         /// </summary>
    //         GameObject marker;
    //         /// <summary>
    //         /// Sets the color of this tile's marker.
    //         /// </summary>
    //         /// <param name="color">The color to set.</param>
    //         /// <param name="parent">Which transform to parent the marker object to.</param>
    //         public void SetMarker(Color color, Transform parent)
    //         {
    //             Destroy(marker);
    //             if (color.a != 0)
    //             {
    //                 marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //                 marker.transform.position = worldPosition;
    //                 marker.transform.parent = parent;
    //                 marker.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
    //                 marker.layer = 5;
    //                 color.a = 0.6f;
    //                 Renderer renderer = marker.GetComponent<Renderer>();
    //                 renderer.material = GameController.playerBoardMarkerMaterial;
    //                 renderer.material.SetColor("_Color", color);
    //             }
    //         }
    //     }

    /// <summary>
    /// The player who owns the board.
    /// </summary>
    public Player owner;
    /// <summary>
    /// The tiles, that the board is made up of.
    /// </summary>
    public BoardTile[,] tiles;
    /// <summary>
    /// The graphical grid.
    /// </summary>
    public GameObject grid;
    /// <summary>
    /// Whether the grid is rendered.
    /// </summary>
    bool gridRendered = false;
    /// <summary>
    /// The dimensions of the board. The length of a square side.
    /// </summary>
    public int dimensions;
    /// <summary>
    /// The material used to render the grid.
    /// </summary>
    Material gridMaterial;

    /// <summary>
    /// Initializes the board.
    /// </summary>
    /// <param name="dimensions">The dimensions of the board. The length of a square side.</param>
    /// <param name="position">The position of this board in the world.</param>
    /// <param name="owner">The owner of this board.</param>
    /// <param name="gridMaterial">The material used to render the grid.</param>    
    public void Initialize(int dimensions, Vector3 position, Player owner, Material gridMaterial)
    {
        tiles = new BoardTile[dimensions, dimensions];

        for (int x = 0; x < dimensions; x++)
        {
            for (int y = 0; y < dimensions; y++)
            {
                tiles[x, y] = new GameObject("Tile X: " + x + " Y: " + y).AddComponent<BoardTile>();
                tiles[x, y].transform.position = -new Vector3(1f, 0, 1f) * ((float)dimensions / 2f + 0.5f) + new Vector3(x + 1, 0f, y + 1);
                tiles[x, y].transform.parent = this.transform;
                tiles[x, y].boardCoordinates = new Vector2(x, y);
                tiles[x, y].board = this;
            }
        }

        //this.position = position;
        this.transform.position = position;
        this.transform.parent = owner.transform;
        this.owner = owner;


        this.dimensions = dimensions;
        this.gridMaterial = gridMaterial;

        //gridRendered = true;
        Set(BoardState.DISABLED);
    }
    /// <summary>
    /// Draws the grid lines.
    /// </summary>
    /// <param name="dimensions">The dimensions of the grid. The length of a square side.</param>
    /// <param name="gridMaterial">The material used to render the grid.</param>
    void DrawGrid(int dimensions, Material gridMaterial)
    {
        grid = new GameObject("RenderGrid");
        grid.layer = 5;

        for (int x = 1; x < dimensions; x++)
        {
            float pos = -(float)dimensions / 2f + x;
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Renderer renderer = tmp.GetComponent<Renderer>();
            renderer.material = gridMaterial;

            if (x == 1 || x == dimensions - 1)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", Color.clear);
                block.SetColor("_EmissionColor", owner.color);
                renderer.SetPropertyBlock(block);

                tmp.transform.localPosition = new Vector3(pos, 0.1f, 0f);
            }
            else
            {
                tmp.transform.localPosition = new Vector3(pos, 0f, 0f);
            }

            tmp.transform.localScale = new Vector3(0.1f, 0.1f, (float)dimensions);
            tmp.transform.parent = grid.transform;
            tmp.layer = 5;
        }

        for (int y = 1; y < dimensions; y++)
        {
            float pos = -(float)dimensions / 2f + y;
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Renderer renderer = tmp.GetComponent<Renderer>();
            renderer.material = gridMaterial;

            if (y == 1 || y == dimensions - 1)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", Color.clear);
                block.SetColor("_EmissionColor", owner.color);
                renderer.SetPropertyBlock(block);

                tmp.transform.localPosition = new Vector3(0f, 0.1f, pos);
            }
            else
            {
                tmp.transform.localPosition = new Vector3(0f, 0f, pos);
            }

            tmp.transform.localScale = new Vector3((float)dimensions, 0.1f, 0.1f);
            tmp.transform.parent = grid.transform;
            tmp.layer = 5;
        }

        grid.transform.parent = this.transform;
        grid.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Sets the board to a different state.
    /// </summary>
    /// <param name="state">The board state to switch to.</param>
    public void Set(BoardState state)
    {
        Destroy(grid);
        gridRendered = true;



        switch (state)
        {
            case BoardState.DISABLED:
                gridRendered = false;
                owner.ShipsShown(false);
                break;
            case BoardState.GRID_ONLY:
                DrawGrid(dimensions, GameController.playerBoardGridMaterial);
                break;
            case BoardState.ENEMY:
                DrawGrid(dimensions, GameController.playerBoardGridMaterial);
                Vector2[] hits = owner.battle.attackingPlayer.hits[owner.ID].ToArray();
                Vector2[] misses = owner.battle.attackingPlayer.misses[owner.ID].ToArray();

                foreach (Vector2 pos in hits)
                {
                    SetMarker(pos, Color.red);
                }

                foreach (Vector2 pos in misses)
                {
                    SetMarker(pos, Color.black);
                }
                break;
            case BoardState.FRIENDLY:
                DrawGrid(dimensions, GameController.playerBoardGridMaterial);
                owner.ShipsShown(true, true);
                break;
            case BoardState.PLACING:
                DrawGrid(dimensions, GameController.playerBoardGridMaterial);
                break;
            case BoardState.OVERHEAD:
                grid = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grid.transform.parent = this.transform;
                grid.transform.localPosition = Vector3.zero;
                grid.transform.localScale = new Vector3(1f, 1f / (float)dimensions, 1f) * (float)dimensions;
                grid.name = "Player Icon";
                Renderer tmp = grid.GetComponent<Renderer>();
                tmp.material = gridMaterial;
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", owner.color);
                block.SetColor("_EmissionColor", Color.clear);
                tmp.SetPropertyBlock(block);
                gridRendered = false;
                owner.ShipsShown(false);
                break;
            case BoardState.SHIPS:
                gridRendered = false;
                owner.ShipsShown(true, true);
                break;
        }

        foreach (BoardTile tile in tiles)
        {
            tile.Refresh(state);
        }
    }

    /// <summary>
    /// Converts a position in the world to a position of the tile on this board.
    /// </summary>
    /// <param name="position">The world position to convert.</param>
    /// <returns>The converted board position.</returns>
    public Vector2 WorldToTilePosition(Vector3 position)
    {
        Vector3 result = position - this.transform.position + Vector3.one * ((float)dimensions / 2f);
        if (result.x < 0 || result.x >= dimensions || result.z < 0 || result.z >= dimensions)
        {
            result = -Vector3.one;
        }

        return new Vector2(Mathf.Floor(result.x), Mathf.Floor(result.z));
    }

    /// <summary>
    /// Sets the color of the marker at position.
    /// </summary>
    /// <param name="position">The position of the tile on the board.</param>
    /// <param name="color">The color to set that tile's marker to.</param>
    public void SetMarker(Vector2 position, Color color)
    {
        position = new Vector2((int)position.x, (int)position.y);
        if (IsPositionValid(position) && gridRendered)
        {
            tiles[(int)position.x, (int)position.y].SetMarker(color, grid.transform);
        }
    }
    /// <summary>
    /// Sets the color of the marker of a tile.
    /// </summary>
    /// <param name="tile">The tile of which to set the marker color.</param>
    /// <param name="color">The color to set that tile's marker to.</param>    
    public void SetMarker(BoardTile tile, Color color)
    {
        tile.SetMarker(color, grid.transform);
    }
    /// <summary>
    /// Determines whether a position is a valid position on the board.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>Validity of this board position.</returns>
    public bool IsPositionValid(Vector2 position)
    {
        return (position.x >= 0 && position.y >= 0 && position.x < dimensions && position.y < dimensions);
    }
}
