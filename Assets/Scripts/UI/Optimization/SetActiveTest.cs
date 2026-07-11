using UnityEngine;

public class SetActiveTest : MonoBehaviour
{
    public GameObject setActiveObject;
    public Canvas setActiveCanvas;
    
    private bool isOpen;
    
    
    private void Update()
    {
        // setActiveObject.SetActive(!setActiveObject.activeSelf);
        
        isOpen = !isOpen;
        setActiveCanvas.enabled = isOpen;
    }
}