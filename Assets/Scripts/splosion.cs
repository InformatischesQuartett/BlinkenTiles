using UnityEngine;

public class splosion : MonoBehaviour
{
    private float _lifetime;

    // Use this for initialization
    private void Start()
    {
        _lifetime = 3;
    }

    // Update is called once per frame
    private void Update()
    {
        _lifetime -= Time.deltaTime;

        if (_lifetime < 0)
            Destroy(gameObject);
    }
}