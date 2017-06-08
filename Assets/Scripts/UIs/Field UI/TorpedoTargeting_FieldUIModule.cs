using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoTargeting_FieldUIModule : FieldUIModule
{
    /// <summary>
    /// The torpedo dummy used to indicate torpedo aiming.
    /// </summary>
    public GameObject dummyTorpedo;
    /// <summary>
    /// The direction the torpedoes are currently aimed in.
    /// </summary>
    Vector3 torpedoFiringDirection;
    /// <summary>
    /// Value used to determine whether the targeted tiles have changed.
    /// </summary>
    Vector2 refreshDecisionTemplate;
    /// <summary>
    /// The tiles which will be hit in the attack.
    /// </summary>
    BoardTile[] hits;
    /// <summary>
    /// The launch position of the torpedoes.
    /// </summary>
    Vector3 launchPosition;
    /// <summary>
    /// Whether a torpedo attack is available.
    /// </summary>
    bool attackAvailable;
    /// <summary>
    /// The initial distance between the dummy and the launch point.
    /// </summary>
    float referenceDistance;

    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected override void Enable()
    {
        base.Enable();
        attackAvailable = FieldInterface.battle.TorpedoAttackAvailable();

        transform.position = FieldInterface.battle.defendingPlayer.board.transform.position + Vector3.right * ((FieldInterface.battle.defendingPlayer.board.dimensions / 2f + 1) + FieldInterface.battle.defendingPlayer.board.dimensions * 0.075f);
        transform.rotation = Quaternion.Euler(Vector3.zero);

        if (!FieldInterface.battle.attackingPlayer.AI || GameController.humanPlayers == 0)
        {
            if (attackAvailable)
            {
                dummyTorpedo.transform.localScale = Vector3.one * FieldInterface.battle.defendingPlayer.board.dimensions;
                launchPosition = FieldInterface.battle.GetTorpedoLaunchPosition();
                ResetDummyPosition();
                Vector3 position = FieldInterface.battle.defendingPlayer.board.transform.position;
                position.y = 0;
                referenceDistance = Vector3.Distance(position, launchPosition) - FieldInterface.battle.defendingPlayer.board.dimensions / 2f;
                dummyTorpedo.SetActive(true);
            }
            else
            {
                if (FieldInterface.battle.attackingPlayer.torpedoRecharge < 8)
                {
                    int bars = FieldInterface.battle.attackingPlayer.torpedoRecharge;
                    float barScale = (float)FieldInterface.battle.defendingPlayer.board.dimensions / 8f;
                    float barSpacing = barScale * 1.2f;
                    float initialPosition = -(barSpacing * bars / 2f - barSpacing / 2f);
                    float barWidth = barScale * 1.5f;


                    for (int i = 0; i < bars; i++)
                    {
                        float actualPosition = initialPosition + i * barSpacing;
                        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        tmp.transform.parent = transform;
                        tmp.transform.localScale = new Vector3(barWidth, 0.1f, barScale);
                        tmp.transform.localPosition = new Vector3(0f, 0f, actualPosition);
                        Renderer renderer = tmp.GetComponent<Renderer>();
                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        renderer.material = GameController.playerBoardMarkerMaterial;
                        block.SetColor("_Color", new Color(1f, 0f, 0f, 0.6f));
                        //renderer.material.SetColor("_Color", colors[color]);
                        renderer.SetPropertyBlock(block);
                    }
                }
                dummyTorpedo.SetActive(false);
            }
        }
        else
        {
            dummyTorpedo.SetActive(false);
        }

        hits = null;
    }

    /// <summary>
    /// Disables the UI module.
    /// </summary>
    protected override void Disable()
    {
        base.Disable();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child != dummyTorpedo)
            {
                Destroy(child);
            }
        }
    }

    /// <summary>
    /// Updates the visuals of the module.
    /// </summary>
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
    }

    /// <summary>
    /// Accepts input from the player.
    /// </summary>
    protected override void UpdateInput()
    {
        base.UpdateInput();
        if (attackAvailable)
        {
            if (InputController.IsDragging(63))
            {
                Vector3 relativePosition = InputController.currentInputPosition - launchPosition;
                //dummyTorpedo.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0));
                //FieldInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
                torpedoFiringDirection = relativePosition.normalized;
                Vector3 targetRotation = new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0);
                Vector3 targetPosition = relativePosition.normalized * referenceDistance + launchPosition;
                targetPosition.y = FieldInterface.battle.defendingPlayer.board.transform.position.y;
                Vector2 deterministic = Vector2.zero;
                hits = FieldInterface.battle.GetTorpedoHits(launchPosition, launchPosition + torpedoFiringDirection * 30f);
                for (int i = 0; i < hits.Length; i++)
                {
                    BoardTile hit = hits[i];
                    //Debug.Log("Hit #: " + i + " Pos: " + hit.boardCoordinates);
                    deterministic += hit.boardCoordinates;
                }

                if (deterministic != refreshDecisionTemplate)
                {
                    FieldInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        BoardTile hit = hits[i];
                        hit.SetMarker(Color.yellow, FieldInterface.battle.defendingPlayer.board.grid.transform);
                    }
                    Debug.Log("Refresh: " + deterministic);
                    refreshDecisionTemplate = deterministic;
                }

                dummyTorpedo.transform.rotation = Quaternion.Euler(targetRotation);
                dummyTorpedo.transform.position = targetPosition;
            }
            else
            {
                if (hits != null)
                {
                    if (InputController.GetEndPress(63) && hits.Length > 0)
                    {
                        Debug.Log("Fire!");
                        //ResetTargetingUI();
                        FieldInterface.battle.TorpedoAttack(torpedoFiringDirection);
                        Disable();
                    }
                }
                ResetDummyPosition();
            }
        }
    }

    /// <summary>
    /// Resets the position of the dummy torpedo.
    /// </summary>
    void ResetDummyPosition()
    {
        dummyTorpedo.transform.localPosition = Vector3.zero;
        dummyTorpedo.transform.rotation = Quaternion.Euler(Vector3.zero);
    }
}
