using UnityEngine;

public class DmxController : MonoBehaviour
{
    // Time Reference and FieldSize is set by the TileController
    public float TimeReference { get; set; }
    public float FieldSize { get; set; }

    //Tick is called by the TileController
    public void Tick()
    {
        //do DMX stuff here
        Debug.Log("Tick");
    }
}
