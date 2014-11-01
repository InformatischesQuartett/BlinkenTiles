using UnityEngine;
using System.Collections;

public class ReferenceBehaviour : MonoBehaviour
{

    public float PositionStart;
    public float PositionEnd;
    public float Speed;
    private Vector3 _vectorStart;

	// Use this for initialization
	void Start ()
	{
        _vectorStart = new Vector3(PositionStart, 0, 0);
	    transform.position = _vectorStart;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (transform.position.x < PositionEnd)
	    {
            transform.Translate(Speed * Time.deltaTime, 0, 0);
	    }
	    else
	    {
	        transform.position = _vectorStart;
	    }
	}
}
