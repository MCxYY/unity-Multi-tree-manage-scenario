using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Menu
{
    [MenuItem("Tools/SaveObjData")]
    public static void SaveObjData()
    {
        Main main = GameObject.Find("Terrain").GetComponent<Main>();
        Transform objsLayer = GameObject.Find("Objs").transform;
        for(int i = 0; i < objsLayer.childCount; ++i)
        {
            Transform childObj = objsLayer.GetChild(i);
            ObjData objData = new ObjData(childObj.gameObject.GetComponent<SceneObjData>().resPath, childObj.transform.position, childObj.transform.rotation);
            main.objList.Add(objData);
        }
        for(int i = 0; i < main.objList.Count; ++i)
        {
            Debug.Log(main.objList[i].resPath);
        }
    }
}
