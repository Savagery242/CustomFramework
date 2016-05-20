using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;

namespace StateControl
{
    public static class SceneUtils
    {

        public static string[] GetNames(this Scene[] scenes)
        {
            string[] names = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; ++i)
            {
                names[i] = scenes[i].name;
            }
            return names;
        }

        public static Scene[] GetOpenScenes()
        {
            Scene[] openScenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                openScenes[i] = SceneManager.GetSceneAt(i);
            }
            return openScenes;
        }
    }
}

