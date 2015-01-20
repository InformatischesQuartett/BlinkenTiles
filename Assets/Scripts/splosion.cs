using UnityEngine;

public class splosion : MonoBehaviour
{

    private float _lifetime;

	// Use this for initialization
	void Start ()
	{
	    _lifetime = 3;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    _lifetime -= Time.deltaTime;

        if (_lifetime < 0)
            Destroy(gameObject);
	}
}
