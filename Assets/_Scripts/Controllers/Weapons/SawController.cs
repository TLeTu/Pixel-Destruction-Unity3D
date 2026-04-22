using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SawController : MonoBehaviour,IWeaponController
{
    [SerializeField]
    private float damage = 10f;
    [SerializeField]
    private float damageTickRate = 0.1f;
    private Collider2D sawCollider;
    private ContactFilter2D contactFilter;
    private List<Collider2D> overlapResults = new List<Collider2D>();
    private float damageTimer = 0f;
    void Start()
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
        transform.Rotate(Vector3.forward, 300f * Time.deltaTime);
    }
    public void DetectTargets()
    {
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
}
