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
    public Canvas[] canvases;
    /// <summary>
    /// All the possible menus.
    /// </summary>
	static Dictionary<string, Canvas> menus;
    /// <summary>
    /// The current menu.
    /// </summary>
    static Canvas currentMenu;
    /// <summary>
    /// Awake function.
    /// </summary>    
    void Awake()
    {
        menus = new Dictionary<string, Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            menus.Add(canvases[i].name, canvases[i]);
        }
    }

    /// <summary>
    /// /// Switches the menu.
    /// </summary>
    /// <param name="menuName">Name of the menu to switch to. (The name of its game object)</param>
    public static void SwitchMenu(string menuName)
    {
        if (menus[menuName] != currentMenu)
        {
            if (currentMenu != null)
            {
                currentMenu.gameObject.SetActive(false);
            }

            currentMenu = menus[menuName];
            currentMenu.gameObject.SetActive(true);
        }
    }

}
