using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{

    //The y layer at which to process input
    public static float referenceY;

    //The current input position
    public static Vector3 currentInputPosition;
    //The current input position (in screen space)
    public static Vector3 currentScreenInputPosition;
    //Last input position
    public static Vector3 lastInputPosition;
    //Did the screen begin to get pressed in this frame?
    public static bool beginPress = false;
    //Did the screen get released in this frame?
    public static bool endPress = false;
    //Did the player release the screen without initiating a drag?
    public static bool tap = false;
    //Is the player dragging on the screen?
    public static bool dragging = false;
    //The initial input position
    public static Vector3 initialInputPosition;
    //Is the screen pressed
    public static bool pressed = false;
    //The distance between the initial and current inputPositions
    public static float deviation = 0f;
    //The distance the player needs to drag in order to register dragging
    public float dragRegisterDistance = 0.3f;

    // Update is called once per frame
    void Update()
    {
        if (Application.isMobilePlatform)
        {
            MobileInput();
        }
        else
        {
            PCInput();
        }

        Shared();

        lastInputPosition = currentInputPosition;
    }

    //Methods unique to pc
    void PCInput()
    {
        float clippingDistance = Camera.main.transform.position.y - referenceY;
        currentInputPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, clippingDistance));
        beginPress = Input.GetMouseButtonDown(0);
        endPress = Input.GetMouseButtonUp(0);
        pressed = Input.GetMouseButton(0);
    }

    //Methods unique to mobile
    bool lastState;
    void MobileInput()
    {
        float clippingDistance = Camera.main.transform.position.y - referenceY;
        if (Input.touchCount > 0)
        {
            pressed = true;
            Touch touch = Input.GetTouch(0);
            currentInputPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, clippingDistance));
        }
        else
        {
            pressed = false;
        }

        beginPress = !lastState && pressed;
        endPress = lastState && !pressed;

        lastState = pressed;
    }

    //Shared methods
    void Shared()
    {
        if (beginPress)
        {
            initialInputPosition = currentInputPosition;
        }

        tap = endPress && !dragging;

        if (pressed)
        {
            deviation = Vector3.Distance(currentInputPosition, initialInputPosition);
            if (deviation > dragRegisterDistance)
            {
                dragging = true;
            }
        }
        else
        {
            deviation = 0;
            dragging = false;
        }

        currentScreenInputPosition = Camera.main.WorldToScreenPoint(currentInputPosition);
    }
}
