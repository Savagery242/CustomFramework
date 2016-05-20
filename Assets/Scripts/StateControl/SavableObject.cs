using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

public class UniqueIdentifierAttribute : PropertyAttribute { }

namespace StateControl
{
    [ExecuteInEditMode]
    public class SavableObject : MonoBehaviour
    {
        [UniqueIdentifier]
        public string guid;

        static List<SavableObject> identifiers = new List<SavableObject>();
        public static List<SavableObject> GetIdentifiers() { return identifiers; }

        SavableObject[] GetSavableObjectsInScene()
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            var savables = new List<SavableObject>();
            foreach (var go in roots)
            {
                savables.AddRange(go.GetComponentsInChildren<SavableObject>());
            }
            return savables.ToArray();
        }

        //==================================================
        //  PRIVATE FIELDS
        //==================================================

        bool isAwake;

        //==================================================
        //  PUBLIC METHODS
        //==================================================

        public void AlertDuplicate()
        {
            ShowResolveConflictPopup();
        }

        //==================================================
        //  PRIVATE METHODS
        //==================================================

        void GetNewGUID()
        {
            Guid guid = Guid.NewGuid();
            this.guid = guid.ToString();
            Debug.LogWarning("New GUID Assigned to " + gameObject.name);
        }
        bool CheckGUIDEmpty()
        {
            return (guid == "" || string.IsNullOrEmpty(guid));
        }
        bool CheckAdded()
        {
            return (identifiers.Exists((x) => x.guid == guid && x.gameObject.GetInstanceID() == gameObject.GetInstanceID()));
        }
        bool CheckDuplicate()
        {
            return (identifiers.Exists((x) => x.guid == guid && x.gameObject.GetInstanceID() != gameObject.GetInstanceID()));
        }
        void ShowResolveConflictPopup()
        {
            SavableObject otherObject = identifiers.Find((x) => x.guid == guid && x.gameObject.GetInstanceID() != gameObject.GetInstanceID());
            bool fixConflict = EditorUtility.DisplayDialog("GUID CONFLICT",
                                                           "Duplicate GUID Detected:" + "\n" +
                                                           "Existing Object: " + otherObject.name + "\n" +
                                                           "New Object: " + this.name,
                                                           "Fix",
                                                           "Ignore");
            if (fixConflict)
            {
                GetNewGUID();
            }
            else
            {
                Debug.LogWarning("You have chosen to ignore a GUID conflict. This may result in corrupted savegames. Please fix manually immediately.");
            }
        }

        //==================================================
        //  UNITY METHODS
        //==================================================

        void Awake()
        {
            if (CheckGUIDEmpty()) GetNewGUID();

            if (CheckDuplicate())
            {
                ShowResolveConflictPopup();
            }
            identifiers.Add(this);
            isAwake = true;
        }
        void OnDestroy()
        {
            identifiers.Remove(this);
        }
    }
}

