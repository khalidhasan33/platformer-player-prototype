using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 20f;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    BoxCollider2D myCollider;

    bool isLanding = false;
    bool isFalling = false;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        Run();
        FlipSprite();
        Falling();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Run()
    {
        if(isLanding) { return;}

        Vector2 playerVelocity = new Vector2 (moveInput.x * moveSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if(playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2 (Mathf.Sign(myRigidbody.velocity.x), 1f);      
        }
    }

    void OnJump(InputValue value)
    {
        if(!myCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return;}

        if(value.isPressed)
        {
            myRigidbody.velocity += new Vector2 (0f, jumpSpeed);
            myAnimator.SetTrigger("jump");
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            isLanding = true;
            myAnimator.SetBool("isLanding", isLanding);
        }
    }

    void Falling()
    {
        if(!myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }
        myAnimator.SetBool("isFalling", isFalling);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if(myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            StartCoroutine(SlowWhileLanding(0.12f));
        }
    }

    IEnumerator SlowWhileLanding(float delay) {
        yield return new WaitForSeconds(delay);
        isLanding = false;
        myAnimator.SetBool("isLanding", isLanding);
    }

}
