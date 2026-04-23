using System.Collections.Generic;
using UnityEngine;

public class SawController : MonoBehaviour, IWeaponController
{
    [SerializeField]
    private float damage = 10f;
    [SerializeField]
    private float damageTickRate = 0.1f;
    [SerializeField]
    private float rotationSpeed = 120f;
    [SerializeField]
    private float rotationSpeedUpgradeStep = 20f;
    [SerializeField]
    private float damageUpgradeStep = 15f;
    [SerializeField]
    private float damageTickRateUpgradeStep = 0.002f;
    [SerializeField]
    private float minDamageTickRate = 0.02f;
    [SerializeField]
    private float maxRangeScaleX = 7f;
    [SerializeField]
    private float rangeUpgradeStep = 0.5f;
    private Collider2D sawCollider;
    private ContactFilter2D contactFilter;
    private List<Collider2D> overlapResults = new List<Collider2D>();
    private float damageTimer = 0f;
    private bool pauseSaw = false;
    void Awake()
    {
        sawCollider = GetComponent<Collider2D>();

        contactFilter = ContactFilter2D.noFilter;
        contactFilter.useTriggers = true;
    }
    void FixedUpdate()
    {
        damageTimer += Time.fixedDeltaTime;
        if (damageTimer >= damageTickRate)
        {
            DetectTargets();
            damageTimer = 0f;
        }
    }
    void Update()
    {
        RotateSaw();
    }
    private void RotateSaw()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
    public void DetectTargets()
    {
        if (pauseSaw)
        {
            return;
        }
        overlapResults.Clear();
        int overlapCount = sawCollider.Overlap(contactFilter, overlapResults);

        for (int i = 0; i < overlapCount; i++)
        {
            Collider2D col = overlapResults[i];

            PixelBlockController block = col.GetComponent<PixelBlockController>();
            if (block != null)
            {
                block.HitArea(sawCollider, damage);
                continue;
            }
        }
    }
    public void Pause(bool shouldPause)
    {
        pauseSaw = shouldPause;
    }
    public void ApplyUpgrade(WeaponUpgrade upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgrade.Damage:
                damage += damageUpgradeStep;
                break;
            case WeaponUpgrade.Time:
                damageTickRate = Mathf.Max(minDamageTickRate, damageTickRate - damageTickRateUpgradeStep);
                rotationSpeed += rotationSpeedUpgradeStep;
                break;
            case WeaponUpgrade.Range:
                if (sawCollider.transform.localScale.x < maxRangeScaleX)
                {
                    sawCollider.transform.localScale += new Vector3(rangeUpgradeStep, rangeUpgradeStep, 0f);
                }
                break;
        }
    }
}
