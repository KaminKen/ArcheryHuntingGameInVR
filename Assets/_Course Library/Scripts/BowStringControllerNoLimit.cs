// using System.Collections.Generic;
// // using System.Numerics;
// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class BowController : MonoBehaviour
// {
//     [SerializeField]
//     private BowString bowStringRenderer;

//     [SerializeField]
//     private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

//     [SerializeField]
//     private Transform midPointGrabObject, midPointVisualObject, midPointParent;

//     [SerializeField]
//     private float bowStringStretchLimit = 0.3f;

//     private Transform interactor;

//     private void Awake()
//     {
//         if (interactable == null)
//         {
//             Debug.LogError("XRGrabInteractable not assigned!");
//         }
//     }

//     private void OnEnable()
//     {
//         interactable.selectEntered.AddListener(PrepareBowString);
//         interactable.selectExited.AddListener(ResetBowString);
//     }

//     private void OnDisable()
//     {
//         interactable.selectEntered.RemoveListener(PrepareBowString);
//         interactable.selectExited.RemoveListener(ResetBowString);
//     }

//     private void PrepareBowString(SelectEnterEventArgs args)
//     {
//         interactor = args.interactorObject.transform;
//     }

//     private void ResetBowString(SelectExitEventArgs args)
//     {
//         interactor = null;
//         midPointGrabObject.localPosition = Vector3.zero;
//         midPointVisualObject.localPosition = Vector3.zero;
//         bowStringRenderer.CreateString(null);
//     }
//     private void Update()
//     {
//         if (interactor != null)
//         {
//             Vector3 midPointLocalSpace = midPointParent.InverseTransformPoint(midPointGrabObject.position);
//             float midPointLocalZAbs = Mathf.Abs(midPointLocalSpace.z);
//             HandleStringPushedBackToStart(midPointLocalSpace);
//             HandleStringPulledBackToLimit(midPointLocalZAbs, midPointLocalSpace);
//             HandlePullingString(midPointLocalZAbs, midPointLocalSpace);

//             bowStringRenderer.CreateString(midPointGrabObject.position);
//         }
//     }

//     private void HandlePullingString(float midPointLocalZAbs, Vector3 midPointLocalSpace)
//     {
//         if(midPointLocalSpace.z < 0 && midPointLocalZAbs < bowStringStretchLimit)
//         {
//             midPointVisualObject.localPosition = new Vector3(0,0,midPointLocalSpace.z);
//         }
//     }

//     private void HandleStringPulledBackToLimit(float midPointLocalZAbs, Vector3 midPointLocalSpace)
//     {
//         if(midPointLocalSpace.z < 0 && midPointLocalZAbs >= bowStringStretchLimit)
//         {
//             midPointVisualObject.localPosition = new Vector3(0,0,-bowStringStretchLimit);
//         }
//     }

//     private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
//     {
//         if(midPointLocalSpace.z >= 0)
//         {
//             midPointVisualObject.localPosition = Vector3.zero;
//         }
//     }
// }