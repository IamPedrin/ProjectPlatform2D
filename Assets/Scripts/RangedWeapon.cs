using UnityEngine;
using UnityEngine.InputSystem;

public class RangedWeapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
