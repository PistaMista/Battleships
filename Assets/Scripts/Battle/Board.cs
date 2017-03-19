using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardState { OVERHEAD, GRID_ONLY, ENEMY, FRIENDLY, SHIPS, DISABLED }

public class Board : ScriptableObject
{
    public struct BoardTile
    {
        public Ship containedShip;
        public bool hit;
        public Vector3 worldPosition;
        GameObject marker;
        public void SetMarker(Color color, Transform parent)
        {
            Destroy(marker);
            if (color.a != 0)
            {
                marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.transform.position = worldPosition;
                marker.transform.parent = parent;
                marker.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);

                color.a = 0.6f;
                Renderer renderer = marker.GetComponent<Renderer>();
                renderer.material = GameController.playerBoardMarkerMaterial;
                renderer.material.SetColor("_Color", color);
            }
        }
    }

    //The owner of this board's ships
    public Player shipOwner;
    //Where should this board be rendered
    public Vector3 position;
    //The tiles of this board
    public BoardTile[,] tiles;
    //The graphical grid
    public GameObject grid;
    //Is the grid rendered
    bool gridRendered = false;
    //The dimensions of the board - always a square
    public int dimensions;
    //The material used to render the grid
    Material gridMaterial;

    public void Initialize(int dimensions, Vector3 position, Player shipOwner, Material gridMaterial)
    {
        tiles = new BoardTile[dimensions, dimensions];

        for (int x = 0; x < dimensions; x++)
        {
            for (int y = 0; y < dimensions; y++)
            {
                tiles[x, y].worldPosition = position - new Vector3(1f, 0, 1f) * ((float)dimensions / 2f + 0.5f) + new Vector3(x + 1, 0f, y + 1);
            }
        }

        this.position = position;
        this.shipOwner = shipOwner;

        this.dimensions = dimensions;
        this.gridMaterial = gridMaterial;

        //gridRendered = true;
        Set(BoardState.DISABLED);
    }

    void DrawGrid(int dimensions, Material gridMaterial)
    {
        grid = new GameObject("RenderGrid");

        for (int x = 1; x < dimensions; x++)
        {
            float pos = -(float)dimensions / 2f + x;
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Material material = gridMaterial;
            if (x == 1 || x == dimensions - 1)
            {
                material = new Material(gridMaterial);
                material.SetColor("_Color", Color.clear);
                material.SetColor("_EmissionColor", shipOwner.color);
                tmp.transform.localPosition = new Vector3(pos, 0.01f, 0f);
            }
            else
            {
                tmp.transform.localPosition = new Vector3(pos, 0f, 0f);
            }
            tmp.GetComponent<Renderer>().material = material;
            tmp.transform.localScale = new Vector3(0.1f, 0.1f, (float)dimensions);
            tmp.transform.parent = grid.transform;
        }

        for (int y = 1; y < dimensions; y++)
        {
            float pos = -(float)dimensions / 2f + y;
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Material material = gridMaterial;
            if (y == 1 || y == dimensions - 1)
            {
                material = new Material(gridMaterial);
                material.SetColor("_Color", Color.clear);
                material.SetColor("_EmissionColor", shipOwner.color);
                tmp.transform.localPosition = new Vector3(0f, 0.01f, pos);
            }
            else
            {
                tmp.transform.localPosition = new Vector3(0f, 0f, pos);
            }
            tmp.GetComponent<Renderer>().material = material;
            tmp.transform.localScale = new Vector3((float)dimensions, 0.1f, 0.1f);
            tmp.transform.parent = grid.transform;
        }

        grid.transform.position = position;
    }

    public void Set(BoardState state)
    {
        Destroy(grid);
        gridRendered = true;
        switch (state)
        {
            case BoardState.DISABLED:
                gridRendered = false;
                break;
            case BoardState.GRID_ONLY:
                DrawGrid(dimensions, GameController.playerBoardGridMaterial);
                break;
            case BoardState.ENEMY:
                DrawGrid(dimensions, GameController.playerBoardGridMaterial);
                Vector2[] hits = shipOwner.battle.attackingPlayer.hits[shipOwner.ID].ToArray();
                Vector2[] misses = shipOwner.battle.attackingPlayer.misses[shipOwner.ID].ToArray();

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
                for (int x = 0; x < dimensions; x++)
                {
                    for (int y = 0; y < dimensions; y++)
                    {
                        BoardTile tile = tiles[x, y];
                        if (tile.containedShip)
                        {
                            if (tile.containedShip.sunk)
                            {
                                SetMarker(tile, new Color(180f / 255f, 0f, 0f));
                            }
                            else
                            {
                                if (tile.hit)
                                {
                                    SetMarker(tile, Color.red);
                                }
                                else
                                {
                                    SetMarker(tile, Color.green);
                                }
                            }
                        }
                        else
                        {
                            if (tile.hit)
                            {
                                SetMarker(tile, Color.black);
                            }
                        }

                        tiles[x, y] = tile;
                    }
                }

                shipOwner.ShipsShown(true);
                break;
            case BoardState.OVERHEAD:
                grid = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grid.transform.position = position;
                grid.transform.localScale = new Vector3(1f, 1f / (float)dimensions, 1f) * (float)dimensions;
                grid.name = "Player Icon";
                Renderer tmp = grid.GetComponent<Renderer>();
                tmp.material = gridMaterial;
                tmp.material.SetColor("_Color", shipOwner.color);
                tmp.material.SetColor("_EmissionColor", Color.clear);

                gridRendered = false;
                shipOwner.ShipsShown(false);
                break;
            case BoardState.SHIPS:
                gridRendered = false;
                shipOwner.ShipsShown(true);
                break;
        }
    }

    public Vector2 WorldToTilePosition(Vector3 position)
    {
        Vector3 result = position - this.position + Vector3.one * ((float)dimensions / 2f);
        if (result.x < 0 || result.x >= dimensions || result.z < 0 || result.z >= dimensions)
        {
            result = -Vector3.one;
        }

        return new Vector2(Mathf.Floor(result.x), Mathf.Floor(result.z));
    }

    public void SetMarker(Vector2 position, Color color)
    {
        position = new Vector2((int)position.x, (int)position.y);
        if (IsPositionValid(position) && gridRendered)
        {
            tiles[(int)position.x, (int)position.y].SetMarker(color, grid.transform);
        }
    }

    public void SetMarker(BoardTile tile, Color color)
    {
        tile.SetMarker(color, grid.transform);
    }

    public bool IsPositionValid(Vector2 position)
    {
        return (position.x >= 0 && position.y >= 0 && position.x < dimensions && position.y < dimensions);
    }
}
