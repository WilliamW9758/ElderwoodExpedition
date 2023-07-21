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

    private bool levelCompleted = false;

    private void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        if (Instance != null && Instance != this)
        {
            Destroy(this);
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

        currentLevel = 1;
        currentLevelType = LevelType.Rest;
        InitLevelPresets();
    }

    private void Update()
    {
        if (!levelCompleted)
        {
            CheckLevelComplete();
        }
    }

    private void CheckLevelComplete()
    {
        if (currentLevelType == LevelType.Rest)
        {
            levelCompleted = true;
            OnLevelComplete();
        } else if (currentLevelType == LevelType.Combat)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                levelCompleted = true;
                OnLevelComplete();
            }
        }
    }

    public void OnLevelComplete()
    {
        LevelComplete?.Invoke();
        currentLevel++;
    }

    public void InitLevelPresets()
    {
        List<LevelType> level1 = new List<LevelType>();
        level1.Add(LevelType.Rest);
        levelPresets.Add(level1);

        List<LevelType> level2 = new List<LevelType>();
        level2.Add(LevelType.Combat);
        level2.Add(LevelType.Event);
        levelPresets.Add(level2);

        List<LevelType> level3 = new List<LevelType>();
        level3.Add(LevelType.Combat);
        level3.Add(LevelType.Event);
        levelPresets.Add(level3);

        List<LevelType> level4 = new List<LevelType>();
        level4.Add(LevelType.Elite);
        levelPresets.Add(level4);

        List<LevelType> level5 = new List<LevelType>();
        level5.Add(LevelType.Combat);
        level5.Add(LevelType.Event);
        level5.Add(LevelType.Rest);
        levelPresets.Add(level5);

        List<LevelType> level6 = new List<LevelType>();
        level6.Add(LevelType.Combat);
        level6.Add(LevelType.Event);
        levelPresets.Add(level6);

        List<LevelType> level7 = new List<LevelType>();
        level7.Add(LevelType.Combat);
        level7.Add(LevelType.Event);
        levelPresets.Add(level7);

        List<LevelType> level8 = new List<LevelType>();
        level8.Add(LevelType.Elite);
        levelPresets.Add(level8);

        List<LevelType> level9 = new List<LevelType>();
        level9.Add(LevelType.Combat);
        level9.Add(LevelType.Event);
        level9.Add(LevelType.Rest);
        levelPresets.Add(level9);

        List<LevelType> level10 = new List<LevelType>();
        level10.Add(LevelType.Combat);
        level10.Add(LevelType.Event);
        levelPresets.Add(level10);

        List<LevelType> level11 = new List<LevelType>();
        level11.Add(LevelType.Combat);
        level11.Add(LevelType.Event);
        level11.Add(LevelType.Rest);
        levelPresets.Add(level11);

        List<LevelType> level12 = new List<LevelType>();
        level12.Add(LevelType.Elite);
        levelPresets.Add(level12);

        List<LevelType> level13 = new List<LevelType>();
        level13.Add(LevelType.Combat);
        level13.Add(LevelType.Event);
        levelPresets.Add(level13);

        List<LevelType> level14 = new List<LevelType>();
        level14.Add(LevelType.Combat);
        level14.Add(LevelType.Event);
        levelPresets.Add(level14);

        List<LevelType> level15 = new List<LevelType>();
        level15.Add(LevelType.Event);
        level15.Add(LevelType.Rest);
        levelPresets.Add(level15);

        List<LevelType> level16 = new List<LevelType>();
        level16.Add(LevelType.Boss);
        levelPresets.Add(level16);
    }

    public static void LoadLevel()
    {
        if (currentLevelType == LevelType.Combat)
        {
            SceneManager.LoadScene("Combat", LoadSceneMode.Single);
        }
    }

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
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
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
}
