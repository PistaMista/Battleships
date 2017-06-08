using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{

    //The y layer at which to process input
    /// <summary>
    /// The y layer at which to process input.
    /// </summary>
    public static float referenceY;

    /// <summary>
    /// The current input world position.
    /// </summary>
    public static Vector3 currentInputPosition;
    /// <summary>
    /// The current input screen position.
    /// </summary>
    public static Vector3 currentScreenInputPosition;
    /// <summary>
    /// Last input world position.
    /// </summary>
    public static Vector3 lastInputPosition;
    /// <summary>
    /// Whether the screen began to get pressed this frame.
    /// </summary>
    static bool beginPress = false;
    /// <summary>
    /// Whether the screen ended getting pressed this frame.
    /// </summary>
    static bool endPress = false;
    /// <summary>
    /// Whether the player released the screen without dragging.
    /// </summary>
    static bool tap = false;
    /// <summary>
    /// Whether the player is dragging.
    /// </summary>
    static bool dragging = false;
    /// <summary>
    /// The initial input world position.
    /// </summary>
    static Vector3 initialInputPosition;
    /// <summary>
    /// Whether the screen is pressed.
    /// </summary>
    static bool pressed = false;
    /// <summary>
    /// The distance between initial and current world input positions.
    /// </summary>
    public static float deviation = 0f;
    /// <summary>
    /// The distance the player needs to drag in order to register dragging.
    /// </summary>
    public float dragRegisterDistance = 0.3f;
    /// <summary>
    /// Determines what data will be returned when asking for variables.
    /// </summary>
    public static int securityLevel = 63;
    /// <summary>
    /// The update function.
    /// </summary>
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

    /// <summary>
    /// Methods used to calculate input for PC.
    /// </summary>
    void PCInput()
    {
        float clippingDistance = Camera.main.transform.position.y - referenceY;
        currentInputPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, clippingDistance));
        beginPress = Input.GetMouseButtonDown(0);
        endPress = Input.GetMouseButtonUp(0);
        pressed = Input.GetMouseButton(0);
    }


    bool lastState;
    /// <summary>
    /// Methods used to calculate input for mobile.
    /// </summary>
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

    /// <summary>
    /// The methods used to calculate input for PC and mobile.
    /// </summary>
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

    public static bool IsDragging(int permission)
    {
        if ((permission & securityLevel) > 0)
        {
            return dragging;
        }
        return false;
    }

    public static bool GetTap(int permission)
    {
        if ((permission & securityLevel) > 0)
        {
            return tap;
        }
        return false;
    }

    public static bool GetEndPress(int permission)
    {
        if ((permission & securityLevel) > 0)
        {
            return endPress;
        }
        return false;
    }

    public static bool GetBeginPress(int permission)
    {
        if ((permission & securityLevel) > 0)
        {
            return beginPress;
        }
        return false;
    }

    public void ChangeSecurityLevel(int level)
    {
        InputController.securityLevel = level;
    }
}
