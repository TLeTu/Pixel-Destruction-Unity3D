using System.Collections.Generic;
using UnityEngine;

public class ShredderController : MonoBehaviour
{
    private Collider2D shredderCollider;
    private ContactFilter2D contactFilter;
    private List<Collider2D> overlapResults = new List<Collider2D>();
    private void Start()
    {
        shredderCollider = GetComponent<Collider2D>();

        contactFilter = ContactFilter2D.noFilter;
        contactFilter.useTriggers = true;
    }
    private void FixedUpdate()
    {
        int overlapCount = shredderCollider.Overlap(contactFilter, overlapResults);

        for (int i = 0; i < overlapCount; i++)
        {
            Collider2D col = overlapResults[i];

            if (col.gameObject.CompareTag("pixel"))
            {
                col.gameObject.GetComponent<PixelController>()?.InstaDestroy();
            }
            else if (col.gameObject.CompareTag("pixelBlock"))
            {
                PixelBlockController block = col.gameObject.GetComponent<PixelBlockController>();
                if (block != null)
                {
                    block.HitArea(shredderCollider.bounds);
                }
            }
        }
    }
}
