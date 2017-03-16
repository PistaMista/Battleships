using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Cameraman : MonoBehaviour
{

    // Use this for initialization
    public struct CameraPosition
    {
        public float transitionTime;
        public Vector3 position;
        public Vector3 rotation;
        public CameraPosition(float transitionSpeed, Vector3 targetPosition, Vector3 targetRotation)
        {
            this.transitionTime = transitionSpeed;
            this.position = targetPosition;
            this.rotation = targetRotation;
        }
    }

    public static CameraPosition currentPosition;
    public static CameraPosition currentTargetPosition;
    public static float transitionProgress;
    static Dictionary<string, CameraPosition> possiblePositions;
    static Vector3 positionChange = Vector3.zero;
    static Vector3 rotationChange = Vector3.zero;
    static float progressChange = 0f;

    void Start()
    {
        possiblePositions = new Dictionary<string, CameraPosition>();
        SetUpBasicPositions();
        TakePosition("Overhead View");
    }

    public static void SetUpBasicPositions()
    {
        possiblePositions.Add("Overhead View", new CameraPosition(1f, new Vector3(0, 40f, 0), new Vector3(90, 0, 0)));
    }
    public static void AddPosition(float transitionTime, Vector3 targetPosition, Vector3 targetRotation, string name)
    {
        if (possiblePositions.ContainsKey(name))
        {
            possiblePositions.Remove(name);
        }
        possiblePositions.Add(name, new CameraPosition(transitionTime, targetPosition, targetRotation));

    }

    // Update is called once per frame
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
    }

    public static void TakePosition(string name)
    {
        currentPosition = new CameraPosition(0f, Camera.main.transform.position, Camera.main.transform.rotation.eulerAngles);
        CameraPosition position = possiblePositions[name];
        position.rotation.x = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.x, position.rotation.x);
        position.rotation.y = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.y, position.rotation.y);
        position.rotation.z = GetTargetRotationComponentForShortestPath(Camera.main.transform.rotation.z, position.rotation.z);
        currentTargetPosition = position;
        transitionProgress = 0f;
    }

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
    }

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

    public static void SetBlur(bool enabled)
    {
        Camera.main.GetComponent<Blur>().enabled = enabled;
    }
}
