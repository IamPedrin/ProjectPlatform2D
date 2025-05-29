using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;

    public Transform attackPoint;
    public float attanckRange = 0.5f;
    public LayerMask enemyLayers;

    [SerializeField] private int damage;
    private float timeBtwAttack;
    [SerializeField] private float startingTimeBtwAtk;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(timeBtwAttack > 0)
        {
            timeBtwAttack -= Time.deltaTime;
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && timeBtwAttack <= 0f)
        {
            Debug.Log("Hit");
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attanckRange, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<Enemy>().TakeDamage(damage);
            }
            timeBtwAttack = startingTimeBtwAtk;
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attanckRange);
    }
}
