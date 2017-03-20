using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameState
{
    NONE,
    TITLE,
    PLACING_SHIPS,
    PLAYER_SELECTION,
    BATTLING
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

        public PlayerInitializer(Color color, bool AI)
        {
            playerColor = color;
            this.AI = AI;
        }
    }

    void Start()
    {
        ChangeState(GameState.TITLE);
    }

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

    //Values accessed by code
    public static int playerBoardDimensions;
    public static float playerBoardElevation;
    public static float playerBoardDistanceFromCenter;
    public static Material playerBoardGridMaterial;
    public static Material playerBoardMarkerMaterial;
    public static GameObject[] playerOverheadStatusMarkers;
    //The battle the game will focus on
    public static Battle mainBattle;
    //The secondary battles
    public static List<Battle> secondaryBattles;
    //Is there only one human player?
    public static bool singleplayer = false;
    //How many human players there are?
    public static int humanPlayers;
    public static bool switchTimesNill = false;
    public static float gravity;
    public static GameObject cannonShell;

    void Awake()
    {
        playerBoardDimensions = defaultPlayerBoardDimensions;
        playerBoardElevation = defaultPlayerBoardElevation;
        playerBoardDistanceFromCenter = defaultPlayerBoardDistanceFromCenter;
        playerBoardGridMaterial = defaultPlayerBoardGridMaterial;
        playerBoardMarkerMaterial = defaultPlayerBoardMarkerMaterial;
        playerOverheadStatusMarkers = defaultPlayerOverheadStatusMarkers;
        gravity = defaultGravity;
        cannonShell = defaultCannonShell;

        switchTimesNill = nullifyBattleSwitchTimes;
        secondaryBattles = new List<Battle>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.TITLE && InputController.beginPress)
        {
            ChangeState(GameState.PLAYER_SELECTION);
        }
    }

    public static void NewBattle(PlayerInitializer[] playersToAdd, bool focus)
    {
        Player[] players = new Player[playersToAdd.Length];
        //state = GameState.PLACING_SHIPS;
        humanPlayers = 0;
        //For all players create their own board
        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new GameObject("Player " + (i + 1)).AddComponent<Player>();
            float angle = 360 / players.Length * i * Mathf.Deg2Rad;
            Vector3 boardPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * playerBoardDistanceFromCenter + Vector3.up * playerBoardElevation;
            Player player = players[i];
            player.board = ScriptableObject.CreateInstance<Board>();
            player.board.Initialize(playerBoardDimensions, boardPosition, player, playerBoardGridMaterial);
            Cameraman.AddPosition(3f, new Vector3(boardPosition.x, playerBoardDimensions + playerBoardElevation, boardPosition.z), new Vector3(90, 0, 0), "Board " + (i + 1));
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
                    player.hits.Add(x, new List<Vector2>());
                    player.misses.Add(x, new List<Vector2>());
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

    public static void ChangeState(GameState state)
    {
        switch (GameController.state)
        {
            case GameState.PLAYER_SELECTION:
                Cameraman.SetBlur(false);
                PlayerSelector.Reset();
                break;
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

                Cameraman.TakePosition("Overhead View");
                Interface.SwitchMenu("Title Screen");

                NewBattle(new PlayerInitializer[] { new PlayerInitializer(Color.red, true), new PlayerInitializer(Color.red, true) }, false);
                break;
            case GameState.BATTLING:
                Cameraman.TakePosition("Overhead View");
                Interface.SwitchMenu("Overhead");
                break;
            case GameState.PLAYER_SELECTION:
                Cameraman.SetBlur(true);
                Interface.SwitchMenu("Player Selection Screen");
                PlayerSelector.Reset();
                break;
        }
    }

    public void BackToTitle()
    {
        GameController.ChangeState(GameState.TITLE);
    }
}
