using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the possible game states.
/// </summary>
public enum GameState
{
    /// <summary>
    /// The starting state.
    /// </summary>
    NONE,
    /// <summary>
    /// The title screen state.
    /// </summary>
    TITLE,
    /// <summary>
    /// Ships are being placed.
    /// </summary>
    PLACING_SHIPS,
    /// <summary>
    /// Main battle competitors are being selected.
    /// </summary>
    PLAYER_SELECTION,
    /// <summary>
    /// The battle is in progress.
    /// </summary>
    BATTLING
}

/// <summary>
/// All the possible attack types.
/// </summary>
public enum AttackType
{
    /// <summary>
    /// Artillery attacks.
    /// </summary>
    ARTILLERY,
    /// <summary>
    /// Torpedo attacks.
    /// </summary>
    TORPEDO
}

public class GameController : MonoBehaviour
{

    // Use this for initialization
    //All the possible game states

    //The current state of the game
    public static GameState state = GameState.NONE;

    public struct PlayerInitializer
    {
        public Color playerColor;
        public bool AI;
        public int boardDimensions;

        public PlayerInitializer(Color color, bool AI, int boardDimensions)
        {
            playerColor = color;
            this.AI = AI;
            this.boardDimensions = boardDimensions;
        }
    }

    void Start()
    {
        ChangeState(GameState.TITLE);
    }
    //The time it takes to process a single turn (in seconds)
    public int defaultTurnProcessingTime;
    //The default values that are adjustable in the inspector
    //The dimensions of the board
    public int defaultPlayerBoardDimensions;
    //The default elevation of the player boards
    public float defaultPlayerBoardElevation;
    //The distance of each player's board from the center of the map
    public float defaultPlayerBoardDistanceFromCenter = 7f;
    //The material used by the boards to render grids
    public Material defaultPlayerBoardGridMaterial;
    //The material used by the boards to render markers
    public Material defaultPlayerBoardMarkerMaterial;
    //The macro markers used by players to show their status in overhead view
    public GameObject[] defaultPlayerOverheadStatusMarkers;
    //Sets all switch times in battles to zero - TESTING FEATURE
    public bool nullifyBattleSwitchTimes;
    //The default acceleration due to gravity
    public float defaultGravity;
    //The prefab for cannon shells
    public GameObject defaultCannonShell;
    //The effect used for ship explosions
    public GameObject defaultShipExplosion;
    //The effect used for ship fires
    public GameObject defaultShipFire;
    //The sea level
    public float defaultSeaLevel;
    //Skips showing action shots of AI vs AI attacks in main battles
    public bool defaultSkipAIvsAIActionShots;
    //The effect used for water splashes.
    public GameObject defaultWaterSplashEffect;
    //The total amount of board tiles in the game. Used to limit the length of each match.
    public int defaultTotalBoardTileLimit;

    //Values accessed by code
    /// <summary>
    /// The dimensions of playing boards.
    /// </summary>
    public static int playerBoardDimensions;
    /// <summary>
    /// The y position of each playing board.
    /// </summary>
    public static float playerBoardElevation;
    /// <summary>
    /// The distance of each playing board from the center of the map.
    /// </summary>
    public static float playerBoardDistanceFromCenter;
    /// <summary>
    /// The material used for rendering playing board grids.
    /// </summary>
    public static Material playerBoardGridMaterial;
    /// <summary>
    /// The material used for marking tiles on playing boards.
    /// </summary>
    public static Material playerBoardMarkerMaterial;
    /// <summary>
    /// A set of prefabs used to display player status in overhead view.
    /// </summary>
    public static GameObject[] playerOverheadStatusMarkers;
    /// <summary>
    /// The main battle of the game.
    /// </summary>
    public static Battle mainBattle;
    /// <summary>
    /// The secondary battles currently going on.
    /// </summary>
    public static List<Battle> secondaryBattles;
    /// <summary>
    /// The number of human players participating in the game.
    /// </summary>
    public static int humanPlayers;
    public static bool switchTimesNill = false;
    /// <summary>
    /// The acceleration due to gravity.
    /// </summary>
    public static float gravity;
    /// <summary>
    /// The prefab for cannon shells.
    /// </summary>
    public static GameObject cannonShell;
    /// <summary>
    /// The prefab for ship explosions.
    /// </summary>
    public static GameObject shipExplosion;
    /// <summary>
    /// The prefab for ship fires.
    /// </summary>
    public static GameObject shipFire;
    /// <summary>
    /// The sea level height.
    /// </summary>
    public static float seaLevel;
    /// <summary>
    /// Skips action shots of AI vs AI attacks in main battles.
    /// </summary>
    public static bool skipAIvsAIActionShots;
    /// <summary>
    /// The prefab for water splashes.
    /// </summary>
    public static GameObject waterSplashEffect;
    /// <summary>
    /// The total amount of board tiles in the game. Used to limit the length of each match.
    /// </summary>
    public static int totalBoardTileLimit;
    /// <summary>
    /// The time it takes to process a single turn. (in seconds)
    /// </summary>
    public static int turnProcessingTime;

    /// <summary>
    /// Awake function.
    /// </summary>
    void Awake()
    {
        totalBoardTileLimit = defaultTotalBoardTileLimit;
        playerBoardDimensions = defaultPlayerBoardDimensions;
        playerBoardElevation = defaultPlayerBoardElevation;
        playerBoardDistanceFromCenter = defaultPlayerBoardDistanceFromCenter;
        playerBoardGridMaterial = defaultPlayerBoardGridMaterial;
        playerBoardMarkerMaterial = defaultPlayerBoardMarkerMaterial;
        playerOverheadStatusMarkers = defaultPlayerOverheadStatusMarkers;
        gravity = defaultGravity;
        cannonShell = defaultCannonShell;
        shipExplosion = defaultShipExplosion;
        shipFire = defaultShipFire;
        seaLevel = defaultSeaLevel;
        waterSplashEffect = defaultWaterSplashEffect;
        skipAIvsAIActionShots = defaultSkipAIvsAIActionShots;
        turnProcessingTime = defaultTurnProcessingTime;

        switchTimesNill = nullifyBattleSwitchTimes;
        secondaryBattles = new List<Battle>();
    }

