using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemController : MonoBehaviour, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    public Item thisItem;
    private SpriteRenderer sr;
    private Image image;

    public Transform parentAfterDrag;
    private InventoryManager inventoryManager;
    [SerializeField]
    private Sprite NoImageSprite;
    // Start is called before the first frame update

    private bool beginDrag;
    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        try
        {
            sr.sprite = thisItem.ItemWorldImage;
        } catch (System.NullReferenceException)
        {
            sr.sprite = NoImageSprite;
        }
        image = GetComponent<Image>();
        try
        {
            image.sprite = thisItem.ItemInventoryImage;
        } catch (System.NullReferenceException)
        {
            image.sprite = NoImageSprite;
        }
        inventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        beginDrag = false;
        if (GetComponent<HoverDetector>())
        {
            GetComponent<HoverDetector>().content = thisItem.ItemInfo;
        }
    }

    void Update()
    {
        if (!beginDrag)
        {
            image.raycastTarget = inventoryManager.inventoryObject.activeSelf;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (!inventoryManager.inventoryObject.activeSelf)
        //{
        //    return;
        //}
        beginDrag = true;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (!inventoryManager.inventoryObject.activeSelf)
        //{
        //    return;
        //}
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (!inventoryManager.inventoryObject.activeSelf)
        //{
        //    return;
        //}
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
        transform.SetAsFirstSibling();
        beginDrag = false;
        inventoryManager.ContentUpdate();
    }
}