using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 _moveDirection;
    private Vector2 _lookDirection;
    private float _jumpDirection;
    
    public float moveSpeed = 2;
    public float maxForwardSpeed = 8;
    public float turnSpeed = 100;
    private float _desiredSpeed;
    private float _forwardSpeed;
    private float jumpSpeed = 30000f;
    private float groundRayDistance = 2f;
    private float _jumpEffort = 0;
    public float xSensitivity = 0.5f;
    public float ySensitivity = 0.5f;

    private const float GroundAccel = 5;
    private const float GroundDecel = 25;

    private Animator _anim;
    private Rigidbody _rigidBody;

    private bool _readyJump = false;
    private bool _onGround = true;

    public Transform weapon;
    public Transform hand;
    public Transform hip;
    public Transform spine;
    private Vector2 lastLookDirection;
    public void PickupGun()
    {
        weapon.SetParent(hand);
        weapon.localPosition = new Vector3(-0.02f,0.09f,0.1f);
        weapon.localRotation = Quaternion.Euler(-78.55f,-167.51f,291.32f);
        weapon.localScale = new Vector3(1, 1, 1);
    }

    public void PutDownGun()
    {
        weapon.SetParent(hip);
        weapon.localPosition = new Vector3(-0.13f,-0.13f,-0.01f);
        weapon.localRotation = Quaternion.Euler(-95.77f,-90.2f,264.52f);
        weapon.localScale = new Vector3(1, 1, 1);
    }
    
    bool IsMoveInput
    {
        get { return !Mathf.Approximately(_moveDirection.sqrMagnitude, 0f); }
    }
    
    // Called when move input key is pressed
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    // Called for deciding look direction
    public void OnLook(InputAction.CallbackContext context)
    {
        _lookDirection = context.ReadValue<Vector2>();
    }
    
    // Called when jump input key is pressed
    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpDirection = context.ReadValue<float>();
    }
    
    // Called when fire input key is pressed
    public void OnFire(InputAction.CallbackContext context)
    {
        if ((int)context.ReadValue<float>() == 1 && _anim.GetBool("Armed"))
        {
            _anim.SetTrigger("Fire");
        }
       
    }
    
    // Called when q input key is pressed
    public void OnArmed(InputAction.CallbackContext context)
    {
        _anim.SetBool("Armed",!_anim.GetBool("Armed"));
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

    void Jump(float direction)
    {
        if (direction > 0 && _onGround)
        {
            _anim.SetBool("ReadyJump", true);
            _readyJump = true;
            _jumpEffort += Time.deltaTime;
        }
        else if(_readyJump)
        {
            _anim.SetBool("Launch", true);
            _anim.SetBool("ReadyJump", false);
            _readyJump = false;
        }
        
    }

    public void Launch()
    {
        _anim.applyRootMotion = false;
        _rigidBody.AddForce(0, jumpSpeed * Mathf.Clamp(_jumpEffort,1,3), 0);
        _anim.SetBool("Launch", false);
    }

    public void Land()
    {
        _anim.SetBool("Land", false);
        _anim.applyRootMotion = true;
        _anim.SetBool("Launch", false);
        _jumpEffort = 0;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = this.GetComponent<Animator>();
        _rigidBody = this.GetComponent<Rigidbody>();
    }

    // Used to apply rotation to body after the animations
    private void LateUpdate()
    {
        //keep track of last look direction to apply later and stop animation stutter
        lastLookDirection += new Vector2( -_lookDirection.y * ySensitivity,_lookDirection.x * xSensitivity);
        lastLookDirection.x = Mathf.Clamp(lastLookDirection.x, -15, 15);
        lastLookDirection.y = Mathf.Clamp(lastLookDirection.y, -30, 60);
        spine.localEulerAngles = lastLookDirection;
    }

    // Update is called once per frame
    void Update()
    {
        Move(_moveDirection);
        Jump(_jumpDirection);

        // casting from character to ground in order to find out if we are in the air
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * groundRayDistance * 0.5f, -Vector3.up);
        if (Physics.Raycast(ray, out hit, groundRayDistance))
        {
            if (!_onGround)
            {
                _onGround = true;
                _anim.SetFloat("LandingVelocity",_rigidBody.velocity.magnitude);
                _anim.SetBool("Land", true);
                _anim.SetBool("Falling", false);
            }
        } else
        {
            _onGround = false;
            _anim.SetBool("Falling", true);
            _anim.applyRootMotion = false;
        }
        Debug.DrawRay(transform.position + Vector3.up * groundRayDistance * 0.5f, -Vector3.up * groundRayDistance, Color.red);
    }
}
