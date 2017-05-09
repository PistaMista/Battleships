using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interface : MonoBehaviour
{
    //Values accessed through the inspector
    /// <summary>
    /// All the possible canvases to switch to.
    /// </summary>
    public InterfaceScreen[] screens;
    /// <summary>
    /// All the possible menus.
    /// </summary>
	static Dictionary<string, InterfaceScreen> menus;
    /// <summary>
    /// The current menu.
    /// </summary>
    static InterfaceScreen currentMenu;
    /// <summary>
    /// The last menu.
    /// </summary>
    static InterfaceScreen overrideMenu;
    /// <summary>
    /// Awake function.
    /// </summary>    
    void Awake()
    {
        menus = new Dictionary<string, InterfaceScreen>();
        for (int i = 0; i < screens.Length; i++)
        {
            menus.Add(screens[i].name, screens[i]);
        }
    }

    /// <summary>
    /// /// Switches the menu.
    /// </summary>
    /// <param name="menuName">Name of the menu to switch to. (The name of its game object)</param>
    public static void SwitchMenu(string menuName)
    {
        if (menus.ContainsKey(menuName))
        {
            if (!menus[menuName].overrideMenuSwitching || overrideMenu == null)
            {
                if (menus[menuName] != currentMenu && overrideMenu == null)
                {
                    if (currentMenu != null)
                    {
                        currentMenu.gameObject.SetActive(false);
                        currentMenu.OnSwitchFrom();
                    }

                    if (!menus[menuName].overrideMenuSwitching)
                    {
                        currentMenu = menus[menuName];
                        currentMenu.gameObject.SetActive(true);
                        currentMenu.OnSwitchTo();
                    }
                    else
                    {
                        overrideMenu = menus[menuName];
                        overrideMenu.gameObject.SetActive(true);
                        overrideMenu.OnSwitchTo();
                    }
                }
            }
            else
            {
                if (menus[menuName] != overrideMenu)
                {
                    overrideMenu.gameObject.SetActive(false);
                    overrideMenu.OnSwitchFrom();

                    overrideMenu = menus[menuName];
                    overrideMenu.OnSwitchTo();
                }
            }
        }
        else
        {
            if (menuName == "CANCEL_OVERRIDE" && overrideMenu != null)
            {
                overrideMenu.gameObject.SetActive(false);
                overrideMenu.OnSwitchFrom();
                if (currentMenu != null)
                {
                    currentMenu.OnSwitchTo();
                    currentMenu.gameObject.SetActive(true);
                }
                overrideMenu = null;
            }
        }
    }

    /// <summary>
    /// Switches the menu by buttons.
    /// </summary>
    /// <param name="menuName"></param>
    public void SwitchUIMenu(string menuName)
    {
        Interface.SwitchMenu(menuName);
    }
}
