using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [SerializeField]
    private BowString bowStringRenderer;

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    [SerializeField]
    private Transform midPointGrabObject, midPointVisualObject, midPointParent;

    [SerializeField]
    private float bowStringStretchLimit = 0.4f;

    private float strength;

    public UnityEvent OnBowPulled;
    public UnityEvent<float> OnBowReleased;

    private Transform interactor;

    private void Awake()
    {
        if (interactable == null)
        {
            Debug.LogError("XRGrabInteractable not assigned!");
        }
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(PrepareBowString);
        interactable.selectExited.AddListener(ResetBowString);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(PrepareBowString);
        interactable.selectExited.RemoveListener(ResetBowString);
    }

    private void PrepareBowString(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject.transform;
        OnBowPulled?.Invoke();
    }

    private void ResetBowString(SelectExitEventArgs args)
    {
        OnBowReleased?.Invoke(strength);
        strength = 0;

        interactor = null;
        midPointGrabObject.localPosition = Vector3.zero;
        midPointVisualObject.localPosition = Vector3.zero;
        bowStringRenderer.CreateString(null);
    }

    private void Update()
    {
        if (interactor != null)
        {
            Vector3 midPointLocalSpace = midPointParent.InverseTransformPoint(midPointGrabObject.position);
            float midPointLocalXAbs = Mathf.Abs(midPointLocalSpace.x);
            HandleStringPushedBackToStart(midPointLocalSpace);
            HandleStringPulledBackToLimit(midPointLocalXAbs, midPointLocalSpace);
            HandlePullingString(midPointLocalXAbs, midPointLocalSpace);

            bowStringRenderer.CreateString(midPointVisualObject.position);
        }
    }

    private void HandlePullingString(float midPointLocalXAbs, Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x < 0 && midPointLocalXAbs < bowStringStretchLimit)
        {
            strength = Remap(midPointLocalXAbs, 0, bowStringStretchLimit,0 , 1);
            midPointVisualObject.localPosition = new Vector3(midPointLocalSpace.x,0,0);
        }
    }

    private void HandleStringPulledBackToLimit(float midPointLocalXAbs, Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x < 0 && midPointLocalXAbs >= bowStringStretchLimit)
        {
            strength = 1;
            midPointVisualObject.localPosition = new Vector3(-bowStringStretchLimit,0,0);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x >= 0)
        {
            strength = 0;
            midPointVisualObject.localPosition = Vector3.zero;
        }
    }

    private float Remap(float value, int fromMin, float fromMax, int toMin, int toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }
}