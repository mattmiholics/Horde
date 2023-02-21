using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TroopPathfinding : MonoBehaviour
{
    [Header("Navigation Pathfinding")]
    public Agent agent;
    // public LayerMask ground;

    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string destinationControl;
    private InputAction _destination;

    [Header("Animation")]
    public Animator animator;

    Vector3 target;
    
    // Start is called before the first frame update
    private void Awake()
    {
        agent = GetComponent<Agent>();

        _playerInput = FindObjectOfType<PlayerInput>();
        _destination = _playerInput.actions[destinationControl];
    }
    private void Update()
    {
        if (agent.remainingNodes <= 0.5)
        {
            animator.SetBool("IsRunning", false);
        }
    }

    private void OnEnable()
    {
        _destination.performed += OnRightClick;
    }

    private void OnDisable()
    {
        _destination.performed -= OnRightClick;
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        Vector3 mouse = Mouse.current.position.ReadValue(); // Get the mouse Position
        Ray castPoint = Camera.main.ScreenPointToRay(mouse); // Cast a ray to get where the mouse is pointing at
        RaycastHit hit; // Stores the position where the ray hit.
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, agent.groundLayer)) // If the raycast doesn't hit a wall
        {
            target = hit.point; // Move the target to the mouse position
            agent.SetTarget(target);
            animator.SetBool("IsRunning", true);
        }
    }
}
