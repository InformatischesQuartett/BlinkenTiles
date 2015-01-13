using UnityEngine;
using Random = UnityEngine.Random;

public class TileBehaviour : MonoBehaviour
{
    private float _tileWidth;
    private float _tileHeight;
    private Rect _positionRect;

    private Material _materialTime;
    private Material _materialDefault;
    private Material _materialOccupied;
    private Material _materialPreview;
    private Material _materialHit;

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
                        renderer.material = _materialDefault;
                        break;
                    case Highlighttype.Occupied:
                        renderer.material = _materialOccupied;
                        break;
                    case Highlighttype.Hit:
                        renderer.material = _materialHit;
                        break;
                    case Highlighttype.Preview:
                        renderer.material = _materialPreview;
                        break;
                    case Highlighttype.Time:
                        renderer.material = _materialTime;
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

        _materialDefault = Config.MaterialWhite;
        _materialTime = Config.MaterialYellow;
        _materialHit = Config.MaterialRed;
        _materialPreview = Config.MaterialOrange;
        _materialOccupied = Config.MaterialGreen;
        renderer.material = _materialDefault;
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

    private void OnMouseDown()
    {
        ForceActive = !ForceActive;
    }
}