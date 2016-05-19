using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;

public class UniqueIdentifierAttribute : PropertyAttribute { }

namespace StateControl
{
    [ExecuteInEditMode]
    public class SavableObject : MonoBehaviour
    {
        [UniqueIdentifier]
        public string guid;

        static List<string> identifiers = new List<string>();
        public static List<string> GetIdentifiers() { return identifiers; }

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
        bool CheckGUIDAdded()
        {
            return (identifiers.Exists((x) => x == guid));
        }
        bool CheckGUIDConflict()
        {
            bool duplicate = false;
            var ids = identifiers.FindAll((x) => x == guid);
            if (ids.Count > 1)
            {
                duplicate = true;
                string message = "";
                foreach (var i in ids)
                {
                    message += (i + "\n");
                }
                EditorUtility.DisplayDialog("GUID CONFLICT(S) DETECTED", message, "OK");
            }
            return duplicate;
        }
        void RemoveGUIDFromPool()
        {
            identifiers.Remove(guid);
        }

        //==================================================
        //  UNITY METHODS
        //==================================================

        void Awake()
        {
            if (CheckGUIDEmpty()) GetNewGUID();

            if (CheckGUIDAdded())
            {
                string otherObject = identifiers.Find((x) => x == guid);
                bool fixConflict = EditorUtility.DisplayDialog("GUID CONFLICT", 
                                                               "Duplicate GUID Detected:" + "\n" + 
                                                               "Existing Object: " + otherObject + "\n" +
                                                               "New Object: " + this.name,
                                                               "Fix", 
                                                               "Ignore");
                if (fixConflict)
                {
                    GetNewGUID();
                    identifiers.Add(guid);
                }
                else
                {
                    Debug.LogWarning("You have chosen to ignore a GUID conflict. This may result in corrupted savegames. Please fix manually immediately.");
                }                
            }
            else
            {
                identifiers.Add(guid);
            }
            isAwake = true;
        }
        void OnDestroy()
        {
            identifiers.Remove(guid);
        }
    }
}

