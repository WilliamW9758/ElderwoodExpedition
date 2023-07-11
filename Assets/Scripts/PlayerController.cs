using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : EntityController
{
    public float maxEnergy;
    public float energy;
    public float energyRegen;

    private float inputX, inputY;

    //public GameObject inventory;
    private InventoryManager inventoryManager;
    private bool canPickUp;
    private bool pickUp;
    public Animator animator;

    public TMP_Text healthText;
    public Image energyFill;

    public bool controlLock;
    private Camera mainCamera;
    public Vector2 mouseWorldPosition;

    public Buff SheathSpeedBuff;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        inventoryManager = GameObject
            .FindGameObjectWithTag("InventoryManager")
            .GetComponent<InventoryManager>();
        animator = GetComponent<Animator>();

        energy = maxEnergy;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        energyFill.fillAmount = energy / maxEnergy;

        mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (canPickUp && Input.GetKeyDown(GameManager.PickUp))
        {
            pickUp = true;
        }

        if (!controlLock)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");
            if (inputX != 0 || inputY != 0)
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("inputX", inputX);
                animator.SetFloat("inputY", inputY);
            } else
            {
                animator.SetBool("isMoving", false);
            }
            moveVec = new Vector2(inputX, inputY).normalized;
        } else
        {
            moveVec = Vector2.zero;
        }

        animator.SetFloat("velX", rb.velocity.x);
        animator.SetFloat("velY", rb.velocity.y);
        if (rb.velocity.magnitude > 0)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("inputX", rb.velocity.normalized.x);
            animator.SetFloat("inputY", rb.velocity.normalized.y);
        } else
        {
            animator.SetBool("isMoving", false);
        }
    }

    protected new void FixedUpdate()
    {
        energy = energy + energyRegen * Time.fixedDeltaTime > maxEnergy ?
            maxEnergy : energy + energyRegen * Time.fixedDeltaTime;

        if (controlLock)
        {
            rb.velocity = dashVelocity + pushVelocity;
        } else
        {
            rb.velocity = moveVec * speed + dashVelocity + pushVelocity;
        }
        base.FixedUpdate();
    }

    public override bool TakeDamage(int damage, int breakAmount = 0, Elements type = Elements.physical)
    {
        bool killed = base.TakeDamage(damage, 0);
        healthText.text = health + " / " + maxHealth;
        return killed;
    }

    public bool TestEnergy(float amount)
    {
        if (amount > 0)
        {
            return true;
        }
        else if(energy + amount >= 0)
        {
            energy += amount;
            return true;
        } else
        {
            ShowFloatingText("Not Enough Energy", Color.white, false);
            return false;
        }
    }

    public void GainEnergy(float amount)
    {
        energy = energy + amount > maxEnergy ? maxEnergy : energy + amount;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Item")
        {
            canPickUp = true;
        }
        if (pickUp && collision.tag == "Item" && inventoryManager.AddItem(collision.gameObject))
        {
            pickUp = false;
            canPickUp = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Item")
        {
            canPickUp = false;
        }
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
}
