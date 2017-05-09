using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceScreen : MonoBehaviour
{
    public bool blur;
    public int inputSecurityLevel;

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
