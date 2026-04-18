using System.Collections;
using UnityEditor.Callbacks;
using UnityEngine;

public class PixelController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    public void SettupRigidbody()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
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

        Destroy(gameObject);
    }
}
