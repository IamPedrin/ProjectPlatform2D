using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Components
    private Rigidbody2D rb;

    private InputAction jumpWasPressed;
    //
    [Header("Movement")]
    private float moveInput; //Recebe o input do jogador se movimentando
    public float moveSpeed = 5f; //Velocidade maxima que o jogador pode alcançar
    public float acceleration;
    public float decceleration;
    public float velPower;
    [Range(0, 1)] public float frictionAmount;

    [Header("Jump")]
    public float jumpForce = 10f;
    public int maxJumps = 1;
    private int jumpsRemaining;
    [Range(0, 0.9f)] public float jumpCutMultiplier;
    public float coyoteTime;
    public float jumpInputBufferTimer;
    bool isJumping = false;

    [Header("Gravity")]
    public float baseGravity;
    public float fallGravityMultiplier;
    public float maxFallSpeed;



    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;

    //Variaveis de controle
    public bool IsFacingRight { get; private set; }
    public float LastOnGroundTime { get; private set; }
    public float LastJumpTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsFacingRight = true;
    }

    void Update()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastJumpTime -= Time.deltaTime;
        Gravity();
        GroundCheck();
        Friction();
        FlipSprite();

    }
    private void FixedUpdate()
    {
        Movement();
    }

    //Função para movimentação horizontal do jogador utilizando o novo InputSystem
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().x;
    }

    //Melhorando a movimentação
    private void Movement()
    {
        //Calcula a direcao e valocidade maxima que queremos
        float targetSpeed = moveInput * moveSpeed;
        //Calcula a diferença da velocidade atual e a velocidade esperada
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        //Modifica o ritmo da aceleraçao dependendo da situação
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        //Aplica a aceleracao na diferenca de velocidade,
        float movement = (float)Math.Pow(Mathf.Abs(speedDiff) * accelerationRate, velPower) * Mathf.Sign(speedDiff);

        //Aplica força ao RigidBody, para o Vector2.right para ele fazer efeito no eixo x
        rb.AddForce(movement * Vector2.right);
    }

    private void Friction()
    {
        if (LastOnGroundTime > 0 && Mathf.Abs(moveInput) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.linearVelocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    //Função para inverter o sprite do jogador de acordo com sua direção
    private void FlipSprite()
    {
        //Verifica se esta olhando para a direita e tem um movimento para a esquerda
        //Ou se não está olhando e tem um movimento para a direita
        if (IsFacingRight && moveInput < 0 || !IsFacingRight && moveInput > 0)
        {
            IsFacingRight = !IsFacingRight;
            Vector3 ls = transform.localScale; //Pega o localscale do player
            ls.x *= -1; //Multiplica por -1 para inverter o sprite no eixo x
            transform.localScale = ls; //O localscale do player recebe o novo localScale ja 
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        
        if (jumpsRemaining > 0 && LastOnGroundTime > 0)
        {
            if (context.performed)
            {
                OnJump();
                LastJumpTime = jumpInputBufferTimer;

            }
            else if (context.canceled)
            {
                OnJumpCut();
                LastJumpTime = jumpInputBufferTimer;
            }
        }
    }

    private void OnJump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        jumpsRemaining--;
        LastOnGroundTime = 0;
        LastJumpTime = 0;
    }

    private void OnJumpCut()
    {
        rb.AddForce(Vector2.down * rb.linearVelocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        isJumping = true;
        jumpsRemaining--;
        LastJumpTime = 0;
        LastOnGroundTime = 0;
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            isGrounded = true;
            isJumping = false;
            jumpsRemaining = maxJumps;
            LastOnGroundTime = coyoteTime;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallGravityMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));

        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }

}
