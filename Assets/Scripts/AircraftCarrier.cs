using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftCarrier : Ship
{
    /// <summary>
    /// The reference object used to spawn the aircraft.
    /// </summary>
    public GameObject aircraftPrefab;
    /// <summary>
    /// All of the aircraft owned by this carrier.
    /// </summary>
    public List<Aircraft> ownedAircraft;
    /// <summary>
    /// All of the aircraft still stationed in the hangar.
    /// </summary> 
    public List<Aircraft> hangarAircraft;
    /// <summary>
    /// The active squadron of aircraft.
    /// </summary>
    public ActiveAircraft activeSquadron;
    /// <summary>
    /// The number of aircraft which can be put on the flight deck.
    /// </summary>
    public int flightDeckCapacity;
    /// <summary>
    /// The number of aircraft which can be put into the hangar.
    /// </summary>
    public int hangarCapacity;
    // Use this for initialization
    void Start()
    {
        InitializeAircraft();
        PrepareAircraft();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Initializes all of the aircraft.
    /// </summary>
    void InitializeAircraft()
    {
        ownedAircraft = new List<Aircraft>();
        hangarAircraft = new List<Aircraft>();
        for (int i = 0; i < hangarCapacity; i++)
        {
            Aircraft aircraft = Instantiate(aircraftPrefab).GetComponent<Aircraft>();

            aircraft.gameObject.SetActive(false);
            aircraft.transform.parent = transform;

            hangarAircraft.Add(aircraft);
            ownedAircraft.Add(aircraft);
        }
    }

    /// <summary>
    /// Prepares the aircraft onto the flight deck.
    /// </summary>
    void PrepareAircraft()
    {
        int centering = 1;
        int preparedAircraft = 0;
        Vector3 startingPos = new Vector3(-0.2f, 0.3f, -1.85f);
        float zSpacing = 0.35f;
        float xSpacing = 0.15f;
        if (activeSquadron == null)
        {
            activeSquadron = new GameObject("Active Squadron").AddComponent<ActiveAircraft>();
            activeSquadron.aircraft = new List<Aircraft>();
            activeSquadron.transform.parent = transform;
            activeSquadron.carrier = this;
        }
        else
        {
            if (!activeSquadron.airborne)
            {
                preparedAircraft = activeSquadron.aircraft.Count;
            }
            else
            {
                preparedAircraft = flightDeckCapacity;
            }
        }

        for (int i = preparedAircraft; i < flightDeckCapacity && i < hangarAircraft.Count + preparedAircraft; i++)
        {
            Vector3 finalPosition = startingPos + Vector3.forward * zSpacing * i + Vector3.right * xSpacing * centering;
            Aircraft aircraft = hangarAircraft[0];
            aircraft.Prepare(finalPosition, Vector3.up * 180f, i);
            aircraft.transform.parent = activeSquadron.transform;
            aircraft.owner = this;

            hangarAircraft.Remove(aircraft);
            activeSquadron.aircraft.Add(aircraft);
            centering *= -1;
        }
    }

    /// <summary>
    /// Refreshes the aircraft carrier.
    /// </summary>
    public void TurnAction()
    {
        if (activeSquadron == null)
        {
            if (!eliminated)
            {
                PrepareAircraft();
            }
        }
        else
        {
            if (!activeSquadron.airborne)
            {
                if (!eliminated)
                {
                    PrepareAircraft();
                }
                else
                {
                    Destroy(activeSquadron);
                }
            }
            activeSquadron.Refresh();
        }

    }
}
