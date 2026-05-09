using UnityEngine;

public class GoalPoint : UnityEngine.MonoBehaviour
{
    public GameObject confettiPrefab; 
    public float destroyDelay = 5f;  

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            GameObject effect = Instantiate(confettiPrefab, transform.position, Quaternion.identity);
            
            Destroy(effect, destroyDelay);
            
            Debug.Log("GOAL!");
        }
    }
}