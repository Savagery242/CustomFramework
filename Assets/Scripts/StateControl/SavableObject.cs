using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UniqueIdentifierAttribute : PropertyAttribute { }

namespace StateControl
{
    [ExecuteInEditMode]
    public class SavableObject : MonoBehaviour
    {
        [UniqueIdentifier]
        public string uniqueID;

        static List<SavableObject> savableObjects = new List<SavableObject>();

        static List<string> identifiers = new List<string>();
        public static List<string> GetIdentifiers() { return identifiers; }

        //==================================================
        //  PRIVATE FIELDS
        //==================================================

        bool isQuitting;

        //==================================================
        //  PRIVATE METHODS
        //==================================================

        void GetNewGUID()
        {
            Guid guid = Guid.NewGuid();
            uniqueID = guid.ToString();
            Debug.LogWarning("New GUID Assigned to " + gameObject.name);
        }

        void CheckGUIDEmpty()
        {
            if (uniqueID == "" || string.IsNullOrEmpty(uniqueID))
            {
                GetNewGUID();
                identifiers.Add(uniqueID);
                return;
            }
        }
        void CheckGUIDAdded()
        {
            if (!identifiers.Exists((x) => x == uniqueID))
            {
                identifiers.Add(uniqueID);
            }
        }

        //==================================================
        //  UNITY METHODS
        //==================================================

       

        void OnValidate()
        {
            CheckGUIDEmpty();
            CheckGUIDAdded();
        }
        void Awake()
        {
            if (savableObjects.Exists((x) => x != this && x.uniqueID == this.uniqueID))
            {
                Debug.Log("Clash!");
            }
            if (!savableObjects.Contains(this)) savableObjects.Add(this);
        }

        void OnApplicationQuit()
        {
            isQuitting = true;
        }
        void OnDestroy()
        {
            if (Application.isPlaying && !isQuitting && !StateController.isLoading)
            {
                StateController.AddToDestroyedPool(this); // Create backup object
            }

        }
    }
}

