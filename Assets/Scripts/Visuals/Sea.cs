using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Sea : MonoBehaviour
{


    public struct SeaPoint
    {
        public Vector3 worldPosition;
        public float targetElevation;
        public float currentElevation;
        public float currentElevationChange;
    }

    public int dimensions;
    public SeaPoint[] points;
    public float spacing;
    public Material seaMaterial;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public float elevationCap;
    public float updateDelay;
    public bool polyMode = false;

    void Start()
    {
        PlacePoints();
        meshFilter.mesh.MarkDynamic();
        //RecalculateVertices();
        //RecalculateTriangles();
        RecalculateMeshComposition();

        //Place the sea in the middle of the map
        transform.position = new Vector3(-(float)dimensions * spacing / 2f - spacing / 4f, 0, -(float)dimensions * spacing / 2f - spacing / 4f);

        StartCoroutine(Cycle());
    }

    void PlacePoints()
    {
        points = new SeaPoint[(int)Mathf.Pow(dimensions, 2f) * 2];
        for (int grid = 0; grid <= 1; grid++)
        {
            for (int i = 0; i < points.Length / 2; i++)
            {
                int pointIndex = i + grid * points.Length / 2;
                Vector3 pointPosition = new Vector3(i % dimensions * spacing + grid * 0.5f * spacing, 0, (i - i % dimensions) / dimensions * spacing + grid * 0.5f * spacing);
                points[pointIndex].worldPosition = pointPosition;
                points[pointIndex].targetElevation = Random.Range(0f, elevationCap);
                points[pointIndex].currentElevationChange = Random.Range(0f, elevationCap * 4f);
            }
        }
    }

    IEnumerator Cycle()
    {
        meshFilter.mesh.Clear();
        UpdatePointElevation();
        if (!polyMode)
        {
            RecalculateVertices();
            RecalculateTriangles();
        }
        else
        {
            RecalculateMeshComposition();
        }
        meshFilter.mesh.RecalculateNormals();
        yield return new WaitForSeconds(updateDelay);
        StartCoroutine(Cycle());
    }

    void UpdatePointElevation()
    {
        for (int i = 0; i < points.Length; i++)
        {
            SeaPoint point = points[i];
            //Change elevation of all points
            point.currentElevation = Mathf.SmoothDamp(point.currentElevation, point.targetElevation, ref point.currentElevationChange, 0.05f / updateDelay);

            if (Mathf.Abs(point.currentElevation - point.targetElevation) < elevationCap / 4f)
            {
                point.targetElevation = Random.Range(0f, elevationCap);
            }

            //Change the world position
            point.worldPosition.y = point.currentElevation;

            points[i] = point;
        }
    }
    void RecalculateVertices()
    {
        Vector3[] vertices = new Vector3[points.Length];

        //Assign vertex positions
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = points[i].worldPosition;
        }

        meshFilter.mesh.vertices = vertices;
        vertices = null;
    }

    void RecalculateTriangles()
    {
        List<int> triangles = new List<int>();



        //Calculate the triangles
        //Calculate triangles for the first half
        for (int i = 0; i < points.Length / 2; i++)
        {
            int[] quad = new int[] { i, i + 1, i + (int)Mathf.Pow(dimensions, 2f), i + (int)Mathf.Pow(dimensions, 2f) - dimensions };
            if (CheckForCorrectQuadAssembly(quad))
            {
                triangles.Add(quad[0]);
                triangles.Add(quad[1]);
                triangles.Add(quad[2]);
                triangles.Add(quad[3]);
                triangles.Add(quad[1]);
                triangles.Add(quad[0]);
            }
            quad = null;
        }

        triangles.Reverse();

        //Calculate triangles for the second half
        for (int i = points.Length / 2; i < points.Length; i++)
        {
            int[] quad = new int[] { i, i + 1, i - (int)Mathf.Pow(dimensions, 2f) + 1, i - (int)Mathf.Pow(dimensions, 2f) + dimensions + 1 };
            if (CheckForCorrectQuadAssembly(quad))
            {
                triangles.Add(quad[0]);
                triangles.Add(quad[1]);
                triangles.Add(quad[2]);
                triangles.Add(quad[3]);
                triangles.Add(quad[1]);
                triangles.Add(quad[0]);
            }
            quad = null;
        }


        meshFilter.mesh.triangles = triangles.ToArray();
        triangles = null;
    }

    void RecalculateMeshComposition()
    {
        List<int> triangles = new List<int>();
        List<Vector3> vertices = new List<Vector3>();

        //Calculate the triangles
        //Calculate triangles for the first half
        for (int i = 0; i < points.Length / 2; i++)
        {
            int[] quad = new int[] { i, i + 1, i + (int)Mathf.Pow(dimensions, 2f), i + (int)Mathf.Pow(dimensions, 2f) - dimensions };
            if (CheckForCorrectQuadAssembly(quad))
            {
                vertices.Add(points[quad[0]].worldPosition);
                vertices.Add(points[quad[1]].worldPosition);
                vertices.Add(points[quad[2]].worldPosition);

                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);

                vertices.Add(points[quad[3]].worldPosition);
                vertices.Add(points[quad[1]].worldPosition);
                vertices.Add(points[quad[0]].worldPosition);

                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);
            }
            quad = null;
        }

        triangles.Reverse();

        //Calculate triangles for the second half
        for (int i = points.Length / 2; i < points.Length; i++)
        {
            int[] quad = new int[] { i, i + 1, i - (int)Mathf.Pow(dimensions, 2f) + 1, i - (int)Mathf.Pow(dimensions, 2f) + dimensions + 1 };
            if (CheckForCorrectQuadAssembly(quad))
            {
                vertices.Add(points[quad[0]].worldPosition);
                vertices.Add(points[quad[1]].worldPosition);
                vertices.Add(points[quad[2]].worldPosition);

                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);

                vertices.Add(points[quad[3]].worldPosition);
                vertices.Add(points[quad[1]].worldPosition);
                vertices.Add(points[quad[0]].worldPosition);

                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);
            }
            quad = null;
        }

        meshFilter.mesh.vertices = vertices.ToArray();
        vertices = null;

        triangles.Reverse();
        meshFilter.mesh.triangles = triangles.ToArray();
        triangles = null;
    }

    bool CheckForCorrectQuadAssembly(int[] vertices)
    {
        if (vertices[0] < dimensions || vertices[0] > points.Length - dimensions - 1)
        {
            return false;
        }

        int rangeNumber = Mathf.FloorToInt((float)vertices[0] / (float)dimensions);
        float number1 = (float)vertices[0] / (float)dimensions;
        float number2 = (float)vertices[1] / (float)dimensions;
        return (number1 > rangeNumber && number1 < rangeNumber + 1 && number2 > rangeNumber && number2 < rangeNumber + 1) && (vertices[2] >= 0 && vertices[2] < points.Length && vertices[3] >= 0 && vertices[3] < points.Length);
    }
}
