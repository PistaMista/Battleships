using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPlacer : MonoBehaviour
{
    //Values accessed through the editor
    /// <summary>
    /// The default ship loadout for each player.
    /// </summary>
    public GameObject[] defaultShipLoadout;
    /// <summary>
    /// The color of valid placement tiles.
    /// </summary>
    public Color defaultValidTileColor;
    /// <summary>
    /// The color of selected tiles.
    /// </summary>
    public Color defaultSelectedTileColor;
    /// <summary>
    /// The four cardinal directions.
    /// </summary>
    static Vector2[] cardinalDirections;
    //Static values accessed by anything else
    /// <summary>
    /// The color of valid placement tiles.
    /// </summary>
    static Color validTileColor;
    /// <summary>
    /// The color of selected tiles.
    /// </summary>
    static Color selectedTileColor;
    /// <summary>
    /// The ship loadout for each player.
    /// </summary>
    static GameObject[] shipLoadout;
    /// <summary>
    /// The ID of the player currently having his ships placed.
    /// </summary>
    static int playerNumber;
    /// <summary>
    /// The player currently having his ships placed.
    /// </summary>
    static Player player;
    /// <summary>
    /// All the players having their ships placed.
    /// </summary>
    static Player[] customers;
    /// <summary>
    /// The ships to place for the current player.
    /// </summary>
    static List<Ship> shipsToPlace;
    /// <summary>
    /// Locks the screen after placing a ship. (To prevent accidental placement)
    /// </summary>
    static bool placementLock = false;
    /// <summary>
    /// The battle these players will compete in.
    /// </summary>
    static Battle battle;

    /// <summary>
    /// The awake function.
    /// </summary>
    void Awake()
    {
        cardinalDirections = new Vector2[] { Vector2.up, Vector2.left, Vector2.right, Vector2.down };
        shipLoadout = defaultShipLoadout;
        validTileColor = defaultValidTileColor;
        selectedTileColor = defaultSelectedTileColor;
        validPositions = new List<Vector2>();
    }

    /// <summary>
    /// The positions selected by the player for the ship to sit in.
    /// </summary>
    static List<Vector2> selectedPositions;
    /// <summary>
    /// The positions which are valid.
    /// </summary>
    static List<Vector2> validPositions;

    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        if (customers != null)
        {
            if (InputController.beginPress && selectedPositions.Count == 0) //If the player presses the screen...
            {
                placementLock = false; //Disable placement lock
                Vector2 pos = player.board.WorldToTilePosition(InputController.currentInputPosition);
                if (!(pos.x < 0))
                {
                    Ship targetShip = player.board.tiles[(int)pos.x, (int)pos.y].containedShip;
                    if (targetShip) //If the player clicks on a tile containing a placed ship, remove it
                    {
                        for (int i = 0; i < targetShip.length; i++)
                        {
                            player.board.tiles[(int)targetShip.tiles[i].x, (int)targetShip.tiles[i].y].containedShip = null;
                            player.board.SetMarker(targetShip.tiles[i], Color.clear);
                        }

                        targetShip.tiles = new Vector2[targetShip.length];
                        targetShip.gameObject.SetActive(false);
                        //player.allShips.Remove(targetShip);
                        //battle.ships.Remove(targetShip);

                        PlacementPreview(targetShip);

                        if (shipsToPlace.Count == 0)
                        {
                            Interface.SwitchMenu("Placing");
                        }

                        shipsToPlace.Insert(0, targetShip);

                        RecalculateValidPositions();
                    }
                }
            }

            if (InputController.dragging && !placementLock)
            {
                Vector2 candidatePosition = player.board.WorldToTilePosition(InputController.currentInputPosition);

                if (validPositions.Contains(candidatePosition)) //If the positions being dragged over is valid, select it to contain the currently placed ship
                {
                    SelectPosition(candidatePosition); //Select the tile to have the ship in it

                    if (selectedPositions.Count == shipsToPlace[0].length) //If the amount of selected tiles is equal to the ships length...
                    {
                        Ship lastShip = shipsToPlace[0];
                        FinishPlacingShip(); //Finish placing the ship
                        if (shipsToPlace.Count == 0) //If all ships are placed...
                        {
                            Interface.SwitchMenu("Placing Done"); //Switch the interface to show a button to go to the next player
                            foreach (Vector2 pos in validPositions) //Clear the tile colors of valid positions
                            {
                                player.board.SetMarker(pos, Color.clear);
                            }
                            validPositions = new List<Vector2>(); //Clear the list of valid positions
                            PlacementPreview(null);
                        }
                        else //If all ships are not placed...
                        {
                            RecalculateValidPositions(); //Recalculate the list of valid positions to start placing the next ship
                            PlacementPreview(shipsToPlace[0]);
                        }

                        placementLock = true; //Enable placement lock
                        //player.ShipsShown(true);
                        lastShip.gameObject.SetActive(true);
                    }
                    else //If the amount of selected tiles is not equal to the ship's length...
                    {
                        RecalculateValidPositions(); //Recalculate valid positions to place the next tile
                    }
                }
            }
        }
    }
    /// <summary>
    /// Finalizes placing a ship.
    /// </summary>    
    static void FinishPlacingShip()
    {
        Ship processedShip = shipsToPlace[0];

        for (int i = 0; i < processedShip.length; i++)
        {
            Vector2 pos = selectedPositions[i];
            player.board.tiles[(int)pos.x, (int)pos.y].containedShip = processedShip; //Marks the selected tiles, that they now contain a ship
            processedShip.tiles[i] = pos; //Lets the ship know which tiles it is located in
        }

        //player.allShips.Add(processedShip); //Adds the ship to the players ship list
        //battle.ships.Add(processedShip);

        //processedShip.gameObject.SetActive(showShip); //Shows the ship
        //processedShip.transform.position = player.board.tiles[(int)selectedPositions[Mathf.CeilToInt((float)processedShip.length / 2f) - 1].x, (int)selectedPositions[Mathf.CeilToInt((float)processedShip.length / 2f) - 1].y].worldPosition - Vector3.up * (GameController.playerBoardElevation - 0.4f); //Sets up the position of the ship
        Vector3 position1 = player.board.tiles[(int)selectedPositions[0].x, (int)selectedPositions[0].y].worldPosition;
        Vector3 position2 = player.board.tiles[(int)selectedPositions[selectedPositions.Count - 1].x, (int)selectedPositions[selectedPositions.Count - 1].y].worldPosition;

        processedShip.boardPosition = position1 + (position2 - position1) / 2f - Vector3.up * (GameController.playerBoardElevation - GameController.seaLevel);
        if ((selectedPositions[0] - selectedPositions[1]).x != 0) //Rotates the ship correctly
        {
            processedShip.boardRotation = Vector3.up * (90f - 180f * Random.Range(0, 2));
        }
        else
        {
            processedShip.boardRotation = Vector3.zero;
        }

        processedShip.PositionOnPlayingBoard();

        shipsToPlace.RemoveAt(0); //Specifies that the ship is now placed and removes it from the list of ships that still have to be placed
        selectedPositions = new List<Vector2>(); //Resets the list of selected positions
    }
    /// <summary>
    /// Selects a position for ship placement.
    /// </summary>
    /// <param name="position">Position to select.</param>
    static void SelectPosition(Vector2 position)
    {
        if (selectedPositions.Count <= 1)
        {
            selectedPositions.Add(position);
        }
        else
        {
            if (Vector2.Distance(selectedPositions[selectedPositions.Count - 1], position) > 1)
            {
                selectedPositions.Insert(0, position);
            }
            else
            {
                selectedPositions.Add(position);
            }
        }

        validPositions.Remove(position);
        player.board.SetMarker(position, selectedTileColor);
    }
    /// <summary>
    /// Marks a position as valid.
    /// </summary>
    /// <param name="position">Position to mark.</param>
    static void MarkAsValidPosition(Vector2 position)
    {
        if (!validPositions.Contains(position))
        {
            validPositions.Add(position);
            player.board.SetMarker(position, validTileColor);
        }
    }
    /// <summary>
    /// Recalculates valid positions for ship placement.
    /// </summary>
    static void RecalculateValidPositions()
    {
        if (validPositions != null)
        {
            foreach (Vector2 pos in validPositions) //Clears the graphical representation of the valid tiles
            {
                player.board.SetMarker(pos, Color.clear);
            }
        }

        validPositions = new List<Vector2>(); //Clears the valid position list

        if (selectedPositions.Count == 0) //If the player hasnt started selecting where his ship will be go through all the tiles and mark the valid ones
        {
            for (int x = 0; x < player.board.dimensions; x++)
            {
                for (int y = 0; y < player.board.dimensions; y++)
                {
                    if (!validPositions.Contains(new Vector2(x, y)))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (ShipPlaceable(new Vector2(x, y), cardinalDirections[i], true))
                            {
                                MarkAsValidPosition(new Vector2(x, y));
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if (selectedPositions.Count == 1) //If he decided on 1 tile, where his ship will be check the neighbouring tiles for validity
        {
            for (int i = 0; i < cardinalDirections.Length; i++)
            {
                Vector2 candidatePosition = selectedPositions[0] + cardinalDirections[i];

                if (ShipPlaceable(candidatePosition, cardinalDirections[i], false))
                {
                    MarkAsValidPosition(candidatePosition);
                }

            }
        }
        else if (selectedPositions.Count > 1) //If he decided on more than 1 tile check the 2 sides of the strip he created
        {
            Vector2 direction = selectedPositions[0] - selectedPositions[1];
            Vector2 candidatePosition = selectedPositions[0] + direction;

            if (ShipPlaceable(candidatePosition, direction, false))
            {
                MarkAsValidPosition(candidatePosition);
            }

            candidatePosition = selectedPositions[selectedPositions.Count - 1] - direction;

            if (ShipPlaceable(candidatePosition, direction, false))
            {
                MarkAsValidPosition(candidatePosition);
            }
        }
    }
    /// <summary>
    /// Checks if the tile can contain the currently placed ship.
    /// </summary>
    /// <param name="position">The position of the tile to check.</param>
    /// <param name="direction">The direction in which to check.</param>
    /// <param name="initValidate">Automatically validate checked positions.</param>
    /// <returns>Tile validity for current ship containment.</returns>
    static bool ShipPlaceable(Vector2 position, Vector2 direction, bool initValidate) //Checks if the tile can contain a ship of a specified size
    {
        List<Vector2> confirmedPositions = new List<Vector2>(); //A list of the positions confirmed to be suitable for ship placement
        bool rootPositionConfirmed = false; //Specifies whether the actual tile (provided by the variable position) is free

        for (int i = -(shipsToPlace[0].length - 1); i <= (shipsToPlace[0].length - 1); i++) //Checks the tiles in direction for a distance of the ships length
        {
            Vector2 checkedPosition = position + direction * i; //The position currently being checked
            if (!(checkedPosition.x < 0 || checkedPosition.y < 0 || checkedPosition.x >= player.board.dimensions || checkedPosition.y >= player.board.dimensions) && !player.board.tiles[(int)checkedPosition.x, (int)checkedPosition.y].containedShip) //Checks if the tile is free
            {
                confirmedPositions.Add(checkedPosition); //Add the tile to the list of confirmed positions
                if (i == 0)
                {
                    rootPositionConfirmed = true; //If the current tile is the root tile, mark it as confirmed
                }
            }
            else if (i < 0)
            {
                confirmedPositions = new List<Vector2>(); //If the chain is broken before reaching the root tile, clear the list of confirmed positions
            }
            else
            {
                break; //If the chain is broken after reaching the root tile stop the loop
            }
        }

        if (confirmedPositions.Count >= shipsToPlace[0].length && initValidate) //Automatically marks the valid positions
        {
            foreach (Vector2 pos in confirmedPositions)
            {
                MarkAsValidPosition(pos);
            }
        }

        return confirmedPositions.Count >= shipsToPlace[0].length && rootPositionConfirmed; //If the amount of confirmed positions is more than or equal to the ship's length the tile is suitable to have a ship in it
    }
    /// <summary>
    /// Switches to the next player.
    /// </summary>
    /// <returns>Switching successful.</returns>
    static bool NextPlayer()
    {
        playerNumber++; //Increment the id of currently handled player
        if (playerNumber > customers.Length - 1) //If there are no more players to handle return false
        {
            customers = null;
            return false;
        }
        else //If there are more players to handle...
        {
            if (player != null)
            {
                if (battle.isMainBattle)
                {
                    player.board.Set(BoardState.OVERHEAD);
                }
            }

            player = customers[playerNumber]; //Set up a link to the currently handled player
            if (!player.AI) //Skip AI players
            {
                Cameraman.TakePosition("Board " + (playerNumber + 1), 0.6f); //Camera takes up a position above the board
                player.board.Set(BoardState.GRID_ONLY);

                InitializeShipsForCurrentPlayer();

                PlacementPreview(shipsToPlace[0]);
                Interface.SwitchMenu("Placing"); //Make sure the interface is set to the placing menu

                return true;
            }
            else
            {
                return NextPlayer();
            }
        }
    }
    /// <summary>
    /// Initializes ships for the current player.
    /// </summary>
    private static void InitializeShipsForCurrentPlayer()
    {
        shipsToPlace = new List<Ship>(); //Readies a list of ships for the player to place (blank)

        for (int i = 0; i < shipLoadout.Length; i++) //TEMPORARY - Add 5 ships to the list
        {
            Ship ship = CreateShip(i);
            ship.owner = player;
            ship.transform.parent = player.transform;

            player.AssignShip(ship);
            shipsToPlace.Add(ship);
        }

        selectedPositions = new List<Vector2>(); //Reset the list of selected positions for the current ship
        RecalculateValidPositions(); //Recalculate valid positions for ship placement
    }
    /// <summary>
    /// Creates a new ship.
    /// </summary>
    /// <param name="shipLoadoutID">The ID of the ship in the loadout array.</param>
    /// <returns>New ship.</returns>
    static Ship CreateShip(int shipLoadoutID) //Creates a new ship
    {
        GameObject ship = Instantiate(shipLoadout[shipLoadoutID]);
        ship.SetActive(false);

        return ship.GetComponent<Ship>();
    }
    /// <summary>
    /// Handles placing the ships for a battle.
    /// </summary>
    /// <param name="battle">The battle to handle.</param>
    public static void HandleShipsForBattle(Battle battle)
    {
        Reset();
        ShipPlacer.battle = battle;        //Specifies that the ship placer is now handling ship placement for the provided Battle
        customers = battle.players;        //Sets the players to have their ships placed
        playerNumber = -1;        //Start with player #0 (the number will increment by 1)
        PlaceShipsForAIPlayers();
        if (!NextPlayer())
        {
            PlacementFinished();
        }
        else
        {       //Switch to the next player
            Interface.SwitchMenu("Placing"); //Switches the interface to the placement menu
        }
    }
    /// <summary>
    /// Places ships for all AI players.
    /// </summary>
    static void PlaceShipsForAIPlayers()
    {
        for (int i = 0; i < customers.Length; i++)
        {
            Player currentPlayer = customers[i];
            if (currentPlayer.AI)
            {
                player = currentPlayer;
                InitializeShipsForCurrentPlayer();

                while (shipsToPlace.Count > 0)
                {
                    Vector2 pickedPosition = validPositions[(int)Random.Range(0f, validPositions.Count)];

                    SelectPosition(pickedPosition); //Select the tile to have the ship in it

                    if (selectedPositions.Count == shipsToPlace[0].length) //If the amount of selected tiles is equal to the ships length...
                    {
                        FinishPlacingShip(); //Finish placing the ship
                        if (shipsToPlace.Count == 0) //If all ships are placed...
                        {
                            validPositions = new List<Vector2>(); //Clear the list of valid positions
                        }
                        else //If all ships are not placed...
                        {
                            RecalculateValidPositions(); //Recalculate the list of valid positions to start placing the next ship
                        }
                    }
                    else //If the amount of selected tiles is not equal to the ship's length...
                    {
                        RecalculateValidPositions(); //Recalculate valid positions to place the next tile
                    }

                }
            }
        }
    }


    /// <summary>
    /// The parent of all objects used to preview the currently placed ship.
    /// </summary>
    static GameObject previewParent;
    /// <summary>
    /// The ship currently being previewed.
    /// </summary>
    static Ship currentlyPreviewed;
    /// <summary>
    /// Previews a ship on the left.
    /// </summary>
    /// <param name="ship">The ship to preview.</param>
    static void PlacementPreview(Ship ship)
    {
        if (currentlyPreviewed != null)
        {
            currentlyPreviewed.gameObject.SetActive(false);
        }

        if (ship != null)
        {
            Destroy(previewParent);
            previewParent = new GameObject("Ship Preview");
            previewParent.transform.position = player.board.transform.position + (Vector3.left * ((float)player.board.dimensions / 2f + 1f));

            currentlyPreviewed = ship;
            Vector3 originPosition = new Vector3(0f, 0f, -1.1f * (Mathf.Floor(ship.length / 2f) - (ship.length + 1) % 2 * 0.5f));

            for (int i = 0; i < ship.length; i++)
            {
                GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tmp.transform.localScale = new Vector3(1f, 0.2f, 1f);
                tmp.transform.parent = previewParent.transform;

                Renderer renderer = tmp.GetComponent<Renderer>();

                renderer.material = GameController.playerBoardMarkerMaterial;
                renderer.material.SetColor("_Color", selectedTileColor);

                tmp.transform.localPosition = originPosition + Vector3.forward * i * 1.1f;
            }

            ship.gameObject.SetActive(true);
            ship.transform.position = previewParent.transform.position;
            ship.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        else
        {
            Destroy(previewParent);
            if (currentlyPreviewed != null)
            {
                currentlyPreviewed.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// Used by UI elements to switch to the next player.
    /// </summary>
    public void PlayerFinished()
    {
        if (!NextPlayer())
        {
            PlacementFinished();
        }
    }
    /// <summary>
    /// Finishes ship placement.
    /// </summary>
    public static void PlacementFinished()
    {
        Reset();
        battle.StartBattle();
    }
    /// <summary>
    /// Resets the ship placer.
    /// </summary>
    public static void Reset()
    {
        if (shipsToPlace != null)
        {
            foreach (Ship ship in shipsToPlace)
            {
                Destroy(ship.gameObject);
            }
        }

        customers = null;
        shipsToPlace = null;
        player = null;
        Destroy(previewParent);
        if (currentlyPreviewed)
        {
            currentlyPreviewed.gameObject.SetActive(false);
        }
    }
}
