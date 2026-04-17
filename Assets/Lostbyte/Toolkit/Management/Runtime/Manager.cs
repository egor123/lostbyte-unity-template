using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Management
{
    public abstract class Manager<T> : Manager where T : Manager
    {

        private static T _instance;
        public static T Instance
        {
            get
            {
                if (Quitting) return null;
                if (_instance) return Instance;
                //TODO management scene loading
                return _instance = new GameObject($"({nameof(Manager)}){typeof(T)}")
                           .AddComponent<T>();
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnRuntimeMethodLoad()
        {
            _instance = null;
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject); //Optional???
            OnAwake();
        }

        protected virtual void OnAwake() { }

    }
    public abstract class Manager : MonoBehaviour
    {
        public static bool Quitting { get; private set; }
        public static bool Avalible => !Quitting; //TODO ???
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnRuntimeMethodLoad()
        {
            Quitting = false;
        }
        private void OnApplicationQuit()
        {
            Quitting = true;
        }

    }
}
