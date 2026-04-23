using System.Collections;
using UnityEngine;

public class PixelController : MonoBehaviour
{
    private Coroutine shrinkRoutine;
    public bool isReturningToPool = false;
    private Vector3 initialLocalScale;

    private void Awake()
    {
        initialLocalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = initialLocalScale;
        isReturningToPool = false;
        if (shrinkRoutine != null)
        {
            StopCoroutine(shrinkRoutine);
            shrinkRoutine = null;
        }
    }

    private void OnDisable()
    {
        if (shrinkRoutine != null)
        {
            StopCoroutine(shrinkRoutine);
            shrinkRoutine = null;
        }
        isReturningToPool = false;
    }

    public void InstaDestroy()
    {
        if (isReturningToPool || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            return;

        isReturningToPool = true;

        if (shrinkRoutine != null)
        {
            StopCoroutine(shrinkRoutine);
        }

        shrinkRoutine = StartCoroutine(ShrinkAndDestroy());
    }

    private IEnumerator ShrinkAndDestroy()
    {
        Vector3 startScale = transform.localScale;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        shrinkRoutine = null;
        isReturningToPool = false;

        PoolManager.instance.ReturnToPool(gameObject, false);
        
    }

}
