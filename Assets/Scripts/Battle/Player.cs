using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    // Use this for initialization
    //The ID number of this player
    public int ID;
    //Is the player still alive?
    public bool alive = true;
    //The color of this Player
    public Color color;
    //This player's ship board
    public Board board;
    //The places on enemy boards this player has hit
    public Dictionary<int, List<Vector2>> hits;
    //The places on enemy boards this player has missed
    public Dictionary<int, List<Vector2>> misses;
    //Is this player controlled by AI?
    public bool AI = false;
    //The macro marker of this player (markers shown in the overhead view such as if the player is on the turn/dead)
    public GameObject macroMarker;
    //The ships owned by this player
    public List<Ship> ships;
    //The battle this player is taking part in
    public Battle battle;

    void Awake()
    {
        hits = new Dictionary<int, List<Vector2>>();
        misses = new Dictionary<int, List<Vector2>>();
        ships = new List<Ship>();
    }

    // Update is called once per frame
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

    public void ShipsShown(bool enabled)
    {
        foreach (Ship ship in ships)
        {
            ship.gameObject.SetActive(enabled);
        }
    }

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
            macroMarker.transform.position = board.position + Vector3.up * 2f;
            macroMarker.transform.localScale = new Vector3(board.dimensions, 1f, board.dimensions);
        }
    }

    public void UpdateSpecialMarkerBehaviour()
    {
        switch (macroMarker.name)
        {
            case "Player On-Turn Marker(Clone)":
                macroMarker.transform.Rotate(new Vector3(0f, 90f * Time.deltaTime, 0f));
                break;
        }
    }

    public void ShipSunk(Ship ship)
    {
        ships.Remove(ship);

        if (ships.Count == 0)
        {
            alive = false;
            if (!AI)
            {
                GameController.humanPlayers--;
            }
        }
    }
}
