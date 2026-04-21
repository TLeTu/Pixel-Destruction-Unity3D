using System.Collections.Generic;
using UnityEngine;

public class SawController : MonoBehaviour,IWeaponController
{
    [SerializeField]
    private float damage = 10f;
    private Collider2D sawCollider;
    private ContactFilter2D contactFilter;
    private List<Collider2D> overlapResults = new List<Collider2D>();
    void Start()
    {
        sawCollider = GetComponent<Collider2D>();

        contactFilter = ContactFilter2D.noFilter;
        contactFilter.useTriggers = true;
    }
    void FixedUpdate()
    {
            DetectTargets();
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
