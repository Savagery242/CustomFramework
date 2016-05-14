/*
LoopController should be set to run before any MyBehaviours in "Script Execution Order"
to ensure the singleton is created before it's accessed
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

[System.Serializable]
public class LoopController
{
    static LoopController _S;
    public static LoopController S
    { get { return (_S != null) ? _S : (_S = new LoopController()); }}

    //==================================================
    //  PUBLIC STATIC
    //==================================================

    //--------------------------------------------------
    //  Logging Flags, set from Editor Menu
    //--------------------------------------------------

    public static bool logMessages { get { return EditorPrefs.GetBool(MyStrings.menu_Logging_LogMessages, false); }}
    public static bool logWarnings { get { return EditorPrefs.GetBool(MyStrings.menu_Logging_LogWarnings, false); }}
    public static bool logErrors   { get { return EditorPrefs.GetBool(MyStrings.menu_Logging_LogErrors, false); }}

    //==================================================
    //  PRIVATE
    //==================================================

    List<LoopElement> updates      = new List<LoopElement>();
    List<LoopElement> fixedUpdates = new List<LoopElement>();
    List<LoopElement> lateUpdates  = new List<LoopElement>();

    LCHook lcGO;

    //==================================================
    //  CONSTRUCTOR
    //==================================================

    public LoopController()
    {
        LCHook currentLCGO = GameObject.FindObjectOfType<LCHook>();
        if (currentLCGO != null)
        {
            Debug.Log("Existing loop interface found");
            lcGO = currentLCGO;
        }
        else
        {
            Debug.Log("No Existing loop interface found, creating new");
            Type[] components = new Type[1] { typeof(LCHook) };
            GameObject go = new GameObject("** Loop Controller Interface **", components);
            lcGO = go.GetComponent<LCHook>();
            go.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    //==================================================
    //  PUBLIC METHODS
    //==================================================

    public void RegisterLoops(MyBehaviour MB)
    {

        //--------------------------------------------------
        //  LOOP METHODS
        //--------------------------------------------------

        MethodInfo U = MB.GetType().GetMethod("MyUpdate",
                                              BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);
        MethodInfo FU = MB.GetType().GetMethod("MyFixedUpdate",
                                              BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);
        MethodInfo LU = MB.GetType().GetMethod("MyLateUpdate",
                                              BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);

        //--------------------------------------------------
        //  LOOP CONDITIONS
        //-------------------------------------------------- 

        MethodInfo UC = MB.GetType().GetMethod("UpdateCondition",
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Instance); 
        MethodInfo FUC = MB.GetType().GetMethod("FixedUpdateCondition",
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Instance); 
        MethodInfo LUC = MB.GetType().GetMethod("LateUpdateCondition",
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Instance);

        //--------------------------------------------------
        //  CREATE CONDITION DELEGATES
        //--------------------------------------------------

        Func<bool> runUpdate      = () => true;
        Func<bool> runFixedUpdate = () => true;
        Func<bool> runLateUpdate  = () => true;
        
        // Check if conditions return boolean

        if (UC != null && !CheckReturnType(MB, UC))    UC = null;
        if (FUC != null && !CheckReturnType(MB, FUC))  FUC = null;
        if (LUC != null && !CheckReturnType(MB, LUC))  LUC = null;

        if (UC != null) runUpdate       = () => (bool)UC.Invoke(MB, null);
        if (FUC != null) runFixedUpdate = () => (bool)FUC.Invoke(MB, null);
        if (LUC != null) runLateUpdate  = () => (bool)LUC.Invoke(MB, null);


        // Create an Action delegate for running loop methods and insert
        // into their respective lists.

        if (U != null && U.GetParameters().Length == 0)
        {
            Action act = (Action)Delegate.CreateDelegate(typeof(Action), MB, U);
            LoopElement element = new LoopElement(MB,
                                                    act,
                                                    MB.updatePriority,
                                                    runUpdate);
            InsertIntoList(updates, element);     
        }
        if (FU != null && FU.GetParameters().Length == 0)
        {
            Action act = (Action)Delegate.CreateDelegate(typeof(Action), MB, FU);
            LoopElement element = new LoopElement(MB,
                                                  act,
                                                  MB.fixedUpdatePriority,
                                                  runFixedUpdate);
            InsertIntoList(fixedUpdates, element);            
        }
        if (LU != null && LU.GetParameters().Length == 0)
        {
            Action act = (Action)Delegate.CreateDelegate(typeof(Action), MB, LU);
            LoopElement element = new LoopElement(MB,
                                                  act,
                                                  MB.lateUpdatePriority,
                                                  runLateUpdate);
            InsertIntoList(lateUpdates, element);
        }                
    }

    public void RunUpdate()
    {
        RunLoop(updates);
    }
    public void RunFixedUpdate()
    {
        RunLoop(fixedUpdates);
    }
    public void RunLateUpdate()
    {
        RunLoop(lateUpdates);
    }

    //==================================================
    //  PRIVATE METHODS
    //==================================================

    bool CheckReturnType(MyBehaviour MB, MethodInfo MI)
    {
        bool result;
        if (MI.ReturnType != typeof(bool))
        {            
            Debug.LogError(error_NonBoolReturnType + " in: " + MB.name + " , for method " + MI.Name);
            result = false;
        }
        else
        {
            result = true;
        }
        return result;
    }

    void RunLoop(List<LoopElement> elements)
    {        
        bool rebuild = false;
        for (int i = 0; i < elements.Count; ++i)
        {
            if (elements[i].behaviour == null)  // Rebuild if an element has become null (has been removed or destroyed)
            {
                rebuild = true;
                continue;
            }
            if (!elements[i].behaviour.enabled || !elements[i].behaviour.gameObject.activeSelf) // Skip if inactive or disabled
            {
                continue;
            }
            if (elements[i].condition == null || elements[i].condition.Invoke()) // Invoke action if condition is null or false
            {
                elements[i].method.Invoke();
            }
        }
        if (rebuild)
        {
            elements.RemoveAll((x) => x.behaviour == null);
        }
    }

    void InsertIntoList(List<LoopElement> elements, LoopElement element)
    {
        var index = elements.BinarySearch(element);
        if (index < 0) index = ~index;
        elements.Insert(index, element);
    }

    //==================================================
    //  UNITY METHODS
    //==================================================

    private struct LoopElement : IComparable<LoopElement>
    {
        public MyBehaviour behaviour { get; private set; }  // Reference to the MyBehaviour
        public Action      method { get; private set; }     // Method to run (MyUpdate, MyFixedUpdate, MyLateUpdate)
        public Func<bool>  condition { get; private set; }  // bool check whether to run the method
        public int         order { get; private set; }      // determines sort order in list, and order of execution.

        public LoopElement(MyBehaviour behaviour, Action method, int order = 0, Func<bool> condition = null)
        {
            this.behaviour = behaviour;
            this.method    = method;
            this.condition = condition;
            this.order     = order;            
        }

        public int CompareTo(LoopElement other)
        {
            return order.CompareTo(other.order);
        }
    }

    //==================================================
    //  ERROR MESSAGES
    //==================================================

    const string error_NonBoolReturnType = "Return type of method invalid. Return type must be boolean.";

}





