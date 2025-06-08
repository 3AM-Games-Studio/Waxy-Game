using UnityEngine;

public class MOCK_Constelacion : MonoBehaviour
{
    public GameObject linea1;
    public GameObject linea2;
    public GameObject linea3;
    public Material GoldMaterial;

    private bool fire;
    private bool moved;

    public void TurnFire()
    {
        if(fire) return;
        linea1.GetComponent<Renderer>().material = GoldMaterial;
        fire = true;
        if(moved)
            linea3.GetComponent<Renderer>().material = GoldMaterial;
    }

    public void MovedObject()
    {
        if(moved) return;
        linea2.GetComponent<Renderer>().material = GoldMaterial;
        moved = true;
        if(fire)
            linea3.GetComponent<Renderer>().material = GoldMaterial;
    }
}
