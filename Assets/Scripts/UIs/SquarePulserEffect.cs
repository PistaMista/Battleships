using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquarePulserEffect : MonoBehaviour
{

    /// <summary>
    /// The color of the effect.
    /// </summary>
    public Color color;
    /// <summary>
    /// The length of the side of the square inside.
    /// </summary>
    public float insideLength;
    /// <summary>
    /// The maximum distance the squares can travel before dissipating.
    /// </summary> 
    public float maxDistance;
    /// <summary>
    /// The interval between each pulse.
    /// </summary>
    public float pulseInterval;
    /// <summary>
    /// The width of the lines of the squares.
    /// </summary>
    public float squareWidth;
    /// <summary>
    /// The speed of the pulses.
    /// </summary>
    public float pulseSpeed;

    void Start()
    {
        sideCubes = new List<GameObject>();
        topCubes = new List<GameObject>();
        cubes = new List<SquareSide>();
    }

    struct SquareSide
    {
        public float currentVelocity;
        public GameObject cube;
    }
    List<GameObject> sideCubes;
    List<GameObject> topCubes;
    List<SquareSide> cubes;
    float elapsedTime = 0f;
    void Update()
    {
        if (elapsedTime > pulseInterval)
        {
            DrawSquare();
            elapsedTime = 0f;
        }

        elapsedTime += Time.deltaTime;

        for (int i = 0; i < cubes.Count; i++)
        {
            SquareSide side = cubes[i];
            Vector3 direction = side.cube.transform.localPosition.normalized;
            float distance = Mathf.Abs(side.cube.transform.localPosition.x) + Mathf.Abs(side.cube.transform.localPosition.z);

            distance = Mathf.SmoothDamp(distance, maxDistance + insideLength, ref side.currentVelocity, maxDistance / pulseSpeed);
            side.cube.transform.localScale = new Vector3(squareWidth + Mathf.Abs(direction.z) * (2f * distance - 2f * squareWidth), 0.1f, squareWidth + Mathf.Abs(direction.x) * (2f * distance));

            side.cube.transform.localPosition = direction * distance;


            Renderer renderer = side.cube.GetComponent<Renderer>();
            renderer.material = GameController.playerBoardMarkerMaterial;
            Color color2 = color;
            color2.a = (1f - distance / (maxDistance + insideLength / 2f));
            //color2.g = color.g * (1f - distance / (maxDistance + insideLength / 2f));
            //color2.b = color.b * (1f - distance / (maxDistance + insideLength / 2f));

            renderer.material.SetColor("_Color", color2);

            if (color2.a <= 0.01f)
            {
                Destroy(side.cube);
                cubes.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Draws another square.
    /// </summary>
    void DrawSquare()
    {
        for (int i = -1; i <= 1; i += 2)
        {
            Vector3 position = Vector3.right * (insideLength / 2f + squareWidth / 2f) * i;
            SquareSide tmp = new SquareSide();
            GameObject sideCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideCube.transform.parent = transform;
            sideCube.transform.localPosition = position;
            sideCube.transform.localScale = new Vector3(squareWidth, 0.1f, insideLength);

            Renderer renderer = sideCube.GetComponent<Renderer>();
            renderer.material = GameController.playerBoardMarkerMaterial;
            renderer.material.SetColor("_Color", color);

            tmp.cube = sideCube;
            tmp.currentVelocity = 0;
            //sideCubes.Add(sideCube);
            cubes.Add(tmp);
        }

        for (int i = -1; i <= 1; i += 2)
        {
            Vector3 position = Vector3.forward * (insideLength / 2f + squareWidth / 2f) * i;
            SquareSide tmp = new SquareSide();
            GameObject topCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topCube.transform.parent = transform;
            topCube.transform.localPosition = position;
            topCube.transform.localScale = new Vector3(insideLength + 2f * squareWidth, 0.1f, squareWidth);

            Renderer renderer = topCube.GetComponent<Renderer>();
            renderer.material = GameController.playerBoardMarkerMaterial;
            renderer.material.SetColor("_Color", color);

            tmp.cube = topCube;
            tmp.currentVelocity = 0;
            //topCubes.Add(topCube);
            cubes.Add(tmp);
        }
    }
}
