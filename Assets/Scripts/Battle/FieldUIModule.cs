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
    /// Tells the module to decide whether it has to be enabled right now.
    /// </summary>
    public void ReconsiderActivity()
    {
        if (CheckConditions() && !gameObject.activeInHierarchy)
        {
            Enable();
        }
        else if (gameObject.activeInHierarchy)
        {
            Disable();
        }
    }

    /// <summary>
    /// Checks whether conditions for activation are met.
    /// </summary>
    /// <returns>Whether the module should be enabled.</returns>
    bool CheckConditions()
    {
        if (currentStateConditions.Length != nextStateConditions.Length)
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
                    return true;
                }
            }
        }
        return false;
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
}
