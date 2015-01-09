using UnityEngine;

public class DmxController : MonoBehaviour
{
    // Time Reference and FieldSize is set by the TileController
    public float TimeReference { get; set; }
    public float FieldSize { get; set; }
    public GameObject LightControllerObject;
    private LightController _lightControllerScript;

    void Start()
    {
        _lightControllerScript = LightControllerObject.GetComponent<LightController>();
    }
    //Tick is called by the TileController
    public void Tick()
    {
        _lightControllerScript.UpdateFaderValues();

        //do DMX stuff here
    }
}
