using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MovePlayerNew : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        
        if (agent.remainingDistance <= 0.5)
        {
           animator.SetBool("IsRunning", false);
        }

    }

    public void MoveToDestination(Vector3 destination)
    {
        animator.SetBool("IsRunning", true);
        agent.SetDestination(destination);

    }
}
