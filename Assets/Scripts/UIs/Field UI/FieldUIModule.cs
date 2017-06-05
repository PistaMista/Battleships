using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldUIModule : MonoBehaviour
{



    /// <summary>
    /// The current state activation conditions.
    /// </summary>
    public BattleState[] currentStateConditions;
    /// <summary>
    /// The next state activation conditions.
    /// </summary>
    public BattleState[] nextStateConditions;
    /// <summary>
    /// The input activation conditions.
    /// </summary>
    public bool[] controlEnabledConditions;

    /// <summary>
    /// Whether this module is allowed to accept user input.
    /// </summary>
    public bool inputEnabled;

    /// <summary>
    /// Tells the module to decide whether it has to be enabled right now.
    /// </summary>
    public void ReconsiderActivity()
    {
        if (CheckConditions())
        {
            if (!gameObject.activeInHierarchy)
            {
                Enable();
            }
        }
        else
        {
            if (gameObject.activeInHierarchy)
            {
                Disable();
            }
        }
    }

    /// <summary>
    /// Checks whether conditions for activation are met.
    /// </summary>
    /// <returns>Whether the module should be enabled.</returns>
    bool CheckConditions()
    {
        if (currentStateConditions.Length != nextStateConditions.Length || controlEnabledConditions.Length != nextStateConditions.Length)
        {
            Debug.LogError("UI MODULE ERROR: Condition lengths do not match. Module will never be enabled.");
        }
        else if (FieldInterface.battle != null)
        {
            for (int i = 0; i < currentStateConditions.Length; i++)
            {
                bool currentStateCondition = FieldInterface.battle.currentState == currentStateConditions[i] || currentStateConditions[i] == BattleState.ALL;
                bool nextStateCondition = FieldInterface.battle.nextState == nextStateConditions[i] || nextStateConditions[i] == BattleState.ALL;
                if (currentStateCondition && nextStateCondition)
                {
                    inputEnabled = controlEnabledConditions[i];
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Updates the UI module when it is enabled.
    /// </summary>
    void Update()
    {
        UpdateVisuals();
        if (inputEnabled)
        {
            UpdateInput();
        }
    }

    /// <summary>
    /// Enables the UI module.
    /// </summary>
    protected virtual void Enable()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Disables the UI module.
    /// </summary>
    protected virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the visuals of the module.
    /// </summary>
    protected virtual void UpdateVisuals()
    {

    }

    /// <summary>
    /// Accepts input from the player.
    /// </summary>
    protected virtual void UpdateInput()
    {

    }
}
