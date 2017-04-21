using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    /// <summary>
    /// The ship which occupies this tile.
    /// </summary>
    public Ship containedShip;
    /// <summary>
    /// Whether this tile was hit by a shell.
    /// </summary>
    public bool hit;
    /// <summary>
    /// The marker object of this tile.
    /// </summary>
    GameObject marker;
    /// <summary>
    /// The list of players who are able to detect a ship in this tile, without shooting at it.
    /// </summary>
    List<Player> revealedTo;
    /// <summary>
    /// The coordinates of this tile on the board.
    /// </summary>
    public Vector2 boardCoordinates;
    /// <summary>
    /// The board that contains this tile.
    /// </summary>
    public Board board;
    /// <summary>
    /// The awake function.
    /// </summary>
    void Awake()
    {
        revealedTo = new List<Player>();
    }
    /// <summary>
    /// Sets the color of this tile's marker.
    /// </summary>
    /// <param name="color">The color to set.</param>
    /// <param name="parent">Which transform to parent the marker object to.</param>
    public void SetMarker(Color color, Transform parent)
    {
        Destroy(marker);
        if (color.a != 0)
        {
            marker = new GameObject("Marker - " + name + " Simple.");
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.transform.parent = marker.transform;
            marker.transform.position = transform.position;
            marker.transform.parent = parent;
            marker.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
            marker.layer = 5;
            color.a = 0.6f;
            Renderer renderer = tmp.GetComponent<Renderer>();
            renderer.material = GameController.playerBoardMarkerMaterial;
            renderer.material.SetColor("_Color", color);
        }
    }

    /// <summary>
    /// /// Refreshes the graphics of this tile.
    /// </summary>
    /// <param name="boardState">The target state of the board.</param>
    public void Refresh(BoardState boardState)
    {
        switch (boardState)
        {
            case BoardState.FRIENDLY:
                if (containedShip != null)
                {
                    if (hit)
                    {
                        DrawShipStrip(Color.red);
                    }
                    else
                    {
                        DrawShipStrip(Color.green);
                    }
                }
                else
                {
                    if (hit)
                    {
                        SetMarker(Color.black, board.grid.transform);
                    }
                }
                break;
            case BoardState.ENEMY:
                if (board.owner.battle.attackingPlayer.hits[board.owner.ID].Contains(boardCoordinates))
                {
                    SetMarker(Color.red, board.grid.transform);
                }

                if (board.owner.battle.attackingPlayer.misses[board.owner.ID].Contains(boardCoordinates))
                {
                    SetMarker(Color.black, board.grid.transform);
                }
                break;
        }
    }

    /// <summary>
    /// Draws the central strip showing the location of the contained ship.
    /// </summary>
    /// <param name="stripColor"></param>
    void DrawShipStrip(Color stripColor)
    {

        float stripWidth = 0.35f;
        stripColor.a = 0.85f;
        Destroy(marker);
        marker = new GameObject("Marker - " + name + " Ship.");
        marker.transform.parent = board.grid.transform;
        marker.transform.position = transform.position;

        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "Ship Strip";
        Renderer renderer = strip.GetComponent<Renderer>();
        renderer.material = GameController.playerBoardMarkerMaterial;
        renderer.material.SetColor("_Color", stripColor);
        strip.transform.parent = marker.transform;
        strip.transform.localPosition = Vector3.zero;
        Vector2 shipDirection = Vector2.zero;
        bool middleSegment = false;
        foreach (Vector2 position in containedShip.tiles)
        {
            int distance = (int)(Mathf.Abs(position.x - boardCoordinates.x) + Mathf.Abs(position.y - boardCoordinates.y));
            if (distance == 1)
            {
                if (shipDirection != Vector2.zero)
                {
                    middleSegment = true;
                }
                shipDirection = position - boardCoordinates;
            }
        }

        if (middleSegment)
        {
            strip.transform.localScale = new Vector3(stripWidth + (0.9f - stripWidth) * Mathf.Abs(shipDirection.x), 0.1f, stripWidth + (0.9f - stripWidth) * Mathf.Abs(shipDirection.y));
        }
        else
        {
            strip.transform.localScale = new Vector3(stripWidth + (0.45f - stripWidth) * Mathf.Abs(shipDirection.x), 0.1f, stripWidth + (0.45f - stripWidth) * Mathf.Abs(shipDirection.y));
            strip.transform.localPosition = Vector3.Scale(new Vector3(0.225f, 0f, 0.225f), new Vector3(shipDirection.x, 0f, shipDirection.y));
        }
    }
}
