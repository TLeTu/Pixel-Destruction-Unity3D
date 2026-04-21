using System;
using UnityEngine;
using UnityEngine.InputSystem; 

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    private Camera mainCam;
    private float damageRadius = 5f;
    private int maxTapDamage = 3;
    private int minTapDamage = 1;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        mainCam = Camera.main;
        GameManager.instance.OnGameStarted += EnableInput;
        GameManager.instance.OnGameResumed += EnableInput;
        GameManager.instance.OnMainMenu += DisableInput;
        GameManager.instance.OnGameWin += DisableInput;
        GameManager.instance.OnGamePaused += DisableInput;
        DisableInput();
    }

    void Update()
    {
        bool isTapped = false;
        Vector2 screenPosition = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            isTapped = true;
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isTapped = true;
            screenPosition = Mouse.current.position.ReadValue();
        }

        if (isTapped)
        {
            Vector2 worldPos = mainCam.ScreenToWorldPoint(screenPosition);
            
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);

            if (hitCollider != null)
            {
                PixelBlockController block = hitCollider.GetComponent<PixelBlockController>();
                if (block != null)
                {
                    block.HitAtPoint(worldPos, damageRadius, maxTapDamage, minTapDamage);
                }
            }
        }
    }
    public void SetTapDamage(float radius, int maxDamage, int minDamage)
    {
        damageRadius = radius;
        maxTapDamage = maxDamage;
        minTapDamage = minDamage;
    }
    private void EnableInput()
    {
        enabled = true;
    }

    private void DisableInput()
    {
        enabled = false;
    }
}