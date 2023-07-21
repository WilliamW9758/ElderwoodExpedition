using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public Image playerHealth;
    public TMP_Text playerHealthText;
    public Image playerEnergy;


    public static UnityAction<string, Color> SpawnSystemText;
    public GameObject systemText;

    private void Update()
    {
        if (Input.GetKeyDown(GameManager.Inventory))
        {
            ToggleInventoryUI();
        }
    }

    public void ToggleInventoryUI()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        if (inventoryUI.activeSelf)
            GameManager.PauseGame();
        else
            GameManager.ResumeGame();
    }

    private void OnEnable()
    {
        SpawnSystemText += _SpawnSystemText;
        PlayerController.HealthUpdate += (health, maxHealth) => {
            playerHealth.fillAmount = (float)health / maxHealth;
            playerHealthText.text = health + " / " + maxHealth;
        };
        PlayerController.EnergyUpdate += (energy, maxEnergy) => {
            playerEnergy.fillAmount = energy / maxEnergy;
        };
    }

    public void _SpawnSystemText(string text, Color color)
    {
        GameObject message = Instantiate(systemText, transform);
        message.GetComponentInChildren<TMP_Text>().text = text;
        message.GetComponentInChildren<TMP_Text>().color = color;
    }
}
