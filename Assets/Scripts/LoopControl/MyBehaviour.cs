using UnityEngine;
using System;
using System.Reflection;


public abstract class MyBehaviour : MonoBehaviour
{

    //==================================================
    //  PUBLIC
    //==================================================

    public bool showMessages = false;
    public bool showWarnings = false;

    public int updatePriority = 0;
    public int fixedUpdatePriority = 0;
    public int lateUpdatePriority = 0;

    //==================================================
    //  DEBUG LOGGING
    //==================================================

    protected void LogMessage(string message)
    {        
        if (!showMessages || !LoopController.logMessages) return;
        Debug.Log(message);
    }
    protected void LogWarning(string message)
    {
        if (!showWarnings || !LoopController.logWarnings) return;
        Debug.LogWarning(message);
    }
    protected void LogError(string message)
    {
        // if (!showErrors || !LoopController.logErrors) return;
        Debug.LogError(message);
    }

    //==================================================
    //  ERROR CHECKING
    //==================================================

    void LogErrorOnUnityLoop(string loopName)
    {
        MethodInfo I = GetType().GetMethod(loopName,
                                          BindingFlags.Public |
                                          BindingFlags.NonPublic |
                                          BindingFlags.Instance);

        if (I != null)
        {
            Debug.LogError("Use of Built-In Unity Loops not allowed on a MyBehaviour! Please remove the " + I.Name + " method from " + this.name);
        }
    }

    //==================================================
    //  UNITY METHODS
    //==================================================

    protected virtual void Awake()
    {

        // Check for Unity built-in loops and throw error if exist

        LogErrorOnUnityLoop("Update");
        LogErrorOnUnityLoop("FixedUpdate");
        LogErrorOnUnityLoop("LateUpdate");

        // If there is a MyInit method, run immediately. This should update the run_ and priority_ properties
        // with the desired values

        MethodInfo I = GetType().GetMethod("MyInit",
                                           BindingFlags.Public |
                                           BindingFlags.NonPublic |
                                           BindingFlags.Instance);

        if (I != null && I.GetParameters().Length == 0)
        {
            Action act = (Action)Delegate.CreateDelegate(typeof(Action), this, I);
            act.Invoke();            
        }

        // Register this MyBehaviour with the LoopController

        LoopController.S.RegisterLoops(this);

        // Execute the MyAwake method in this MyBehaviour, if applicable

        MethodInfo A = GetType().GetMethod("MyAwake",
                                           BindingFlags.Public |
                                           BindingFlags.NonPublic |
                                           BindingFlags.Instance);

        if (A != null && A.GetParameters().Length == 0)
        {
            Action act = (Action)Delegate.CreateDelegate(typeof(Action), this, A);
            act.Invoke();            
        }
    }

    void Start()
    {
        // Execute the MyStart method in this MyBehaviour, if applicable

        MethodInfo S = GetType().GetMethod("MyStart",
                                           BindingFlags.Public |
                                           BindingFlags.NonPublic |
                                           BindingFlags.Instance);

        if (S != null && S.GetParameters().Length == 0)
        {
            Action act = (Action)Delegate.CreateDelegate(typeof(Action), this, S);
            act.Invoke();
        }
    }


}

