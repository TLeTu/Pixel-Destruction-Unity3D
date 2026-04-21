using UnityEngine;

public class SawController : MonoBehaviour, IWeaponController
{
    public float damage = 10f;
    public float range = 1f;
    public LayerMask targetLayer;

    private void Update()
    {
        DetectTargets();
    }

    public void DetectTargets()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, range, targetLayer);
        foreach (Collider2D target in hitTargets)
        {
            // Apply damage to the target
            Debug.Log("Hit target: " + target.name);
            // Here you would typically call a method on the target to apply damage
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}