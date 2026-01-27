// using System.Collections.Generic;
// using System.Reflection.Metadata;

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
    private float bowStringStretchLimit = 0.3f;

    private float strength, previousStrength;

    [SerializeField]
    private float stringSoundThreshold = 0.001f;

    [SerializeField]
    private AudioSource audioSource;

    public UnityEvent OnBowPulled;
    public UnityEvent<float> OnBowReleased;

    private Transform interactor;

    // private void Awake()
    // {
    //     if (interactable == null)
    //     {
    //         Debug.LogError("XRGrabInteractable not assigned!");
    //     }
    // }

    // private void OnEnable()
    // {
    //     if (!interactable) return;
    //     interactable.selectEntered.AddListener(PrepareBowString);
    //     interactable.selectExited.AddListener(ResetBowString);
    // }

    // private void OnDisable()
    // {
    //     if (!interactable) return;
    //     interactable.selectEntered.RemoveListener(PrepareBowString);
    //     interactable.selectExited.RemoveListener(ResetBowString);
    // }
    private void Awake()
    {
        if (!interactable) Debug.LogError("interactable missing", this);
        if (!audioSource) Debug.LogError("audioSource missing", this);
        if (!bowStringRenderer) Debug.LogError("bowStringRenderer missing", this);
        if (!midPointGrabObject || !midPointVisualObject || !midPointParent)
            Debug.LogError("midpoints missing", this);
    }

    private void OnEnable()
    {
        if (!interactable) return;
        interactable.selectEntered.AddListener(PrepareBowString);
        interactable.selectExited.AddListener(ResetBowString);
    }

    private void OnDisable()
    {
        if (!interactable) return;
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
        previousStrength = 0;
        audioSource.pitch = 1;
        audioSource.Stop();

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
            
            previousStrength = strength;

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
            if(audioSource.isPlaying == false && strength <= 0.01f)
            {
                audioSource.Play();
            }
            strength = Remap(midPointLocalXAbs, 0f, bowStringStretchLimit, 0f, 1f);

            midPointVisualObject.localPosition = new Vector3(midPointLocalSpace.x,0,0);

            PlayStringPullingSound();
        }
    }

    private void PlayStringPullingSound()
    {
        if(Mathf.Abs(strength - previousStrength) > stringSoundThreshold)
        {
            if(strength < previousStrength)
            {
                audioSource.pitch = -1;
            } 
            else
            {
                audioSource.pitch = 1;  
            }
            audioSource.UnPause();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void HandleStringPulledBackToLimit(float midPointLocalXAbs, Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x < 0 && midPointLocalXAbs >= bowStringStretchLimit)
        {
            audioSource.Pause();
            strength = 1;
            midPointVisualObject.localPosition = new Vector3(-bowStringStretchLimit,0,0);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if(midPointLocalSpace.x >= 0)
        {
            audioSource.pitch = 1;
            audioSource.Stop();
            strength = 0;
            midPointVisualObject.localPosition = Vector3.zero;
        }
    }

    private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (((value - fromMin) / (fromMax - fromMin)) * (toMax - toMin)) + toMin;
    }
}