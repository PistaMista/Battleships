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
    public int travelTime;
}
