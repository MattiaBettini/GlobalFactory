using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Utils
{
    public static class PoolManager
    {
        private static Dictionary<Type, Tuple<Pool, object, object>> poolDictionary;

        static PoolManager()
        {
            poolDictionary = new Dictionary<Type, Tuple<Pool, object, object>>();
        }

        public static void Init<T>(GameObject prefab, Action<T> disable, Action<T> enable)
        {
            poolDictionary.Add(typeof(T), new Tuple<Pool, object, object>(new Pool(prefab), disable, enable));
        }

        public static T GetInstance<T>(GameObject GameObject)
            where T: Component
        {
            T component;
            GameObject gameObject = GameObject;
            Tuple<Pool, object, object> tuple = InternalGetPool<T>(GameObject);
            Action<T> enable = tuple.Item3 as Action<T>;

            component = tuple.Item1.Get().GetComponent<T>();
            component.gameObject.SetActive(true);

            enable.Invoke(component);

            return component;
        }

        public static void Recycle<T>(GameObject gameObject)
            where T : Component
        {
            gameObject.SetActive(false);

            Tuple<Pool, object, object> tuple = InternalGetPool<T>(gameObject);

            Action<T> disable = tuple.Item2 as Action<T>;

            disable.Invoke(gameObject.GetComponent<T>());

            tuple.Item1.Recycle(gameObject);
        }

        private static Tuple<Pool, object, object> InternalGetPool<T>(GameObject gameObject)
        {
            if (poolDictionary.ContainsKey(typeof(T)))
                return poolDictionary[typeof(T)];
            else
                throw new NotImplementedException("Pool Not Implemented");
        }

        #region Pool
        public class Pool
        {
            private GameObject TemplatePrefab;
            private Queue<GameObject> poolQueue;

            public Pool(GameObject Template)
            {
                TemplatePrefab = Template;
                poolQueue = new Queue<GameObject>();
            }

            public GameObject Get()
            {
                return poolQueue.Count > 0 ? poolQueue.Dequeue() : GameObject.Instantiate(TemplatePrefab) as GameObject;
            }

            public void Recycle(GameObject other)
            {
                poolQueue.Enqueue(other);
            }
        }
        #endregion

    }
}

