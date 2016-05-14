using UnityEngine;
using System.Collections;

public class LCHook : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);        
    }
    void Update()
    {
        LoopController.S.RunUpdate();
    }
    void FixedUpdate()
    {
        LoopController.S.RunFixedUpdate();
    }
    void LateUpdate()
    {
        LoopController.S.RunLateUpdate();
    }
}
