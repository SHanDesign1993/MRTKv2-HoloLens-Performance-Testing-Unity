using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;
using TMPro;

public class DemoExample : MonoBehaviour
{
    [SerializeField]
    TextMeshPro objInfo;
    string objID = "demo_anchor";
    [SerializeField]
    GameObject demoObj;

    public void RecenterView()
    {
        //For a seated-scale experience, to let the user later recenter the seated origin, you can call the XR.InputTracking.Recenter method:
        InputTracking.Recenter();
    }

    public void SaveObj(GameObject obj)
    {
        // Save data about holograms positioned by this world anchor
        WorldAnchor anchor= null;
        if (obj.GetComponent<WorldAnchor>() != null)
        {
            anchor = obj.GetComponent<WorldAnchor>();
            if (WorldAnchorManager.Instance.SaveSceneObject(objID, anchor))
                objInfo.text = "save "+ obj.name+ " completed";
            else
                objInfo.text = "save " + obj.name + " failed";
        }
    }

    public void LoadAllObj()
    {
        //To discover previously stored anchors, call GetAllIds.
        
        string[] ids = WorldAnchorManager.Instance.anchorStore.GetAllIds();
        objInfo.text = "";
        for (int index = 0; index < ids.Length; index++)
        {
            objInfo.text += "Find stored obj :" + ids[index]+"\n";
            if (ids[index] == objID)
                demoObj.transform.position = WorldAnchorManager.Instance.LoadSceneObject(ids[index]).transform.position;
        }
        

        /*
        WorldAnchor anchor = WorldAnchorManager.Instance.LoadSceneObject(id);
        if (anchor != null)
        {
            objInfo.text = "load " + anchor.gameObject.name + " completed";
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = anchor.gameObject.transform.position;
        }
        else
        {
            objInfo.text = "load " + anchor.gameObject.name + " failed";
        }  
        */
            

        //return anchor.gameObject;
    }

    public void DeleteAllObj() {
        WorldAnchorManager.Instance.ClearAllObject();
    }
}
