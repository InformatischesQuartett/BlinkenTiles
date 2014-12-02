using UnityEngine;

public class ReferenceBehaviour : MonoBehaviour
{
    private float _positionEnd;
    private float _speed;
    private Vector3 _vectorStart;
    private bool _active = false;

    // Use this for initialization
	void Start ()
	{
	    _speed = Config.Speed;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (_active)
	    {
	        if (transform.position.x < _positionEnd)
	        {
	            transform.Translate(_speed*Time.deltaTime, 0, 0);
	        }
	        else
	        {
	            transform.position = _vectorStart;
	        }
	    }
    }

    public void Init(float startPosition, float endPosition, float speed = 0)
    {
        _active = true;
        _positionEnd = endPosition;
        _vectorStart = new Vector3(startPosition, 0, 0);
        transform.position = _vectorStart;
        if (speed != 0)
        {
            _speed = speed;
            Config.Speed = speed;
        }
        else
        {
            _speed = Config.Speed;
        }
        
    }
}
