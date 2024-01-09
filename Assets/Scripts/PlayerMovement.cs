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
    RigidbodyConstraints2D originalConstraints;

    BoxCollider2D myCollider;

    PlayerInput playerInput;

    Animator myAnimator;

    bool isLanding = false;
    bool isFalling = false;
    bool isOnGround = true;

    // Physics Jump
    float gravity = 1f;
    float fallMultiplier = 7.5f;
    float linearDrag = 4f;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        originalConstraints = myRigidbody.constraints;
        myCollider = GetComponent<BoxCollider2D>();
        playerInput = GetComponent<PlayerInput>();
        myAnimator = GetComponent<Animator>();
    }

    void Update()
    {
    	OnGround();
    	ModifyPhysics();
    }

    void FixedUpdate()
    {
        Run();
        FlipSprite();
        Falling();
        Landing();
    }

    void OnGround()
    {
		if(myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
		{
			isOnGround = true;
		}
		else
		{
			isOnGround = false;
		}
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Landing()
    {
        if(isLanding)
        {
            myRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX;
        }
        else
        {
            myRigidbody.constraints = originalConstraints;
        }
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2 (moveInput.x * moveSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        float facing = Mathf.Sign(myRigidbody.velocity.x);

        if(playerHasHorizontalSpeed)
        {
            if(facing == 1f)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180f, 0);
            }
        }
    }

    void OnJump(InputValue value)
    {
        if(value.isPressed && isOnGround && !isLanding)
        {
            myRigidbody.velocity = new Vector2 (myRigidbody.velocity.x, 0f);
            myRigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            myAnimator.SetTrigger("jump");
            StartCoroutine(JumpSqueeze(1.25f, 0.8f, 0.05f));
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(!isOnGround)
        {
            isLanding = true;
            myAnimator.SetBool("isLanding", isLanding);
        }
    }

    void ModifyPhysics()
    {
    	if(isOnGround)
    	{
    		myRigidbody.gravityScale = 0f;
    		myRigidbody.drag = 0f;
    	}
    	else
    	{
    		myRigidbody.gravityScale = gravity;
    		myRigidbody.drag = linearDrag;

    		if(myRigidbody.velocity.y < 0)
    		{
    			myRigidbody.gravityScale = gravity * fallMultiplier;
    		}
            else if(myRigidbody.velocity.y > 0 && !playerInput.actions["jump"].IsPressed())
            {
                myRigidbody.gravityScale = gravity * (fallMultiplier * 1.7f);
            }
    	}
    }

    void Falling()
    {
        if(!isOnGround && myRigidbody.velocity.y < 0)
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
        if(isOnGround && isLanding)
        {
            isLanding = false;
            myAnimator.SetBool("isLanding", isLanding);
        }
    }

    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Debug.Log("Squeeze");
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while (t <= 1.5)
        {
            t += Time.deltaTime / seconds;
            myRigidbody.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1.5)
        {
            t += Time.deltaTime / seconds;
            myRigidbody.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }
    }

}
