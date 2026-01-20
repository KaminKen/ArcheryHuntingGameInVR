using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [SerializeField]
    private BowString bowStringRenderer;

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    [SerializeField]
    private Transform midPointGrabObject;


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
        bowStringRenderer.CreateString(null);
    }

    private void Update()
    {
        if (interactor != null)
        {
            bowStringRenderer.CreateString(midPointGrabObject.position);
        }
    }
}

