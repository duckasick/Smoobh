using UnityEngine;

public class DeleteScript : MonoBehaviour
{
    public PlayerScript1 ps1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            ps1.Kill();
            Destroy(this.transform.parent.gameObject);
        }
    }
}
