using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveAircraft : MonoBehaviour
{
    /// <summary>
    /// The player targeted by this squadron.
    /// </summary>
    public Player target;
    /// <summary>
    /// The player this squadron is supposed to target.
    /// </summary>
    public Player nextTarget;
    /// <summary>
    /// The aircraft of this squadron.
    /// </summary>
    public List<Aircraft> aircraft;
    /// <summary>
    /// Whether this squadron has taken off.
    /// </summary>
    public bool airborne;
    /// <summary>
    /// The number of turns remaining until the squadron gets to the target.
    /// </summary>
    public int travelTime = 2;
    /// <summary>
    /// The base carrier of this squadron.
    /// </summary>
    public AircraftCarrier carrier;

    /// <summary>
    /// Refreshes the squadron at the beginning of each turn.
    /// </summary>
    public void Refresh()
    {
        if (aircraft.Count > 0)
        {
            if (target == nextTarget)
            {
                travelTime = (travelTime > 0) ? travelTime - 1 : travelTime;
                if (target != null)
                {
                    if (travelTime == 0)
                    {
                        if (!target.overheadSquadrons.Contains(this))
                        {
                            target.overheadSquadrons.Add(this);
                        }
                    }
                }
            }
            else
            {
                Redirect(nextTarget);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets a new target for the squadron.
    /// </summary>
    /// <param name="target">Target.</param>
    void Redirect(Player target)
    {
        if (this.target != null)
        {
            if (this.target.overheadSquadrons.Contains(this))
            {
                this.target.overheadSquadrons.Remove(this);
            }
        }

        this.target = target;
        travelTime = 2;
    }

    /// <summary>
    /// Rolls the dice for the aircraft spotting the attacking player.
    /// </summary>
    public void Spot()
    {
        float spottingChance = aircraft.Count * ((FieldInterface.battle.recentTurnInformation.type == AttackType.ARTILLERY) ? 9 : 12);

        if (Random.Range(0, 100) < spottingChance)
        {
            if (FieldInterface.battle.recentTurnInformation.type == AttackType.ARTILLERY)
            {
                Ship selectedShip = target.livingShips[Random.Range(0, target.livingShips.Count)];
                selectedShip.RevealTo(carrier.owner);
            }
            else
            {
                List<Destroyer> destroyers = new List<Destroyer>();
                foreach (Ship ship in target.livingShips)
                {
                    if (ship.type == ShipType.DESTROYER)
                    {
                        destroyers.Add((Destroyer)ship);
                    }
                }

                Ship selectedShip = destroyers[Random.Range(0, destroyers.Count)];
                selectedShip.RevealTo(carrier.owner);
            }
        }
    }
}
