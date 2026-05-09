using UnityEngine;

public class JammoMovement : MonoBehaviour
{
    [Header("Components")]
    public CharacterController controller;
    public Animator anim;
    public GameObject kickButton; // Kéo KickButton vào đây

    [Header("Movement Settings")]
    public float speed = 8f;
    public float gravity = -9.81f;
    public float turnTime = 0.1f;
    
    [Header("Detection Settings")]
    public float detectRadius = 2f; // Khoảng cách để hiện nút Kick

    float turnVelocity;
    Vector3 velocity;
    public float kickPower = 20f; // Lực sút

    void Update()
    {
        if (anim == null) return;

        HandleMovement();
        CheckForBall(); // Gọi hàm kiểm tra bóng mỗi khung hình
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
        // Tìm tất cả các vật thể có Tag là "Ball" trong một phạm vi hình cầu xung quanh Jammo
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

        // Hiện nút nếu có bóng gần, ẩn nếu không có
        if (kickButton != null)
        {
            kickButton.SetActive(ballNearby);
        }
    }

    // Vẽ vòng tròn đỏ trong Scene để bạn dễ quan sát phạm vi kích hoạt nút
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
    
    public void KickBall()
    {
        // 1. Tìm quả bóng gần Jammo nhất
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
            // 2. Tìm khung thành gần quả bóng đó nhất
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
                // 3. Thực hiện cú sút
                Rigidbody ballRb = targetBall.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    // Tính toán hướng từ bóng đến khung thành
                    // Chúng ta cộng thêm một chút độ cao (Vector3.up) để bóng bay bổng lên
                    Vector3 shootDirection = (targetGoal.transform.position - targetBall.transform.position).normalized;
                    shootDirection += Vector3.up * 0.2f; 

                    // Áp dụng lực tức thời (Impulse)
                    ballRb.AddForce(shootDirection * kickPower, ForceMode.Impulse);
                    
                    // Chạy animation sút (nếu có trong Animator của bạn)
                    // anim.SetTrigger("Kick"); 
                    
                    Debug.Log("Gooooal! Jammo đã sút bóng vào khung thành!");
                }
            }
        }
    }
}