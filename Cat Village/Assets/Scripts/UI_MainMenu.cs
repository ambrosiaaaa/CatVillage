using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public GameObject startCanvas;
    public GameObject saveFileCanvas;
    public Button startButton;
    public Button exitButton;
    public Button returnButton;
    public Button startNewSaveButton;
    public Button loadCurrentSaveButton;

    public bool playerSaved = false;

    void Start()
    {
        // Check if files for a player exist
        playerSaved = CheckPlayerDataExist();

        // Assign buttons if not set in inspector
        if (startButton == null)
            startButton = GameObject.Find("StartButton").GetComponent<Button>();
        if (exitButton == null)
            exitButton = GameObject.Find("ExitButton").GetComponent<Button>();
        if (returnButton == null)
            returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
        if (startNewSaveButton == null)
            startNewSaveButton = GameObject.Find("Start New Save").GetComponent<Button>();
        if (loadCurrentSaveButton == null)
            loadCurrentSaveButton = GameObject.Find("Load Current Save").GetComponent<Button>();
        if (startCanvas == null)
            startCanvas = GameObject.Find("StartCanvas");
        if (saveFileCanvas == null)
            saveFileCanvas = GameObject.Find("SaveFileCanvas");

        // Button listeners
        startButton.onClick.AddListener(OnStartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        returnButton.onClick.AddListener(OnReturnClicked);
        startNewSaveButton.onClick.AddListener(OnStartNewSaveClicked);
        loadCurrentSaveButton.onClick.AddListener(OnLoadCurrentSaveClicked);

        // Initial state
        startCanvas.SetActive(true);
        saveFileCanvas.SetActive(false);

        // Enable or disable loadCurrentSaveButton based on playerSaved
        if (loadCurrentSaveButton != null)
        {
            loadCurrentSaveButton.interactable = playerSaved;
        }
    }

    void OnStartClicked()
    {
        startCanvas.SetActive(false);
        saveFileCanvas.SetActive(true);
    }

    void OnReturnClicked()
    {
        saveFileCanvas.SetActive(false);
        startCanvas.SetActive(true);
    }

    void OnExitClicked()
    {
        Application.Quit();
    }

    void OnStartNewSaveClicked()
    {
        SceneManager.LoadScene("Character Creator");
    }

    void OnLoadCurrentSaveClicked()
    {
        // Make sure Application.persistentDataPath + "/playerData.json"; and assetPath = "Assets/Player/PlayerSavedMaterial.mat"; exist
        SceneManager.LoadScene("Game");

        //if not exist, load 
    }

    bool CheckPlayerDataExist()
    {
        // Helper to check that the playerData and playerSavedMaterial exist

        bool playerDataExists = false;
        string playerDataPath = Application.persistentDataPath + "/playerData.json";
        string materialPath = "Assets/Player/PlayerSavedMaterial.mat";
        Debug.Log($"Checking for player data at: {playerDataPath}");
        Debug.Log($"Checking for material at: {materialPath}");

        if (!System.IO.File.Exists(playerDataPath) && !System.IO.File.Exists(materialPath))
        {
            Debug.LogWarning($"Player data file not found: {playerDataPath}");
        }
        else
        {
            playerDataExists = true;
        }

        return playerDataExists;

    }
}
