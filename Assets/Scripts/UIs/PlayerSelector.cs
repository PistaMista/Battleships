using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelector : MonoBehaviour
{
    //The colors of players which will be available
    /// <summary>
    /// The available player colors.
    /// </summary>
    public Color[] defaultAvailablePlayerColors;
    /// <summary>
    /// The selection screen.
    /// </summary>
    public Canvas defaultSelectionScreen;
    /// <summary>
    /// The perimeter circle.
    /// </summary>
    public Image defaultPerimeter;
    /// <summary>
    /// The selection screen.
    /// </summary>
    static Canvas selectionScreen;
    /// <summary>
    /// The available player colors.
    /// </summary>
    static Color[] availablePlayerColors;

    /// <summary>
    /// The sprite used for human players.
    /// </summary>
    public Sprite defaultHumanPlayerIcon;
    /// <summary>
    /// The sprite used for human players.
    /// </summary>
    static Sprite humanPlayerIcon;
    /// <summary>
    /// The sprite used for AI players.
    /// </summary>
    public Sprite defaultAIPlayerIcon;
    /// <summary>
    /// The sprite used for AI players.
    /// </summary>
    static Sprite AIPlayerIcon;
    /// <summary>
    /// The players set to compete in the new battle.
    /// </summary>
    static Dictionary<Color, bool> selectedPlayers;
    /// <summary>
    /// The anchors.
    /// </summary>
    static Image[] anchors;
    /// <summary>
    /// Top anchors.
    /// </summary>
    static List<Image> topAnchors;
    /// <summary>
    /// Center anchors.
    /// </summary>
    static List<Image> centerAnchors;
    /// <summary>
    /// The perimeter circle.
    /// </summary>
    static Image perimeter;

    /// <summary>
    /// Awake function.
    /// </summary>
    void Awake()
    {
        availablePlayerColors = defaultAvailablePlayerColors;
        selectionScreen = defaultSelectionScreen;
        humanPlayerIcon = defaultHumanPlayerIcon;
        AIPlayerIcon = defaultAIPlayerIcon;
        perimeter = defaultPerimeter;
        perimeter.rectTransform.localScale = new Vector3(1, 1, 1) * Screen.height / 2f;

        //topAnchors = new List<Image>();
        //centerAnchors = new List<Image>();
        //UpdateGraphics();
    }

    /// <summary>
    /// The currently dragged anchor.
    /// </summary>
    Image currentlyDragged;
    /// <summary>
    /// The update function.
    /// </summary>
    void Update()
    {
        if (InputController.GetBeginPress(63))
        {
            currentlyDragged = GetAnchorAtPosition(InputController.currentScreenInputPosition);
        }

        if (InputController.GetTap(63) && currentlyDragged != null)
        {
            if (selectedPlayers.ContainsKey(currentlyDragged.color))
            {
                selectedPlayers[currentlyDragged.color] = !selectedPlayers[currentlyDragged.color];
            }
            UpdateGraphics();
        }
        else if (InputController.GetEndPress(63))
        {
            if (currentlyDragged != null)
            {
                if (Vector2.Distance(new Vector2(Screen.width / 2f, Screen.height / 2f), currentlyDragged.rectTransform.position) < Screen.height / 3.5f)
                {
                    if (!selectedPlayers.ContainsKey(currentlyDragged.color))
                    {
                        selectedPlayers.Add(currentlyDragged.color, false);
                        centerAnchors.Add(currentlyDragged);
                        topAnchors.Remove(currentlyDragged);
                    }
                }
                else
                {
                    if (!topAnchors.Contains(currentlyDragged))
                    {
                        topAnchors.Add(currentlyDragged);
                        centerAnchors.Remove(currentlyDragged);
                        selectedPlayers.Remove(currentlyDragged.color);
                    }
                }
            }

            UpdateGraphics();
            currentlyDragged = null;
        }



        if (InputController.IsDragging(63) && currentlyDragged != null)
        {
            currentlyDragged.rectTransform.position = InputController.currentScreenInputPosition;
        }
    }

    /// <summary>
    /// Selection done.
    /// </summary>
    public void Done()
    {

        if (selectedPlayers.Count > 1)
        {
            //GameController.NewBattle(selectedPlayers.ToArray(), true);
            GameController.PlayerInitializer[] initializers = new GameController.PlayerInitializer[selectedPlayers.Count];

            for (int i = 0; i < centerAnchors.Count; i++)
            {

                initializers[i] = new GameController.PlayerInitializer(centerAnchors[i].color, selectedPlayers[centerAnchors[i].color]);
            }

            GameController.NewBattle(initializers, true);
        }
    }
    /// <summary>
    /// Code that gets executed when this object is deactivated in the scene.
    /// </summary>
    void OnDisable()
    {
        //Reset();
    }
    /// <summary>
    /// Code that gets executed when this object is activated in the scene.
    /// </summary>
    void OnEnable()
    {
        Reset();
    }

    /// <summary>
    /// Resets the selection screen.
    /// </summary>
    static void Reset()
    {
        if (anchors != null)
        {
            if (anchors.Length > 0)
            {
                foreach (Image anchor in anchors)
                {
                    Destroy(anchor.gameObject);
                }
            }
        }


        anchors = new Image[availablePlayerColors.Length];
        selectedPlayers = new Dictionary<Color, bool>();

        for (int i = 0; i < anchors.Length; i++)
        {
            GameObject anchor = new GameObject("Anchor " + (i + 1));

            anchor.transform.parent = selectionScreen.transform;
            anchors[i] = anchor.AddComponent<Image>();
            anchors[i].sprite = humanPlayerIcon;
            anchors[i].color = availablePlayerColors[i];
            anchors[i].rectTransform.anchorMin = Vector2.zero;
            anchors[i].rectTransform.anchorMax = Vector2.zero;
        }

        topAnchors = new List<Image>(anchors);
        centerAnchors = new List<Image>();

        UpdateGraphics();
    }
    /// <summary>
    /// Updates the selection screen.
    /// </summary>
    static void UpdateGraphics()
    {


        foreach (Image anchor in anchors)
        {
            if (selectedPlayers.ContainsKey(anchor.color))
            {
                if (selectedPlayers[anchor.color])
                {
                    anchor.sprite = AIPlayerIcon;
                    anchor.rectTransform.localScale = new Vector3(1f, 1.0848522942f, 1f);
                }
                else
                {
                    anchor.sprite = humanPlayerIcon;
                    anchor.rectTransform.localScale = Vector3.one;
                }
            }
            else
            {
                anchor.sprite = humanPlayerIcon;
                anchor.rectTransform.localScale = Vector3.one;
            }
        }

        float lengthStep = Screen.width / (float)(topAnchors.Count + 1);

        for (int i = 0; i < topAnchors.Count; i++)
        {
            topAnchors[i].rectTransform.position = new Vector2(lengthStep * (i + 1), Screen.height - topAnchors[i].rectTransform.rect.height);
            topAnchors[i].rectTransform.rotation = Quaternion.Euler(Vector3.zero);
        }

        if (centerAnchors.Count > 0)
        {
            float angularStep = 360f / centerAnchors.Count;
            for (int i = 0; i < centerAnchors.Count; i++)
            {
                float angle = Mathf.Deg2Rad * angularStep * i;
                Vector2 finalPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * Screen.height / 3f + new Vector2(Screen.width / 2f, Screen.height / 2f);

                centerAnchors[i].rectTransform.position = finalPosition;
                centerAnchors[i].rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle * Mathf.Rad2Deg - 90f));
            }
        }
    }
    /// <summary>
    /// Gets the anchor at position.
    /// </summary>
    /// <param name="position">The screen position to check.</param>
    /// <returns>The anchor at position.</returns>
    static Image GetAnchorAtPosition(Vector2 position)
    {
        foreach (Image anchor in anchors)
        {
            float minDistance = anchor.rectTransform.rect.height / 2f;
            if (Vector2.Distance(anchor.rectTransform.position, position) < minDistance)
            {
                return anchor;
            }
        }

        return null;
    }
}
