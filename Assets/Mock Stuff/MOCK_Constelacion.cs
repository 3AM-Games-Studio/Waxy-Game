using System.Collections;
using UnityEngine;

public class MOCK_Constelacion : MonoBehaviour
{
    public GameObject linea1;
    public GameObject linea2;
    public GameObject linea3;
    public Material GoldMaterial;
    public Transform visagraDer;
    public Transform visagraIzq;
    private bool fire;
    private bool moved;

    public void TurnFire()
    {
        if(fire) return;
        linea1.GetComponent<Renderer>().material = GoldMaterial;
        fire = true;
        if (moved)
        {
            StartCoroutine(OpenDoor());
            linea3.GetComponent<Renderer>().material = GoldMaterial;
        }
    }

    public void MovedObject()
    {
        if(moved) return;
        linea2.GetComponent<Renderer>().material = GoldMaterial;
        moved = true;
        if (fire)
        {
            StartCoroutine(OpenDoor());
            linea3.GetComponent<Renderer>().material = GoldMaterial;
        }
    }

    private IEnumerator OpenDoor()
    {
        float duration = 3f;
        float elapsed = 0f;

        Quaternion startRotDer = Quaternion.Euler(0, 90, 0);
        Quaternion endRotDer = Quaternion.Euler(0, 180, 0);

        Quaternion startRotIzq = Quaternion.Euler(0, 90, 0);
        Quaternion endRotIzq = Quaternion.Euler(0, 0, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            visagraDer.localRotation = Quaternion.Lerp(startRotDer, endRotDer, t);
            visagraIzq.localRotation = Quaternion.Lerp(startRotIzq, endRotIzq, t);

            yield return null;
        }

        // Asegurar rotaciones finales exactas al terminar
        visagraDer.localRotation = endRotDer;
        visagraIzq.localRotation = endRotIzq;
    }

}
