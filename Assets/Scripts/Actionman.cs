using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actionman : MonoBehaviour
{
    /// <summary>
    /// The currently running action shot module.
    /// </summary>
    static ActionShotModule currentActionShot;


    // Update is called once per frame
    void Update()
    {
        if (currentActionShot != null)
        {
            currentActionShot.Refresh();
        }
    }

    /// <summary>
    /// Prepares the game for an action shot of weapons firing.
    /// </summary>
    public static void ActionView()
    {
        int randomModuleID = 1;
        switch (FieldInterface.battle.recentTurnInformation.type)
        {
            case AttackType.ARTILLERY:
                //randomModuleID = Random.Range(1, 3);
                //TEST
                randomModuleID = 1;
                //TEST
                switch (randomModuleID)
                {
                    case 1:
                        currentActionShot = (BarrelInlineFollowActionShotModule)ScriptableObject.CreateInstance("BarrelInlineFollowActionShotModule");
                        break;
                    case 2:
                        currentActionShot = (AerialViewActionShotModule)ScriptableObject.CreateInstance("AerialViewActionShotModule");
                        break;
                }
                break;
            case AttackType.TORPEDO:
                randomModuleID = 1;
                switch (randomModuleID)
                {
                    case 1:
                        currentActionShot = (SingleDestroyerBaseModule)ScriptableObject.CreateInstance("SingleDestroyerBaseModule");
                        break;
                }
                break;
        }

        currentActionShot.Prepare();
    }

    /// <summary>
    /// Ends the action shot.
    /// </summary>
    public static void EndActionView()
    {
        Destroy(currentActionShot);
        FieldInterface.battle.switchTime = 1f;
    }

    /// <summary>
    /// Types of action views.
    /// </summary>
    enum ActionViewType
    {
        NONE,
        BARREL_LINEAR_FOLLOW,
        AERIAL_VIEW,
    }
}
