using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovementPedrin : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("InputAction")]
    public InputActionReference movementReference;
    public InputActionReference jumpReference;

    [Header("Movement")]
    public float moveSpeed = 5f; //Velocidade maxima que o jogador pode alcançar
    public float acceleration;
    public float decceleration;
    public float velPower;
    [Range(0, 1)] public float frictionAmount;
    private Vector2 moveInput; //Recebe o input do jogador se movimentando

    [Header("Jump")]
    public float jumpForce = 10f;
    public int maxJumps = 2;
    [Range(0, 0.9f)] public float jumpCutMultiplier;
    public float coyoteTime = 0.2f;
    public float jumpBufferTimer = 0.2f;
    private int remainingJumps;

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
    //Variaveis de controle e tempo
    public bool IsFacingRight { get; private set; }
    public float LastGroundTime { get; private set; }
    public float CoyoteTimer { get; private set; }
    public float JumpBufferCounter { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsFacingRight = true;
    }

    void Update()
    {
        GroundCheck();
        TimersCheck();
        FlipSprite();
    }

    void FixedUpdate()
    {
        Movement();
        InitiateJump();
        Friction();
        Gravity();
    }

    #region Movement
    public void MovementInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Movement()
    {
        //Calcula a direcao e valocidade maxima que queremos
        float targetSpeed = moveInput.x * moveSpeed;
        //Calcula a diferença da velocidade atual e a velocidade esperada
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        //Modifica o ritmo da aceleraçao dependendo da situação
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        //Aplica a aceleracao na diferenca de velocidade,
        float movement = (float)Math.Pow(Mathf.Abs(speedDiff) * accelerationRate, velPower) * Mathf.Sign(speedDiff);

        //Aplica força ao RigidBody, para o Vector2.right para ele fazer efeito no eixo x
        rb.AddForce(movement * Vector2.right);
    }


    //Função para implementar um mecanismo de atrito e desacelerar o jogador enquanto estiver no chao
    private void Friction()
    {
        if (LastGroundTime > 0 && Mathf.Abs(moveInput.x) < 0.01f)
        {
            //Quantidade de força de atrito será aplicado no jgogador
            float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), Mathf.Abs(frictionAmount));
            //Ajusta o sinal para ser oposto a direção do movimento
            amount *= Mathf.Sign(rb.linearVelocity.x);
            //Aplica força ao rigidBody e cria um vetor na direção oposta ao que está o movimento
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    #endregion

    #region Jump

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpBufferCounter = jumpBufferTimer;
        }
        else if (context.canceled)
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector2.down * rb.linearVelocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
            }
            LastGroundTime = 0;
        }
    }

    private void InitiateJump()
    {
        if (JumpBufferCounter > 0 && ((isGrounded || CoyoteTimer > 0f) || (isJumping && remainingJumps > 0)))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            JumpBufferCounter = 0f;
            CoyoteTimer = 0f;
            isJumping = true;
            remainingJumps--;
        }
    }

    #endregion

    #region Gravity

    //Função para aumentar a gravidade enquanto o jogador está caindo
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

    #endregion

    #region Checkers

    //Verifica se o jogador esta em colisão com o chão
    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            isGrounded = true;
            isJumping = false;
            remainingJumps = maxJumps;
        }
        else
        {
            isGrounded = false;
        }

    }

    private void TimersCheck()
    {

        LastGroundTime = Mathf.Max(0, LastGroundTime - Time.deltaTime);
        JumpBufferCounter = Mathf.Max(0, JumpBufferCounter - Time.deltaTime);

        #region Coyote Checkers
        if (!isGrounded)
        {
            CoyoteTimer = Mathf.Max(0, CoyoteTimer - Time.deltaTime);
        }
        else
        {
            CoyoteTimer = coyoteTime;
        }

        if (jumpReference.action.WasReleasedThisFrame())
        {
            CoyoteTimer = 0f;
        }
        #endregion
    }
    #endregion

    #region Auxiliar
    private void FlipSprite()
    {
        //Verifica se esta olhando para a direita e tem um movimento para a esquerda
        //Ou se não está olhando e tem um movimento para a direita
        if (IsFacingRight && moveInput.x < 0 || !IsFacingRight && moveInput.x > 0)
        {
            IsFacingRight = !IsFacingRight;
            Vector3 ls = transform.localScale; //Pega o localscale do player
            ls.x *= -1; //Multiplica por -1 para inverter o sprite no eixo x
            transform.localScale = ls; //O localscale do player recebe o novo localScale ja 
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }
    #endregion
}