using System.Diagnostics;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField]
    private GameObject midPointVisual;

    public void PrepareArrow()
    {
        midPointVisual.SetActive(true);
    }

    public void ReleasedArrow(float strength)
    {
        midPointVisual.SetActive(false);
        print($"Bow strength is {strength}");
    }

}
