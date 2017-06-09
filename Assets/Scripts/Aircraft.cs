using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aircraft : MonoBehaviour
{
    /// <summary>
    /// The owner of this aircraft.
    /// </summary>
    public AircraftCarrier owner;
    /// <summary>
    /// Takeoff order.
    /// </summary>
    public int takeoffOrder;
    /// <summary>
    /// Whether the aircraft was shot down.
    /// </summary>
    public bool shotDown;

    /// <summary>
    /// Prepares the aircraft onto the flight deck.
    /// </summary>
    public void Prepare(Vector3 targetPosition, Vector3 targetRotation, int takeoffOrder)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            transform.localPosition = targetPosition;
            transform.localRotation = Quaternion.Euler(targetRotation);
            this.takeoffOrder = takeoffOrder;
        }
    }

    /// <summary>
    /// Destroys the aircraft.
    /// </summary>
    public void ShotDown()
    {
        shotDown = true;
        transform.parent = null;
        owner.activeSquadron.aircraft.Remove(this);
    }
}
