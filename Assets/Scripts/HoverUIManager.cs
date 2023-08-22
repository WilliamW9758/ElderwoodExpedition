using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HoverUIManager : MonoBehaviour
{
    public TMP_Text contentText;
    public RectTransform hoverWindow;

    public static Action<string, Vector2> OnMouseHover;
    public static Action OnMouseLoseFocus;

    public void OnEnable()
    {
        OnMouseHover += ShowMessage;
        OnMouseLoseFocus += HideMessage;
    }

    public void OnDisable()
    {
        OnMouseHover -= ShowMessage;
        OnMouseLoseFocus -= HideMessage;
    }

    public void Start()
    {
        contentText.text = default;
        hoverWindow.gameObject.SetActive(false);
    }

    private void ShowMessage(string content, Vector2 mousePos)
    {
        Debug.Log("HUIM Show Message");

        contentText.text = content;

        Debug.Log(content);

        hoverWindow.sizeDelta = new Vector2(contentText.preferredWidth > 300 ? 300 : contentText.preferredWidth, contentText.preferredHeight);

        hoverWindow.gameObject.SetActive(true);
        hoverWindow.transform.position = new Vector2(mousePos.x + 10, mousePos.y);
    }

    private void HideMessage()
    {
        contentText.text = default;
        hoverWindow.gameObject.SetActive(false);
    }
}
