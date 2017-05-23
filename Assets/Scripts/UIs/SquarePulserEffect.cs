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
    /// <summary>
    /// The material used to render the squares.
    /// </summary>
    public Material material;
    /// <summary>
    /// Whether the squares will be rendered as opaque.
    /// </summary>
    public bool opaque = false;


    void Start()
    {
        cubes = new List<SquareSide>();
        material.renderQueue = 9000;
    }

    struct SquareSide
    {
        public float currentVelocity;
        public GameObject cube;
    }
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
            Vector3 heading = side.cube.transform.localPosition.normalized;
            float distance = Mathf.Abs(side.cube.transform.localPosition.x) + Mathf.Abs(side.cube.transform.localPosition.z);

            distance = Mathf.SmoothDamp(distance, maxDistance + insideLength, ref side.currentVelocity, maxDistance / pulseSpeed);
            side.cube.transform.localScale = new Vector3(squareWidth + Mathf.Abs(heading.z) * (2f * distance - 2f * squareWidth), 0.1f, squareWidth + Mathf.Abs(heading.x) * (2f * distance));

            side.cube.transform.localPosition = heading * distance;


            Renderer renderer = side.cube.GetComponent<Renderer>();
            //renderer.material = material;

            Color color2 = color;
            color2.a = (distance / (maxDistance + insideLength / 2f) > 0.55f) ? (1f - (distance / (maxDistance + insideLength / 2f) - 0.55f) / 0.45f) : 1;
            //color2.g = color.g * (1f - distance / (maxDistance + insideLength / 2f));
            //color2.b = color.b * (1f - distance / (maxDistance + insideLength / 2f));
            if (!opaque)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", color2);
                renderer.SetPropertyBlock(block);
            }
            else
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", color);
                renderer.SetPropertyBlock(block);
            }

            if (color2.a <= 0.01f)
            {
                cubes.RemoveAt(i);
                Destroy(side.cube);
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
            tmp.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.cube.transform.parent = transform;
            tmp.cube.transform.localPosition = position;
            tmp.cube.transform.localScale = new Vector3(squareWidth, 0.1f, insideLength);

            Renderer renderer = tmp.cube.GetComponent<Renderer>();
            renderer.material = material;
            tmp.currentVelocity = 0;
            cubes.Add(tmp);
        }

        for (int i = -1; i <= 1; i += 2)
        {
            Vector3 position = Vector3.forward * (insideLength / 2f + squareWidth / 2f) * i;
            SquareSide tmp = new SquareSide();
            tmp.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.cube.transform.parent = transform;
            tmp.cube.transform.localPosition = position;
            tmp.cube.transform.localScale = new Vector3(insideLength + 2f * squareWidth, 0.1f, squareWidth);

            Renderer renderer = tmp.cube.GetComponent<Renderer>();
            renderer.material = material;
            tmp.currentVelocity = 0;
            cubes.Add(tmp);
        }
    }
}
