
using UnityEngine;
// using UnityEngine.Debug;

public class ArrowController : MonoBehaviour
{
    [SerializeField]
    private GameObject midPointVisual, arrowPrefab, arrowSpawnPoint;

    [SerializeField]
    private float arrowMaxSpeed = 10f;
    
    [SerializeField]
    private AudioSource bowReleaseAudioSource;

    public void PrepareArrow()
    {
        midPointVisual.SetActive(true);
    }

    // public void ReleasedArrow(float strength)
    // {
    //     midPointVisual.SetActive(false);
    //     //print($"Bow strength is {strength}");

    //     GameObject arrow = Instantiate(arrowPrefab);
    //     arrow.transform.position = arrowSpawnPoint.transform.position;
    //     // arrow.transform.rotation = midPointVisual.transform.rotation;
    //     Vector3 shootDir = midPointVisual.transform.right;

    //     // rotate arrow so its FORWARD points along shootDir
    //     arrow.transform.rotation = Quaternion.LookRotation(shootDir, Vector3.up);

    //     Rigidbody rb = arrow.GetComponent<Rigidbody>();
    //     // Debug.DrawRay(midPointVisual.transform.position, midPointVisual.transform.forward * 2f, Color.green, 2f);
    //     // Debug.DrawRay(midPointVisual.transform.position, midPointVisual.transform.up * 2f, Color.blue, 2f);
    //     // Debug.DrawRay(midPointVisual.transform.position, midPointVisual.transform.right * 2f, Color.red, 2f);

    //     rb.AddForce(shootDir * strength * arrowMaxSpeed, ForceMode.Impulse);
    // }

    public void ReleasedArrow(float strength)
    {
        if (!arrowPrefab || !arrowSpawnPoint || !midPointVisual) return;

        bowReleaseAudioSource.Play();
        midPointVisual.SetActive(false);

        GameObject arrow = Instantiate(arrowPrefab);
        arrow.transform.position = arrowSpawnPoint.transform.position;

        Vector3 shootDir = midPointVisual.transform.right; //from forward
        arrow.transform.rotation = Quaternion.LookRotation(shootDir, Vector3.up);

        Rigidbody rb = arrow.GetComponentInChildren<Rigidbody>(); //  change THIS
        if (!rb) { Destroy(arrow); return; }                      //  add THIS

        rb.AddForce(shootDir * strength * arrowMaxSpeed, ForceMode.Impulse);
        // GameObject arrow = Instantiate(arrowPrefab);
        // arrow.transform.position = arrowSpawnPoint.transform.position;
        // arrow.transform.rotation = midPointVisual.transform.rotation;
        // Rigidbody rb = arrow.GetComponent<Rigidbody>();
        // rb.AddForce(midPointVisual.transform.forward * strength * arrowMaxSpeed, ForceMode.Impulse);
    }


}
