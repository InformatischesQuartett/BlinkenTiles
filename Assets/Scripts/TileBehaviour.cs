using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public GameObject ExplosionGo;
    private Vector3 _explosionOffset;
    private Texture2D _footprintDino;
    private Texture2D _footprintDog;
    private Texture2D _footprintHuman;
    private Texture2D _mediaCampLogo;

    private Highlighttype _isHighlight;

    //Shake stuff
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private Rect _positionRect;
    private float _shakeDecay;
    private float _shakeIntensity;
    private Texture2D _tileBorder;
    private float _tileHeight;
    private float _tileWidth;
    public bool ForceActive { get; set; }

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
                        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorDefault);
                        break;
                    case Highlighttype.Occupied:
                        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorOccupied);
                        break;
                    case Highlighttype.Hit:
                        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorHit);
                        break;
                    case Highlighttype.Preview:
                        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorPreview);
                        break;
                    case Highlighttype.Time:
                        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorTime);
                        break;
                    case Highlighttype.Fail:
                        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorFail);
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

        _footprintHuman = (Texture2D) Resources.Load("Textures/Footprints_human", typeof (Texture2D));
        _footprintDino = (Texture2D) Resources.Load("Textures/Footprints_dino", typeof (Texture2D));
        _footprintDog = (Texture2D) Resources.Load("Textures/Footprints_dog", typeof (Texture2D));
		_mediaCampLogo = (Texture2D) Resources.Load("Textures/mediacamp", typeof (Texture2D));

        _tileBorder = (Texture2D) Resources.Load("Textures/Tile_border", typeof (Texture2D));

        var mat = new Material(Shader.Find(Config.ShaderType));

        GetComponent<Renderer>().material = mat;
        GetComponent<Renderer>().material.SetColor("_Color", Config.ColorDefault);
        Highlight = Highlighttype.None;

        _originPosition = transform.position;
        _originRotation = transform.rotation;

        _explosionOffset = new Vector3(0, 0, -2);

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

        var splosion =
            Instantiate(ExplosionGo, transform.position + _explosionOffset, Quaternion.identity) as GameObject;
        splosion.transform.parent = transform;
    }

    public void SetBorder()
    {
        GetComponent<Renderer>().material.SetTexture("_MainTex", _tileBorder);
    }

    /**
	 * Sets random Footprint texture.
	 **/

    public void SetFootprint()
    {
        int randNumber = Random.Range(1, 6);

        switch (randNumber)
        {
            case 1:
                GetComponent<Renderer>().material.SetTexture("_MainTex", _footprintHuman);
                break;
            case 2:
                GetComponent<Renderer>().material.SetTexture("_MainTex", _footprintDino);
                break;
            case 3:
                GetComponent<Renderer>().material.SetTexture("_MainTex", _footprintDog);
                break;
            case 4:
                //fall through to case 5 to increase chance of mecia camp logo to appear
            case 5:
                GetComponent<Renderer>().material.SetTexture("_MainTex", _mediaCampLogo);
                break;
        }

        ForceActive = true;
    }

    public void SetSingleFootprint()
    {
        GetComponent<Renderer>().material.SetTexture("_MainTex", _footprintDog);
    }

    /**
	 * Resets/removes Footprint texture.
	 **/

    public void ResetTexture()
    {
        GetComponent<Renderer>().material.SetTexture("_MainTex", null);
        ForceActive = false;
    }

    private void OnMouseDown()
    {
        ForceActive = !ForceActive;
    }
}