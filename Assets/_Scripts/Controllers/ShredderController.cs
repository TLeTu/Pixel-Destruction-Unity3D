using UnityEngine;

public class ShredderController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("pixel"))
        {
            collision.gameObject.GetComponent<PixelController>()?.InstaDestroy();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("pixelBlock"))
        {
            PixelBlockController block = collision.gameObject.GetComponent<PixelBlockController>();
            if (block != null)
            {
                block.HitArea(GetComponent<Collider2D>().bounds);
            }
        }
    }
}
