using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoTargetingBattleUIModule : MonoBehaviour
{

    // Use this for initialization
    void Awake()
    {
        dummyTorpedo = defaultDummyTorpedo;
    }
    /// <summary>
    /// The line used to show torpedo firing direction.
    /// </summary>
    public GameObject defaultDummyTorpedo;
    static GameObject dummyTorpedo;

    /// <summary>
    /// The direction the torpedoes are currently aimed in.
    /// </summary>
    static Vector3 torpedoFiringDirection;
    /// <summary>
    /// Value used to determine whether the targeted tiles have changed.
    /// </summary>
    static Vector2 refreshDecisionTemplate;

    //VERSION 2
    /// <summary>
    /// The tiles which will be hit in the attack.
    /// </summary>
    static BoardTile[] hits;
    /// <summary>
    /// The launch position of the torpedoes.
    /// </summary>
    static Vector3 launchPosition;
    /// <summary>
    /// The parent of all UI objects used by this UI.
    /// </summary>
    static GameObject UIParent;
    /// <summary>
    /// Whether the UI is enabled.
    /// </summary>
    static bool activated;
    /// <summary>
    /// Whether a torpedo attack is available.
    /// </summary>
    static bool attackAvailable;
    /// <summary>
    /// The initial distance between the dummy and the launch point.
    /// </summary>
    static float referenceDistance;

    /// <summary>
    /// Enables the torpedo UI.
    /// </summary>
    public static void Enable()
    {
        if (!activated)
        {
            attackAvailable = BattleInterface.battle.TorpedoAttackAvailable();
            if (attackAvailable)
            {
                UIParent = dummyTorpedo;
                dummyTorpedo.transform.localScale = Vector3.one * BattleInterface.battle.defendingPlayer.board.dimensions;
                launchPosition = BattleInterface.battle.GetTorpedoLaunchPosition();
                ResetDummyPosition();
                Vector3 position = BattleInterface.battle.defendingPlayer.board.transform.position;
                position.y = 0;
                referenceDistance = Vector3.Distance(position, launchPosition) - BattleInterface.battle.defendingPlayer.board.dimensions / 2f;
            }
            else
            {
                UIParent = new GameObject("Recharge Time Indicator");
                if (BattleInterface.battle.attackingPlayer.torpedoRecharge < 8)
                {
                    int bars = BattleInterface.battle.attackingPlayer.torpedoRecharge;
                    float barSpacing = (float)BattleInterface.battle.defendingPlayer.board.dimensions / (bars + 3);
                    float initialPosition = -(barSpacing * bars / 2f - barSpacing / 2f);

                    for (int i = 0; i < bars; i++)
                    {
                        float actualPosition = initialPosition + i * barSpacing;
                        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        tmp.transform.parent = UIParent.transform;
                        tmp.transform.localScale = new Vector3(1f, 0.1f, barSpacing / 2f);
                        tmp.transform.localPosition = new Vector3(0f, 0f, actualPosition);
                        Renderer renderer = tmp.GetComponent<Renderer>();
                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        renderer.material = GameController.playerBoardMarkerMaterial;
                        block.SetColor("_Color", new Color(1f, 0f, 0f, 0.6f));
                        //renderer.material.SetColor("_Color", colors[color]);
                        renderer.SetPropertyBlock(block);
                    }
                }
                ResetDummyPosition();
            }
            UIParent.SetActive(true);
            activated = true;
        }
    }

    /// <summary>
    /// Updates the torpedo UI.
    /// </summary>
    public void Update()
    {
        if (activated)
        {
            if (attackAvailable)
            {
                if (BattleInterface.battle.switchTime <= 0)
                {
                    if (InputController.IsDragging(63))
                    {
                        Vector3 relativePosition = InputController.currentInputPosition - launchPosition;
                        //dummyTorpedo.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0));
                        //BattleInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
                        torpedoFiringDirection = relativePosition.normalized;
                        Vector3 targetRotation = new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0);
                        Vector3 targetPosition = relativePosition.normalized * referenceDistance + launchPosition;
                        targetPosition.y = BattleInterface.battle.defendingPlayer.board.transform.position.y;
                        Vector2 deterministic = Vector2.zero;
                        hits = BattleInterface.battle.GetTorpedoHits(launchPosition, launchPosition + torpedoFiringDirection * 30f);
                        for (int i = 0; i < hits.Length; i++)
                        {
                            BoardTile hit = hits[i];
                            //Debug.Log("Hit #: " + i + " Pos: " + hit.boardCoordinates);
                            deterministic += hit.boardCoordinates;
                        }

                        if (deterministic != refreshDecisionTemplate)
                        {
                            BattleInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
                            for (int i = 0; i < hits.Length; i++)
                            {
                                BoardTile hit = hits[i];
                                hit.SetMarker(Color.yellow, BattleInterface.battle.defendingPlayer.board.grid.transform);
                            }
                            Debug.Log("Refresh: " + deterministic);
                            refreshDecisionTemplate = deterministic;
                        }

                        dummyTorpedo.transform.rotation = Quaternion.Euler(targetRotation);
                        dummyTorpedo.transform.position = targetPosition;
                    }
                    else
                    {
                        if (InputController.GetEndPress(63) && hits.Length > 0)
                        {
                            Debug.Log("Fire!");
                            //ResetTargetingUI();
                            BattleInterface.battle.TorpedoAttack(torpedoFiringDirection);
                            Disable();
                        }
                        ResetDummyPosition();
                    }
                }
            }
            else
            {

            }
        }
    }

    /// <summary>
    /// Disables the torpedo UI.
    /// </summary>
    public static void Disable()
    {
        if (activated)
        {

            UIParent.SetActive(false);
            activated = false;
            if (!attackAvailable)
            {
                Destroy(UIParent.gameObject);
            }
        }
    }

    /// <summary>
    /// Resets the position of the dummy torpedo.
    /// </summary>
    static void ResetDummyPosition()
    {
        UIParent.transform.position = BattleInterface.battle.defendingPlayer.board.transform.position + Vector3.right * ((BattleInterface.battle.defendingPlayer.board.dimensions / 2f + 1) + BattleInterface.battle.defendingPlayer.board.dimensions * 0.075f);
        UIParent.transform.rotation = Quaternion.Euler(Vector3.zero);
    }
}
