using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using StateControl;

[InitializeOnLoad]
class EditorBGTimer
{
    //==================================================
    //  PRIVATE
    //==================================================

    static List<TimedEvent> eventList = new List<TimedEvent>();
    static long lastMS;
    static long startTicks;
    static long curMS;
    static long deltaMS;
    static int minResolution;
    static int maxResolution;

    //--------------------------------------------------
    //  CONSTRUCTOR
    //--------------------------------------------------

    static EditorBGTimer()
    {
        EditorApplication.update += TimerLoop;
        startTicks = DateTime.Now.Ticks;

        //--------------------------------------------------
        //  Add Verification Methods
        //--------------------------------------------------

        eventList.Add(new TimedEvent(500, new Action(CheckGUIDConflict))); // Make sure no conflicting GUIDs exist
    }

    //==================================================
    //  VERIFICATION METHODS
    //==================================================

    static void CheckGUIDConflict()
    {
        var ids = SavableObject.GetIdentifiers();
        if (ids.Count > 1)
        {
            ids.Sort((x, y) => x.guid.CompareTo(y.guid));

            string lastString = "not a duplicate";

            bool duplicate = false;
            foreach (var i in ids)
            {
                if (i.guid == lastString)
                {
                    lastString = i.guid;
                    duplicate = true;
                    break;
                }
                lastString = i.guid;
            }

            if (duplicate)
            {
                string message = lastString;
                foreach (var i in ids)
                {
                    i.AlertDuplicate();
                }
                
            } 
        }
    }

    //==================================================
    //  TIMER LOOP
    //==================================================

    static void TimerLoop()
    {
        lastMS = curMS;
        curMS = (DateTime.Now.Ticks - startTicks) / 10000;
        deltaMS = curMS - lastMS;
        for (int i = 0; i < eventList.Count; ++i)
        {
            eventList[i].SetCounter(deltaMS);
        }
    }
}

//==================================================
//  HELPER CLASSES
//==================================================

public class TimedEvent
{
    public void SetCounter(long increment)
    {
        totalCount += increment;
        if (totalCount > frequency)
        {
            totalCount = 0;
            action.Invoke();
        }
    }

    private long totalCount;
    private int frequency;
    private Action action;

    public TimedEvent(int frequency, Action action)
    {
        frequency = Mathf.Clamp(frequency, 50, int.MaxValue); // Cannot be more often than 50ms
        this.frequency = frequency;
        this.action = action;
    }
}