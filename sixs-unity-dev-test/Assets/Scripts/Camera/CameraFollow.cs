using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Kéo Jammo vào đây
    public Vector3 offset = new Vector3(0, 10, -5); // Khoảng cách so với nhân vật
    public float smoothSpeed = 0.125f;             // Độ mượt khi bám theo

    void LateUpdate() // Sử dụng LateUpdate để camera chạy sau khi nhân vật đã di chuyển
    {
        if (target == null) return;

        // Tính toán vị trí mong muốn
        Vector3 desiredPosition = target.position + offset;
        
        // Làm mượt chuyển động của camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        transform.position = smoothedPosition;

        // Luôn nhìn về phía nhân vật
        transform.LookAt(target);
    }
}