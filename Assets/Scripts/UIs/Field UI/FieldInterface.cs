using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldInterface : MonoBehaviour
{
    /// <summary>
    /// The attached battle.
    /// </summary>
    public static Battle battle;
    /// <summary>
    /// The attached UI modules.
    /// </summary>
    public FieldUIModule[] modules;

    BattleState currentState_last;
    BattleState nextState_last;
    bool linked = true;

    void Update()
    {
        if (battle != null)
        {
            if (currentState_last != battle.currentState || nextState_last != battle.nextState)
            {
                ReconsiderModules();
                currentState_last = battle.currentState;
                nextState_last = battle.nextState;

                Debug.Log("CS: " + battle.currentState);
                Debug.Log("NS: " + battle.nextState);
            }
            linked = true;
        }
        else if (linked)
        {
            linked = false;
            ReconsiderModules();
        }
    }

    /// <summary>
    /// Reconsiders whether the modules should be enabled right now.
    /// </summary>
    void ReconsiderModules()
    {
        for (int i = 0; i < modules.Length; i++)
        {
            modules[i].ReconsiderActivity();
        }
    }
}
