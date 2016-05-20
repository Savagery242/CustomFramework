using UnityEngine;
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

    public class StateController : MonoBehaviour
    {
        const string fileName = "/playerInfo.dat";

        //==================================================
        //  SINGLETON
        //==================================================

        public static StateController instance { get; private set; }

        //==================================================
        //  STATIC
        //==================================================

        public static bool isLoading { get; private set; }

        static List<SavableObject> destroyedObjectPool = new List<SavableObject>();

        public static void AddToDestroyedPool(SavableObject so)
        {
            SavableObject oldSO = destroyedObjectPool.Find((x) => x.guid == so.guid);
            if (oldSO != null)
            {
                Debug.Log("object already exists, removing old copy");
                destroyedObjectPool.Remove(oldSO);
            }

            GameObject newGO = GameObject.Instantiate(so.gameObject);
            newGO.name = so.gameObject.name;
            newGO.transform.parent = so.gameObject.transform.parent;
            SavableObject newSO = newGO.GetComponent<SavableObject>();
            newSO.guid = so.guid;
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

        //==================================================
        //  SAVE
        //==================================================   


        public void SaveState()
        {
            string path = Application.persistentDataPath + fileName;

            SaveGame state = new SaveGame();

            //--------------------------------------------------
            //  Discover open scenes
            //--------------------------------------------------

            state.openScenes = SceneUtils.GetOpenScenes().GetNames();

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
            using (file = File.Open(path, FileMode.Create))
            {
                try
                {
                    bf.Serialize(file, state);
                }
                catch
                {
                    Debug.LogError("Save Error: Could not serialize object!");
                    throw;
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

            Debug.Log("loading");

            //--------------------------------------------------
            //  Open File and get state
            //--------------------------------------------------

            string path = Application.persistentDataPath + fileName;
            
            SaveGame state;
            BinaryFormatter bf = GetBinaryFormatter();

            if (File.Exists(path))
            {
                FileStream file;
                using (file = File.Open(path, FileMode.Open))
                {
                    try
                    {
                        state = (SaveGame)bf.Deserialize(file);
                    }
                    catch
                    {
                        Debug.LogError("Load Error: Could not deserialize object!");
                        throw;
                    }
                }
            }
            else
            {
                Debug.LogError("Error opening file: File Not Found (" + path + ")");
                yield break;
            }

            //--------------------------------------------------
            // Restore scene layout
            //--------------------------------------------------

            /*

            If it's the editor, just close all scenes after prompting, and open them fresh.

            */

            Scene managerScene = gameObject.scene; // Assume this class is on an object in the manager scene; never load/unload this scene.
            var openScenes = SceneUtils.GetOpenScenes();

            //--------------------------------------------------
            //  Close current scenes (except Manager scene)
            //--------------------------------------------------

            var scenesToClose = new List<Scene>();
            foreach (var s in openScenes)
            {
                if (s == managerScene) continue; // Do not close manager scene, of which this object is a part
                SceneManager.UnloadScene(s.name);
            }

            //--------------------------------------------------
            //  Open up new set of scenes
            //--------------------------------------------------

            var scenesToOpen = new List<Scene>();

            foreach (var s in state.openScenes)
            {
                if (s == managerScene.name) continue;
                AsyncOperation ao = SceneManager.LoadSceneAsync(s, LoadSceneMode.Additive);
                while (!ao.isDone) { yield return null; }
            }
            
            //--------------------------------------------------
            //  Restore Objects
            //--------------------------------------------------

            var loadedObjects = state.goData;
            var savableObjects = SavableObject.GetIdentifiers();
            var orphans = new List<GameObjectData>();

            foreach (var o in loadedObjects)
            {
                SavableObject destObject = savableObjects.Find((x) => x.guid == o.guid); // Find object by GUID
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


        static GameObjectData BuildGOD(SavableObject so)
        {
            GameObjectData god = new GameObjectData();
            god.SetTransform(so.transform);
            god.guid = so.guid;
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

        //==================================================
        //  UNITY METHODS
        //==================================================

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Duplicate StateController found, destroying...");
                Destroy(this);
            }
        }

    }
}


