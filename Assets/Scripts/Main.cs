using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Main : MonoBehaviour
{
    [SerializeField]
    public List<ObjData> objList = new List<ObjData>();
    public Bounds mainBound;
    
    private Tree tree;
    private bool bInitEnd = false;

    private Role role;
    
    public void Awake()
    {
        tree = new Tree(mainBound);
        for(int i = 0; i < objList.Count; ++i)
        {
            tree.InsertObj(objList[i]);
        }
        role = GameObject.Find("Role").GetComponent<Role>();
        bInitEnd = true;
    }

    private void Update()
    {
        if (role.bMove)
        {
            tree.TriggerMove(role.mCamera);
        }
    }
    private void OnDrawGizmos()
    {
        if (bInitEnd)
        {
            tree.DrawBound();
        }
        else
        {
            Gizmos.DrawWireCube(mainBound.center, mainBound.size);
        }
    }

}
