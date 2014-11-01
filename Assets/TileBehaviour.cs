using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileBehaviour : MonoBehaviour
{

    private Transform _reference;
    private float _tileWidth;
    private float _tileHeight;
    private Material _materialYellow;
    private Material _materialWhite;
    private Material _materialRed;
    private Boolean _isOn;
    private Transform _people;
    private Rect _positionRect;

	// Use this for initialization
	void Start ()
	{
	    _reference = GameObject.Find("Reference").transform;
	    _people = GameObject.Find("People").transform;
	    _tileWidth = transform.localScale.x*10;
	    _tileHeight = transform.localScale.y*10;
        _materialWhite = new Material(Shader.Find("Sprites/Default"));
        _materialWhite.SetColor("_Color", new Color(1,1,1));
        _materialYellow = new Material(Shader.Find("Sprites/Default"));
        _materialYellow.SetColor("_Color", new Color(1, 1, 0));
        _materialRed = new Material(Shader.Find("Sprites/Default"));
        _materialRed.SetColor("_Color", new Color(1, 0, 0));
        renderer.material = _materialWhite;
	    _positionRect.x = transform.position.x - _tileWidth/2;
	    _positionRect.y = transform.position.y - _tileWidth/2;
	    _positionRect.width = _tileWidth;
	    _positionRect.height = _tileHeight;
        _isOn = false;
        _originPosition = transform.position;
        _originRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if ((_reference.position.x > transform.position.x - _tileWidth/2) && (_reference.position.x < transform.position.x + _tileWidth/2) && !_isOn)
	    {
	        renderer.material = _materialYellow;
	        _isOn = true;
	        for (int i = 0; i < _people.transform.childCount; i++)
	        {
	            Transform child = _people.GetChild(i);
	            if (_positionRect.Contains(child.position))
	            {
	                renderer.material = _materialRed;
                    Shake();
                    break;
	            }
	        }
	    }
        else if (((_reference.position.x < transform.position.x - _tileWidth / 2) || (_reference.position.x > transform.position.x + _tileWidth / 2)) && _isOn)
	    {
	        renderer.material = _materialWhite;
	        _isOn = false;
	    }

	    if (_shakeIntensity > 0)
	    {
	        transform.position = _originPosition + Random.insideUnitSphere*_shakeIntensity;
	        transform.rotation = new Quaternion(
	            _originRotation.x + Random.Range(-_shakeIntensity, _shakeIntensity)*0.1f,
	            _originRotation.y + Random.Range(-_shakeIntensity, _shakeIntensity)*0.1f,
	            _originRotation.z + Random.Range(-_shakeIntensity, _shakeIntensity)*0.1f,
	            _originRotation.w + Random.Range(-_shakeIntensity, _shakeIntensity)*0.1f);
	        _shakeIntensity -= _shakeDecay;
	    }
	    else
	    {
	        transform.position = _originPosition;
	        transform.rotation = _originRotation;
	    }

	}

    // Shake stuff

    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private float _shakeDecay;
    private float _shakeIntensity;

    public void Shake()
    {
        _shakeIntensity = 0.8f;
        _shakeDecay = 0.015f;
    }
}
