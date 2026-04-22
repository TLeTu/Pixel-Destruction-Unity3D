using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ShredderController : MonoBehaviour, IWeaponController
{
    [SerializeField]
    private float damage = 1000f;

    private Collider2D shredderCollider;
    private ContactFilter2D contactFilter;
    private List<Collider2D> overlapResults = new List<Collider2D>();
    private bool pauseShredder = false;
    void Start()
    {
        shredderCollider = GetComponent<Collider2D>();

        contactFilter = ContactFilter2D.noFilter;
        contactFilter.useTriggers = true;
    }
    void FixedUpdate()
    {
        DetectTargets();
    }
    public void DetectTargets()
    {
        if (pauseShredder)
        {
            return;
        }
        // clear previous results
        overlapResults.Clear();
        int overlapCount = shredderCollider.Overlap(contactFilter, overlapResults);

        for (int i = 0; i < overlapCount; i++)
        {
            Collider2D col = overlapResults[i];

            PixelBlockController block = col.GetComponent<PixelBlockController>();
            if (block != null)
            {
                block.HitArea(shredderCollider, damage);
                continue;
            }

            PixelController pixel = col.GetComponent<PixelController>();
            if (pixel != null && !pixel.isReturningToPool)
            {
                pixel.InstaDestroy();
                ScoreManager.instance.UpdateScore(1);
            }
        }       
    }
    public void Pause(bool shouldPause)
    {
        pauseShredder = shouldPause;
    }
}