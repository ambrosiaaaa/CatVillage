using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UI_GameMenu : MonoBehaviour
{
    [Header("Menu Canvases")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject saveMenu;
    public GameObject quitWarningMenu;
    public GameObject saveSuccessfulMessage;
    public GameObject loadingScreen;
    public GameObject loadingConfirmedSaveScreen;
    public GameObject keybindingsMenu;

    [Header("Paw Canvas")]
    public GameObject pawCanvas;

    private Game_LoadPlayer gameLoadPlayer;

    public string playerName;
    public SkinnedMeshRenderer catMeshRenderer;
    public Renderer playerRenderer;

    [Header("Save Message Settings")]
    [Tooltip("How long to show the save successful message (seconds)")]
    public float saveMessageDuration = 3f;

    private void Start()
    {
        // Hide all menus except main gameplay
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (saveMenu != null) saveMenu.SetActive(false);
        if (quitWarningMenu != null) quitWarningMenu.SetActive(false);
        if (saveSuccessfulMessage != null) saveSuccessfulMessage.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (loadingConfirmedSaveScreen != null) loadingConfirmedSaveScreen.SetActive(false);
        if (keybindingsMenu != null) keybindingsMenu.SetActive(false);

        // Get Game_LoadPlayer component from this GameObject (GameManager)
        gameLoadPlayer = GetComponent<Game_LoadPlayer>();
        if (gameLoadPlayer != null)
        {
            playerName = gameLoadPlayer.playerName;
            catMeshRenderer = gameLoadPlayer.catMeshRenderer;
            playerRenderer = gameLoadPlayer.catMeshRenderer != null ? gameLoadPlayer.catMeshRenderer : null;
            // If you want playerRenderer to be the MeshRenderer or SkinnedMeshRenderer, adjust as needed
        }
        else
        {
            Debug.LogWarning("Game_LoadPlayer component not found on GameManager.");
        }
    }
    
    private void Update()
    {
        // Toggle pause menu with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu != null && !pauseMenu.activeSelf)
            {
                ShowPauseMenu();
                // Get Game_LoadPlayer component from this GameObject (GameManager)
                gameLoadPlayer = GetComponent<Game_LoadPlayer>();
                if (gameLoadPlayer != null)
                {
                    playerName = gameLoadPlayer.playerName;
                }
                else
                {
                    Debug.LogWarning("Game_LoadPlayer component not found on GameManager.");
                }
            }
            else if (pauseMenu != null && pauseMenu.activeSelf)
            {
                HideAllMenus();
            }
        }
    }

    public void ShowPauseMenu()
    {
        HideAllMenus();
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void OpenSettingsMenu()
    {
        HideAllMenus();
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    public void OpenSaveMenu()
    {
        HideAllMenus();
        if (saveMenu != null) saveMenu.SetActive(true);
    }

    public void OpenQuitWarningMenu()
    {
        HideAllMenus();
        if (quitWarningMenu != null) quitWarningMenu.SetActive(true);
    }

    public void HideAllMenus()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (saveMenu != null) saveMenu.SetActive(false);
        if (quitWarningMenu != null) quitWarningMenu.SetActive(false);
    }

    // Save menu functionality

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

    [System.Serializable]
    public class PlayerSaveData
    {
        public string playerName;
        public PlayerAppearanceData appearance;
    }

    public PlayerAppearanceData GetPlayerAppearanceData()
    {
        var data = new PlayerAppearanceData();
        data.playerName = playerName;
        data.materialAssetPath = "Assets/PlayerSavedMaterial.mat";
        if (catMeshRenderer != null && catMeshRenderer.sharedMesh != null)
        {
            data.earLength = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("EarLength"));
            data.earWidth = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("EarWidth"));
            data.headSize = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("HeadSize"));
            data.fluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("Fluff"));
            data.neckFluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("NeckFluff"));
            data.buttFluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("ButtFluff"));
            data.tailFluff = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("TailFluff"));
            data.bodyThin = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("BodyThin"));
            data.nuzzleLength = catMeshRenderer.GetBlendShapeWeight(catMeshRenderer.sharedMesh.GetBlendShapeIndex("NuzzleLength"));
        }
        return data;
    }

    public void SavePlayerMaterialAsset()
    {
        if (playerRenderer == null || playerRenderer.material == null)
        {
            Debug.LogError("Player renderer or material is missing.");
            return;
        }
#if UNITY_EDITOR
        string assetPath = "Assets/Player/PlayerSavedMaterial.mat";
        Material mat = playerRenderer.material;
        UnityEditor.AssetDatabase.CreateAsset(Object.Instantiate(mat), assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Player material asset saved to {assetPath}");
#else
        Debug.LogWarning("SavePlayerMaterialAsset only works in the Unity Editor.");
#endif
    }

    public void SavePlayerDataToJson()
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

    public void SaveGame()
    {
        // Example usage of the new methods
        GetPlayerAppearanceData();
        SavePlayerMaterialAsset();
        SavePlayerDataToJson();
        Debug.Log("Game saved!");

        // Hide menu
        HideAllMenus();

        // Show save successful message
        if (saveSuccessfulMessage != null)
        {
            StartCoroutine(ShowSaveSuccessfulMessage());
        }
    }

    private IEnumerator ShowSaveSuccessfulMessage()
    {
        saveSuccessfulMessage.SetActive(true);
        yield return new WaitForSeconds(saveMessageDuration);
        saveSuccessfulMessage.SetActive(false);
    }

    // EXIT GAME BUTTONS

    public void SaveAndExitToMainMenu()
    {
        GetPlayerAppearanceData();
        SavePlayerMaterialAsset();
        SavePlayerDataToJson();
        Debug.Log("Game saved!");

        //show loading screen here
        StartCoroutine(LoadConfirmedSaveMainMenuWithDelay(3f));
    }

    public void ExitToMainMenuWithoutSaving()
    {
        //show loading screen here
        StartCoroutine(LoadMainMenuWithDelay(3f));
    }

    private IEnumerator LoadMainMenuWithDelay(float delay)
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator LoadConfirmedSaveMainMenuWithDelay(float delay)
    {
        if (loadingConfirmedSaveScreen != null) loadingConfirmedSaveScreen.SetActive(true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MainMenu");
    }

    // SETTINGS MENU BUTTONS

    // master volume

    // sound effects volume

    // footsteps volume

    // mouse sensitivity

    // invert mouse Y

    // settings to keybindings menu
    public void OpenKeybindingsMenu()
    {
        HideAllMenus();
        if (keybindingsMenu != null) keybindingsMenu.SetActive(true);
    }

    // keybindings return to settings menu
    public void CloseKeybindingsMenu()
    {
        HideAllMenus();
        if (keybindingsMenu != null) keybindingsMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

}
