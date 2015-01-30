using UnityEngine;

public class CamController : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        transform.position = Config.CamPosition;
    }
}