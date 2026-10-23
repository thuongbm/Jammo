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

            // Prefab has playOnAwake = false, so start every particle system manually
            foreach (ParticleSystem ps in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Play();
            }

            Destroy(effect, destroyDelay);

            Debug.Log("GOAL!");
        }
    }
}