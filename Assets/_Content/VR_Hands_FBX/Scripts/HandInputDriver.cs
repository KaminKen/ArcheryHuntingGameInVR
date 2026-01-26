
using UnityEngine;
using UnityEngine.InputSystem;

public class HandInputDriver : MonoBehaviour
{
    public HandAnimation handAnimation;
    public InputActionProperty gripAction;

    [SerializeField] private float pressThreshold = 0.75f;
    [SerializeField] private float releaseThreshold = 0.55f;

    private bool isGrabbing;

    void Update()
    {
        float v = gripAction.action.ReadValue<float>();

        // Press: only once when crossing press threshold
        if (!isGrabbing && v >= pressThreshold)
        {
            isGrabbing = true;
            handAnimation.TriggerGrab();
        }
        // Release: only once when crossing release threshold
        else if (isGrabbing && v <= releaseThreshold)
        {
            isGrabbing = false;
            handAnimation.TriggerLet();
        }
    }
}
