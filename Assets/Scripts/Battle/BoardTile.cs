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
            // Renderer renderer = tmp.GetComponent<Renderer>();
            // renderer.material = GameController.playerBoardMarkerMaterial;
            // renderer.material.SetColor("_Color", color);

            Renderer renderer = tmp.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.material = GameController.playerBoardMarkerMaterial;
            block.SetColor("_Color", color);
            //renderer.material.SetColor("_Color", colors[color]);
            renderer.SetPropertyBlock(block);
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
                        List<Color> hitBy = new List<Color>();
                        foreach (Player player in board.owner.battle.players)
                        {
                            if (player.hits.ContainsKey(board.owner.ID))
                            {
                                if (player.hits[board.owner.ID].Contains(boardCoordinates))
                                {
                                    hitBy.Add(player.color);
                                }
                            }
                        }

                        DrawSideStrips(hitBy.ToArray());
                    }
                    else
                    {
                        DrawShipStrip(Color.green);
                        DrawSideStrips(new Color[] { new Color(10f / 255f, 120f / 255f, 0f, 1f) });
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
            case BoardState.PLACING:
                if (containedShip != null)
                {
                    DrawShipStrip(Color.yellow);
                    DrawSideStrips(new Color[] { Color.black });
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
    /// Whether this ship strip is in the middle of a ship.
    /// </summary>
    bool middleSegment;
    /// <summary>
    /// The orientation of the ship contained in this tile.
    /// </summary>
    Vector2 shipDirection;
    /// <summary>
    /// The last color of the ship strip.
    /// </summary>
    Color lastColor;
    /// <summary>
    /// The width of the ship strip.
    /// </summary>
    float stripWidth = 0.35f;
    /// <summary>
    /// Draws the central strip showing the location of the contained ship.
    /// </summary>
    /// <param name="stripColor"></param>
    void DrawShipStrip(Color stripColor)
    {
        if (stripColor != lastColor)
        {
            stripColor.a = 0.85f;
            Destroy(marker);
            marker = new GameObject("Marker - " + name + " Ship.");
            marker.transform.parent = board.grid.transform;
            marker.transform.position = transform.position;

            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "Ship Strip";
            // Renderer renderer = strip.GetComponent<Renderer>();
            // renderer.material = GameController.playerBoardMarkerMaterial;
            // renderer.material.SetColor("_Color", stripColor);
            Renderer renderer = strip.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.material = GameController.playerBoardMarkerMaterial;
            block.SetColor("_Color", stripColor);
            //renderer.material.SetColor("_Color", colors[color]);
            renderer.SetPropertyBlock(block);

            strip.transform.parent = marker.transform;
            strip.transform.localPosition = Vector3.zero;
            shipDirection = Vector2.zero;
            middleSegment = false;
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

            float scaleModifier = 0f;
            if (middleSegment)
            {
                scaleModifier = (0.9f - stripWidth);
            }
            else
            {
                scaleModifier = (0.45f - stripWidth / 2f);
                float positionModifier = (scaleModifier + stripWidth) / 2f;
                strip.transform.localPosition = new Vector3(shipDirection.x, 0f, shipDirection.y) * 0.45f - new Vector3(shipDirection.x, 0f, shipDirection.y) * positionModifier;
            }
            strip.transform.localScale = new Vector3(stripWidth + scaleModifier * Mathf.Abs(shipDirection.x), 0.1f, stripWidth + scaleModifier * Mathf.Abs(shipDirection.y));
        }
        lastColor = stripColor;
    }

    /// <summary>
    /// Draws the side strip, showing the color of players who have hit this tile.
    /// </summary>
    void DrawSideStrips(Color[] colors)
    {
        float totalStripWidth = 0.45f - stripWidth / 2f;
        float individualStripWidth = totalStripWidth / colors.Length;
        float startingPosition = stripWidth / 2f + individualStripWidth / 2f;
        if (middleSegment)
        {
            for (int color = 0; color < colors.Length; color++)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector3 positionModifier = new Vector3(-shipDirection.y, 0f, shipDirection.x) * (startingPosition + individualStripWidth * color) * i;

                    Vector3 stripLengthScaleModifier = new Vector3((shipDirection.x != 0) ? 0.9f : 0, 0.1f, (shipDirection.y != 0) ? 0.9f : 0);
                    Vector3 stripWidthScaleModifier = new Vector3((stripLengthScaleModifier.x == 0) ? individualStripWidth : 0, 0f, (stripLengthScaleModifier.z == 0) ? individualStripWidth : 0);

                    GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    strip.transform.parent = marker.transform;
                    strip.transform.localPosition = positionModifier;
                    strip.transform.localScale = stripLengthScaleModifier + stripWidthScaleModifier;

                    Renderer renderer = strip.GetComponent<Renderer>();
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    renderer.material = GameController.playerBoardMarkerMaterial;
                    block.SetColor("_Color", colors[color]);
                    //renderer.material.SetColor("_Color", colors[color]);
                    renderer.SetPropertyBlock(block);
                }
            }
        }
        else
        {
            for (int color = 0; color < colors.Length; color++)
            {
                GameObject strip;
                Renderer renderer;
                MaterialPropertyBlock block;
                float scaleModifier;
                Vector3 positionModifier;
                Vector3 secondaryPositionModifier;
                Vector3 stripLengthScaleModifier;
                Vector3 stripWidthScaleModifier;
                for (int i = -1; i <= 1; i += 2)
                {
                    scaleModifier = (0.45f + stripWidth / 2f) + individualStripWidth * color;
                    float positionCoefficient = scaleModifier / 2f;

                    positionModifier = new Vector3(-shipDirection.y, 0f, shipDirection.x) * (startingPosition + individualStripWidth * color) * i;
                    secondaryPositionModifier = new Vector3(shipDirection.x, 0f, shipDirection.y) * 0.45f - new Vector3(shipDirection.x, 0f, shipDirection.y) * positionCoefficient;

                    stripLengthScaleModifier = new Vector3((shipDirection.x != 0) ? scaleModifier : 0, 0.1f, (shipDirection.y != 0) ? scaleModifier : 0);
                    stripWidthScaleModifier = new Vector3((stripLengthScaleModifier.x == 0) ? individualStripWidth : 0, 0f, (stripLengthScaleModifier.z == 0) ? individualStripWidth : 0);

                    strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    strip.transform.parent = marker.transform;
                    strip.transform.localPosition = positionModifier + secondaryPositionModifier;
                    strip.transform.localScale = stripLengthScaleModifier + stripWidthScaleModifier;

                    renderer = strip.GetComponent<Renderer>();
                    block = new MaterialPropertyBlock();
                    renderer.material = GameController.playerBoardMarkerMaterial;
                    block.SetColor("_Color", colors[color]);
                    //renderer.material.SetColor("_Color", colors[color]);
                    renderer.SetPropertyBlock(block);
                }
                scaleModifier = stripWidth + 2f * individualStripWidth * (color + 1);
                positionModifier = -new Vector3(shipDirection.x, 0f, shipDirection.y) * (startingPosition + individualStripWidth * color);
                stripLengthScaleModifier = new Vector3((shipDirection.y != 0) ? scaleModifier : 0, 0.1f, (shipDirection.x != 0) ? scaleModifier : 0);
                stripWidthScaleModifier = new Vector3((stripLengthScaleModifier.x == 0) ? individualStripWidth : 0, 0f, (stripLengthScaleModifier.z == 0) ? individualStripWidth : 0);

                strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.transform.parent = marker.transform;
                strip.transform.localPosition = positionModifier;
                strip.transform.localScale = stripLengthScaleModifier + stripWidthScaleModifier;

                renderer = strip.GetComponent<Renderer>();
                block = new MaterialPropertyBlock();
                renderer.material = GameController.playerBoardMarkerMaterial;
                block.SetColor("_Color", colors[color]);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
