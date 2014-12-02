using UnityEngine;

public class TileDmxBehaviour : MonoBehaviour {

    private Transform _reference;
    private float _tileWidth;
    private Material _materialOn;
    private Material _materialIdle;
    private bool _isOn;
    private bool _active;

	// Use this for initialization
	void Start () {
        // _reference = GameObject.Find("Reference").transform;
        _tileWidth = transform.localScale.x * 10;
	    _materialIdle = Config.MaterialWhite;
	    _materialOn = Config.MaterialOrange;
        renderer.material = _materialIdle;
	    _isOn = false;
	    _active = false;
	}
	
	// Update is called once per frame
	void Update () {
	    if (_active)
	    {
	        if ((_reference.position.x > transform.position.x - _tileWidth/2) && (_reference.position.x < transform.position.x + _tileWidth/2) && !_isOn)
	        {
	            renderer.material = _materialOn;
	            _isOn = true;
	        }
	        else if (((_reference.position.x < transform.position.x - _tileWidth/2) || (_reference.position.x > transform.position.x + _tileWidth/2)) && _isOn)
	        {
	            renderer.material = _materialIdle;
	            _isOn = false;
	        }
	    }
	    else
	    {
	        _reference = GameObject.Find("Reference").transform;
	        if (_reference != null)
	        {
	            _active = true;
	        }
	    }

	}
}
