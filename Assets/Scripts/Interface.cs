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
    static InterfaceScreen lastMenu;
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
            if (menus[menuName] != currentMenu)
            {
                if (currentMenu != null)
                {
                    currentMenu.gameObject.SetActive(false);
                    currentMenu.OnSwitchFrom();
                }

                lastMenu = currentMenu;
                currentMenu = menus[menuName];
                currentMenu.gameObject.SetActive(true);
                currentMenu.OnSwitchTo();
            }
        }
        else
        {
            if (menuName == "Last")
            {
                InterfaceScreen menu = currentMenu;
                currentMenu.gameObject.SetActive(false);
                currentMenu.OnSwitchFrom();
                currentMenu = lastMenu;
                currentMenu.OnSwitchTo();
                currentMenu.gameObject.SetActive(true);
                lastMenu = menu;
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
