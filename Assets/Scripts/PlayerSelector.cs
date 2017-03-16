using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelector : MonoBehaviour
{
    //The colors of players which will be available
    public Color[] defaultAvailablePlayerColors;
    public Canvas defaultSelectionScreen;
    static Canvas selectionScreen;
    static Color[] availablePlayerColors;

    //The sprite used for human players
    public Sprite defaultHumanPlayerIcon;
    static Sprite humanPlayerIcon;
    //The sprite used for AI players
    public Sprite defaultAIPlayerIcon;
    static Sprite AIPlayerIcon;
    //What players will compete in the battle
    static Dictionary<Color, bool> selectedPlayers;
    static Image[] anchors;

    static List<Image> topAnchors;
    static List<Image> centerAnchors;

    void Awake()
    {
        availablePlayerColors = defaultAvailablePlayerColors;
        selectionScreen = defaultSelectionScreen;
        humanPlayerIcon = defaultHumanPlayerIcon;
        AIPlayerIcon = defaultAIPlayerIcon;
        //topAnchors = new List<Image>();
        //centerAnchors = new List<Image>();
        //UpdateGraphics();
    }

    // Update is called once per frame

    Image currentlyDragged;
    void Update()
    {
        if (InputController.beginPress)
        {
            currentlyDragged = GetAnchorAtPosition(InputController.currentScreenInputPosition);
        }

        if (InputController.tap && currentlyDragged != null)
        {
            if (selectedPlayers.ContainsKey(currentlyDragged.color))
            {
                selectedPlayers[currentlyDragged.color] = !selectedPlayers[currentlyDragged.color];
            }
            UpdateGraphics();
        }
        else if (InputController.endPress)
        {
            if (currentlyDragged != null)
            {
                if (Vector2.Distance(new Vector2(Screen.width / 2f, Screen.height / 2f), currentlyDragged.rectTransform.position) < Screen.height / 3f)
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
                    topAnchors.Add(currentlyDragged);
                    centerAnchors.Remove(currentlyDragged);
                    selectedPlayers.Remove(currentlyDragged.color);
                }
            }

            UpdateGraphics();
            currentlyDragged = null;
        }



        if (InputController.dragging && currentlyDragged != null)
        {
            currentlyDragged.rectTransform.position = InputController.currentScreenInputPosition;
        }
    }

    public void Done()
    {

        if (selectedPlayers.Count > 1)
        {
            //GameController.NewBattle(selectedPlayers.ToArray(), true);
            GameController.PlayerInitializer[] initializers = new GameController.PlayerInitializer[selectedPlayers.Count];
            List<Color> playerColors = new List<Color>(selectedPlayers.Keys);
            List<bool> aiPlayers = new List<bool>(selectedPlayers.Values);

            for (int i = 0; i < initializers.Length; i++)
            {
                initializers[i] = new GameController.PlayerInitializer(playerColors[i], aiPlayers[i]);
            }

            GameController.NewBattle(initializers, true);

            Reset();
        }
    }

    public static void Reset()
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
