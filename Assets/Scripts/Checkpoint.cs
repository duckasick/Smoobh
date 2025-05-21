using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform resetPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerScript1 ps1 = other.gameObject.GetComponent<PlayerScript1>();
            ps1.Reset(resetPoint);
        }
    }
}
