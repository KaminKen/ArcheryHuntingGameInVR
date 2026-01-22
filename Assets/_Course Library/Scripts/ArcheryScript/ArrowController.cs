using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField]
    private GameObject midPointVisual, arrowPrefab, arrowSpawnPoint;

    [SerializeField]
    private float arrowMaxSpeed = 10f;

    [SerializeField]
    private AudioSource bowReleaseAudioSource;

    // Debug toggles (no logic change; just controls logging)
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool logArrowHierarchy = true;

    public void PrepareArrow()
    {
        // if (enableDebugLogs)
        // {
        //     Debug.Log($"[ArrowController] PrepareArrow() | midPointVisual active before: {(midPointVisual ? midPointVisual.activeSelf : false)}");
        // }

        midPointVisual.SetActive(true);

        // if (enableDebugLogs)
        // {
        //     Debug.Log($"[ArrowController] PrepareArrow() | midPointVisual active after: {(midPointVisual ? midPointVisual.activeSelf : false)}");
        // }
    }

    public void ReleasedArrow(float strength)
    {
        // keep your original guard
        if (!arrowPrefab || !arrowSpawnPoint || !midPointVisual) return;

        // if (enableDebugLogs)
        // {
        //     Debug.Log(
        //         "[ArrowController] ReleasedArrow() START\n" +
        //         $"  strength: {strength}\n" +
        //         $"  arrowMaxSpeed: {arrowMaxSpeed}\n" +
        //         $"  arrowPrefab: {arrowPrefab.name}\n" +
        //         $"  arrowSpawnPoint: {arrowSpawnPoint.name}\n" +
        //         $"  midPointVisual: {midPointVisual.name}\n" +
        //         $"  spawnPoint world pos: {V3(arrowSpawnPoint.transform.position)}\n" +
        //         $"  spawnPoint world rot (euler): {V3(arrowSpawnPoint.transform.eulerAngles)}\n" +
        //         $"  midPointVisual world pos: {V3(midPointVisual.transform.position)}\n" +
        //         $"  midPointVisual world rot (euler): {V3(midPointVisual.transform.eulerAngles)}\n" +
        //         $"  midPointVisual axes (world) | right: {V3(midPointVisual.transform.right)} forward: {V3(midPointVisual.transform.forward)} up: {V3(midPointVisual.transform.up)}"
        //     );
        // }

        if (bowReleaseAudioSource)
        {
            // if (enableDebugLogs)
            //     Debug.Log($"[ArrowController] bowReleaseAudioSource.Play() | clip: {(bowReleaseAudioSource.clip ? bowReleaseAudioSource.clip.name : "null")}");
            bowReleaseAudioSource.Play();
        }
        else
        {
            // if (enableDebugLogs)
            //     Debug.LogWarning("[ArrowController] bowReleaseAudioSource is NULL");
        }

        midPointVisual.SetActive(false);

        GameObject arrow = Instantiate(arrowPrefab);
        arrow.transform.position = arrowSpawnPoint.transform.position;

        // your direction logic unchanged
        Vector3 shootDir = midPointVisual.transform.right; //from forward
        arrow.transform.rotation = Quaternion.LookRotation(shootDir, Vector3.up);

        // if (enableDebugLogs)
        // {
        //     Debug.Log(
        //         "[ArrowController] After Instantiate + Initial Setup\n" +
        //         $"  arrow instance: {arrow.name}\n" +
        //         $"  arrow world pos (after set): {V3(arrow.transform.position)}\n" +
        //         $"  arrow world rot (euler after set): {V3(arrow.transform.eulerAngles)}\n" +
        //         $"  shootDir (world): {V3(shootDir)} | magnitude: {shootDir.magnitude:F6}\n" +
        //         $"  expected impulse magnitude: {(strength * arrowMaxSpeed):F6}\n" +
        //         $"  arrow root forward/right/up: fwd {V3(arrow.transform.forward)} right {V3(arrow.transform.right)} up {V3(arrow.transform.up)}\n" +
        //         $"  alignment check (dot arrow.forward vs shootDir.normalized): {Vector3.Dot(arrow.transform.forward, shootDir.normalized):F6}"
        //     );
        // }

        // your rigidbody fetch logic unchanged
        Rigidbody rb = arrow.GetComponentInChildren<Rigidbody>(); //  change THIS
        if (!rb)
        {
            // if (enableDebugLogs)
            // {
            //     Debug.LogError(
            //         "[ArrowController] Rigidbody NOT found in children. Destroying arrow.\n" +
            //         $"  arrow instance: {arrow.name}\n" +
            //         $"  arrow world pos: {V3(arrow.transform.position)}"
            //     );

            //     if (logArrowHierarchy) LogHierarchy(arrow.transform);
            // }

            Destroy(arrow);
            return;
        }

        // if (enableDebugLogs)
        // {
        //     Transform rbT = rb.transform;
        //     Debug.Log(
        //         "[ArrowController] Rigidbody found\n" +
        //         $"  rb object: {rb.gameObject.name}\n" +
        //         $"  rb world pos: {V3(rbT.position)}\n" +
        //         $"  rb world rot (euler): {V3(rbT.eulerAngles)}\n" +
        //         $"  rb isKinematic: {rb.isKinematic}\n" +
        //         $"  rb useGravity: {rb.useGravity}\n" +
        //         $"  rb mass: {rb.mass}\n" +
        //         $"  rb velocity (before): {V3(rb.linearVelocity)}\n" +
        //         $"  rb angularVelocity (before): {V3(rb.angularVelocity)}"
        //     );

        //     if (logArrowHierarchy) LogHierarchy(arrow.transform);
        // }

        // your force logic unchanged
        rb.AddForce(shootDir * strength * arrowMaxSpeed, ForceMode.Impulse);

        // if (enableDebugLogs)
        // {
        //     Debug.Log(
        //         "[ArrowController] After AddForce\n" +
        //         $"  rb velocity (after): {V3(rb.linearVelocity)}\n" +
        //         $"  rb angularVelocity (after): {V3(rb.angularVelocity)}\n" +
        //         $"  arrow world pos (end): {V3(arrow.transform.position)}"
        //     );
        // }
    }

    // ---- helpers (debug only) ----
    private static string V3(Vector3 v) => $"({v.x:F4}, {v.y:F4}, {v.z:F4})";

    private static void LogHierarchy(Transform root)
    {
        // prints transform tree with local/world positions (useful if prefab has nested offsets)
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[ArrowController] Arrow Hierarchy Dump:");
        Dump(root, 0, sb);
        Debug.Log(sb.ToString());
    }

    private static void Dump(Transform t, int depth, System.Text.StringBuilder sb)
    {
        string indent = new string(' ', depth * 2);
        sb.AppendLine(
            $"{indent}- {t.name} | localPos {V3(t.localPosition)} localRot {V3(t.localEulerAngles)} | worldPos {V3(t.position)} worldRot {V3(t.eulerAngles)}"
        );

        for (int i = 0; i < t.childCount; i++)
            Dump(t.GetChild(i), depth + 1, sb);
    }
}
