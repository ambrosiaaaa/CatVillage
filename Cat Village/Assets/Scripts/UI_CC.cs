using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UI_CC : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public string playerName;
        public PlayerAppearanceData appearance;
    }

    public void SetEarLengthSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("EarLength"), value); }
    public void SetEarWidthSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("EarWidth"), value); }
    public void SetHeadSizeSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("HeadSize"), value); }
    public void SetFluffSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("Fluff"), value); }
    public void SetNeckFluffSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("NeckFluff"), value); }
    public void SetButtFluffSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("ButtFluff"), value); }
    public void SetTailFluffSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("TailFluff"), value); }
    public void SetBodyThinSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("BodyThin"), value); }
    public void SetNuzzleLengthSlider(float value) { if (catMeshRenderer != null) catMeshRenderer.SetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("NuzzleLength"), value); }

    [Header("Menu3 & Blendshapes")]
    public GameObject menu3;
    public Button menu3NextButton;
    public Button menu3BackButton;

    public SkinnedMeshRenderer catMeshRenderer;
    [Header("Default marking images for material")]
    public Texture2D defaultWholeBodyWhiteImage;
    public Texture2D defaultEarWhiteImage;
    public Texture2D defaultNoseWhiteImage;
    public Texture2D defaultNuzzleWhiteImage;
    public Texture2D defaultTummyWhiteImage;
    public Texture2D defaultTailWhiteImage;
    public Texture2D defaultUnderTailWhiteImage;
    public Texture2D defaultSiameseImage;
    public Texture2D defaultTortoiseShellImage;

    [Header("Menu2 White Sliders")]
    public Slider wholeBodyWhiteSlider;
    public Texture2D wholeBodyWhiteImage;
    public Texture2D[] wholeBodyWhiteImages; // 12 textures, 0 = blank

    public Slider earsWhiteSlider;
    public Texture2D earsWhiteImage;
    public Texture2D[] earsWhiteImages; // 5 textures, 0 = blank

    public Slider noseWhiteSlider;
    public Texture2D noseWhiteImage;
    public Texture2D[] noseWhiteImages; // 5 textures, 0 = blank

    public Slider nuzzleChinWhiteSlider;
    public Texture2D nuzzleChinWhiteImage;
    public Texture2D[] nuzzleChinWhiteImages; // 8 textures, 0 = blank

    public Slider tummySlider;
    public Texture2D tummyImage;
    public Texture2D[] tummyImages; // 4 textures, 0 = blank

    public Slider underTailSlider;
    public Texture2D underTailImage;
    public Texture2D[] underTailImages; // 4 textures, 0 = blank

    public Slider entireTailSlider;
    public Texture2D entireTailImage;
    public Texture2D[] entireTailImages; // 4 textures, 0 = blank

    [Header("Menu 1 - Color/Pattern Selection")]

    [Header("Fur Color Selection")]
    public Button[] furColorButtons;
    public Color[] furColors;

    [Header("Eye Color Selection")]
    public Button[] eyeColorButtons;
    public Color[] eyeColors;

    [Header("Fur Pattern Selection")]
    public Button[] furPatternButtons;
    public Texture2D[] furPatterns; // Assign textures in inspector
    public Texture2D defaultFurPattern; // Assign in inspector

    [Header("Siamese Selection")]
    public Toggle siameseToggle;
    public Texture2D siameseImage; // Assign in inspector
    private bool isSiamese = false;

    [Header("Tortoise Shell Selection")]
    public Button[] tortoiseShellColorButtons;
    public Texture2D[] tortoiseShellColors;
    public Color tortoiseColor;

    [Header ("Clothing Set Blank")]
    public Texture2D clothingSetBlankImage; // Assign in inspector

    [Header("Menus")]
    public GameObject menu1;
    public GameObject menu2;
    public Button menu1NextButton;
    public Button menu2NextButton;
    public Button menu2BackButton;

    public GameObject player; // Assign CC_Cat in inspector
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public float rotateSpeed = 100f;

    public Color baseFurColor = Color.black;
    public Color eyeColor = Color.black;

    public Renderer playerRenderer;
    private bool rotatingLeft = false;
    private bool rotatingRight = false;

    [Header("Menu4 & Player Name")]
    public GameObject menu4;
    public Button menu4FinishButton;
    public Button menu4BackButton;
    public TMP_InputField NameInputField;
    public string playerName;
    public GameObject nameWarning;
    public GameObject transitionScreen;

    void Start()
    {
        // Hide menu3 on load
        if (menu3 != null)
            menu3.SetActive(false);

        if (menu2 != null)
            menu2.SetActive(false);

        if (menu4 != null)
            menu4.SetActive(false);

        if (nameWarning != null)
            nameWarning.SetActive(false);

        if (transitionScreen != null)
            transitionScreen.SetActive(false);

        // Get SkinnedMeshRenderer from CC_Cat -> Cat
        if (player != null)
        {
            Transform catTransform = player.transform.Find("Cat");
            if (catTransform != null)
            {
                catMeshRenderer = catTransform.GetComponent<SkinnedMeshRenderer>();
                if (catMeshRenderer != null)
                {
                    Debug.Log($"catMeshRenderer assigned: {catMeshRenderer.gameObject.name}, mesh: {catMeshRenderer.sharedMesh.name}");
                }
                else
                {
                    Debug.LogError("catMeshRenderer is null after assignment! Check that the Cat object has a SkinnedMeshRenderer.");
                }
            }
            else
            {
                Debug.LogError("Cat transform not found under player!");
            }
        }

        // Menu2 -> Menu3 navigation
        if (menu2NextButton != null)
            menu2NextButton.onClick.AddListener(() =>
            {
                if (menu2 != null) menu2.SetActive(false);
                if (menu3 != null) menu3.SetActive(true);
            });

        // Menu3 -> Menu2 navigation
        if (menu3BackButton != null)
            menu3BackButton.onClick.AddListener(() =>
            {
                if (menu3 != null) menu3.SetActive(false);
                if (menu2 != null) menu2.SetActive(true);
            });

        // Menu3 -> Menu4 navigation
        if (menu3NextButton != null)
            menu3NextButton.onClick.AddListener(() =>
            {
                if (menu3 != null) menu3.SetActive(false);
                if (menu4 != null) menu4.SetActive(true);
            });

        // Menu4 -> Menu3 navigation
        if (menu4BackButton != null)
            menu4BackButton.onClick.AddListener(() =>
            {
                if (menu4 != null) menu4.SetActive(false);
                if (menu3 != null) menu3.SetActive(true);
            });

        // Menu3 -> Menu4 navigation
        if (menu3NextButton != null)
            menu3NextButton.onClick.AddListener(() =>
            {
                if (menu3 != null) menu3.SetActive(false);
                if (menu4 != null) menu4.SetActive(true);
            });

        // Menu4 -> Menu3 navigation
        if (menu4BackButton != null)
            menu4BackButton.onClick.AddListener(() =>
            {
                if (menu4 != null) menu4.SetActive(false);
                if (menu3 != null) menu3.SetActive(true);
            });

        // Menu4 FinishButton: always add listener
        if (menu4FinishButton != null)
        {
            menu4FinishButton.onClick.AddListener(OnMenu4Finish);
        }

        // Hide warning when NameInputField is selected (clicked)
        if (NameInputField != null && nameWarning != null)
        {
            NameInputField.onSelect.AddListener((string _) =>
            {
                nameWarning.SetActive(false);
            });
        }

        // Setup white sliders and listeners
        if (wholeBodyWhiteSlider != null)
        {
            wholeBodyWhiteSlider.maxValue = wholeBodyWhiteImages.Length - 1;
            wholeBodyWhiteSlider.onValueChanged.AddListener(OnWholeBodyWhiteSliderChanged);
        }
        if (earsWhiteSlider != null)
        {
            earsWhiteSlider.maxValue = earsWhiteImages.Length - 1;
            earsWhiteSlider.onValueChanged.AddListener(OnEarsWhiteSliderChanged);
        }
        if (noseWhiteSlider != null)
        {
            noseWhiteSlider.maxValue = noseWhiteImages.Length - 1;
            noseWhiteSlider.onValueChanged.AddListener(OnNoseWhiteSliderChanged);
        }
        if (nuzzleChinWhiteSlider != null)
        {
            nuzzleChinWhiteSlider.maxValue = nuzzleChinWhiteImages.Length - 1;
            nuzzleChinWhiteSlider.onValueChanged.AddListener(OnNuzzleChinWhiteSliderChanged);
        }
        if (tummySlider != null)
        {
            tummySlider.maxValue = tummyImages.Length - 1;
            tummySlider.onValueChanged.AddListener(OnTummySliderChanged);
        }
        if (underTailSlider != null)
        {
            underTailSlider.maxValue = underTailImages.Length - 1;
            underTailSlider.onValueChanged.AddListener(OnUnderTailSliderChanged);
        }
        if (entireTailSlider != null)
        {
            entireTailSlider.maxValue = entireTailImages.Length - 1;
            entireTailSlider.onValueChanged.AddListener(OnEntireTailSliderChanged);
        }
        

        if (siameseToggle != null)
        {
            //siameseToggle.isOn = isSiamese;
            siameseToggle.onValueChanged.AddListener(OnSiameseToggleChanged);
            // Automatically assign default texture on start
            if (playerRenderer != null && playerRenderer.material != null && defaultSiameseImage != null)
            {
                var mat = playerRenderer.material;
                if (mat.HasProperty("_SiameseImage"))
                    mat.SetTexture("_SiameseImage", defaultSiameseImage);
            }
        }

        // Hide menu2 on load
        if (menu2 != null)
            menu2.SetActive(false);

        if (player == null)
            player = GameObject.Find("CC_Cat");

        if (rotateLeftButton == null)
            rotateLeftButton = GameObject.Find("RotateLeft").GetComponent<Button>();
        if (rotateRightButton == null)
            rotateRightButton = GameObject.Find("RotateRight").GetComponent<Button>();

        // Get the Renderer from the 'Cat' child object specifically
        Transform catTransform2 = player.transform.Find("Cat");
        if (catTransform2 != null)
            playerRenderer = catTransform2.GetComponent<Renderer>();
        else
            playerRenderer = player.GetComponentInChildren<Renderer>(); // fallback

        // Ensure the SiameseImage property is set to defaultSiameseImage on startup
        if (playerRenderer != null && playerRenderer.material != null)
        {
            var mat = playerRenderer.material;
            if (defaultSiameseImage != null && mat.HasProperty("_SiameseImage"))
                mat.SetTexture("_SiameseImage", defaultSiameseImage);
            // Assign clothingSetBlankImage to ClothingTop and ClothingBottom
            if (clothingSetBlankImage != null)
            {
                if (mat.HasProperty("_ClothingTop"))
                    mat.SetTexture("_ClothingTop", clothingSetBlankImage);
                if (mat.HasProperty("_ClothingBottom"))
                    mat.SetTexture("_ClothingBottom", clothingSetBlankImage);
            }
        }

        // Set initial colors and default fur pattern on the material
        UpdatePlayerColors();
        if (defaultFurPattern != null)
            SetFurPattern(defaultFurPattern);

        // Add event triggers for hold functionality
        AddHoldEvents(rotateLeftButton, () => rotatingLeft = true, () => rotatingLeft = false);
        AddHoldEvents(rotateRightButton, () => rotatingRight = true, () => rotatingRight = false);

        // Add listeners for fur color buttons
        for (int i = 0; i < furColorButtons.Length && i < furColors.Length; i++)
        {
            int index = i;
            furColorButtons[i].onClick.AddListener(() =>
            {
                baseFurColor = furColors[index];
                UpdatePlayerColors();
            });
        }

        // Add listeners for eye color buttons
        for (int i = 0; i < eyeColorButtons.Length && i < eyeColors.Length; i++)
        {
            int index = i;
            eyeColorButtons[i].onClick.AddListener(() =>
            {
                eyeColor = eyeColors[index];
                UpdatePlayerColors();
            });
        }

        // Add listeners for fur pattern buttons
        for (int i = 0; i < furPatternButtons.Length && i < furPatterns.Length; i++)
        {
            int index = i;
            furPatternButtons[i].onClick.AddListener(() =>
            {
                SetFurPattern(furPatterns[index]);
            });
        }

        // Add listeners for tortoiseshell buttons
        for (int i = 0; i < tortoiseShellColorButtons.Length && i < tortoiseShellColorButtons.Length; i++)
        {
            int index = i;
            tortoiseShellColorButtons[i].onClick.AddListener(() =>
            {
                SetTortoisePattern(tortoiseShellColors[index]);
            });
        }

        // Menu navigation setup
        if (menu1NextButton != null)
            menu1NextButton.onClick.AddListener(() =>
            {
                if (menu1 != null) menu1.SetActive(false);
                if (menu2 != null) menu2.SetActive(true);
            });

        if (menu2BackButton != null)
            menu2BackButton.onClick.AddListener(() =>
            {
                if (menu2 != null) menu2.SetActive(false);
                if (menu1 != null) menu1.SetActive(true);
            });
    }

    // Helper to set marking texture on material
    void SetMarkingTexture(string property, Texture2D tex)
    {
        if (playerRenderer != null && playerRenderer.material != null && tex != null)
        {
            var mat = playerRenderer.material;
            if (mat.HasProperty(property))
                mat.SetTexture(property, tex);
        }
    }

    void OnSiameseToggleChanged(bool isOn)
    {
        isSiamese = isOn;
        if (playerRenderer != null && playerRenderer.material != null)
        {
            var mat = playerRenderer.material;
            if (mat.HasProperty("_SiameseImage"))
            {
                mat.SetTexture("_SiameseImage", isSiamese ? siameseImage : defaultSiameseImage);
            }
        }
    }

    // Slider change handlers
    void OnWholeBodyWhiteSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (wholeBodyWhiteImages.Length > index && wholeBodyWhiteImages[index] != null)
        {
            wholeBodyWhiteImage = wholeBodyWhiteImages[index];
            SetMarkingTexture("_WholeBodyWhiteImage", wholeBodyWhiteImage);
        }
    }
    void OnEarsWhiteSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (earsWhiteImages.Length > index && earsWhiteImages[index] != null)
        {
            earsWhiteImage = earsWhiteImages[index];
            SetMarkingTexture("_EarWhiteImage", earsWhiteImage);
        }
    }
    void OnNoseWhiteSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (noseWhiteImages.Length > index && noseWhiteImages[index] != null)
        {
            noseWhiteImage = noseWhiteImages[index];
            SetMarkingTexture("_NoseWhiteImage", noseWhiteImage);
        }
    }
    void OnNuzzleChinWhiteSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (nuzzleChinWhiteImages.Length > index && nuzzleChinWhiteImages[index] != null)
        {
            nuzzleChinWhiteImage = nuzzleChinWhiteImages[index];
            SetMarkingTexture("_NuzzleWhiteImage", nuzzleChinWhiteImage);
        }
    }
    void OnTummySliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (tummyImages.Length > index && tummyImages[index] != null)
        {
            tummyImage = tummyImages[index];
            SetMarkingTexture("_TummyWhiteImage", tummyImage);
        }
    }

    void OnUnderTailSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (underTailImages.Length > index && underTailImages[index] != null)
        {
            underTailImage = underTailImages[index];
            SetMarkingTexture("_UnderTailWhiteImage", underTailImage);
        }
    }

    void OnEntireTailSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        if (entireTailImages.Length > index && entireTailImages[index] != null)
        {
            entireTailImage = entireTailImages[index];
            SetMarkingTexture("_TailWhiteImage", entireTailImage);
        }
    }

    public void SetTortoisePattern(Texture2D pattern)
    {
        if (playerRenderer != null && playerRenderer.material != null && pattern != null)
        {
            var mat = playerRenderer.material;
            if (mat.HasProperty("_TortoiseImage"))
            {
                mat.SetTexture("_TortoiseImage", pattern);
                ChangeTortoiseColour();
            }
        }

        //_TortoiseColour, set as oppsoite of colour index chosen by the player
    }

    public void ChangeTortoiseColour()
    {
        if (playerRenderer != null && playerRenderer.material != null)
        {
            var mat = playerRenderer.material;
            // Change tortoise colour to complement base fur colour
            if (mat.HasProperty("_TortoiseColour") && mat.HasProperty("_TortoiseImage"))
            {
                // Simple logic: if base fur color is light, use dark tortoise color and vice versa
                // Calculate tortoise colour as the dilute version of the base fur colour:
                if(baseFurColor == furColors[0])
                {
                    tortoiseColor = furColors[3]; // Orange
                }
                else if(baseFurColor == furColors[5]) // Grey
                {
                    tortoiseColor = furColors[4]; // Pale yellow
                }
                else if(baseFurColor == furColors[6]) // Light grey
                {
                    tortoiseColor = furColors[7]; // Very pale yellow
                }
                else
                {
                    mat.SetTexture("_TortoiseImage", defaultTortoiseShellImage); // Fully transparent - tortoiseshell cant show up in diluted cats.
                }
                mat.SetColor("_TortoiseColour", tortoiseColor);
            }
        }
    }

    public void SetFurPattern(Texture2D pattern)
    {
        if (playerRenderer != null && playerRenderer.material != null && pattern != null)
        {
            var mat = playerRenderer.material;
            if (mat.HasProperty("_FurPattern"))
            {
                mat.SetTexture("_FurPattern", pattern);
            }
        }
    }

    void Update()
    {
        if (rotatingLeft)
            player.transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);
        if (rotatingRight)
            player.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);


        //updates colour of player during runtime
        UpdatePlayerColors();
    }

    void AddHoldEvents(Button button, System.Action onDown, System.Action onUp)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((_) => onDown());
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((_) => onUp());
        trigger.triggers.Add(pointerUp);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((_) => onUp());
        trigger.triggers.Add(pointerExit);
    }

    // Update the material's color properties
    public void UpdatePlayerColors()
    {
        if (playerRenderer != null && playerRenderer.material != null)
        {
            var mat = playerRenderer.material;
            if (mat.HasProperty("_BaseFurColor"))
                mat.SetColor("_BaseFurColor", baseFurColor);
            if (mat.HasProperty("_EyeColor"))
                mat.SetColor("_EyeColor", eyeColor);
            ChangeTortoiseColour();
        }
    }

    // Example: Change material/shader
    public void SetPlayerMaterial(Material newMat)
    {
        if (playerRenderer != null)
            playerRenderer.material = newMat;
        UpdatePlayerColors();
    }

    [System.Serializable]
    public class PlayerAppearanceData
    {
        public string playerName;
        public string materialAssetPath;
        public float earLength;
        public float earWidth;
        public float headSize;
        public float fluff;
        public float neckFluff;
        public float buttFluff;
        public float tailFluff;
        public float bodyThin;
        public float nuzzleLength;
    }

    // Collect all appearance data for saving
    public PlayerAppearanceData GetPlayerAppearanceData()
    {
        var data = new PlayerAppearanceData();
        data.playerName = playerName;
        data.materialAssetPath = "Assets/PlayerSavedMaterial.mat";
        data.earLength = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("EarLength"));
        data.earWidth = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("EarWidth"));
        data.headSize = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("HeadSize"));
        data.fluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("Fluff"));
        data.neckFluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("NeckFluff"));
        data.buttFluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("ButtFluff"));
        data.tailFluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("TailFluff"));
        data.bodyThin = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("BodyThin"));
        data.nuzzleLength = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("NuzzleLength"));
        return data;
    }

    public void SavePlayerMaterialAsset()
    {
        if (playerRenderer == null || playerRenderer.material == null)
        {
            Debug.LogError("Player renderer or material is missing.");
            return;
        }
        // Save the material asset to a known path
        string assetPath = "Assets/Player/PlayerSavedMaterial.mat";
        Material mat = playerRenderer.material;
        UnityEditor.AssetDatabase.CreateAsset(Object.Instantiate(mat), assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Player material asset saved to {assetPath}");
    }

    void SavePlayerDataToJson()
    {
        var saveData = new PlayerSaveData
        {
            playerName = playerName,
            appearance = GetPlayerAppearanceData()
        };
        string json = JsonUtility.ToJson(saveData);
        string path = Application.persistentDataPath + "/playerData.json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"Player data saved to {path}");
    }

    // Called when FinishButton in menu4 is pressed
    void OnMenu4Finish()
    {
        if (NameInputField != null)
        {
            playerName = NameInputField.text;
            // Check for empty name
            if (string.IsNullOrWhiteSpace(playerName))
            {
                if (nameWarning != null)
                    nameWarning.SetActive(true);
                return;
            }
            nameWarning.SetActive(false);
            Debug.Log($"Player name set to: {playerName}");
            SavePlayerDataToJson();
            SavePlayerMaterialAsset();

            // Go to the game scene
            StartCoroutine(LoadGameSceneWithDelay());
        }
    }

    private IEnumerator LoadGameSceneWithDelay()
    {
        if (transitionScreen != null)
        {
            transitionScreen.SetActive(true);
            CanvasGroup cg = transitionScreen.GetComponentInChildren<CanvasGroup>();
            if (cg != null)
            {
                float duration = 2f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                cg.alpha = 1f;
            }
        }
        yield return new WaitForSeconds(2f); // Small buffer after fade
        SceneManager.LoadScene("Game");
    }
}