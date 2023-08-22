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

    public GameObject pauseMenu;


    public static UnityAction<string, Color> SpawnSystemText;
    public GameObject systemText;

    private void Start()
    {
        ToggleInventoryUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(GameManager.Inventory))
        {
            ToggleInventoryUI();
        }
        if (Input.GetKeyDown(GameManager.Pause))
        {
            if (GameManager.IsGamePaused)
            {
                ResumeGame();
            } else
            {
                PauseGame();
            }
        }
    }

    public void ToggleInventoryUI()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        if (inventoryUI.activeSelf)
            GameManager.SetPlayerCanAttack(false);
        else
            GameManager.SetPlayerCanAttack(true);
    }

    private void OnEnable()
    {
        SpawnSystemText += _SpawnSystemText;
        PlayerController.HealthUpdate += HealthUpdate;
        PlayerController.EnergyUpdate += EnergyUpdate;
    }

    private void OnDisable()
    {
        SpawnSystemText -= _SpawnSystemText;
        PlayerController.HealthUpdate -= HealthUpdate;
        PlayerController.EnergyUpdate -= EnergyUpdate;
    }

    void HealthUpdate(int health, int maxHealth)
    {
        playerHealth.fillAmount = (float)health / maxHealth;
        playerHealthText.text = health + " / " + maxHealth;
    }

    void EnergyUpdate(float energy, float maxEnergy)
    {
        playerEnergy.fillAmount = energy / maxEnergy;
    }

    public void _SpawnSystemText(string text, Color color)
    {
        GameObject message = Instantiate(systemText, transform);
        message.GetComponentInChildren<TMP_Text>().text = text;
        message.GetComponentInChildren<TMP_Text>().color = color;
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        GameManager.PauseGame();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        GameManager.ResumeGame();
    }

    public void QuitGame()
    {
        GameManager.QuitGame();
    }
}
