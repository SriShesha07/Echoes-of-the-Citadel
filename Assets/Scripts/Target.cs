using UnityEngine;

public class Target : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void TakeDamage()
    {
        Debug.Log("Target was hit and destroyed!");

        Destroy(gameObject);
    }
}
