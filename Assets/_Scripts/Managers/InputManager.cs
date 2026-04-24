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
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        CheckTapInput();
    }
    public void SetTapDamage(float radius, int maxDamage, int minDamage)
    {
        damageRadius = radius;
        maxTapDamage = maxDamage;
        minTapDamage = minDamage;
    }
    public void EnableInput()
    {
        enabled = true;
    }

    public void DisableInput()
    {
        enabled = false;
    }
    public void CheckTapInput()
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
                if (GameManager.instance.gameState == GameState.Playing)
                {
                    PixelBlockController block = hitCollider.GetComponent<PixelBlockController>();
                    if (block != null)
                    {
                        block.HitAtPoint(worldPos, damageRadius, maxTapDamage, minTapDamage);
                    }
                }
            }
        }
    }
}