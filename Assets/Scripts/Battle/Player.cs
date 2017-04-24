using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    // Use this for initialization
    /// <summary>
    /// The ID number of this player.
    /// </summary>
    public int ID;
    /// <summary>
    /// Whether this player is alive.
    /// </summary>    
    public bool alive = true;
    /// <summary>
    /// The color of this player.
    /// </summary>
    public Color color;
    /// <summary>
    /// This player's board.
    /// </summary>
    public Board board;
    /// <summary>
    /// The positions of tiles on enemy boards this player has hit.
    /// </summary>
    public Dictionary<int, List<Vector2>> hits;
    /// <summary>
    /// The positions of tiles on enemy boards this player has missed.
    /// </summary>
    public Dictionary<int, List<Vector2>> misses;
    /// <summary>
    /// Whether this player is controlled by AI.
    /// </summary>
    public bool AI = false;
    /// <summary>
    /// The marker used to mark this player's status in battle overhead view. 
    /// </summary>
    public GameObject macroMarker;
    /// <summary>
    /// All ships owned by this player.
    /// </summary>
    public List<Ship> allShips;
    /// <summary>
    /// Ships owned by this player which are still intact.
    /// </summary>
    public List<Ship> livingShips;
    /// <summary>
    /// The battle this player is taking part in.
    /// </summary>
    public Battle battle;

    /// <summary>
    /// The awake function.
    /// </summary>    
    void Awake()
    {
        hits = new Dictionary<int, List<Vector2>>();
        misses = new Dictionary<int, List<Vector2>>();
        allShips = new List<Ship>();
        livingShips = new List<Ship>();
    }

    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        //TEST
        if (Input.GetMouseButtonDown(0))
        {
            // Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            InputController.referenceY = GameController.playerBoardElevation;
            //Debug.Log("WorldToTilePosition: " + board.WorldToTilePosition(InputController.currentInputPosition));
        }

        if (macroMarker)
        {
            UpdateSpecialMarkerBehaviour();
        }
    }

    /// <summary>
    /// Shows or hides this player's ships.
    /// </summary>
    /// <param name="enabled">Ships shown.</param>
    public void ShipsShown(bool enabled, bool onlyLiving)
    {
        foreach (Ship ship in allShips)
        {
            if (onlyLiving && !ship.eliminated || !onlyLiving)
            {
                ship.gameObject.SetActive(enabled);
            }
        }
    }
    /// <summary>
    /// Shows or hides this player's ships.
    /// </summary>
    /// <param name="enabled">Ships shown.</param>
    public void ShipsShown(bool enabled)
    {
        foreach (Ship ship in allShips)
        {
            ship.gameObject.SetActive(enabled);
        }
    }
    /// <summary>
    /// Sets this player's macro marker.
    /// </summary>
    /// <param name="markerID">The ID of the marker to use - more info in GameController.</param>
    public void SetMacroMarker(int markerID)
    {
        Destroy(macroMarker);

        if (markerID < 0 || markerID > (GameController.playerOverheadStatusMarkers.Length - 1))
        {
            macroMarker = null;
        }
        else
        {
            macroMarker = Instantiate(GameController.playerOverheadStatusMarkers[markerID]);
            macroMarker.transform.position = board.transform.position + Vector3.up * 2f;
            macroMarker.transform.localScale = new Vector3(board.dimensions, 1f, board.dimensions);
            switch (markerID)
            {
                case 0:
                    SquarePulserEffect effect = macroMarker.GetComponent<SquarePulserEffect>();
                    effect.color = color;
                    effect.pulseInterval = 0.4f;
                    effect.maxDistance = 1.25f;
                    effect.pulseSpeed = 5f;
                    effect.squareWidth = 0.15f;
                    effect.insideLength = board.dimensions * 0.1f;
                    macroMarker.transform.position = board.transform.position;
                    break;
            }
        }
    }
    /// <summary>
    /// Updates the marker.
    /// </summary>
    public void UpdateSpecialMarkerBehaviour()
    {
        switch (macroMarker.name)
        {
            case "Player On-Turn Marker(Clone)":
                macroMarker.transform.Rotate(new Vector3(0f, 90f * Time.deltaTime, 0f));
                break;
        }
    }
    /// <summary>
    /// Registers that the specified ship of the player's fleet has been destroyed.
    /// </summary>
    /// <param name="ship">The subject ship.</param>
    public void ShipSunk(Ship ship)
    {
        livingShips.Remove(ship);

        if (livingShips.Count == 0)
        {
            alive = false;
            if (!AI)
            {
                GameController.humanPlayers--;
            }
        }
    }
    /// <summary>
    /// Assigns a ship to the player's fleet.
    /// </summary>
    /// <param name="ship">The ship to assign.</param>
    public void AssignShip(Ship ship)
    {
        allShips.Add(ship);
        livingShips.Add(ship);
        ship.owner = this;
    }
}
