using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponInterface : InventoryInterface
{
    public GameObject reloadBar;
    public GameObject castLoadingBar;

    public WeaponController wc;

    protected override void _CreateSlots(int i)
    {
        base._CreateSlots(i);
        inventory.GetSlots[i].UpdateSelected +=
            (active) => inventory.GetSlots[i].slotDisplay.transform.GetChild(1).gameObject.SetActive(active);
    }

    private void OnEnable()
    {
        wc = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponController>();
        inventory.startReload += StartReload;
        wc.StartCast += StartCast;
    }

    private void OnDisable()
    {
        inventory.startReload -= StartReload;
        wc.StartCast -= StartCast;
    }

    private void StartReload(float seconds)
    {
        Debug.Log("UI start reload");
        IEnumerator _StartReload(float seconds)
        {
            GameObject _reloadBar = Instantiate(reloadBar, transform);
            float barX = GetComponent<GridLayoutGroup>().cellSize.x;
            float barY = inventory.GetSize * (GetComponent<GridLayoutGroup>().cellSize.y +
                GetComponent<GridLayoutGroup>().spacing.y) - GetComponent<GridLayoutGroup>().spacing.y;
            _reloadBar.GetComponent<RectTransform>().sizeDelta = new Vector2(barX, barY);
            float tempTimer = seconds;
            while (tempTimer > 0)
            {
                _reloadBar.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(barX, barY * tempTimer / seconds);
                tempTimer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            Destroy(_reloadBar);
        }
        StartCoroutine(_StartReload(seconds));
    }

    private void StartCast(float seconds, Item item)
    {
        if(seconds > 0.2f)
        {
            GameObject bar = Instantiate(castLoadingBar, transform.parent);
            bar.GetComponentInChildren<TMP_Text>().text = item.Name;
            Image loadingFill = bar.transform.GetChild(1).GetComponent<Image>();
            IEnumerator _StartCast(float seconds)
            {
                float tempTimer = seconds;
                while (tempTimer > 0)
                {
                    loadingFill.fillAmount = 1f - tempTimer / seconds;
                    tempTimer -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                Destroy(bar);
            }
            StartCoroutine(_StartCast(seconds));
        }
    }
}
