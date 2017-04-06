using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actionman : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Prepares the game for an action shot of weapons firing.
    /// </summary>
    static void PrepareActionView()
    {
        //Prepares the interface
        Interface.SwitchMenu("Firing Screen");

        foreach (Player player in BattleInterface.battle.players)
        {
            player.board.Set(BoardState.DISABLED);
            player.SetMacroMarker(-1);
        }

        if ((GameController.humanPlayers == 1 && !BattleInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
        {
            BattleInterface.battle.defendingPlayer.board.Set(BoardState.SHIPS);
        }

        if (!BattleInterface.battle.attackingPlayer.AI || (GameController.humanPlayers < 2 && !BattleInterface.battle.defendingPlayer.AI) || GameController.humanPlayers == 0)
        {
            BattleInterface.battle.ChangeState(BattleState.SHOWING_HIT_TILE, 0.5f);
        }
        else
        {
            BattleInterface.battle.ChangeState(BattleState.TURN_FINISHED, 1f);
        }

        //TEST
        BattleInterface.battle.switchTime = 2f;
        //TEST

        //Prepares the attack fleet
        Vector3 fleetPosition = Vector3.zero;
        float fleetRotation = 0f;

        if (Mathf.Abs(BattleInterface.battle.defendingPlayer.board.position.z) < Mathf.Abs(BattleInterface.battle.defendingPlayer.board.position.x))
        {
            fleetPosition = BattleInterface.battle.defendingPlayer.board.position - Vector3.right * GameController.playerBoardDistanceFromCenter * Mathf.Sign(BattleInterface.battle.defendingPlayer.board.position.x) * 1.5f;
            fleetRotation = 90f * Mathf.Sign(BattleInterface.battle.defendingPlayer.board.position.x);
        }
        else
        {
            fleetPosition = BattleInterface.battle.defendingPlayer.board.position - Vector3.forward * GameController.playerBoardDistanceFromCenter * Mathf.Sign(BattleInterface.battle.defendingPlayer.board.position.z) * 1.5f;
            fleetRotation = 90f - 90f * Mathf.Sign(BattleInterface.battle.defendingPlayer.board.position.z);
        }

        fleetPosition.y = GameController.seaLevel;

        // GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // tmp.transform.position = fleetPosition;
        // tmp.transform.localScale = new Vector3(0.1f, 0.1f, 2f);
        // tmp.transform.localRotation = Quaternion.Euler(new Vector3(0f, fleetRotation, 0f));

        List<Ship> destroyers = new List<Ship>();
        List<Ship> cruisers = new List<Ship>();
        List<Ship> battleships = new List<Ship>();
        List<Ship> attackers = new List<Ship>();
        foreach (Ship ship in BattleInterface.battle.attackingPlayer.livingShips)
        {
            if (ship.lengthRemaining == ship.length)
            {
                attackers.Add(ship);
                switch (ship.length)
                {
                    case 3:
                        destroyers.Add(ship);
                        break;
                    case 4:
                        cruisers.Add(ship);
                        break;
                    case 5:
                        battleships.Add(ship);
                        break;
                }
            }
        }

        Vector3 position = Vector3.zero;
        //Place the BattleInterface.battleships
        for (int i = 0; i < battleships.Count; i++)
        {
            Ship ship = battleships[i];
            position = new Vector3(-(battleships.Count / 2f - 0.5f) * 4f + (i) * 4f, ship.transform.position.y, 0f);
            ship.transform.position = position;
        }

        //Place the destroyers
        for (int i = 0; i < destroyers.Count; i++)
        {
            Ship ship = destroyers[i];
            position = new Vector3(-(destroyers.Count / 2f - 0.5f) * 3f + (i) * 3f, ship.transform.position.y, -5.5f);
            ship.transform.position = position;
        }

        //Place the cruisers
        for (int i = 0; i < cruisers.Count; i++)
        {
            Ship ship = cruisers[i];
            position = new Vector3(-(cruisers.Count / 2f - 0.5f) * 5.5f + (i) * 5.5f, ship.transform.position.y, 5.5f);
            ship.transform.position = position;
        }

        //Rotate the ships
        //Vector3 directionModifier = new Vector3(Mathf.Cos(fleetRotation * Mathf.Deg2Rad), 0f, Mathf.Sin(fleetRotation * Mathf.Deg2Rad)).normalized;

        foreach (Ship ship in attackers)
        {
            ship.gameObject.SetActive(true);
            ship.transform.rotation = Quaternion.Euler(Vector3.up * fleetRotation);
            Vector3 currentDirection = ship.transform.position.normalized;
            float radianAngle = fleetRotation * Mathf.Deg2Rad;
            //Vector3 targetDirection = new Vector3(Mathf.Cos(fleetRotation * Mathf.Deg2Rad), 0f, Mathf.Sin(fleetRotation * Mathf.Deg2Rad)).normalized;
            Vector3 localPosition = new Vector3(ship.transform.position.x * Mathf.Cos(radianAngle) - ship.transform.position.z * Mathf.Sin(radianAngle), 0f, ship.transform.position.z * Mathf.Cos(radianAngle) + ship.transform.position.x * Mathf.Sin(radianAngle));
            ship.transform.position = localPosition + fleetPosition;
        }



        ActionViewType[] tmp = (ActionViewType[])ActionViewType.GetValues(typeof(ActionViewType));
        //currentViewType = tmp[Random.Range(1, tmp.Length)];
        currentViewType = ActionViewType.BARREL_LINEAR_FOLLOW;
        actionViewTime = 0f;
        //Setup the specified action view type
        switch (currentViewType)
        {
            case ActionViewType.BARREL_LINEAR_FOLLOW:
                float highestTravelTime = 0f;
                List<Turret> availableTurrets = new List<Turret>();

                foreach (Ship ship in attackers)
                {
                    float travelTime = ship.PrepareToFireAt(BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition, BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].containedShip);
                    highestTravelTime = (travelTime > highestTravelTime) ? travelTime : highestTravelTime;


                    foreach (Turret turret in ship.turrets)
                    {
                        if (turret.canFire)
                        {
                            availableTurrets.Add(turret);
                        }
                    }

                }

                if (availableTurrets.Count > 0)
                {
                    selectedTurret = availableTurrets[Random.Range(0, availableTurrets.Count)];
                    selectedShip = selectedTurret.ship;
                    if (selectedShip != null)
                    {
                        Vector3 direction = selectedTurret.gunDirection;
                        float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
                        Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);

                        direction.y = 0f;
                        Cameraman.TakePosition(new Cameraman.CameraPosition(0.25f, selectedTurret.transform.position + Vector3.up * 0.3f - direction, angle));
                    }
                }


                highestTravelTime += 4f;
                BattleInterface.battle.switchTime = highestTravelTime;
                break;
            case ActionViewType.AERIAL_VIEW:
                float longestTravelTime = 0f;
                List<Turret> fireableTurrets = new List<Turret>();

                foreach (Ship ship in attackers)
                {
                    float travelTime = ship.PrepareToFireAt(BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].worldPosition, BattleInterface.battle.defendingPlayer.board.tiles[(int)BattleInterface.battle.recentlyShot.x, (int)BattleInterface.battle.recentlyShot.y].containedShip);
                    longestTravelTime = (travelTime > longestTravelTime) ? travelTime : longestTravelTime;


                    foreach (Turret turret in ship.turrets)
                    {
                        if (turret.canFire)
                        {
                            fireableTurrets.Add(turret);
                        }
                    }

                }

                if (fireableTurrets.Count > 0)
                {
                    selectedTurret = fireableTurrets[Random.Range(0, fireableTurrets.Count)];
                    selectedShip = selectedTurret.ship;
                    if (selectedShip != null)
                    {
                        Cameraman.TakePosition(new Cameraman.CameraPosition(0.25f, selectedShip.transform.position + Vector3.up * 20f, new Vector3(90f, 0f, 0f)));
                    }
                }


                foreach (Ship ship in attackers)
                {
                    ship.Fire();
                }


                longestTravelTime += 2f;
                BattleInterface.battle.switchTime = longestTravelTime;
                break;
        }
    }

    /// <summary>
    /// Types of action views.
    /// </summary>
    enum ActionViewType
    {
        NONE,
        BARREL_LINEAR_FOLLOW,
        AERIAL_VIEW,
    }

    /// <summary>
    /// The type of action view used for the current action shot.
    /// </summary>
    static ActionViewType currentViewType;

    /// <summary>
    /// The projectile tracked by the camera.
    /// </summary>
    static int trackedProjectileID = 0;
    static Turret selectedTurret;
    static Ship selectedShip;
    static float actionViewTime;
    /// <summary>
    /// Refreshes the action shot.
    /// </summary>
    static void RefreshActionView()
    {
        Debug.Log(currentViewType);
        switch (currentViewType)
        {
            case ActionViewType.BARREL_LINEAR_FOLLOW:
                Projectile projectile = selectedTurret.recentlyFiredProjectiles[trackedProjectileID];
                if (projectile != null)
                {
                    Debug.Log(projectile.name);
                    Vector3 direction = projectile.velocity.normalized;
                    Debug.Log(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg);
                    float xzDistance = Vector2.Distance(Vector2.zero, new Vector2(direction.x, direction.z));
                    Vector3 angle = new Vector3(Mathf.Atan2(-direction.y, xzDistance) * Mathf.Rad2Deg, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
                    //angle.x = -30f;
                    //angle.y += 90f;
                    Cameraman.TakePosition(new Cameraman.CameraPosition(0.22f, projectile.transform.position + Vector3.up, angle));
                }

                if (actionViewTime > 1f && actionViewTime < 1f + Time.deltaTime)
                {
                    foreach (Ship ship in BattleInterface.battle.attackingPlayer.livingShips)
                    {
                        if (ship.length == ship.lengthRemaining)
                        {
                            ship.Fire();
                        }
                    }
                }
                break;
        }

        actionViewTime += Time.deltaTime;
    }
}
