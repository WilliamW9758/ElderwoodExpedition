using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponManager : MonoBehaviour
{
    private Camera mainCamera;
    private InventoryManager inventoryManager;
    public List<Item> weaponOrderLeft;
    public List<Item> weaponOrderRight;
    public int weaponPtrLeft;
    public int weaponPtrRight;
    public Rigidbody2D rb;
    public PlayerController pc;

    public bool canAttack;

    public bool reloadingLeft;
    public bool reloadingRight;
    public float totalReloadLeft;
    public float totalReloadRight;
    public float reloadCDLeft;
    public float reloadCDRight;
    public float reloadTimerLeft;
    public float reloadTimerRight;
    public float cumulatedReloadCDLeft;
    public float cumulatedReloadCDRight;

    public GameObject swordHolder;
    public Animator swordAnim;
    public GameObject sheathParticle;
    public float SwordLerpSpeed;
    public float SwordQuatLerpSpeed;
    public Quaternion swordRot;
    private Vector3 newPos;

    public Attack currentAttack;
    public Spell currentSpell;
    public GameObject currentAttackGO;
    public GameObject currentSpellGO;
    public enum AttackState
    {
        canAttack,
        preAttack,
        attack,
        postAttack
    }
    public AttackState attackState;

    private Vector2 mouseWorldPosition;
    private Vector2 dir;
    private Quaternion dirQ;

    public Dictionary<string, float> currentMods;
    public int wrapCountLeft;
    public int wrapCountRight;

    public enum SwordPosition
    {
        left,
        right,
        up,
        down,
        front,
        back,
        sheathed,
        any
    }
    public SwordPosition currentPos;

    // Start is called before the first frame update
    void Start()
    {
        swordHolder = transform.Find("SwordHolder").gameObject;
        swordAnim = swordHolder.GetComponentInChildren<Animator>();
        canAttack = true;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        inventoryManager = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<InventoryManager>();
        rb = GetComponent<Rigidbody2D>();
        weaponOrderLeft = inventoryManager.GetWeaponLeft();
        weaponOrderRight = inventoryManager.GetWeaponRight();
        WeaponReset(true, true);
        pc = GetComponent<PlayerController>();

        swordRot = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        reloadTimerLeft = (reloadTimerLeft > 0) ? reloadTimerLeft - Time.deltaTime : 0;
        reloadTimerRight = (reloadTimerRight > 0) ? reloadTimerRight - Time.deltaTime : 0;
        reloadingLeft = reloadTimerLeft > 0;
        reloadingRight = reloadTimerRight > 0;

        mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dir = (mouseWorldPosition - new Vector2(transform.position.x,
transform.position.y)).normalized;
        dirQ = Quaternion.LookRotation(Vector3.forward, dir);

        swordHolder.SetActive(currentPos != SwordPosition.sheathed);

        if (!pc.controlLock)
        {
            newPos = (Vector3)(dir * 0.5f) + transform.position;
        }
        MoveSword(newPos, swordRot, SwordLerpSpeed, SwordQuatLerpSpeed);

        //float rotAngle;
        //switch (currentPos)
        //{
        //    case (SwordPosition.front):
        //        rotAngle = 0;
        //        break;
        //    case (SwordPosition.right):
        //        rotAngle = Mathf.PI / 4;
        //        break;
        //    case (SwordPosition.left):
        //        rotAngle = -Mathf.PI / 4;
        //        break;
        //    default:
        //        rotAngle = 0;
        //        break;
        //}
        //swordHolder.transform.localPosition = new Vector2(dir.x * Mathf.Cos(-rotAngle) - dir.y * Mathf.Sin(-rotAngle),
        //    dir.x * Mathf.Sin(-rotAngle) + dir.y * Mathf.Cos(-rotAngle)) * 0.5f;
        if (!inventoryManager.inventoryObject.activeSelf && Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (currentPos == SwordPosition.sheathed)
            {
                DrawSword(SwordPosition.right);
            } else
            {
                SheathSword();
            }
        }
        if (currentPos == SwordPosition.sheathed)
        {
            pc.AddBuff(pc.SheathSpeedBuff.InitializeBuff(gameObject));
        }

        List<Item> tempWeaponOrderLeft = inventoryManager.GetWeaponLeft();
        List<Item> tempWeaponOrderRight = inventoryManager.GetWeaponRight();
        if (!tempWeaponOrderLeft.SequenceEqual(weaponOrderLeft) ||
            !tempWeaponOrderRight.SequenceEqual(weaponOrderRight))
        {
            Debug.Log("Weapon Reset");
            WeaponReset(true, true);
            weaponOrderLeft = tempWeaponOrderLeft;
            weaponOrderRight = tempWeaponOrderRight;
        }
        //Debug.Log("WeaponPtrLeft: " + weaponPtrLeft + "; Can Attack: " + canAttack + "; State: " + attackState);
        if ((Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2")) &&
            currentPos == SwordPosition.sheathed && canAttack)
        {
            DrawSword();
        } else
        {
            if (Input.GetButtonDown("Fire1") && canAttack)
            {
                NextAttack(0);
            }
            else if (Input.GetButtonDown("Fire2") && canAttack)
            {
                NextAttack(1);
            }
        }
    }

    private void NextAttack(int side)
    {
        if (attackState == AttackState.canAttack)
        {
            pc.animator.SetFloat("mouseX", dir.x);
            pc.animator.SetFloat("mouseY", dir.y);
            //Debug.Log("Attacking");
            if (side == 0)
            {
                if (weaponPtrLeft >= weaponOrderLeft.Count) return;
                if (reloadTimerLeft > 0)
                {
                    Debug.Log(pc);
                    pc.ShowFloatingText("Reloading", Color.white, false);
                    return;
                }
                // Left attack
                while (!weaponOrderLeft[weaponPtrLeft])
                {
                    weaponPtrLeft++;
                }
                inventoryManager.ContentUpdate();
                //Debug.Log("Out loop: " + weaponPtrLeft);
                //Debug.Log("Trying: " + weaponPtrLeft);
                //Debug.Log("Start Attack");
                // start attack
                Debug.Log("Left normal attack");
                if (weaponOrderLeft[weaponPtrLeft].ThisItemType == Item.ItemType.attack)
                {
                    currentAttack = (Attack)weaponOrderLeft[weaponPtrLeft];
                    if (pc.TestEnergy(currentAttack.energyDelta))
                    {
                        currentAttackGO = Instantiate(currentAttack.effect, transform.position + ((Vector3)dir) * 0.25f, dirQ, transform);
                        swordRot = dirQ;
                        weaponPtrLeft++;
                    }
                }
                else if (weaponOrderLeft[weaponPtrLeft].ThisItemType == Item.ItemType.spell)
                {
                    currentSpell = (Spell)weaponOrderLeft[weaponPtrLeft];
                    if (pc.TestEnergy(currentSpell.energyDelta))
                    {
                        currentSpellGO = Instantiate(currentSpell.effect, transform.position + ((Vector3)dir) * 0.25f, dirQ, transform);
                        weaponPtrLeft++;
                    }
                }
                inventoryManager.ContentUpdate();
            }
            else
            {
                if (weaponPtrRight >= weaponOrderRight.Count) return;
                if (reloadTimerRight > 0)
                {
                    pc.ShowFloatingText("Reloading", Color.white, false);
                    return;
                }
                // Right attack
                //Debug.Log("Right Attack");
                while (!weaponOrderRight[weaponPtrRight])
                {
                    weaponPtrRight++;
                }
                inventoryManager.ContentUpdate();
                //Debug.Log("Trying: " + weaponPtrRight);
                //Debug.Log("Start Attack");
                Debug.Log("Right normal attack");
                if (weaponOrderRight[weaponPtrRight].ThisItemType == Item.ItemType.attack)
                {
                    currentAttack = (Attack)weaponOrderRight[weaponPtrRight];
                    if (pc.TestEnergy(currentAttack.energyDelta))
                    {
                        currentAttackGO = Instantiate(currentAttack.effect, transform.position + ((Vector3)dir) * 0.25f, dirQ, transform);
                        swordRot = dirQ;
                        currentAttackGO.GetComponent<Animator>().speed = 0.8f;
                        weaponPtrRight++;
                    }
                }
                else if (weaponOrderRight[weaponPtrRight].ThisItemType == Item.ItemType.spell)
                {
                    currentSpell = (Spell)weaponOrderRight[weaponPtrRight];
                    if (pc.TestEnergy(currentSpell.energyDelta))
                    {
                        currentSpellGO = Instantiate(currentSpell.effect, transform.position + ((Vector3)dir) * 0.25f, dirQ, transform);
                        weaponPtrRight++;
                    }
                }
                inventoryManager.ContentUpdate();
            }
        }
        else if (attackState == AttackState.postAttack)
        {
            if (side == 0)
            {
                if (weaponPtrLeft >= weaponOrderLeft.Count) return;
                if (reloadTimerLeft > 0)
                {
                    pc.ShowFloatingText("Reloading", Color.white, false);
                    return;
                }
                // Left attack
                while (!weaponOrderLeft[weaponPtrLeft])
                {
                    weaponPtrLeft++;
                }
                inventoryManager.ContentUpdate();
                //Debug.Log("Out loop: " + weaponPtrLeft);
                //Debug.Log("Trying: " + weaponPtrLeft);
                //Debug.Log("Start Attack");
                // start attack
                if (weaponOrderLeft[weaponPtrLeft].ThisItemType == Item.ItemType.attack)
                {
                    currentAttack = (Attack)weaponOrderLeft[weaponPtrLeft];

                    if (currentAttack.startPos == currentPos
                    || currentAttack.startPos == SwordPosition.any
                    || currentPos == SwordPosition.any)
                    {
                        if (pc.TestEnergy(currentAttack.energyDelta))
                        {
                            Debug.Log("Left skip post attack");
                            Destroy(currentAttackGO); // destroy previous attack so it doesn't trigger finishAttack
                            currentAttackGO = Instantiate(currentAttack.effect, transform.position + ((Vector3)dir) * 0.25f, dirQ, transform);
                            swordHolder.transform.rotation = dirQ;
                            weaponPtrLeft++;
                        }
                    }
                }
                inventoryManager.ContentUpdate();
            }
            else
            {
                if (weaponPtrRight >= weaponOrderRight.Count) return;
                if (reloadTimerLeft > 0)
                {
                    pc.ShowFloatingText("Reloading", Color.white, false);
                    return;
                }
                // Right attack
                //Debug.Log("Right Attack");
                while (!weaponOrderRight[weaponPtrRight])
                {
                    weaponPtrRight++;
                }
                inventoryManager.ContentUpdate();
                //Debug.Log("Trying: " + weaponPtrRight);
                //Debug.Log("Start Attack");
                if (weaponOrderLeft[weaponPtrLeft].ThisItemType == Item.ItemType.attack)
                {
                    currentAttack = (Attack)weaponOrderRight[weaponPtrRight];

                    if (currentAttack.startPos == currentPos
                    || currentAttack.startPos == SwordPosition.any
                    || currentPos == SwordPosition.any)
                    {
                        if (pc.TestEnergy(currentAttack.energyDelta))
                        {
                            Debug.Log("Right skip post attack");
                            Destroy(currentAttackGO); // destroy previous attack so it doesn't trigger finishAttack
                            currentAttackGO = Instantiate(currentAttack.effect, transform.position + ((Vector3)dir) * 0.25f, dirQ, transform);
                            swordHolder.transform.rotation = dirQ;
                            currentAttackGO.GetComponent<Animator>().speed = 0.8f;
                            weaponPtrRight++;
                        }
                    }
                }
                inventoryManager.ContentUpdate();
            }
        }
    }

    public void WeaponReset(bool resetLeft, bool resetRight)
    {
        if (resetLeft)
        {
            weaponPtrLeft = 0;
            wrapCountLeft = 0;
        }
        if (resetRight)
        {
            weaponPtrRight = 0;
            wrapCountRight = 0;
        }
        inventoryManager.ContentUpdate();
        //cumulatedAttackTimer = 0;
        //currentMods = new Dictionary<string, float>();
    }

    public void ReloadLeft()
    {
        pc.TriggerBuff(Buff.TriggerType.reload);
        totalReloadLeft = reloadCDLeft + cumulatedReloadCDLeft;
        reloadTimerLeft = totalReloadLeft;
        inventoryManager.ContentUpdate();
    }

    public void ReloadRight()
    {
        pc.TriggerBuff(Buff.TriggerType.reload);
        totalReloadRight = reloadCDRight + cumulatedReloadCDRight;
        reloadTimerRight = totalReloadRight;
        inventoryManager.ContentUpdate();
    }

    public void AdvanceBuffState()
    {

    }

    public void UpdateBuffTimer()
    {

    }

    public void StartSwordPosReset(float duration = 10f)
    {
        IEnumerator SwordReset_Cor()
        {
            yield return new WaitForSeconds(duration);
            SheathSword();
        }
        StopAllCoroutines();
        StartCoroutine(SwordReset_Cor());
    }

    public void SheathSword()
    {
        StopAllCoroutines();
        currentPos = SwordPosition.sheathed;
        Instantiate(sheathParticle, swordHolder.transform.position, Quaternion.identity);
    }

    public void DrawSword(SwordPosition startPos = SwordPosition.right)
    {
        StartSwordPosReset(4f);
        currentPos = startPos;
        swordRot = Quaternion.identity;
    }

    public void MoveSword(Vector2 pos, Quaternion rot, float LerpSpeed, float QuatLerpSpeed)
    {
        swordHolder.transform.position =
            Vector2.Lerp(swordHolder.transform.position, pos, LerpSpeed * Time.deltaTime);
        swordHolder.transform.rotation = Quaternion.Lerp(swordHolder.transform.rotation,
            rot, QuatLerpSpeed * Time.deltaTime);
    }
}
