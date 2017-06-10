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
            aircraftUnavailableIndicator.SetActive(false);

        }
        else
        {
            aircraftUnavailableIndicator.SetActive(true);
        }

        if (linkedCarrier.activeSquadron == null)
        {
            if (!linkedCarrier.eliminated)
            {
                reserveAircraftIndicator.text = linkedCarrier.hangarAircraft.Count.ToString();
            }
            else
            {
                reserveAircraftIndicator.text = "";
            }
        }
        else
        {
            if (!linkedCarrier.eliminated)
            {
                reserveAircraftIndicator.text = (linkedCarrier.activeSquadron.aircraft.Count) + "/" + (linkedCarrier.flightDeckCapacity) + " + " + linkedCarrier.hangarAircraft.Count.ToString();
            }
            else
            {
                reserveAircraftIndicator.text = (linkedCarrier.activeSquadron.aircraft.Count) + "/" + (linkedCarrier.flightDeckCapacity);
            }
        }

        if (linkedCarrier.activeSquadron != null)
        {
            if (linkedCarrier.activeSquadron.travelTime == 0 && linkedCarrier.activeSquadron.Target != null)
            {
                if (linkedCarrier.activeSquadron.Target.overheadSquadrons.Count > 1)
                {
                    linkedCarrier.activeSquadron.NextState = AircraftState.ATTACKING;
                }
            }
            DrawStatusIndicator();
        }

        currentIndicatorState = 0.5f;
        if (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron != null)
        {
            UpdateIndicatorMesh(true);
        }
        else
        {
            targetIndicatorMesh.mesh = new Mesh();
        }
        MarkThreats();
    }

    /// <summary>
    /// Disables the UI module.
    /// </summary>
    protected override void Disable()
    {
        base.Disable();
        Destroy(statusMarkerParent);
        Destroy(warningMarkerParent);
    }

    /// <summary>
    /// Updates the visuals of the module.
    /// </summary>
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
        if (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron != null)
        {
            UpdateIndicatorMesh(false);
        }

        if (warningMarkerParent != null)
        {
            warningMarkerParent.SetActive(FieldInterface.battle.nextState == BattleState.CHOOSING_TARGET);
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
            ActiveAircraft squadron = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron;
            Player targetedPlayer = TargetedPlayer();
            if (targetedPlayer == FieldInterface.battle.attackingPlayer)
            {
                squadron.NextState = AircraftState.DEFENDING;
            }
            else if (targetedPlayer == null)
            {
                squadron.NextState = AircraftState.LANDING;
            }
            else
            {
                if (squadron.travelTime == 0 && targetedPlayer.overheadSquadrons.Count > 1)
                {
                    squadron.NextState = AircraftState.ATTACKING;
                }
                else
                {
                    squadron.NextState = AircraftState.SPOTTING;
                }
            }

            if (targetedPlayer != squadron.NextTarget)
            {
                squadron.NextTarget = targetedPlayer;
                DrawStatusIndicator();
            }
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
    void UpdateIndicatorMesh(bool instant)
    {
        float targetState = (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.NextTarget == null) ? -0.1f : 1f;

        SetIndicatorMesh(Mathf.Clamp01(instant ? targetState : Mathf.SmoothDamp(currentIndicatorState, targetState, ref currentStateChange, 0.1f, 10f)), 5f, 7f, (GameController.playerBoardDistanceFromCenter - FieldInterface.battle.attackingPlayer.board.dimensions / 2f) / 1.2f);

        float targetAngle = 0;
        if (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.NextTarget != null)
        {
            targetAngle = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.NextTarget.ID * (360f / FieldInterface.battle.players.Length) - 90f;

        }

        currentAngle = instant ? targetAngle : Mathf.SmoothDamp(currentAngle, targetAngle, ref currentAngleChange, 0.2f, 360f);
        targetIndicatorMesh.gameObject.transform.rotation = Quaternion.Euler(0, -currentAngle, 0);
    }

    /// <summary>
    /// Sets the target indicator mesh.
    /// </summary>
    /// <param name="state">The state of the mesh. 0 means circle 1 means arrow.</param>
    void SetIndicatorMesh(float state, float innerRadius, float outerRadius, float arrowRadius)
    {
        if (currentIndicatorState != state)
        {
            Mesh mesh = new Mesh();
            float degreeFill = 360f * (1 - state * 0.75f);
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

    /// <summary>
    /// The indicators used to make up the status indicator.
    /// </summary>
    public GameObject[] indicators;
    public GameObject emptyChevron;
    public GameObject fullChevron;
    /// <summary>
    /// The parent of the status marker.
    /// </summary>
    GameObject statusMarkerParent;
    /// <summary>
    /// Draws the status indicator.
    /// </summary>
    void DrawStatusIndicator()
    {
        Destroy(statusMarkerParent);
        ActiveAircraft squadron = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron;
        statusMarkerParent = new GameObject("Status Indicator");
        statusMarkerParent.transform.parent = transform;
        statusMarkerParent.transform.localPosition = new Vector3(0, 2f, -10f);
        if (squadron.NextState == squadron.lastState && squadron.travelTime == 0)
        {
            GameObject tmp = Instantiate(indicators[(int)squadron.lastState]);
            tmp.transform.parent = statusMarkerParent.transform;
            tmp.transform.localPosition = Vector3.zero;
        }
        else
        {
            int steps = 3;
            if (squadron.NextTarget == squadron.Target)
            {
                steps = 3 + Mathf.Clamp((squadron.initialTravelTime - 1), 0, 20);
            }
            else
            {
                steps = 3 + Mathf.Clamp((squadron.GetTravelTime(squadron.NextTarget) - 1), 0, 20);
            }
            Vector3 initialPosition = new Vector3(-(Mathf.Clamp01((steps - 1)) * 1.5f + Mathf.Clamp((steps - 2) * 1.5f, 0, 10)), 0, 0);
            for (int i = 0; i < steps; i++)
            {
                GameObject tmp;
                if (i == 0)
                {
                    tmp = Instantiate(indicators[(int)squadron.lastState]);
                }
                else if (i == steps - 1)
                {
                    tmp = Instantiate(indicators[(int)squadron.NextState]);
                }
                else
                {
                    int fullChevrons = squadron.initialTravelTime - squadron.travelTime;
                    if ((i - 1) < fullChevrons && squadron.NextTarget == squadron.Target)
                    {
                        tmp = Instantiate(fullChevron);
                    }
                    else
                    {
                        tmp = Instantiate(emptyChevron);
                    }
                }

                tmp.transform.parent = statusMarkerParent.transform;
                tmp.transform.localPosition = initialPosition + Vector3.right * i * 3;
            }
        }
    }

    /// <summary>
    /// Warning indicator.
    /// </summary>
    public GameObject warningIndicator;
    GameObject warningMarkerParent;
    /// <summary>
    /// Marks players who have sent aircraft to the attacking player.
    /// </summary>
    void MarkThreats()
    {
        warningMarkerParent = new GameObject("Threat Markers");
        warningMarkerParent.transform.parent = transform;

        foreach (Player player in FieldInterface.battle.players)
        {
            if (player != FieldInterface.battle.attackingPlayer)
            {
                if (player.aircraftCarrier.activeSquadron != null)
                {
                    if (player.aircraftCarrier.activeSquadron.Target == FieldInterface.battle.attackingPlayer)
                    {
                        GameObject warning = Instantiate(warningIndicator);
                        warning.transform.parent = warningMarkerParent.transform;

                        Player attackerTarget = null;
                        if (FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron != null)
                        {
                            attackerTarget = FieldInterface.battle.attackingPlayer.aircraftCarrier.activeSquadron.Target;
                        }

                        warning.transform.position = player.board.transform.position + new Vector3(0, 1f, 0f);
                    }
                }
            }
        }
    }
}
