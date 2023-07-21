using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    public UnityAction<float, Item> StartCast;
    public UnityAction<int> UpdateWeaponLeft;
    public UnityAction<int> UpdateWeaponRight;

    public InventoryObject weaponLeft;
    public InventoryObject weaponRight;
    public GameObject itemWorldPrefab;

    public int weaponPtrLeft = -1;
    public int weaponPtrRight = -1;

    private EntityController ec;

    public float defaultReloadCDLeft;
    public float defaultReloadCDRight;
    public float cumulatedReloadCDLeft;
    public float cumulatedReloadCDRight;
    public bool reloadingLeft;
    public bool reloadingRight;
    public bool reloadOnEffectEndLeft;
    public bool reloadOnEffectEndRight;

    public Item[] currentItem = new Item[0];
    public Item[] nextItemLeft = new Item[0];
    public Item[] nextItemRight = new Item[0];

    public bool canAttack = true;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    private void Start()
    {
        ec = GetComponent<EntityController>();
        //swordHolder = transform.Find("SwordHolder").gameObject;
        //swordAnim = swordHolder.GetComponentInChildren<Animator>();
        for (int i = 0; i < weaponLeft.GetSize; i++)
        {
            weaponLeft.GetSlots[i].OnAfterUpdate += OnSlotUpdate;
            weaponLeft.GetSlots[i].ItemDropped += OnItemDropped;
        }
        for (int i = 0; i < weaponRight.GetSize; i++)
        {
            weaponRight.GetSlots[i].OnAfterUpdate += OnSlotUpdate;
            weaponRight.GetSlots[i].ItemDropped += OnItemDropped;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnSlotUpdate(InventorySlot _slot)
    {
        //Debug.Log("Weapon OnSlotUpdate");
        if (weaponPtrLeft >= 0)
            weaponLeft.GetSlots[weaponPtrLeft].UpdateSelected?.Invoke(false);
        if (weaponPtrRight >= 0)
            weaponRight.GetSlots[weaponPtrRight].UpdateSelected?.Invoke(false);

        weaponPtrLeft = weaponLeft.GetFirstNonEmptySlotIndex;
        weaponPtrRight = weaponRight.GetFirstNonEmptySlotIndex;
        if (weaponPtrLeft >= 0)
            weaponLeft.GetSlots[weaponPtrLeft].UpdateSelected?.Invoke(true);
        if (weaponPtrRight >= 0)
            weaponRight.GetSlots[weaponPtrRight].UpdateSelected?.Invoke(true);
        reloadOnEffectEndLeft = false;
        reloadOnEffectEndRight = false;

        currentItem = new Item[0];
        if (weaponPtrLeft >= 0)
            UpdateItemLeft(weaponPtrLeft, new Item[] { weaponLeft.GetSlots[weaponPtrLeft].item });
        if (weaponPtrRight >= 0)
            UpdateItemRight(weaponPtrRight, new Item[] { weaponRight.GetSlots[weaponPtrRight].item });
    }

    private void OnItemDropped(InventorySlot _slot)
    {
        int id = _slot.item.Id;
        GameObject itemWorld = Instantiate(itemWorldPrefab, transform.position, Quaternion.identity);
        itemWorld.GetComponent<ItemWorld>().item = weaponLeft.database.ItemObjects[id];
    }

    public void TriggerCurrentItem()
    {
        ec.TriggerStatusEffect(StatusEffectObject.TriggerType.OnCast, gameObject);
        if (currentItem.Length == 0) return;
        Item item = currentItem[0];
        foreach (ItemEffect ie in item.InitialEffects)
        {
            if (ie.effect == Effect.AddStatusEffect)
            {
                ec.AddStatusEffect(ie.statusEffect.InitializeBuff(gameObject));
                ec.GainEnergy(ie.energyDelta);
                if (currentItem.Length > 1)
                {
                    Item[] tempCurItem = new Item[currentItem.Length - 1];
                    Array.Copy(currentItem, 1, tempCurItem, 0, currentItem.Length - 1);
                    currentItem = tempCurItem;
                    TriggerCurrentItem();
                }
            }
            if (ie.effect == Effect.SpawnPrefab)
            {
                Vector2 effectLocation = ie.IsAtSelf ? transform.position : GameManager.GetMouseWorldPosition();
                Transform effectParent = ie.IsAtSelf && ie.velocity == 0 ? transform : null;
                GameObject effect = Instantiate(ie.effectPrefab, effectLocation, transform.rotation, effectParent);
                //Debug.Log("Instantiate effect: " + effect.name);
                effect.SetActive(false);
                EffectManager em = effect.GetComponent<EffectManager>();
                em.attacker = gameObject;
                em.attackPercent = ie.attackPercent;
                em.knockback = ie.knockback;
                em.poiseBreak = ie.poiseBreak;
                em.energyDelta = ie.energyDelta;
                em.lifetime = ie.lifetime;
                if (ie.lifetime < 0)
                    em.lifetime = ie.duration;
                em.velocity = Vector2.up * ie.velocity;
                em.infusedElement = ec.currentElement;
                em.destroyOnHit = ie.destroyOnHit;

                em.SSAttack = (ec.attack + ec.attackFlatMod) * (1 + ec.attackRatioMod);
                em.SSCR = ec.critRate;
                em.SSCD = ec.critDamage;
                em.SSMaxHealth = ec.maxHealth;
                em.SSHealth = ec.health;

                em.OnEffectActive += TriggerEffect;
                em.OnEffectEnd += TriggerEffect;
                em.OnEffectHit += TriggerEffect;
                em.OnEffectBlock += TriggerEffect;

                if (ie.indicator)
                {
                    GameObject indicator = Instantiate(ie.indicator, effectLocation, transform.rotation, effectParent);
                    Animator IndicatorAnim;
                    indicator.TryGetComponent<Animator>(out IndicatorAnim);
                    SpriteRenderer IndicatorSR = indicator.GetComponent<SpriteRenderer>();
                    if (gameObject.tag == "Player")
                    {
                        IndicatorSR.color = new Color(0.3f, 0.6f, 0.7f, 0.5f);
                    } else
                    {
                        IndicatorSR.color = new Color(0.7f, 0.13f, 0.13f, 0.5f);
                    }
                    if (IndicatorAnim)
                        IndicatorAnim.speed = 1f / ie.windUpTime;
                    Destroy(indicator, ie.windUpTime);
                }


                foreach (ItemEffect ief in item.FollowUpEffects)
                {
                    em.attachedFollowUpEffects.Add(ief);
                    Debug.Log("Follow Up added: " + ief + "; trigger type: " + ief.followUpTrigger);
                }

                StartCast?.Invoke(ie.windUpTime, item);

                EffectWindUp(effect, ie.windUpTime, transform.rotation * Vector2.up, ie.movement);
                EffectWindDown(ie.windUpTime + ie.duration);
                EffectEnd(ie.windUpTime + ie.duration + ie.windDownTime, ie.speedDelta);
            }
        }
        Debug.Log(item.Type);
        if (item.Type == ItemType.StatusEffectRune)
        {
            //Debug.Log("Should be reloading");
            if (reloadOnEffectEndLeft)
                ReloadLeft(defaultReloadCDLeft + cumulatedReloadCDLeft);
            if (reloadOnEffectEndRight)
                ReloadRight(defaultReloadCDRight + cumulatedReloadCDRight);
        }
    }

    private void TriggerEffect(EffectManager parentEffect, ItemEffect ie, GameObject attacker)
    {
        if (ie.effect == Effect.AddStatusEffect)
        {
            attacker.GetComponent<EntityController>().AddStatusEffect(ie.statusEffect.InitializeBuff(attacker));
        }
        if (ie.effect == Effect.SpawnPrefab)
        {
            Vector2 effectLocation = ie.IsAtSelf ? attacker.transform.position : transform.position;
            Transform effectParent = ie.IsAtSelf ? attacker.transform : null;
            GameObject effect = Instantiate(ie.effectPrefab, effectLocation, attacker.transform.rotation, effectParent);
            //Debug.Log("Instantiate followup effect: " + effect.name);
            EffectManager em = effect.GetComponent<EffectManager>();
            em.attacker = gameObject;
            em.attackPercent = ie.attackPercent;
            em.knockback = ie.knockback;
            em.poiseBreak = ie.poiseBreak;
            em.energyDelta = ie.energyDelta;
            em.lifetime = ie.lifetime;
            if (ie.lifetime < 0)
                em.lifetime = ie.duration;
            em.velocity = attacker.transform.rotation * (Vector2.up * ie.velocity);
            em.infusedElement = parentEffect.infusedElement;
            em.destroyOnHit = ie.destroyOnHit;

            em.SSAttack = parentEffect.SSAttack;
            em.SSCR = parentEffect.SSCR;
            em.SSCD = parentEffect.SSCD;
            em.SSMaxHealth = parentEffect.SSMaxHealth;
            em.SSHealth = parentEffect.SSHealth;

            if (ie.indicator)
            {
                GameObject indicator = Instantiate(ie.indicator, effectLocation, transform.rotation, effectParent);
                Animator IndicatorAnim;
                indicator.TryGetComponent<Animator>(out IndicatorAnim);
                SpriteRenderer IndicatorSR = indicator.GetComponent<SpriteRenderer>();
                if (gameObject.tag == "Player")
                {
                    IndicatorSR.color = new Color(0.3f, 0.6f, 0.7f, 0.5f);
                }
                else
                {
                    IndicatorSR.color = new Color(0.7f, 0.13f, 0.13f, 0.5f);
                }
                if (IndicatorAnim)
                    IndicatorAnim.speed = 1f / ie.windUpTime;
                Destroy(indicator, ie.windUpTime);
            }

            IEnumerator StartEffect()
            {
                yield return new WaitForSeconds(ie.windUpTime);
                effect.SetActive(true);
                ec.StartDash(transform.rotation * Vector2.up * ie.movement);
                yield return new WaitForSeconds(ie.duration);
                if (ie.movement > 0)
                {
                    ec.EndDash();
                }
            }
            StartCoroutine(StartEffect());
        }
    }

    private void EffectWindUp(GameObject obj, float seconds, Vector2 _dir, float movement)
    {
        IEnumerator _EffectWindUp(GameObject obj, float seconds, Vector2 _dir, float movement)
        {
            yield return new WaitForSeconds(seconds);
            obj.SetActive(true);
            Animator objAnim;
            obj.TryGetComponent<Animator>(out objAnim);
            if (objAnim)
                objAnim.Play(0);
            ec.StartDash(_dir * movement);
            //Debug.Log("Finish Windup: " + obj.name);
        }
        StartCoroutine(_EffectWindUp(obj, seconds, _dir, movement));
    }

    private void EffectWindDown(float seconds)
    {
        IEnumerator _EffectWindDown(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            ec.EndDash();
        }
        StartCoroutine(_EffectWindDown(seconds));
    }

    private void EffectEnd(float seconds, float speedDelta)
    {
        IEnumerator _EffectEnd(float seconds, float speedDelta)
        {
            canAttack = false;
            float originalSpeed = ec.speed;
            ec.speed *= (1 + speedDelta);
            ec.controlLock = ec.speed == 0;
            //Debug.Log("Speed: " + ec.speed + "; IsCL: " + ec.controlLock);
            yield return new WaitForSeconds(seconds);
            ec.speed = originalSpeed;
            canAttack = true;
            ec.controlLock = false;
            ec.TriggerStatusEffect(StatusEffectObject.TriggerType.OnEnd, gameObject);
            if (currentItem.Length > 1)
            {
                Item[] tempCurItem = new Item[currentItem.Length - 1];
                Array.Copy(currentItem, 1, tempCurItem, 0, currentItem.Length - 1);
                currentItem = tempCurItem;
                TriggerCurrentItem();
            }
            
            //Debug.Log("Finish Effect");
            if (reloadOnEffectEndLeft)
                ReloadLeft(defaultReloadCDLeft + cumulatedReloadCDLeft);
            if (reloadOnEffectEndRight)
                ReloadRight(defaultReloadCDRight + cumulatedReloadCDRight);
        }
        StartCoroutine(_EffectEnd(seconds, speedDelta));
    }

    public bool AdvanceWeaponLeft()
    {
        float totalEnergyDelta = 0;
        float tempReloadL = 0;
        float tempReloadR = 0;
        foreach (Item item in nextItemLeft)
        {
            foreach (ItemEffect ie in item.InitialEffects)
            {
                totalEnergyDelta += ie.energyDelta;
                tempReloadL += ie.reloadDeltaCurrentWeapon;
                tempReloadR += ie.reloadDeltaOtherWeapon;
            }
        }
        if (ec.energy > -totalEnergyDelta)
        {
            cumulatedReloadCDLeft += tempReloadL;
            cumulatedReloadCDRight += tempReloadR;
            currentItem = nextItemLeft;
            if (weaponPtrLeft >= 0)
            {
                Debug.Log(currentItem.Length);
                for (int i = weaponPtrLeft; i > weaponPtrLeft - currentItem.Length; i--)
                {
                    Debug.Log("Left Deselect " + i);
                    weaponLeft.GetSlots[i].UpdateSelected?.Invoke(false);
                }
            }
            return true;
        } else
        {
            currentItem = new Item[0];
            return false;
        }
    }

    public bool AdvanceWeaponRight()
    {
        float totalEnergyDelta = 0;
        float tempReloadL = 0;
        float tempReloadR = 0;
        foreach (Item item in nextItemRight)
        {
            foreach (ItemEffect ie in item.InitialEffects)
            {
                totalEnergyDelta += ie.energyDelta;
                tempReloadR += ie.reloadDeltaCurrentWeapon;
                tempReloadL += ie.reloadDeltaOtherWeapon;
            }
        }
        if (ec.energy > -totalEnergyDelta)
        {
            cumulatedReloadCDLeft += tempReloadL;
            cumulatedReloadCDRight += tempReloadR;
            currentItem = nextItemRight;
            if (weaponPtrRight >= 0)
            {
                for (int i = weaponPtrRight; i > weaponPtrRight - currentItem.Length; i--)
                {
                    Debug.Log("Right Deselect " + i);

                    weaponRight.GetSlots[i].UpdateSelected?.Invoke(false);
                }
            }
            return true;
        }
        else
        {
            currentItem = new Item[0];
            return false;
        }
    }

    // search for next left
    public void UpdateLeft()
    {
        int tempLeft = weaponPtrLeft + 1;

        while (tempLeft < weaponLeft.GetSize && weaponLeft.GetSlots[tempLeft].IsEmpty)
        {
            tempLeft++;
        }
        if (tempLeft >= weaponLeft.GetSize)
        {
            // if there is no next left, reload
            reloadOnEffectEndLeft = true;
        }
        else
            // if there is next left at tempLeft, update item
            UpdateItemLeft(tempLeft, new Item[] { weaponLeft.GetSlots[tempLeft].item });
    }

    public void UpdateRight()
    {
        int tempRight = weaponPtrRight + 1;
        while (tempRight < weaponRight.GetSize && weaponRight.GetSlots[tempRight].IsEmpty)
        {
            tempRight++;
        }
        if (tempRight >= weaponRight.GetSize)
            reloadOnEffectEndRight = true;
        else
            UpdateItemRight(tempRight, new Item[] { weaponRight.GetSlots[tempRight].item });
    }

    private void ReloadLeft(float seconds)
    {
        reloadOnEffectEndLeft = false;
        ec.TriggerStatusEffect(StatusEffectObject.TriggerType.OnReload, gameObject);
        IEnumerator _ReloadLeft(float seconds)
        {
            nextItemLeft = new Item[0];
            weaponLeft.startReload?.Invoke(seconds);
            yield return new WaitForSeconds(seconds);
            for (int i = 0; i < weaponLeft.GetSize; i++)
            {
                if (weaponLeft.GetSlots[i].item.Id >= 0)
                {
                    UpdateItemLeft(i, new Item[] { weaponLeft.GetSlots[i].item });
                    break;
                }
            }
        }
        StartCoroutine(_ReloadLeft(seconds));
        cumulatedReloadCDLeft = 0;
    }

    private void ReloadRight(float seconds)
    {
        reloadOnEffectEndRight = false;
        ec.TriggerStatusEffect(StatusEffectObject.TriggerType.OnReload, gameObject);
        IEnumerator _ReloadRight(float seconds)
        {
            nextItemRight = new Item[0];
            weaponRight.startReload?.Invoke(seconds);
            yield return new WaitForSeconds(seconds);
            for (int i = 0; i < weaponRight.GetSize; i++)
            {
                if (weaponRight.GetSlots[i].item.Id >= 0)
                {
                    UpdateItemRight(i, new Item[] { weaponRight.GetSlots[i].item });
                    break;
                }
            }
        }
        StartCoroutine(_ReloadRight(seconds));
        cumulatedReloadCDRight = 0;
    }

    // already found next item at idx
    private void UpdateItemLeft(int idx, Item[] nextItems)
    {
        // Add support for multicast
        Item lastItem = nextItems[nextItems.Length-1];

        foreach (ItemEffect ie in lastItem.InitialEffects)
        {
            if (ie.triggerNext)
            {
                int tempLeft = idx + 1;
                while (tempLeft < weaponLeft.GetSize && weaponLeft.GetSlots[tempLeft].IsEmpty)
                {
                    tempLeft++;
                }
                if (tempLeft >= weaponLeft.GetSize)
                {
                    break;
                }
                else
                {
                    nextItemLeft = new Item[nextItems.Length+1];
                    for (int i = 0; i < nextItems.Length; i++)
                    {
                        nextItemLeft[i] = nextItems[i];
                    }
                    nextItemLeft[nextItemLeft.Length - 1] = weaponLeft.GetSlots[tempLeft].item;
                    weaponLeft.GetSlots[idx].UpdateSelected?.Invoke(true);
                    UpdateItemLeft(tempLeft, nextItemLeft);
                    return;
                }
            }
        }

        nextItemLeft = nextItems;
        weaponPtrLeft = idx;
        weaponLeft.GetSlots[weaponPtrLeft].UpdateSelected?.Invoke(true);
    }

    private void UpdateItemRight(int idx, Item[] nextItems)
    {
        // Add support for multicast
        Item lastItem = nextItems[nextItems.Length - 1];

        // otherwise check if it can trigger next
        foreach (ItemEffect ie in lastItem.InitialEffects)
        {
            if (ie.triggerNext)
            {
                int tempRight = idx + 1;
                while (tempRight < weaponRight.GetSize && weaponRight.GetSlots[tempRight].IsEmpty)
                {
                    tempRight++;
                }
                if (tempRight >= weaponRight.GetSize)
                {
                    break;
                }
                else
                {
                    nextItemRight = new Item[nextItems.Length + 1];
                    for (int i = 0; i < nextItems.Length; i++)
                    {
                        nextItemRight[i] = nextItems[i];
                    }
                    nextItemRight[nextItemRight.Length - 1] = weaponRight.GetSlots[tempRight].item;
                    weaponRight.GetSlots[weaponPtrRight].UpdateSelected?.Invoke(true);
                    UpdateItemRight(tempRight, nextItemRight);
                    return;
                }
            }
        }

        nextItemRight = nextItems;
        weaponPtrRight = idx;
        weaponRight.GetSlots[weaponPtrRight].UpdateSelected?.Invoke(true);
    }

    private void OnApplicationQuit()
    {
        weaponLeft.Clear();
        weaponRight.Clear();
    }
}