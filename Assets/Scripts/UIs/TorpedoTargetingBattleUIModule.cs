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

    // Update is called once per frame
    void Update()
    {

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
    /// Whether the dummy torpedo is set up.
    /// </summary>
    static bool dummySetUp = false;

    /// <summary>
    /// Value used to determine whether the targeted tiles have changed.
    /// </summary>
    static Vector2 refreshDecisionTemplate;

    /// <summary>
    /// Cycles the torpedo targeting module.
    /// </summary>
    public static void Cycle()
    {
        if (BattleInterface.battle.state == BattleState.CHOOSING_TILE_TO_SHOOT && BattleInterface.battle.switchTime < -Time.deltaTime && !BattleInterface.battle.attackingPlayer.AI && BattleInterface.battle.attackingPlayer.torpedoRecharge == 0)
        {
            Vector3 launchPosition = BattleInterface.battle.GetTorpedoLaunchPosition();
            Vector3 targetPosition = Vector3.zero;
            Vector3 targetRotation = Vector3.zero;
            if (InputController.IsDragging(63) && dummySetUp)
            {
                targetPosition = InputController.currentInputPosition;
                targetPosition.y = BattleInterface.battle.defendingPlayer.board.transform.position.y;
                Vector3 relativePosition = InputController.currentInputPosition - launchPosition;
                //dummyTorpedo.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0));
                //BattleInterface.battle.defendingPlayer.board.Set(BoardState.ENEMY);
                torpedoFiringDirection = relativePosition.normalized;
                targetRotation = new Vector3(0, Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg, 0);

                Vector2 deterministic = Vector2.zero;
                BoardTile[] hits = BattleInterface.battle.GetTorpedoHits(launchPosition, torpedoFiringDirection * 30f);
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

            }
            else
            {
                BoardTile[] hits = BattleInterface.battle.GetTorpedoHits(launchPosition, torpedoFiringDirection * 30f);
                if (InputController.GetEndPress(63) && hits.Length > 0)
                {
                    Debug.Log("Fire!");
                    //ResetTargetingUI();
                    BattleInterface.battle.TorpedoAttack(torpedoFiringDirection);
                    dummyTorpedo.SetActive(false);
                }
                targetPosition = BattleInterface.battle.defendingPlayer.board.transform.position + Vector3.right * ((BattleInterface.battle.defendingPlayer.board.dimensions / 2f + 1) + BattleInterface.battle.defendingPlayer.board.dimensions * 0.075f);
                targetRotation = Vector3.zero;
            }

            if (!dummySetUp)
            {
                dummyTorpedo.transform.position = targetPosition;
                dummyTorpedo.transform.rotation = Quaternion.Euler(targetRotation);
                dummyTorpedo.transform.localScale = Vector3.one * BattleInterface.battle.defendingPlayer.board.dimensions;
                dummyTorpedo.SetActive(true);
                dummySetUp = true;
            }

            dummyTorpedo.transform.position = targetPosition;
            dummyTorpedo.transform.rotation = Quaternion.Euler(targetRotation);
        }
        else
        {
            if (dummySetUp)
            {
                dummyTorpedo.SetActive(false);
                dummySetUp = false;
            }
        }
    }
}
