using UnityEngine;
using System.Collections.Generic;

//==================================================
//  Data for entire GameObject
//==================================================

namespace StateControl
{
    [System.Serializable]
    public class GameObjectData
    {
        public string guid;
        public List<MonoBehaviourData> behaviourData = new List<MonoBehaviourData>(); // Components

        // Transform

        public SVector3 localPosition;
        public SVector3 localScale;
        public SQuaternion localRotation;

        public void GetTransform(Transform t)
        {
            t.localPosition = this.localPosition;
            t.localScale = this.localScale;
            t.localRotation = this.localRotation;
        }

        public void SetTransform(Transform t)
        {
            localPosition = t.localPosition;
            localScale = t.localScale;
            localRotation = t.localRotation;
        }

        public System.Type[] GetComponentArray()
        {
            var returnVal = new System.Type[behaviourData.Count];
            for (int i = 0; i < behaviourData.Count; ++i)
            {
                returnVal[i] = behaviourData[i].behaviourType;
            }
            return returnVal;
        }
    }
}
