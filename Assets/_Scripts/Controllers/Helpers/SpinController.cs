using UnityEngine;

public class SpinController : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.forward, 300f * Time.deltaTime);

    }
}
