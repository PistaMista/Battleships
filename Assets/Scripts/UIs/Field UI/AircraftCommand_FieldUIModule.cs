using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftCommand_FieldUIModule : FieldUIModule
{
    /// <summary>
    /// The text mesh for showing how many aircraft are left in the hangar.
    /// </summary>
    public TextMesh reserveAircraftIndicator;
    /// <summary>
    /// The indicator used to show that aircraft are no longer available for launch.
    /// </summary>
    public GameObject aircraftUnavailableIndicator;
    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        AircraftCarrier linkedCarrier = FieldInterface.battle.attackingPlayer.aircraftCarrier;
        base.Enable();
        if (!linkedCarrier.eliminated && linkedCarrier.ownedAircraft.Count > 0)
        {
            reserveAircraftIndicator.gameObject.SetActive(true);
            aircraftUnavailableIndicator.SetActive(false);
            if (linkedCarrier.activeSquadron == null)
            {
                reserveAircraftIndicator.text = linkedCarrier.hangarAircraft.Count.ToString();
            }
            else
            {
                reserveAircraftIndicator.text = linkedCarrier.hangarAircraft.Count.ToString() + ">" + (Mathf.Clamp(linkedCarrier.flightDeckCapacity - linkedCarrier.activeSquadron.aircraft.Count, 0, linkedCarrier.hangarAircraft.Count));
            }
        }
        else
        {
            reserveAircraftIndicator.gameObject.SetActive(false);
            aircraftUnavailableIndicator.SetActive(true);
        }

        Renderer renderer = targetIndicatorMesh.gameObject.GetComponent<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_EmissionColor", FieldInterface.battle.attackingPlayer.color);
        renderer.SetPropertyBlock(block);

        if (linkedCarrier.activeSquadron != null)
        {
            if (linkedCarrier.activeSquadron.target != null)
            {
                MarkTargetedPlayer();
            }
        }
    }

    /// <summary>
    /// Disables the UI module.
    /// </summary>
    protected override void Disable()
    {
        base.Disable();
        Destroy(targetMarkerParent);
    }

    /// <summary>
    /// Updates the visuals of the module.
    /// </summary>
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
        if (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron != null)
        {
            UpdateIndicatorMesh();
        }
        else
        {
            targetIndicatorMesh.mesh = new Mesh();
        }

        if (targetMarkerParent != null)
        {
            Vector3 position = targetMarkerParent.transform.position;
            position.y = inputEnabled ? 3.6f : 0.1f;
            targetMarkerParent.transform.position = position;
        }
    }

    /// <summary>
    /// Accepts input from the player.
    /// </summary>
    protected override void UpdateInput()
    {
        base.UpdateInput();
        if (InputController.IsDragging(63) && InputController.deviation > 1f)
        {
            FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.nextTarget = TargetedPlayer();
        }
    }

    Player TargetedPlayer()
    {
        foreach (Player player in FieldInterface.battle.players)
        {
            float distance = Vector3.Distance(InputController.currentInputPosition, player.board.transform.position);
            if (distance < player.board.dimensions / 2f)
            {
                return player;
            }
        }
        return null;
    }

    /// <summary>
    /// The mesh of the player target indicator.
    /// </summary>
    public MeshFilter targetIndicatorMesh;
    /// <summary>
    /// The current state of the indicator.
    /// </summary>
    float currentIndicatorState = 0.5f;
    float currentStateChange;
    float currentAngle = 0;
    float currentAngleChange;
    void UpdateIndicatorMesh()
    {
        float targetState = (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.nextTarget == null) ? 0f : 1f;
        SetIndicatorMesh(Mathf.SmoothDamp(currentIndicatorState, targetState, ref currentStateChange, 0.1f, 10f), 5f, 7f, GameController.playerBoardDistanceFromCenter - FieldInterface.battle.attackingPlayer.board.dimensions / 2f);

        float targetAngle = 0;
        if (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.nextTarget != null)
        {
            targetAngle = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.nextTarget.ID * (360f / FieldInterface.battle.players.Length) - 90f;

        }

        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref currentAngleChange, 0.2f, 360f);
        targetIndicatorMesh.gameObject.transform.rotation = Quaternion.Euler(0, -currentAngle, 0);
    }

    /// <summary>
    /// Sets the target indicator mesh.
    /// </summary>
    /// <param name="state">The state of the mesh. 0 means semi-circle 1 means arrow.</param>
    void SetIndicatorMesh(float state, float innerRadius, float outerRadius, float arrowRadius)
    {
        if (currentIndicatorState != state)
        {
            Mesh mesh = new Mesh();
            float degreeFill = 270f * (1 - state * 0.75f);
            int sideSteps = (int)(degreeFill / 20f);
            Vector3[] vertices = new Vector3[2 + sideSteps * 4];
            vertices[0] = new Vector3(0, 0, innerRadius);
            vertices[1] = new Vector3(0, 0, outerRadius + (arrowRadius - outerRadius) * state);

            int[] triangles = new int[3 * (vertices.Length - 2)];

            int vertexIndex = 2;
            int triangleIndex = 0;

            for (int direction = -1; direction <= 1; direction += 2)
            {
                for (int i = 1; i <= sideSteps; i++)
                {
                    float angle = (10 * i * direction + 90) * Mathf.Deg2Rad;
                    Vector3 normal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    int lIndex1 = 0;
                    int lIndex2 = 1;

                    if (i != 1)
                    {
                        lIndex1 = vertexIndex - 2;
                        lIndex2 = vertexIndex - 1;
                    }

                    vertices[vertexIndex] = normal * innerRadius;
                    vertexIndex++;
                    vertices[vertexIndex] = normal * outerRadius;
                    vertexIndex++;

                    if (direction != 1)
                    {
                        triangles[triangleIndex * 3] = lIndex1;
                        triangles[triangleIndex * 3 + 1] = lIndex2;
                        triangles[triangleIndex * 3 + 2] = vertexIndex - 2;
                        triangleIndex++;
                        triangles[triangleIndex * 3] = lIndex2;
                        triangles[triangleIndex * 3 + 1] = vertexIndex - 1;
                        triangles[triangleIndex * 3 + 2] = vertexIndex - 2;
                        triangleIndex++;
                    }
                    else
                    {
                        triangles[triangleIndex * 3] = lIndex1;
                        triangles[triangleIndex * 3 + 1] = vertexIndex - 2;
                        triangles[triangleIndex * 3 + 2] = lIndex2;
                        triangleIndex++;
                        triangles[triangleIndex * 3] = lIndex2;
                        triangles[triangleIndex * 3 + 1] = vertexIndex - 2;
                        triangles[triangleIndex * 3 + 2] = vertexIndex - 1;
                        triangleIndex++;
                    }
                }
            }


            mesh.vertices = vertices;
            mesh.triangles = triangles;

            targetIndicatorMesh.mesh = mesh;
        }
        currentIndicatorState = state;
    }


    public GameObject markerEye;
    public GameObject markerChevron;
    /// <summary>
    /// The marker used to mark the targeted player of the aircraft.
    /// </summary>
    GameObject targetMarkerParent;
    /// <summary>
    /// Marks the targeted player.
    /// </summary>
    void MarkTargetedPlayer()
    {
        targetMarkerParent = new GameObject("Spotting Target Marker");
        targetMarkerParent.transform.parent = transform;
        targetMarkerParent.transform.position = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.target.board.transform.position + Vector3.up;

        int turnsLeft = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.travelTime;
        if (turnsLeft > 0)
        {
            Vector3 initialPosition = new Vector3(0, 0, -(Mathf.Clamp01((turnsLeft - 1)) * 0.5f + Mathf.Clamp((turnsLeft - 2) / 2f, 0, 10)));
            for (int i = 0; i < turnsLeft; i++)
            {
                Vector3 position = initialPosition + Vector3.forward * i;
                GameObject chevron = Instantiate(markerChevron);
                chevron.transform.parent = targetMarkerParent.transform;
                chevron.transform.localPosition = position;
            }
        }
        else
        {
            GameObject eye = Instantiate(markerEye);
            eye.transform.parent = targetMarkerParent.transform;
            eye.transform.localPosition = Vector3.zero;
        }
    }
}
