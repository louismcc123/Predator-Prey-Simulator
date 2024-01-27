using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITest : MonoBehaviour
{
    int UILayer;
    
    // Start is called before the first frame update
    private void Start()
    {
        UILayer = LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (!IsPointerOverUIElement())
            {
                Debug.Log("Pointer not over UI");
            }
        }

        //print(IsPointerOverUIElement() ? "Over UI" : "Not over UI");
    }

    /*public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }*/

    /*private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
    {
        for(int index = 0; index < eventSystemRaycastResults.Count; index++)
        {
            RaycastResult curRaycastResult = eventSystemRaycastResults[index];
            if (curRaycastResult.gameObject.layer == UILayer 
                && curRaycastResult.gameObject == this.gameObject)
                return true;
        }
        return false;
    }*/

    //static List<RaycastResult> GetEventSystemRaycastResults()
    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults.Count > 0;
    }
}
