using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneObjStatus
{
    Old,//这次刷新没有加载
    Loading,//加载中
    New,//这次刷新加载
}


public class SceneObj
{
    public ObjData data;
    public SceneObjStatus status;
    public GameObject obj;

    public SceneObj(ObjData data)
    {
        this.data = data;
        this.obj = null;
    }
}

public class ResourcesObj
{
    public GameObject obj;
    private int insNum;
    public ResourcesObj(GameObject obj)
    {
        this.obj = obj;
        this.insNum = 0;
    }

    public void CreateIns()
    {
        ++insNum;
    }

    public void DelIns()
    {
        --insNum;
    }

    public bool CheckInsZero()
    {
        return insNum <= 0;
    }
}

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance;

    public float delTime = 2;
    private Dictionary<string, SceneObj> activeObjDic;//<suid,SceneObj>
    private Dictionary<string, SceneObj> inActiveObjDic;//<suid,SceneObj>
    private List<string> delKeysList;

    private Dictionary<string, ResourcesObj> resourcesObjDic;//<resPath,ResourcesObj>
    #region get set
    public Dictionary<string, SceneObj> ActiveObjDic
    {
        get
        {
            if (activeObjDic == null)
            {
                activeObjDic = new Dictionary<string, SceneObj>();
            }
            return activeObjDic;
        }
        set
        {
            activeObjDic = value;
        }
    }

    public Dictionary<string, SceneObj> InActiveObjDic {
        get {
            if(inActiveObjDic == null)
            {
                inActiveObjDic = new Dictionary<string, SceneObj>();
            }
            return inActiveObjDic;
        }
        set {
            inActiveObjDic = value;
        }
    }

    public List<string> DelKeysList
    {
        get
        {
            if(delKeysList == null)
            {
                delKeysList = new List<string>();
            }
            return delKeysList;
        }

        set
        {
            delKeysList = value;
        }
    }

    public Dictionary<string, ResourcesObj> ResourcesObjDic
    {
        get
        {
            if(resourcesObjDic == null)
            {
                resourcesObjDic = new Dictionary<string, ResourcesObj>();
            }
            return resourcesObjDic;
        }

        set
        {
            resourcesObjDic = value;
        }
    }
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(IEDel());
    }

    private IEnumerator IEDel()
    {
        while (true)
        {
            bool bDel = false;
            foreach(var pair in InActiveObjDic)
            {
                ResourcesObj resourceObj;
                if(ResourcesObjDic.TryGetValue(pair.Value.data.resPath,out resourceObj))
                {
                    resourceObj.DelIns();
                    if (resourceObj.CheckInsZero())
                    {
                        bDel = true;
                        resourceObj.obj = null;
                        ResourcesObjDic.Remove(pair.Value.data.resPath);
                    }
                }
                Destroy(pair.Value.obj);
            }
            InActiveObjDic.Clear();
            if (bDel)
            {
                Resources.UnloadUnusedAssets();
            }
            yield return new WaitForSeconds(delTime);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    Debug.Log("==================\nactive:");
        //    foreach (var item in ActiveObjDic)
        //    {
        //        Debug.Log(item.Value.data.pos);
        //    }
        //    Debug.Log("inactive:");
        //    foreach (var item in InActiveObjDic)
        //    {
        //        Debug.Log(item.Value.data.pos);
        //    }
        //    Debug.Log("===================");
        //}
        
    }
    public SceneObj CheckIsActive(string sUid)
    {
        SceneObj obj;
        if(ActiveObjDic.TryGetValue(sUid, out obj))
        {
            return obj;
        }
        return null;
    }

    public SceneObj CheckIsInActive(string sUid)
    {
        SceneObj obj;
        if (InActiveObjDic.TryGetValue(sUid, out obj))
        {
            return obj;
        }
        return null;
    }

    private bool MoveToActive(ObjData obj)
    {
        SceneObj sceneObj;
        if (InActiveObjDic.TryGetValue(obj.sUid, out sceneObj))
        {
            sceneObj.obj.SetActive(true);
            sceneObj.status = SceneObjStatus.New;
            ActiveObjDic.Add(obj.sUid, sceneObj);
            InActiveObjDic.Remove(obj.sUid);
            return true;
        }
        return false;
    }

    public bool MoveToInActive(ObjData obj)
    {
        SceneObj sceneObj;
        if (ActiveObjDic.TryGetValue(obj.sUid, out sceneObj))
        {
            sceneObj.obj.SetActive(false);
            InActiveObjDic.Add(obj.sUid, sceneObj);
            ActiveObjDic.Remove(obj.sUid);
            return true;
        }
        return false;
    }

    private void CreateObj(GameObject prefab, SceneObj sceneObj)
    {
        sceneObj.obj = Instantiate(prefab);
        sceneObj.obj.transform.position = sceneObj.data.pos;
        sceneObj.obj.transform.rotation = sceneObj.data.rotation;
    }
    
    public void Load(ObjData obj)
    {
        if (CheckIsActive(obj.sUid) != null)
        {
            return;
        }
        if (!MoveToActive(obj))
        {
            SceneObj sceneObj = new SceneObj(obj);
            sceneObj.status = SceneObjStatus.New;

            GameObject resObj = null;
            ResourcesObj resourceObj;
            if (ResourcesObjDic.TryGetValue(obj.resPath, out resourceObj))
            {
                resObj = resourceObj.obj;
                resourceObj.CreateIns();
            }
            else
            {
                resObj = Resources.Load<GameObject>(obj.resPath);
            }
            
            CreateObj(resObj, sceneObj);
            ActiveObjDic.Add(obj.sUid, sceneObj);
        }
    }
    public void LoadAsync(ObjData obj)
    {
        if (CheckIsActive(obj.sUid) != null)
        {
            return;
        }
        if (!MoveToActive(obj))
        {
            StartCoroutine(IELoad(obj));
        }
    }

    private IEnumerator IELoad(ObjData obj)
    {
        SceneObj sceneObj = new SceneObj(obj);
        sceneObj.status = SceneObjStatus.Loading;
        ActiveObjDic.Add(obj.sUid, sceneObj);
        GameObject resObj = null;
        ResourcesObj resourceObj;
        if(ResourcesObjDic.TryGetValue(obj.resPath,out resourceObj))
        {
            resObj = resourceObj.obj;
            resourceObj.CreateIns();
        }
        else
        {
            ResourceRequest request = Resources.LoadAsync<GameObject>(obj.resPath);
            yield return request;
            resObj = request.asset as GameObject;
        }
        
        CreateObj(resObj, sceneObj);
        sceneObj.status = SceneObjStatus.New;
    }

    
    public void RefreshStatus()
    {
        DelKeysList.Clear();
        foreach (var pair in ActiveObjDic)
        {
            SceneObj sceneObj = pair.Value;
            if(sceneObj.status == SceneObjStatus.Old)
            {
                DelKeysList.Add(pair.Key);
            }
            else if(sceneObj.status == SceneObjStatus.New)
            {
                sceneObj.status = SceneObjStatus.Old;
            }
        }
        for(int i = 0; i < DelKeysList.Count; ++i)
        {
            MoveToInActive(ActiveObjDic[DelKeysList[i]].data);
        }
    }
}
