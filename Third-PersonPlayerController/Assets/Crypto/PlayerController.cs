using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 _moveDirection;
    public float moveSpeed = 2;
    public float maxForwardSpeed = 8;
    public float turnSpeed = 100;
    private float _desiredSpeed;
    private float _forwardSpeed;

    private const float GroundAccel = 5;
    private const float GroundDecel = 25;

    private Animator _anim;

    bool IsMoveInput
    {
        get { return !Mathf.Approximately(_moveDirection.sqrMagnitude, 0f); }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    void Move(Vector2 direction)
    {
        float turnAmount = direction.x;
        float fDirection = direction.y;
        
        if (direction.sqrMagnitude > 1f)
        {
            // Normalizes to 1 if value is not between 0 and 1
            direction.Normalize();
        }
        
        // Get the sign to decide forward or backward
        _desiredSpeed = direction.magnitude * maxForwardSpeed * Mathf.Sign(fDirection);
        
        // if move input is in use accel otherwise decel
        float acceleration = IsMoveInput ? GroundAccel : GroundDecel;

        // smoothly accelerate or decelerate
        _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, _desiredSpeed, acceleration * Time.deltaTime);
        _anim.SetFloat("ForwardSpeed", _forwardSpeed);

        transform.Rotate(0,turnAmount * turnSpeed * Time.deltaTime,0);
        
        //transform.Translate(direction.x * moveSpeed * Time.deltaTime, 0, direction.y * moveSpeed * Time.deltaTime);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move(_moveDirection);
    }
}
