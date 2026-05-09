using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JammoMovement : MonoBehaviour
{
    [Header("Components")]
    public CharacterController controller;
    public Animator anim;
    public GameObject kickButton; 

    [Header("Movement Settings")]
    public float speed = 8f;
    public float gravity = -9.81f;
    public float turnTime = 0.1f;
    
    [Header("Detection Settings")]
    public float detectRadius = 2f; // Distance for kick
    
    [Header("Camera Control")]
    public Camera mainCamera; 
    public Vector3 offsetToBall = new Vector3(0, 5, -10); 
    public Vector3 offsetToPlayer = new Vector3(0, 5, -10); 
    public float cameraFollowSpeed = 5f;

    // Following state
    private Transform currentTarget; 
    private Vector3 currentOffset;
    private bool isTrackingBall = false;
    
    float turnVelocity;
    Vector3 velocity;
    public float kickPower = 20f; // Power

    void Start()
    {
        currentTarget = this.transform; 
        currentOffset = offsetToPlayer;
    }
    
    void Update()
    {
        if (anim == null) return;

        HandleMovement();
        CheckForBall(); 
    }

    private void LateUpdate()
    {
        if (mainCamera != null && currentTarget != null)
        {
            // Calculate position
            Vector3 desiredPosition = currentTarget.position + currentOffset;
            
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, cameraFollowSpeed * Time.deltaTime);
            
            mainCamera.transform.LookAt(currentTarget.position);
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        anim.SetFloat("Blend", Mathf.Lerp(anim.GetFloat("Blend"), direction.magnitude, Time.deltaTime * 10f));

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void CheckForBall()
    {
        // Check tag ball
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        bool ballNearby = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball"))
            {
                ballNearby = true;
                break;
            }
        }

        // Hide or display button
        if (kickButton != null)
        {
            kickButton.SetActive(ballNearby);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
    
    public void KickBall()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        GameObject targetBall = null;
        float minDistanceBall = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Ball"))
            {
                float dist = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (dist < minDistanceBall)
                {
                    minDistanceBall = dist;
                    targetBall = hitCollider.gameObject;
                }
            }
        }

        if (targetBall != null)
        {
            GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
            GameObject targetGoal = null;
            float minDistanceGoal = float.MaxValue;

            foreach (GameObject goal in goals)
            {
                float dist = Vector3.Distance(targetBall.transform.position, goal.transform.position);
                if (dist < minDistanceGoal)
                {
                    minDistanceGoal = dist;
                    targetGoal = goal;
                }
            }

            if (targetGoal != null)
            {
                Rigidbody ballRb = targetBall.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector3 shootDirection = (targetGoal.transform.position - targetBall.transform.position).normalized;
                    shootDirection += Vector3.up * 0.2f; 
                    
                    ballRb.AddForce(shootDirection * kickPower, ForceMode.Impulse);
                    
                    StartCoroutine(TrackBallAndReturn(targetBall.transform));
                }
            }
        }
    }
    
    public void AutoKickBall()
    {
        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("Ball");
        
        if (allBalls.Length == 0) return; 

        GameObject farthestBall = null;
        float maxDistance = -1f;
        
        foreach (GameObject ball in allBalls)
        {
            float dist = Vector3.Distance(transform.position, ball.transform.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                farthestBall = ball;
            }
        }

        if (farthestBall != null)
        {
            GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
            GameObject targetGoal = null;
            float minGoalDist = float.MaxValue;

            foreach (GameObject goal in goals)
            {
                float dist = Vector3.Distance(farthestBall.transform.position, goal.transform.position);
                if (dist < minGoalDist)
                {
                    minGoalDist = dist;
                    targetGoal = goal;
                }
            }

            if (targetGoal != null)
            {
                Rigidbody ballRb = farthestBall.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector3 shootDirection = (targetGoal.transform.position - farthestBall.transform.position).normalized;
                    
                    shootDirection += Vector3.up * 0.25f;
                    
                    ballRb.AddForce(shootDirection * (kickPower * 1.2f), ForceMode.Impulse);
                    
                    StartCoroutine(TrackBallAndReturn(farthestBall.transform));
                }
            }
        }
    }
    
    private IEnumerator TrackBallAndReturn(Transform ballTransform)
    {
        isTrackingBall = true;
        
        currentTarget = ballTransform;
        currentOffset = offsetToBall;
        
        yield return new WaitForSeconds(2f);
        
        currentTarget = this.transform;
        currentOffset = offsetToPlayer;
        isTrackingBall = false;
    }
}