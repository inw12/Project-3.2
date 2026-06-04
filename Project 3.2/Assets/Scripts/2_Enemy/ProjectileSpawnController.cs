using UnityEngine;
public class ProjectileSpawnController : MonoBehaviour
{
    [SerializeField] private Transform attachTo;

    void Update()
    {
        transform.SetPositionAndRotation(attachTo.position, attachTo.rotation);
    } 
}
