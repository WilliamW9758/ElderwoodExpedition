using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelType
{
    Combat,
    Event,
    Rest,
    Elite,
    Boss
}
public enum Language
{
    English,
    Chinese
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private static Camera mainCamera;
    private static LevelLoader levelLoader;

    public static Action LevelComplete;
    public static int currentLevel;
    public static LevelType currentLevelType;
    public static bool IsGamePaused { get; private set; }

    public static List<List<LevelType>> levelPresets = new List<List<LevelType>>();

    public static Language CurrentLanguage { get; set;}
    public static KeyCode Up { get; set; }
    public static KeyCode Down { get; set; }
    public static KeyCode Left { get; set; }
    public static KeyCode Right { get; set; }
    public static KeyCode PrimaryAttack { get; set; }
    public static KeyCode SecondaryAttack { get; set; }
    public static KeyCode MoveItem { get; set; }
    public static KeyCode Inventory { get; set; }
    public static KeyCode Interact { get; set; }
    public static KeyCode Pause { get; set; }

    private WorldGenerator worldGen;
    private GameObject player;
    private GameObject boss;

    public bool generateMap;
    public bool spawnEntity;
    public bool spawnPlayer;
    public bool spawnBoss;

    public Vector3 GetPlayerLocation { get { return player.transform.position; } }

    [SerializeField]
    private ItemObject[] StartingItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);

        CurrentLanguage = Language.English;
        Up = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnUp", "W"));
        Down = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnDown", "S"));
        Left = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnLeft", "A"));
        Right = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnRight", "D"));
        PrimaryAttack = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnPrimary", "Mouse0"));
        SecondaryAttack = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnSecondary", "Mouse1"));
        MoveItem = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnMoveItem", "LeftShift"));
        Inventory = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnInventory", "Tab"));
        Interact = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnInteract", "E"));
        Pause = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnPause", "Escape"));

        worldGen = GetComponentInChildren<WorldGenerator>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        currentLevel = 1;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        levelLoader = GameObject.Find("LevelLoader").GetComponent<LevelLoader>();

        worldGen.ClearMap();

        if (scene.buildIndex == 0) // if is start
        {
            worldGen.GenerateMap(false);
        }
        else if (scene.buildIndex == 1) // if is prep
        {
            worldGen.SpawnPlayer(new Vector3(-0.8f, 0, 0));
            player = worldGen.player;

            WeaponController wc = player.GetComponent<WeaponController>();
            PlayerController pc = player.GetComponent<PlayerController>();

            wc.canAttack = false;
            pc.root = true;

            wc.weaponLeft.Clear();
            wc.weaponRight.Clear();
            pc.inventory.Clear();
            for (int i = 0; i < pc.inventory.GetSize; i++)
            {
                pc.inventory.GetSlots[i].SetLocked(false);
            }
            for (int i = 0; i < wc.weaponLeft.GetSize; i++)
            {
                wc.weaponLeft.GetSlots[i].SetLocked(false);
            }
            for (int i = 0; i < wc.weaponRight.GetSize; i++)
            {
                wc.weaponRight.GetSlots[i].SetLocked(false);
            }

            foreach (ItemObject item in StartingItems)
            {
                InventorySlot slot = player.GetComponent<PlayerController>().inventory.AddItem(item.data);
                slot.SetLocked(true);
            }
        }
        else if (scene.buildIndex == 2) // if is game
        {
            worldGen.GenerateMap(true); // generate world as normal
            player = worldGen.player;

            PlayerController pc = player.GetComponent<PlayerController>();
            for (int i = 0; i < pc.inventory.GetSize; i++)
            {
                pc.inventory.GetSlots[i].SetLocked(false);
            }
        }
        else if (scene.buildIndex == 3) // if is boss
        {
            worldGen.SpawnPlayer(new Vector3(0, 0, 0));
            player = worldGen.player;
            worldGen.SpawnBoss(new Vector3(0, 17, 0));
            boss = worldGen.boss;
            boss.GetComponent<EntityController>().OnEntityDestroy += OnBossDeath;
        }
        else if (scene.buildIndex == 4) // if is death
        {
            worldGen.SpawnPlayer(new Vector3(-0.8f, 0, 0));
            player = worldGen.player;

            WeaponController wc = player.GetComponent<WeaponController>();
            PlayerController pc = player.GetComponent<PlayerController>();

            wc.canAttack = false;
            pc.root = true;

            for (int i = 0; i < pc.inventory.GetSize; i++)
            {
                pc.inventory.GetSlots[i].SetLocked(true);
            }
            for (int i = 0; i < wc.weaponLeft.GetSize; i++)
            {
                wc.weaponLeft.GetSlots[i].SetLocked(true);
            }
            for (int i = 0; i < wc.weaponRight.GetSize; i++)
            {
                wc.weaponRight.GetSlots[i].SetLocked(true);
            }
        }
        else if (scene.buildIndex == 5) // if is victory
        {
            worldGen.SpawnPlayer(new Vector3(-0.8f, 0, 0));
            player = worldGen.player;

            WeaponController wc = player.GetComponent<WeaponController>();
            PlayerController pc = player.GetComponent<PlayerController>();

            wc.canAttack = false;
            pc.root = true;

            for (int i = 0; i < pc.inventory.GetSize; i++)
            {
                pc.inventory.GetSlots[i].SetLocked(true);
            }
            for (int i = 0; i < wc.weaponLeft.GetSize; i++)
            {
                wc.weaponLeft.GetSlots[i].SetLocked(true);
            }
            for (int i = 0; i < wc.weaponRight.GetSize; i++)
            {
                wc.weaponRight.GetSlots[i].SetLocked(true);
            }
        }
    }

    private void OnBossDeath()
    {
        boss.GetComponent<EntityController>().OnEntityDestroy -= OnBossDeath;
        currentLevel++;
        //levelLoader.GoToGame();

        // demo version only 1 boss
        levelLoader.GoToVictory();
    }

    //private void Update()
    //{
    //    if (!levelCompleted)
    //    {
    //        CheckLevelComplete();
    //    }
    //}

    //private void CheckLevelComplete()
    //{
    //    if (currentLevelType == LevelType.Rest)
    //    {
    //        levelCompleted = true;
    //        OnLevelComplete();
    //    } else if (currentLevelType == LevelType.Combat)
    //    {
    //        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
    //        {
    //            levelCompleted = true;
    //            OnLevelComplete();
    //        }
    //    }
    //}

    //public void OnLevelComplete()
    //{
    //    LevelComplete?.Invoke();
    //    currentLevel++;
    //}

    public static void QuickTimeScale(float duration, float timeScale)
    {
        IEnumerator _QuickTimeScale(float duration, float timeScale)
        {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
        Instance.StartCoroutine(_QuickTimeScale(duration, timeScale));
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    public static void SetPlayerCanAttack(bool value)
    {
        Instance.player.GetComponent<WeaponController>().canAttack = value;
    }

    public static void PauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;
    }

    public static void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void ToBossFight()
    {
        levelLoader.GoToBoss();
    }

    public static void PlayerDeath()
    {
        levelLoader.GoToDeath();
    }
}
