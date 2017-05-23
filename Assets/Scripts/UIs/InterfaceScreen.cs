using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceScreen : MonoBehaviour
{
    public bool blur = false;
    public int inputSecurityLevel = 63;
    public bool overrideMenuSwitching = false;

    public virtual void OnSwitchTo()
    {
        Cameraman.SetBlur(blur);
        InputController.securityLevel = inputSecurityLevel;
    }

    public virtual void OnSwitchFrom()
    {
        Cameraman.SetBlur(false);
        InputController.securityLevel = 63;
    }
}
