﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace StateControl
{
    //==================================================
    // GameState - This is what's written to file
    //==================================================

    [System.Serializable]
    public class SaveGame
    {
        public string[] openScenes;
        public List<GameObjectData> goData = new List<GameObjectData>();
        public bool isSaved;
    }

    public class StateController : MyBehaviour
    {
        //==================================================
        //  SINGLETON
        //==================================================

        static StateController _instance;
        public static StateController instance
        {
            get { return _instance ?? (_instance = new StateController()); }
        }

        //==================================================
        //  STATIC
        //==================================================

        public static bool isLoading { get; private set; }

        static List<SavableObject> destroyedObjectPool = new List<SavableObject>();

        public static void AddToDestroyedPool(SavableObject so)
        {
            SavableObject oldSO = destroyedObjectPool.Find((x) => x.uniqueID == so.uniqueID);
            if (oldSO != null)
            {
                Debug.Log("object already exists, removing old copy");
                destroyedObjectPool.Remove(oldSO);
            }

            GameObject newGO = GameObject.Instantiate(so.gameObject);
            newGO.name = so.gameObject.name;
            newGO.transform.parent = so.gameObject.transform.parent;
            SavableObject newSO = newGO.GetComponent<SavableObject>();
            newSO.uniqueID = so.uniqueID;
            destroyedObjectPool.Add(newSO);
            newGO.SetActive(false);
        }

        public static SavableObject[] GetAllSavableObjects()
        {
            var savableObjects = new List<SavableObject>();
            savableObjects.AddRange(Object.FindObjectsOfType<SavableObject>());
            savableObjects.AddRange(destroyedObjectPool);
            return savableObjects.ToArray();
        }

        /*
        public static SavableObject GetFromDestroyedPool(string guid)
        {
            return (destroyedObjectPool.Find((x) => x.uniqueID == guid));
        }
        */

        //==================================================
        //  SAVE
        //==================================================   


        public void SaveState()
        {
            SaveGame state = new SaveGame();

            //--------------------------------------------------
            //  Discover open scenes
            //--------------------------------------------------

            state.openScenes = FindOpenScenes();

            //--------------------------------------------------
            //  Gather object information
            //--------------------------------------------------

            var savableObjects = new List<SavableObject>();
            savableObjects.AddRange(GetAllSavableObjects());
            foreach (var o in savableObjects)
            {
                state.goData.Add(BuildGOD(o));
            }
            state.isSaved = true;

            //--------------------------------------------------
            //  Write to file
            //--------------------------------------------------

            BinaryFormatter bf = GetBinaryFormatter();
            FileStream file;
            using (file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Create))
            {
                try
                {
                    bf.Serialize(file, state);
                }
                catch
                {
                    Debug.LogError("Could not save game!");
                    throw;
                }
                finally
                {
                    file.Close();
                }
            }
            Debug.Log("Game Saved! " + Application.persistentDataPath);
        }

        //==================================================
        //  LOAD
        //================================================== 

        public void LoadState()
        {
            StartCoroutine(Load());
        }

        IEnumerator Load()
        {
            //--------------------------------------------------
            //  Open File and get state
            //--------------------------------------------------

            SaveGame state;
            BinaryFormatter bf = GetBinaryFormatter();

            if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
            {

                FileStream file;
                using (file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open))
                {
                    try
                    {
                        state = (SaveGame)bf.Deserialize(file);
                    }
                    catch
                    {
                        Debug.LogError("Could not load game!");
                        throw;
                    }
                    finally
                    {
                        file.Close();
                    }
                }
            }
            else
            {
                yield break;
            }

            //--------------------------------------------------
            // Restore scene layout
            //--------------------------------------------------

            /*

            If it's the editor, just close all scenes after prompting, and open them fresh.

            */

            isLoading = true;

            var desiredOpenScenes = new List<string>();
            var currentOpenScenes = new List<string>();
            var scenesToOpen = new List<string>();
            var scenesToClose = new List<string>();

            desiredOpenScenes.AddRange(state.openScenes);
            currentOpenScenes.AddRange(FindOpenScenes());

            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);

            yield return new WaitForEndOfFrame();

            foreach (var s in currentOpenScenes)
            {
                Debug.Log(s);

                //if (s == "Scene01")
                SceneManager.UnloadScene(s);
                yield return new WaitForEndOfFrame();
            }
            foreach (var s in desiredOpenScenes)
            {
                SceneManager.LoadScene(s, LoadSceneMode.Additive);
                yield return new WaitForEndOfFrame();
            }

            SceneManager.UnloadScene("Loading");

            yield return new WaitForEndOfFrame();

            //isLoading = false;

            /*

            bool scenesMatch = true;

            if (currentOpenScenes.Count != desiredOpenScenes.Count)
            {
                scenesMatch = false;
            }
            else if (!(desiredOpenScenes.TrueForAll((x) => currentOpenScenes.Contains(x))))
            {
                scenesMatch = false;
            }       

            if (!scenesMatch)
            {
                foreach (var s in desiredOpenScenes)
                {
                    if (!currentOpenScenes.Contains(s)) scenesToOpen.Add(s);
                }
                foreach (var s in currentOpenScenes)
                {
                    if (!desiredOpenScenes.Contains(s)) scenesToClose.Add(s);
                }
                foreach (var s in scenesToClose)
                {
                    string sceneName = SceneManager.GetSceneByPath(s).name;
                    if (!Application.isPlaying)
                    {
                        Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(s);
                        UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                    }
                    else
                    {                    
                        SceneManager.UnloadScene(sceneName);
                    }                
                }
                foreach (var s in scenesToOpen)
                {
                    string sceneName = SceneManager.GetSceneByPath(s).name;
                    if (!Application.isPlaying)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(s, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                    }
                    else
                    {
                        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                    }               
                }
            }

            */

            //--------------------------------------------------
            //  Restore Objects
            //--------------------------------------------------

            var loadedObjects = state.goData;
            var savableObjects = new List<SavableObject>();
            var orphans = new List<GameObjectData>();

            savableObjects.AddRange(GetAllSavableObjects());

            foreach (var o in loadedObjects)
            {
                SavableObject destObject = savableObjects.Find((x) => x.uniqueID == o.guid); // Find object by GUID
                if (destObject == null)
                {
                    orphans.Add(o);
                    continue;
                }

                destObject.gameObject.SetActive(true);
                o.GetTransform(destObject.transform);
                var components = destObject.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c is SavableObject) continue;
                    MonoBehaviourData data = o.behaviourData.Find((x) => x.behaviourType == c.GetType());
                    if (data == null)
                    {
                        Debug.LogWarning("Load failed for object " + c.name);
                        continue;
                    }

                    FieldInfo[] fields;
                    PropertyInfo[] props;

                    GetComponentVars(c, out fields, out props);

                    foreach (var f in fields)
                    {
                        if (data.fieldDict.ContainsKey(f.Name)) f.SetValue(c, data.fieldDict[f.Name]);
                        else Debug.Log("key not found " + f.Name);
                    }
                    foreach (var p in props)
                    {
                        if (data.propDict.ContainsKey(p.Name)) p.SetValue(c, data.propDict[p.Name], null);
                        else Debug.Log("key not found");
                    }
                }

            }

            //--------------------------------------------------
            // Handle orphaned objects, which were saved
            // but not found upon loading
            //--------------------------------------------------

            int numOrphans = orphans.Count;
            Debug.Log("Load Successful! Number of orphans: " + numOrphans);

        }


        //==================================================
        //  HELPER METHODS
        //==================================================

        string[] FindOpenScenes()
        {
            string[] openScenes = new string[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                openScenes[i] = SceneManager.GetSceneAt(i).name;
            }
            return openScenes;
        }

        static GameObjectData BuildGOD(SavableObject so)
        {
            GameObjectData god = new GameObjectData();
            god.SetTransform(so.transform);
            god.guid = so.uniqueID;
            var behaviours = so.GetComponents<Component>();
            foreach (var b in behaviours)
            {
                if (b is SavableObject) continue;
                god.behaviourData.Add(new MonoBehaviourData(b));
            }
            return god;
        }

        public BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector ss = new SurrogateSelector();

            var v3ss = new Vector3SerializationSurrogate();
            var v2ss = new Vector2SerializationSurrogate();
            var qss = new QuaternionSerializationSurrogate();
            var css = new ColorSerializationSurrogate();
            var mss = new MeshSerializationSurrogate();
            var goss = new GameObjectSerializationSurrogate();

            ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);
            ss.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), v2ss);
            ss.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), qss);
            ss.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), css);
            ss.AddSurrogate(typeof(Mesh), new StreamingContext(StreamingContextStates.All), mss);
            ss.AddSurrogate(typeof(GameObject), new StreamingContext(StreamingContextStates.All), goss);

            // Have the formatter use our surrogate selector
            bf.SurrogateSelector = ss;

            return bf;
        }

        public static void GetComponentVars(Component m, out FieldInfo[] fields, out PropertyInfo[] props)
        {
            System.Type t = m.GetType();

            var filteredFields = new List<FieldInfo>();
            var filteredProps = new List<PropertyInfo>();

            //--------------------------------------------------
            //  Get all fields and properties on component
            //--------------------------------------------------

            FieldInfo[] allFields = t.GetFields(BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Static |
                                                        BindingFlags.Instance);

            PropertyInfo[] allProps = t.GetProperties(BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Static |
                                                        BindingFlags.Instance);

            //--------------------------------------------------
            //  Filter fields and props by what's serializable
            //  Item is filtered when these conditions are met:
            //  - Private vars on non-monobehaviours
            //  - Non-Serializable types
            //  - Properties without a public setter
            //--------------------------------------------------

            foreach (var f in allFields)
            {
                //Ignore private fields on non-monobehaviours (Unity components etc)
                if (!(m is MonoBehaviour) && !f.IsPublic) continue;
                // Check values to see if serializable (or has surrogate)
                if (f.FieldType.GetInterface(typeof(IEnumerable<>).FullName) != null)
                {
                    System.Type elementType = f.FieldType.GetElementType();
                    if (elementType == null) elementType = f.FieldType.GetGenericArguments()[0];

                    Debug.Log(f.FieldType.GetElementType() + f.Name);
                    if (elementType == null) continue;
                    if (!elementType.IsSerializable && !IsAllowedType(elementType, f.Name)) continue;
                }
                if (!f.FieldType.IsSerializable && !IsAllowedType(f.FieldType, f.Name)) continue;
                // Check collections to see if contained types are serializable (or have surrogate)

                filteredFields.Add(f);
            }
            foreach (var p in allProps)
            {
                if (!p.CanWrite) continue;
                //Ignore private fields on non-monobehaviours (Unity components etc) and readonly properties
                if (!(m is MonoBehaviour) && !(p.CanWrite && p.GetSetMethod(/*nonPublic*/ true).IsPublic)) continue;
                // Check values to see if serializable (or has surrogate)
                if (!p.PropertyType.IsSerializable && !IsAllowedType(p.PropertyType, p.Name)) continue;
                // Check collections to see if contained types are serializable (or have surrogate)
                if (p.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null)
                {
                    System.Type elementType = p.PropertyType.GetElementType();
                    if (elementType == null) continue;
                    if (!elementType.IsSerializable && !IsAllowedType(elementType, p.Name)) continue;
                }

                filteredProps.Add(p);
            }

            //--------------------------------------------------
            //  Assign back to output arrays
            //--------------------------------------------------

            fields = filteredFields.ToArray();
            props = filteredProps.ToArray();

        }

        static bool IsAllowedType(System.Type t, string name)
        {
            bool returnVal = false;
            if (t == typeof(Vector3) ||
                t == typeof(Vector2) ||
                t == typeof(Quaternion) ||
                t == typeof(GameObject) ||
                t == typeof(Color))
            {
                returnVal = true;
            }
            return returnVal;
        }

    }
}


