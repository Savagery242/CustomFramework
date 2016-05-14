using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

//==================================================
// Field and property data for MonoBehaviours
// and components individually
//==================================================

namespace StateControl
{
    [System.Serializable]
    public class MonoBehaviourData
    {
        public System.Type behaviourType;

        public Dictionary<string, object> fieldDict = new Dictionary<string, object>();
        public Dictionary<string, object> propDict = new Dictionary<string, object>();

        public MonoBehaviourData(Component m)
        {
            behaviourType = m.GetType();

            FieldInfo[] fields;
            PropertyInfo[] props;

            StateController.GetComponentVars(m, out fields, out props);

            foreach (var f in fields)
            {
                object fieldValue;
                try
                {
                    fieldValue = f.GetValue(m);
                }
                catch (System.Exception e)
                {
                    Debug.Log("(Exception Caught - Unable to get field value) " + e.Message);
                    continue;
                }
                fieldDict.Add(f.Name, fieldValue);
            }
            foreach (var p in props)
            {
                object propValue;
                try
                {
                    propValue = p.GetValue(m, null);
                }
                catch (System.Exception e)
                {
                    Debug.Log("(Exception Caught - Unable to get property value) " + e.Message);
                    continue;
                }
                propDict.Add(p.Name, propValue);
            }
        }
    }
}

