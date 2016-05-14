using UnityEngine;
using System.Collections;

public abstract class MySingleton<T> : MyBehaviour where T : MySingleton<T>
{
    public static T instance { get; private set; }

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = (T)this;
            base.Awake();
        }
        else
        {
            if (instance != this)
            {
                LogWarning("Instance already exists for " + typeof(T) + " on " + this.gameObject.name + ", destroying.");
                Destroy(gameObject);
            }
        }        
    }
}
