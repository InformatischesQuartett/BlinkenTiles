using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileBehaviour : MonoBehaviour
{
    private float _tileWidth;
    private float _tileHeight;
    private Rect _positionRect;

    private Material _materialHighlight;
    private Material _materialDefault;
    private Material _materialHit;
    
    //Shake stuff
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private float _shakeDecay;
    private float _shakeIntensity;

	// Use this for initialization
	void Start ()
	{
        //_soundEmitter = findSoundEmitter(transform.position.y);

        _tileWidth = transform.localScale.x*10;
	    _tileHeight = transform.localScale.y*10;
        _positionRect.x = transform.position.x - _tileWidth / 2;
        _positionRect.y = transform.position.y - _tileWidth / 2;
        _positionRect.width = _tileWidth;
        _positionRect.height = _tileHeight;
        
        _materialDefault = Config.MaterialWhite;
	    _materialHighlight = Config.MaterialYellow;
	    _materialHit = Config.MaterialRed;
        renderer.material = _materialDefault;

        _originPosition = transform.position;
        _originRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
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

    public void Shake()
    {
        _shakeIntensity = 0.8f;
        _shakeDecay = 0.015f;
    }
}
