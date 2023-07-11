using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Language
    {
        English,
        Chinese
    }
    public static Language CurrentLanguage { get; set;}
    public static KeyCode Up { get; set; }
    public static KeyCode Down { get; set; }
    public static KeyCode Left { get; set; }
    public static KeyCode Right { get; set; }
    public static KeyCode PrimaryAttack { get; set; }
    public static KeyCode SecondaryAttack { get; set; }
    public static KeyCode DrawSheathSword { get; set; }
    public static KeyCode Inventory { get; set; }
    public static KeyCode PickUp { get; set; }
    public static KeyCode Pause { get; set; }

    private void Awake()
    {
        CurrentLanguage = Language.English;
        Up = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnUp", "W"));
        Down = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnDown", "S"));
        Left = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnLeft", "A"));
        Right = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnRight", "D"));
        PrimaryAttack = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnPrimary", "Mouse0"));
        SecondaryAttack = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnSecondary", "Mouse1"));
        DrawSheathSword = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnSheath", "LeftShift"));
        Inventory = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnInventory", "Tab"));
        PickUp = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnPickUp", "E"));
        Pause = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BtnPause", "Escape"));
    }
}
