using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 5;
    public Rigidbody2D rb;

    void Start()
    {
        rb.linearVelocity = transform.right * speed;        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Enemy enemy = other.GetComponent<Enemy>();
        if(enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        Debug.Log(other.name);
        Destroy(gameObject);    
    }
}
