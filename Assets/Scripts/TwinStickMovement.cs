using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class TwinStickMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float gravityValue = -2f;
    [SerializeField] private float controllerDeadzone = 0.1f;
    [SerializeField] private float gamepadRotateSmoothing = 1000f;

    [SerializeField] private bool isGamepad;

    private CharacterController controller;

    [SerializeField] private Rigidbody rbPlayer;
    [SerializeField] private GameObject player;
    private Vector2 movement;
    private Vector2 aim;

    private Vector3 playerVelocity;

    private PlayerControls playerControls;
    private PlayerInput playerInput;

    private float dodgeTime = 30f;
    private float dodgeLength = 20f;
    private bool canDodge = true;
    [SerializeField] private float dodgeSpeed = 50f;

    public float dashSpeed;
    public float dashTime;
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerControls = new PlayerControls();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleRotation();
    }
    
    void HandleInput()
    {
        movement = playerControls.Controls.Movement.ReadValue<Vector2>();
        aim = playerControls.Controls.Aim.ReadValue<Vector2>();
        
    }

    void HandleMovement()
    {
        Vector3 move = new Vector3(movement.x, 0, movement.y);
        controller.Move(move * Time.deltaTime * playerSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
        
    }
    public void DodgeAction()
    {
        if (canDodge == true)
        {
            StartCoroutine(DodgeTimer());
        }
    }
    
    void OnDodge(InputValue Dodge)
    {
        if (Dodge.isPressed )
        {
            DodgeAction();
        }
    }
    
    private IEnumerator DodgeTimer()
    {
        Debug.Log("DodgeAction");

        // Only problem with this: It dashes towards location you're facing, not the location you're moving
        
        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            transform.Translate(Vector3.forward * dodgeSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        canDodge = false;
        yield return new WaitForSeconds(2f);
        canDodge = true;    
          
        
    }

    void HandleRotation()
    {
        if (isGamepad)
        {
            if (Mathf.Abs(aim.x) > controllerDeadzone || Mathf.Abs(aim.y) > controllerDeadzone)
            {
                Vector3 playerDirection = Vector3.right * aim.x + Vector3.forward * aim.y;
                if (playerDirection.sqrMagnitude > 0.0f)
                {
                    Quaternion newrotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newrotation,
                        gamepadRotateSmoothing * Time.deltaTime);
                }
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(aim);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                LookAt(point);
            }
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
    public void OnDeviceChange(PlayerInput pi)
    {
        isGamepad = pi.currentControlScheme.Equals("Gamepad") ? true : false;
    }
    
    
}
