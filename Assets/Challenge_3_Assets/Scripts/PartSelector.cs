using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PartSelector : MonoBehaviour
{
    [Header("References")]
    public Controller orbitCam;
    public GameObject infoPanel;
    public TextMeshProUGUI titleText;

    [Header("Current Target")]
    public MotorPart currentSelected;

    void Start()
    {
        // Panel remains hidden until the very first click
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Ignore click if clicking over UI elements (buttons, sliders, etc.)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MotorPart part = hit.collider.GetComponent<MotorPart>();
                if (part != null)
                {
                    SelectPart(part);
                }
            }
        }
    }

    public void SelectPart(MotorPart part)
    {
        // If clicking the same part already active, do nothing
        if (currentSelected == part) return;

        // Reset previous highlight
        if (currentSelected != null)
        {
            currentSelected.SetHighlight(false);
            if (currentSelected.labelObject != null)
                currentSelected.labelObject.SetActive(false);
        }

        currentSelected = part;
        currentSelected.SetHighlight(true);

        if (currentSelected.labelObject != null)
        {
            currentSelected.labelObject.SetActive(true);
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            titleText.text = part.partName;
        }

        if (orbitCam != null)
        {
            orbitCam.SetTarget(part.transform);
        }
    }

    // Call this if clicking empty background space or if you add a clear button
    public void DeselectAll()
    {
        if (currentSelected != null)
        {
            currentSelected.SetHighlight(false);
            currentSelected = null;
        }

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}