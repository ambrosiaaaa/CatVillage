using UnityEngine;

public class Game_LoadPlayer : MonoBehaviour
{
    public GameObject player;
    public GameObject cat;
    public Material catMaterial;
    public SkinnedMeshRenderer catMeshRenderer;
    public string playerName;

    void Start()
    {
        // Load the player, cat, cat material, and mesh renderer
        if (player != null)
        {
            Transform catTransform = player.transform.Find("Cat");
            if (catTransform != null)
            {
                cat = catTransform.gameObject;
                Debug.Log($"Cat child found: {cat.name}");

                // Get SkinnedMeshRenderer
                catMeshRenderer = cat.GetComponent<SkinnedMeshRenderer>();
                if (catMeshRenderer != null)
                {
                    Debug.Log("Cat SkinnedMeshRenderer found.");
                }
                else
                {
                    Debug.LogError("Cat SkinnedMeshRenderer not found!");
                }
            }
            else
            {
                Debug.LogError("Cat child not found under player!");
            }
        }
        else
        {
            Debug.LogError("Player GameObject not assigned!");
        }

        // Load player appearance and name from JSON
        string path = Application.persistentDataPath + "/playerData.json";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);
            if (saveData != null)
            {
                playerName = saveData.playerName;
                Debug.Log($"Loaded player name: {playerName}");

                // Set blendshapes
                if (catMeshRenderer != null && saveData.appearance != null)
                {
                    SetBlendShape("EarLength", saveData.appearance.earLength);
                    SetBlendShape("EarWidth", saveData.appearance.earWidth);
                    SetBlendShape("HeadSize", saveData.appearance.headSize);
                    SetBlendShape("Fluff", saveData.appearance.fluff);
                    SetBlendShape("NeckFluff", saveData.appearance.neckFluff);
                    SetBlendShape("ButtFluff", saveData.appearance.buttFluff);
                    SetBlendShape("TailFluff", saveData.appearance.tailFluff);
                    SetBlendShape("BodyThin", saveData.appearance.bodyThin);
                    SetBlendShape("NuzzleLength", saveData.appearance.nuzzleLength);
                }
            }
            else
            {
                Debug.LogError("Failed to load player save data from JSON.");
            }
        }
        else
        {
            Debug.LogError($"Player data file not found: {path}");
        }

#if UNITY_EDITOR
        // Load material from asset file
        string assetPath = "Assets/Player/PlayerSavedMaterial.mat";
        Material loadedMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (loadedMat != null && catMeshRenderer != null)
        {
            catMeshRenderer.material = loadedMat;
            catMaterial = loadedMat;
            Debug.Log($"Loaded and assigned material: {loadedMat.name}");
        }
        else
        {
            Debug.LogError($"Material not found at {assetPath}");
        }
#endif
    }

    void SetBlendShape(string name, float value)
    {
        int index = catMeshRenderer.sharedMesh.GetBlendShapeIndex(name);
        if (index >= 0)
            catMeshRenderer.SetBlendShapeWeight(index, value);
        else
            Debug.LogWarning($"BlendShape '{name}' not found.");
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

    [System.Serializable]
    public class PlayerSaveData
    {
        public string playerName;
        public PlayerAppearanceData appearance;
    }
}
