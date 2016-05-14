using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SphereMover : MyBehaviour
{
    [SerializeField]
    bool runUpdate;
    [SerializeField]
    bool test;
    public List<GameObject> children = new List<GameObject>();
    public int curChild = 0;

    bool UpdateCondition()
    {
        return runUpdate;
    }

    void MyAwake()
    {
        LogMessage("running MyAwake");
    }

    void MyStart()
    {
        LogWarning("running MyStart");
    }
    void MyUpdate()
    {
        transform.position += (Vector3.up * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.C))
        {
            Transform[] c = GetComponentsInChildren<Transform>();
            children.Add(c[curChild].gameObject);
            curChild++;
        }

    }
    void MyFixedUpdate()
    {

    }
    void MyLateUpdate()
    {

    }

    
}
