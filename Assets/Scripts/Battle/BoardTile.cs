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
    /// The position in the world of this tile.
    /// </summary>
    public Vector3 worldPosition;
    /// <summary>
    /// The marker object of this tile.
    /// </summary>
    GameObject marker;
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
            marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.position = transform.position;
            marker.transform.parent = parent;
            marker.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
            marker.layer = 5;
            color.a = 0.6f;
            Renderer renderer = marker.GetComponent<Renderer>();
            renderer.material = GameController.playerBoardMarkerMaterial;
            renderer.material.SetColor("_Color", color);
        }
    }
}
