using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabelManager : MonoBehaviour
{
    [Header("All Label Root Objects")]
    public List<GameObject> labels = new List<GameObject>();

    [Header("Optional Button Text Reference")]
    public TextMeshProUGUI toggleButtonText;

    private bool labelsVisible = true;

    void Start()
    {
        UpdateLabelVisibility();
    }

    public void ToggleLabels()
    {
        labelsVisible = !labelsVisible;
        UpdateLabelVisibility();
    }

    private void UpdateLabelVisibility()
    {
        foreach (GameObject label in labels)
        {
            if (label != null)
            {
                label.SetActive(labelsVisible);
            }
        }

        if (toggleButtonText != null)
        {
            toggleButtonText.text = labelsVisible ? "Hide Labels" : "Show Labels";
        }
    }
}