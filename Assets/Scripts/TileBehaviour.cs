using UnityEngine;
using Random = UnityEngine.Random;

public class TileBehaviour : MonoBehaviour
{
    private float _tileWidth;
    private float _tileHeight;
    private Rect _positionRect;

	private Texture2D _footprintHuman;
	private Texture2D _footprintDino;
	private Texture2D _footprintDog;

    private Highlighttype _isHighlight;

    public bool ForceActive { get; set; }

    //Shake stuff
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private float _shakeDecay;
    private float _shakeIntensity;

    public Highlighttype Highlight
    {
        get { return _isHighlight; }
        set
        {
            if (value != _isHighlight)
            {
                switch (value)
                {
                    case Highlighttype.None:
                        renderer.material.SetColor("_Color", Config.ColorDefault);
                        break;
                    case Highlighttype.Occupied:
                        renderer.material.SetColor("_Color", Config.ColorOccupied);
                        break;
                    case Highlighttype.Hit:
                        renderer.material.SetColor("_Color", Config.ColorHit);
                        break;
                    case Highlighttype.Preview:
                        renderer.material.SetColor("_Color", Config.ColorPreview);
                        break;
                    case Highlighttype.Time:
                        renderer.material.SetColor("_Color", Config.ColorTime);
                        break;
                    case Highlighttype.Fail:
                        renderer.material.SetColor("_Color", Config.ColorFail);
                        break;
                }
            }
            _isHighlight = value;
        }
    }

    // Use this for initialization
    private void Start()
    {
        //_soundEmitter = findSoundEmitter(transform.position.y);


        _tileWidth = transform.localScale.x*10;
        _tileHeight = transform.localScale.y*10;
        _positionRect.x = transform.position.x - _tileWidth/2;
        _positionRect.y = transform.position.y - _tileWidth/2;
        _positionRect.width = _tileWidth;
        _positionRect.height = _tileHeight;

		_footprintHuman = (Texture2D) Resources.Load ("Textures/Footprints_human", typeof(Texture2D));
		_footprintDino = (Texture2D) Resources.Load ("Textures/Footprints_dino", typeof(Texture2D));
		_footprintDog = (Texture2D) Resources.Load ("Textures/Footprints_dog", typeof(Texture2D));

        var mat = new Material(Shader.Find(Config.ShaderType));

        renderer.material = mat;
        renderer.material.SetColor("_Color", Config.ColorDefault);
        Highlight = Highlighttype.None;

	    _originPosition = transform.position;
        _originRotation = transform.rotation;

        ForceActive = false;
    }

    // Update is called once per frame
    private void Update()
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

	/**
	 * Sets random Footprint texture.
	 **/
	public void handleFootprintTexture () {
		var randNumber = Random.Range (1, 4);

		switch (randNumber) {
			case 1:
				renderer.material.SetTexture ("_MainTex", _footprintHuman);
				break;
			case 2:
				renderer.material.SetTexture ("_MainTex", _footprintDino);
				break;
			case 3:
				renderer.material.SetTexture ("_MainTex", _footprintDog);
				break;
		}

		ForceActive = true;
	}

    private void OnMouseDown()
    {
        ForceActive = !ForceActive;
    }
}