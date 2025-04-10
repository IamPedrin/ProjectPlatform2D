using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;

    public Transform attackPoint;
    public float attanckRange = 0.5f;
    public LayerMask enemyLayers;

    private InputAction attackInput;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {

    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Hit");

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attanckRange, enemyLayers);

            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("Acertou " + enemy.name);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attanckRange);
    }
}
