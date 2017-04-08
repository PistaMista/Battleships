using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetAttackFormationBaseModule : ActionShotModule
{

    public override void Prepare()
    {
        base.Prepare();

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

        destroyers = new List<Ship>();
        cruisers = new List<Ship>();
        battleships = new List<Ship>();
        attackers = new List<Ship>();
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
    }

    public override void Refresh()
    {
        base.Refresh();

    }
}
