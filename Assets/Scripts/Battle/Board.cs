using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : ScriptableObject
{
    public struct BoardTile
    {
        public Ship containedShip;
        public bool hit;
        public Vector3 worldPosition;
        GameObject marker;
        public void SetMarker(Color color)
        {
            if (color.a != 0)
            {
                if (marker == null)
                {
                    marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.transform.position = worldPosition;
                    marker.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
                }

                color.a = 0.6f;
                Renderer renderer = marker.GetComponent<Renderer>();
                renderer.material = GameController.playerBoardMarkerMaterial;
                renderer.material.SetColor("_Color", color);
            }
            else
            {
                if (marker)
                {
                    Destroy(marker);
                    marker = null;
                }
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

        gridRendered = true;
        SetGridEnabled(false);
    }

    void DrawGrid(int dimensions, Material gridMaterial)
    {
        Destroy(grid);
        grid = new GameObject("RenderGrid");

        for (int x = 1; x < dimensions; x++)
        {
            float pos = -(float)dimensions / 2f + x;
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.GetComponent<Renderer>().material = gridMaterial;
            tmp.transform.localScale = new Vector3(0.1f, 0.1f, (float)dimensions);
            tmp.transform.localPosition = new Vector3(pos, 0f, 0f);
            tmp.transform.parent = grid.transform;
        }

        for (int y = 1; y < dimensions; y++)
        {
            float pos = -(float)dimensions / 2f + y;
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.GetComponent<Renderer>().material = gridMaterial;
            tmp.transform.localScale = new Vector3((float)dimensions, 0.1f, 0.1f);
            tmp.transform.localPosition = new Vector3(0f, 0f, pos);
            tmp.transform.parent = grid.transform;
        }

        grid.transform.position = position;
    }

    public void SetGridEnabled(bool active)
    {
        if (active != gridRendered)
        {
            gridRendered = active;
            if (active)
            {
                DrawGrid(dimensions, gridMaterial);
            }
            else
            {
                Destroy(grid);
                foreach (Ship ship in shipOwner.ships)
                {
                    ship.gameObject.SetActive(false);
                }

                foreach (BoardTile tile in tiles)
                {
                    tile.SetMarker(Color.clear);
                }
            }
        }
    }

    public void SetGridEnabled(bool active, bool hideShips)
    {
        if (active != gridRendered)
        {
            gridRendered = active;
            if (active)
            {
                DrawGrid(dimensions, gridMaterial);
            }
            else
            {
                Destroy(grid);
                if (hideShips)
                {
                    foreach (Ship ship in shipOwner.ships)
                    {
                        ship.gameObject.SetActive(false);
                    }
                }

                foreach (BoardTile tile in tiles)
                {
                    tile.SetMarker(Color.clear);
                }
            }
        }
    }

    public void ShowToFriendly()
    {
        SetGridEnabled(true);
        shipOwner.ShipsShown(true);

        for (int x = 0; x < dimensions; x++)
        {
            for (int y = 0; y < dimensions; y++)
            {
                if (tiles[x, y].containedShip)
                {
                    if (tiles[x, y].containedShip.sunk)
                    {
                        tiles[x, y].SetMarker(new Color(180f / 255f, 0f, 0f));
                    }
                    else
                    {
                        if (tiles[x, y].hit)
                        {
                            tiles[x, y].SetMarker(Color.red);
                        }
                        else
                        {
                            tiles[x, y].SetMarker(Color.green);
                        }
                    }
                }
                else
                {
                    if (tiles[x, y].hit)
                    {
                        tiles[x, y].SetMarker(Color.black);
                    }
                }
            }
        }
    }

    public void ShowToEnemy(Player enemy)
    {
        SetGridEnabled(true);
        Vector2[] hits = enemy.hits[shipOwner.ID].ToArray();
        Vector2[] misses = enemy.misses[shipOwner.ID].ToArray();

        foreach (Vector2 pos in hits)
        {
            SetMarkerAt(pos, Color.red);
        }

        foreach (Vector2 pos in misses)
        {
            SetMarkerAt(pos, Color.black);
        }
    }

    public void CamouflageBoard()
    {
        SetGridEnabled(false);
        Destroy(grid);
        grid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grid.transform.position = position;
        grid.transform.localScale = new Vector3(1f, 1f / (float)dimensions, 1f) * (float)dimensions;
        grid.name = "Player Icon";
        Renderer tmp = grid.GetComponent<Renderer>();
        tmp.material = gridMaterial;
        tmp.material.SetColor("_Color", shipOwner.color);
        tmp.material.SetColor("_EmissionColor", Color.clear);

        shipOwner.ShipsShown(false);
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

    public void SetMarkerAt(Vector2 position, Color color)
    {
        position = new Vector2((int)position.x, (int)position.y);
        if (IsPositionValid(position) && gridRendered)
        {
            tiles[(int)position.x, (int)position.y].SetMarker(color);
        }
    }

    public bool IsPositionValid(Vector2 position)
    {
        return (position.x >= 0 && position.y >= 0 && position.x < dimensions && position.y < dimensions);
    }
}
