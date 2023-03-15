using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TroopPathfinding : MonoBehaviour
{
    [HideInInspector]
    public bool isMoving = false;

    [Header("Navigation Pathfinding")]
    public Agent agent;
    public string staticLayer = "Troop";
    public string activeLayer = "Troop Ignore";
    // public LayerMask ground;

    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string destinationControl;
    private InputAction _destination;

    [Header("Troop Selection")]
    public GameObject modelToHighlight;

    // [Header("Animation")]
    // public Animator animator;

    Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        UnitSelections.Instance.unitList.Add(this.gameObject);
    }

    // Update is called once per frame
    void OnDestroy()
    {
        UnitSelections.Instance.unitList.Remove(this.gameObject);
    }
    
    // Start is called before the first frame update
    private void Awake()
    {
        agent = GetComponent<Agent>();

        _playerInput = FindObjectOfType<PlayerInput>();
        _destination = _playerInput.actions[destinationControl];
    }
    private void Update()
    {
        // if (agent.remainingNodes <= 0.5)
        // {
        //     animator.SetBool("IsRunning", false);
        // }
    }

    private void OnEnable()
    {
        _destination.performed += OnRightClick;
        agent.startMovingEvent += StartMoving;
        agent.stopMovingEvent += StopMoving;
    }

    private void OnDisable()
    {
        _destination.performed -= OnRightClick;
        agent.startMovingEvent -= StartMoving;
        agent.stopMovingEvent -= StopMoving;
    }

    private void StartMoving()
    {
        agent.rigidbody.isKinematic = false;

        transform.GetComponentsInChildren<Collider>().ForEach(c => c.gameObject.layer = LayerMask.NameToLayer(activeLayer));
    }

    private void StopMoving()
    {
        agent.rigidbody.isKinematic = true;

        transform.GetComponentsInChildren<Collider>().ForEach(c => c.gameObject.layer = LayerMask.NameToLayer(staticLayer));
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (UnitSelections.Instance.unitsSelected.Contains(this.gameObject))
        {
            Vector3 mouse = Mouse.current.position.ReadValue(); // Get the mouse Position
            Ray castPoint = Camera.main.ScreenPointToRay(mouse); // Cast a ray to get where the mouse is pointing at
            RaycastHit hit; // Stores the position where the ray hit.
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, agent.groundLayer) && World.Instance != null) // If the raycast doesn't hit a wall
            {
                target = World.Instance.GetBlockPos(hit, true); // Move the target to the mouse position
                UnitSelections.Instance.FindNearestAvailiablePosition(agent, target);
                isMoving = true;
            }
        }
    }
}
