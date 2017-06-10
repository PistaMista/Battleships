using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Used by the GUI to show the current state of the aircraft.
/// </summary>
public enum AircraftState
{
    LANDING,
    SPOTTING,
    ATTACKING,
    DEFENDING
}
public class ActiveAircraft : MonoBehaviour
{

    public AircraftState lastState;
    AircraftState nextState;
    public AircraftState NextState
    {
        get { return nextState; }
        set
        {
            if (!(value == AircraftState.LANDING && carrier.eliminated))
            {
                nextState = value;
            }
        }
    }

    /// <summary>
    /// The player targeted by this squadron.
    /// </summary>
    Player target;
    public Player Target
    {
        get { return target; }
        set { target = value; }
    }
    /// <summary>
    /// The player this squadron is supposed to target.
    /// </summary>
    Player nextTarget;
    public Player NextTarget
    {
        get { return nextTarget; }
        set
        {
            if (!(value == null && carrier.eliminated))
            {
                nextTarget = value;
            }
        }
    }
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
    public int travelTime = 0;
    public int initialTravelTime;
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
            if (Target == null && carrier.eliminated)
            {
                Redirect(carrier.owner);
            }

            if (Target != NextTarget)
            {
                Redirect(NextTarget);
            }

            travelTime = (travelTime > 0) ? travelTime - 1 : travelTime;



            if (travelTime == 0)
            {
                lastState = NextState;
                if (Target != null)
                {
                    if (!Target.overheadSquadrons.Contains(this))
                    {
                        Target.overheadSquadrons.Add(this);
                    }

                    bool defenderBonus = Target == carrier.owner;
                    foreach (ActiveAircraft squadron in Target.overheadSquadrons)
                    {
                        if (squadron != this)
                        {
                            for (int i = 0; i < squadron.aircraft.Count; i++)
                            {
                                Aircraft aircraft = squadron.aircraft[i];
                                if (Random.Range(0, 100) < (defenderBonus ? 30 : 8))
                                {
                                    aircraft.ShotDown();
                                    i--;
                                }
                            }
                        }
                    }
                }
                else
                {
                    airborne = false;
                }
            }

        }
        else
        {
            if (Target != null)
            {
                if (travelTime == 0)
                {
                    Target.overheadSquadrons.Remove(this);
                }
            }

            Destroy(gameObject);
        }

        transform.parent = airborne ? carrier.owner.battle.transform : carrier.transform;
    }

    /// <summary>
    /// Sets a new target for the squadron.
    /// </summary>
    /// <param name="target">Target.</param>
    void Redirect(Player target)
    {
        if (this.Target != null)
        {
            if (this.Target.overheadSquadrons.Contains(this))
            {
                this.Target.overheadSquadrons.Remove(this);
            }
        }

        travelTime = GetTravelTime(target);
        initialTravelTime = travelTime;
        airborne = true;
        this.Target = target;
    }

    /// <summary>
    /// Gets the travel time needed to reach the target.
    /// </summary>
    /// <param name="target">Target player.</param>
    /// <returns>Travel time.</returns>
    public int GetTravelTime(Player target)
    {
        if (this.Target != null)
        {
            if (this.Target == carrier.owner && target == null)
            {
                return 1;
            }
            else
            {
                return 3;
            }
        }
        else
        {
            if (target == carrier.owner)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }

    /// <summary>
    /// Rolls the dice for the aircraft spotting the attacking player.
    /// </summary>
    public void Spot()
    {
        float spottingChance = aircraft.Count * ((carrier.owner.battle.recentTurnInformation.type == TurnType.ARTILLERY) ? 9 : 12);

        if (Random.Range(0, 100) < spottingChance)
        {
            if (carrier.owner.battle.recentTurnInformation.type == TurnType.ARTILLERY)
            {
                Ship selectedShip = Target.livingShips[Random.Range(0, Target.livingShips.Count)];
                selectedShip.RevealTo(carrier.owner);
            }
            else
            {
                List<Destroyer> destroyers = new List<Destroyer>();
                foreach (Ship ship in Target.livingShips)
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
