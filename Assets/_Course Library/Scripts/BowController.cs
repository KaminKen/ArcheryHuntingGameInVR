using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;
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
    private float bowStringStretchLimit = 0.3f;

    private float strength;

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
    }

    private void ResetBowString(SelectExitEventArgs args)
    {
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
            midPointVisualObject.localPosition = new Vector3(midPointLocalSpace.x,0,0);
        }
    }

    private void HandleStringPulledBackToLimit(float midPointLocalXAbs, Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x < 0 && midPointLocalXAbs >= bowStringStretchLimit)
        {
            midPointVisualObject.localPosition = new Vector3(-bowStringStretchLimit,0,0);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x >= 0)
        {
            midPointVisualObject.localPosition = Vector3.zero;
        }
    }
}