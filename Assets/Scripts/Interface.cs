using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interface : MonoBehaviour {
	//Values accessed through the inspector
	//All the canvases to be loaded
    public Canvas[] canvases;

	//Values accessed by everything else
	static Dictionary<string, Canvas> menus;

    //The current menu
    static Canvas currentMenu;

    void Awake() {
        menus = new Dictionary<string, Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            menus.Add(canvases[i].name, canvases[i]);
        }
    }
    
    public static void SwitchMenu(string menuName) {
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
