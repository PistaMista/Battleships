using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Cameraman : MonoBehaviour
{

    // Use this for initialization
    /// <summary>
    /// A position for the camera to use.
    /// </summary>
    public struct CameraPosition
    {
        /// <summary>
        /// The default transition time.
        /// </summary>
        public float transitionTime;
        /// <summary>
        /// The world position.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The world rotation.
        /// </summary>
        public Vector3 rotation;
        public CameraPosition(float transitionSpeed, Vector3 targetPosition, Vector3 targetRotation)
        {
            if (Mathf.Abs(targetRotation.x) > 180f)
            {
                targetRotation.x = (targetRotation.x % 360f) - 360f * Mathf.Sign(targetRotation.x);
            }

            if (Mathf.Abs(targetRotation.y) > 180f)
            {
                targetRotation.y = (targetRotation.y % 360f) - 360f * Mathf.Sign(targetRotation.y);
            }

            if (Mathf.Abs(targetRotation.z) > 180f)
            {
                targetRotation.z = (targetRotation.z % 360f) - 360f * Mathf.Sign(targetRotation.z);
            }


            this.transitionTime = transitionSpeed;
            this.position = targetPosition;
            this.rotation = targetRotation;
        }
    }

    /// <summary>
    /// The current position of the camera.
    /// </summary>
    public static CameraPosition currentPosition;
    /// <summary>
    /// The target position of the camera.
    /// </summary>
    public static CameraPosition currentTargetPosition;
    /// <summary>
    /// The transition progress.
    /// </summary>
    public static float transitionProgress;
    /// <summary>
    /// The list of possible camera positions.
    /// </summary>
    static Dictionary<string, CameraPosition> possiblePositions;
    /// <summary>
    /// The rate of change in position.
    /// </summary>
    static Vector3 positionChange = Vector3.zero;
    /// <summary>
    /// The rate of change in rotation.
    /// </summary>
    static Vector3 rotationChange = Vector3.zero;
    /// <summary>
    /// The rate of change in transition progress.
    /// </summary>
    static float progressChange = 0f;
    /// <summary>
    /// The start function.
    /// </summary>
    void Start()
    {
        possiblePositions = new Dictionary<string, CameraPosition>();
        SetUpBasicPositions();
        TakePosition("Overhead View");
    }
    /// <summary>
    /// Sets up basic positions for use by the camera.
    /// </summary>
    public static void SetUpBasicPositions()
    {
        possiblePositions.Add("Overhead Title View", new CameraPosition(1f, new Vector3(0, 15f, -20f), new Vector3(30f, 0, 0)));
        //possiblePositions.Add("Overhead Title View", new CameraPosition(1f, new Vector3(0, 15f, -20f), new Vector3(, 0, 0)));

        possiblePositions.Add("Overhead View", new CameraPosition(1f, new Vector3(0, 40f, 0), new Vector3(90, 0, 0)));
    }
    /// <summary>
    /// Adds a new position to the list of possible positions.
    /// </summary>
    /// <param name="transitionTime">The default transition time.</param>
    /// <param name="targetPosition">The world position.</param>
    /// <param name="targetRotation">The world rotation.</param>
    /// <param name="name">The name of this position.</param>
    public static void AddPosition(float transitionTime, Vector3 targetPosition, Vector3 targetRotation, string name)
    {
        if (possiblePositions.ContainsKey(name))
        {
            possiblePositions.Remove(name);
        }
        possiblePositions.Add(name, new CameraPosition(transitionTime, targetPosition, targetRotation));

    }

    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        if (transitionProgress < 100f)
        {
            transitionProgress = Mathf.SmoothDamp(transitionProgress, 100f, ref progressChange, currentTargetPosition.transitionTime);

            currentPosition.position = Vector3.SmoothDamp(currentPosition.position, currentTargetPosition.position, ref positionChange, currentTargetPosition.transitionTime);
            currentPosition.rotation = Vector3.SmoothDamp(currentPosition.rotation, currentTargetPosition.rotation, ref rotationChange, currentTargetPosition.transitionTime);

            transform.position = currentPosition.position;
            transform.rotation = Quaternion.Euler(currentPosition.rotation);
        }
        //Debug.Log(transitionProgress);
        //Debug.Log(currentTargetPosition.position);
    }
    /// <summary>
    /// Transfers the camera to a position of that name.
    /// </summary>
    /// <param name="name">The name of the position to use.</param>
    public static void TakePosition(string name)
    {
        currentPosition = new CameraPosition(0f, Camera.main.transform.position, Camera.main.transform.rotation.eulerAngles);
        CameraPosition position = possiblePositions[name];
        position.rotation.x = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.x, position.rotation.x);
        position.rotation.y = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.y, position.rotation.y);
        position.rotation.z = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.z, position.rotation.z);
        currentTargetPosition = position;
        transitionProgress = 0f;
        progressChange = 0f;
        //positionChange = Vector3.zero;
        //rotationChange = Vector3.zero;
    }
    /// <summary>
    /// Transfers the camera to a position of that name. Overrides transition time.
    /// </summary>
    /// <param name="name">The name of the position to use.</param>
    /// <param name="transitionTimeOverride">New transition time.</param>
    public static void TakePosition(string name, float transitionTimeOverride)
    {
        currentPosition = new CameraPosition(0f, Camera.main.transform.position, Camera.main.transform.rotation.eulerAngles);
        CameraPosition position = possiblePositions[name];
        position.rotation.x = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.x, position.rotation.x);
        position.rotation.y = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.y, position.rotation.y);
        position.rotation.z = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.z, position.rotation.z);
        position.transitionTime = transitionTimeOverride;
        currentTargetPosition = position;
        transitionProgress = 0f;
        progressChange = 0f;
        //positionChange = Vector3.zero;
        //rotationChange = Vector3.zero;
    }

    /// <summary>
    /// Transfers the camera to the newly created position. The position will not be saved.
    /// </summary>
    /// <param name="position">The position which to take.</param>
    public static void TakePosition(CameraPosition position)
    {

        currentPosition = new CameraPosition(0f, Camera.main.transform.position, Camera.main.transform.rotation.eulerAngles);
        position.rotation.x = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.x, position.rotation.x);
        position.rotation.y = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.y, position.rotation.y);
        position.rotation.z = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.z, position.rotation.z);
        currentTargetPosition = position;
        transitionProgress = 0f;
        progressChange = 0f;
        //positionChange = Vector3.zero;
        //rotationChange = Vector3.zero;
    }

    /// <summary>
    /// Determines which way to rotate in order to achieve the shortest rotation.
    /// </summary>
    /// <param name="currentRotation">The current rotation.</param>
    /// <param name="targetRotation">The target rotation.</param>
    /// <returns>New target rotation.</returns>
    static float GetTargetRotationComponentForShortestPath(float currentRotation, float targetRotation)
    {
        if (Mathf.Abs(currentRotation - targetRotation) > Mathf.Abs(currentRotation - targetRotation + 360))
        {
            return targetRotation + 360;
        }
        else
        {
            return targetRotation;
        }
    }
    /// <summary>
    /// Sets the blur effect.
    /// </summary>
    /// <param name="enabled">Blur effect enabled.</param>
    public static void SetBlur(bool enabled)
    {
        Camera.main.GetComponent<Blur>().enabled = enabled;
    }
    /// <summary>
    /// Enables/Disables orthographic mode.
    /// </summary>
    /// <param name="enabled">Whether to enable or disable orthographic mode.</param>
    public static void SetOrthographic(bool enabled)
    {
        Camera.main.orthographic = enabled;
    }
    /// <summary>
    /// Compares two camera positions.
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns>Whether the camera positions match.</returns>
    static bool PositionsEqual(CameraPosition position1, CameraPosition position2)
    {
        bool cond1 = position1.position == position2.position;
        bool cond2 = position1.rotation == position2.rotation;
        bool cond3 = position1.transitionTime == position2.transitionTime;
        return cond1 && cond2 && cond3;
    }
}
