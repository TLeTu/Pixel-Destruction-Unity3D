using System.Collections;
using UnityEditor.Callbacks;
using UnityEngine;

public class PixelController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D col;
    public void Detach()
    {
        col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.compositeOperation = Collider2D.CompositeOperation.None;
            col.size *= 0.8f;

        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
    }
    public void Attach()
    {
        if (col != null)
        {
            col.compositeOperation = Collider2D.CompositeOperation.Merge;
            col.size /= 0.8f;
        }

        if (rb != null)
        {
            Destroy(rb);
            rb = null;
        }
    }
    public void RevertState()
    {
        transform.localScale = Vector3.one;
        // Reset the rotation to default
        transform.localRotation = Quaternion.identity;
        
    }
    public void InstaDestroy()
    {
        StartCoroutine(ShrinkAndDestroy());
    }

    private IEnumerator ShrinkAndDestroy()
    {
        Vector3 startScale = transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        PoolManager.instance.ReturnToPool(gameObject);
    }
}
