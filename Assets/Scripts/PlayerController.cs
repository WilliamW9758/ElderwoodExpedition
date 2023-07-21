using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PlayerController : EntityController
{
    public static UnityAction<InventoryObject> Interact;
    public static UnityAction<int, int> HealthUpdate;
    public static UnityAction<float, float> EnergyUpdate;

    private float inputX, inputY;

    public InventoryObject inventory;
    public GameObject itemWorldPrefab;

    public Vector2 mouseWorldPosition;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        for (int i = 0; i < inventory.GetSize; i++)
        {
            inventory.GetSlots[i].ItemDropped += OnItemDropped;
        }
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            inventory.Save();
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            inventory.Load();
        }
        if (Input.GetKeyDown(GameManager.Interact))
        {
            Interact?.Invoke(inventory);
        }

        switch (currentElement)
        {
            case (Elements.physical):
                GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case (Elements.fire):
                Debug.Log("Showing Fire text");
                GetComponent<SpriteRenderer>().color = new Color(1, 0.15f, 0.15f);
                break;
            case (Elements.ice):
                GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.94f, 1);
                break;
            case (Elements.electric):
                GetComponent<SpriteRenderer>().color = new Color(0.84f, 0.17f, 0.9f);
                break;
            case (Elements.light):
                GetComponent<SpriteRenderer>().color = new Color(1, 0.9f, 0.4f);
                break;
            case (Elements.dark):
                GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.17f, 0.9f);
                break;
        }
        

        if (!controlLock)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");
            moveVec = new Vector2(inputX, inputY).normalized;
        }
        else
        {
            moveVec = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            wc.weaponLeft.Save();
            wc.weaponRight.Save();
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            wc.weaponLeft.Load();
            wc.weaponRight.Load();
        }

        if (Input.GetKeyDown(GameManager.PrimaryAttack)
            && wc.canAttack
            && wc.nextItemLeft.Length > 0
            && !GameManager.IsGamePaused)
        {
            if (!wc.AdvanceWeaponLeft())
                UIManager.SpawnSystemText?.Invoke("Not Enough Energy", Color.white);
            wc.UpdateLeft();
            wc.TriggerCurrentItem();
        }
        else if (Input.GetKeyDown(GameManager.SecondaryAttack)
            && wc.canAttack
            && wc.nextItemRight.Length > 0
            && !GameManager.IsGamePaused)
        {
            if (!wc.AdvanceWeaponRight())
                UIManager.SpawnSystemText?.Invoke("Not Enough Energy", Color.white);
            wc.UpdateRight();
            wc.TriggerCurrentItem();
        }
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override Quaternion calcTargetRot()
    {
        mouseWorldPosition = GameManager.GetMouseWorldPosition();
        Vector2 dir = (mouseWorldPosition - new Vector2(transform.position.x, transform.position.y)).normalized;
        Quaternion dirQ = Quaternion.LookRotation(Vector3.forward, dir);
        return dirQ;
    }

    public override bool TakeDamage(int damage, int breakAmount, bool _crit, Elements type = Elements.physical)
    {
        bool killed = base.TakeDamage(damage, 0, _crit, type);
        HealthUpdate?.Invoke(health, maxHealth);
        return killed;
    }

    public override void GainEnergy(float amount)
    {
        base.GainEnergy(amount);
        EnergyUpdate?.Invoke(energy, maxEnergy);
    }

    protected new IEnumerator TakeDamage_Cor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        invincible = true;
        sr.material.SetInt("_hit", 1);
        yield return new WaitForSecondsRealtime(0.2f);
        sr.material.SetInt("_hit", 0);
        yield return new WaitForSecondsRealtime(0.2f);
        sr.material.SetInt("_hit", 1);
        yield return new WaitForSecondsRealtime(0.2f);
        sr.material.SetInt("_hit", 0);
        invincible = false;
    }

    private void OnItemDropped(InventorySlot _slot)
    {
        int id = _slot.item.Id;
        if (id >= 0)
        {
            GameObject itemWorld = Instantiate(itemWorldPrefab, transform.position, Quaternion.identity);
            itemWorld.GetComponent<ItemWorld>().item = inventory.database.ItemObjects[id];
        }
    }

    private void OnApplicationQuit()
    {
        inventory.Clear();
    }
}