    // Update is called once per frame
    /// <summary>
    /// Update function.
    /// </summary>
    void Update()
    {
        if (state == GameState.TITLE && InputController.GetBeginPress(63))
        {
            ChangeState(GameState.PLAYER_SELECTION);
        }
    }

    /// <summary>
    /// Begins a new battle.
    /// </summary>
    /// <param name="playersToAdd">The player initializers of players to participate in the battle.</param>
    /// <param name="focus">Whether this battle is the main battle of the game.</param>
    public static void NewBattle(PlayerInitializer[] playersToAdd, bool focus)
    {
        Player[] players = new Player[playersToAdd.Length];
        //state = GameState.PLACING_SHIPS;
        humanPlayers = 0;
        //playerBoardDimensions = Mathf.Clamp(Mathf.FloorToInt(Mathf.Sqrt((float)totalBoardTileLimit / (float)players.Length)), 6, 20);

        //For all players create their own board
        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new GameObject("Player " + (i + 1)).AddComponent<Player>();
            float angle = 360 / players.Length * i * Mathf.Deg2Rad;
            Vector3 boardPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * playerBoardDistanceFromCenter + Vector3.up * playerBoardElevation;
            Player player = players[i];
            player.board = new GameObject("Board " + i).AddComponent<Board>();
            player.board.Initialize(playersToAdd[i].boardDimensions, boardPosition, player, playerBoardGridMaterial);
            Cameraman.AddPosition(3f, new Vector3(boardPosition.x, playersToAdd[i].boardDimensions + playerBoardElevation, boardPosition.z), new Vector3(90, 0, 0), "Board " + (i + 1));
            player.color = playersToAdd[i].playerColor;
            player.AI = playersToAdd[i].AI;
            player.ID = i;

            if (!player.AI)
            {
                humanPlayers++;
            }

            for (int x = 0; x < players.Length; x++)
            {
                if (x != i)
                {
                    player.hits.Add(x, new List<BoardTile>());
                    player.misses.Add(x, new List<BoardTile>());
                }
            }
        }

        //Add the battle manager GameObject
        Battle battle = new GameObject("Battle").AddComponent<Battle>();

        if (focus)
        {
            mainBattle = battle;
            ChangeState(GameState.PLACING_SHIPS);
            foreach (Battle secondaryBattle in secondaryBattles)
            {
                secondaryBattle.End();
            }

            secondaryBattles = new List<Battle>();
        }
        else
        {
            secondaryBattles.Add(battle);
        }

        battle.Initialize(players);
    }

    /// <summary>
    /// Changes the state of the game.
    /// </summary>
    /// <param name="state">The game state to change to.</param>
    public static void ChangeState(GameState state)
    {
        switch (GameController.state)
        {

        }
        GameController.state = state;
        switch (state)
        {
            case GameState.TITLE:
                if (mainBattle)
                {
                    BattleInterface.Dettach();
                    mainBattle.End();

                }

                Cameraman.TakePosition("Overhead Title View");
                Interface.SwitchMenu("CANCEL_OVERRIDE");
                Interface.SwitchMenu("Title Screen");
                Soundman.ChangeTrack(0, true, true);
                NewBattle(new PlayerInitializer[] { new PlayerInitializer(Color.red, true, 7), new PlayerInitializer(Color.red, true, 7) }, false);
                //NewBattle(new PlayerInitializer[] { new PlayerInitializer(Color.red, true), new PlayerInitializer(Color.red, true), new PlayerInitializer(Color.red, true), new PlayerInitializer(Color.red, true), new PlayerInitializer(Color.red, true) }, false);
                break;
            case GameState.BATTLING:
                Cameraman.TakePosition("Overhead View");
                Interface.SwitchMenu("Overhead");
                Soundman.ChangeTrack(2, true, true);
                break;
            case GameState.PLACING_SHIPS:
                Soundman.ChangeTrack(-1, true, false);
                break;
            case GameState.PLAYER_SELECTION:
                Soundman.ChangeTrack(1, true, false);
                Interface.SwitchMenu("Player Selection Screen");
                PlayerSelector.Reset();
                break;
        }
    }

    /// <summary>
    /// Accessed by gui elements to return back to the title screen.
    /// </summary>    
    public void BackToTitle()
    {
        GameController.ChangeState(GameState.TITLE);
    }

    /// <summary>
    /// Gets the minimum game time for an amount of players.
    /// </summary>
    /// <returns>Minimum game time (in seconds)</returns>
    public static int GetGameTime(int players, int boardDimensions)
    {
        if (players == 0 || boardDimensions == 0)
        {
            return 0;
        }
        int result = 0;
        float totalTileCount = boardDimensions * boardDimensions * players;
        float totalShipTiles = 0;
        foreach (GameObject ship in ShipPlacer.shipLoadout)
        {
            Ship s = ship.GetComponent<Ship>();
            totalShipTiles += s.length;
        }


        totalShipTiles *= players;
        Debug.Log(totalShipTiles);

        result = (int)(12f * (totalShipTiles / ((totalShipTiles / totalTileCount) * 2.2f)));
        Debug.Log((12f * (totalShipTiles / (((float)totalShipTiles / (float)totalTileCount) * 5f))));
        return result;
    }
}
