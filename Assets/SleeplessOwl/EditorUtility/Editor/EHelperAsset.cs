using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SleeplessOwl.EditorUtil
{
    public static class EHelperAsset
    {
        [MenuItem("SleeplessOwl/Gameobject Tool/清理Missing component")]
        public static void CleanGameObjectMissingComponent()
        {
            Transform obj = Selection.activeGameObject.transform;
            Clean(obj);


            void Clean(Transform obj)
            {
                if (obj == null)
                    obj = Selection.activeGameObject.transform;

                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj.gameObject);
                for (int i = 0; i < obj.childCount; i++)
                {
                    Clean(obj.GetChild(i));
                }

            }
        }

        public static List<T> FindAssetList<T>(string folderPath) where T : Object
        {

            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });

            var assetList = new List<T>();
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) == false)
                {
                    var so = AssetDatabase.LoadAssetAtPath<T>(path);
                    assetList.Add(so);
                }
            }

            return assetList;
        }

        public static List<T> FindAssetList<T>(string folderPath , Func<T,bool> filter) where T : Object
        {
            var list = FindAssetList<T>(folderPath);

            Assert.IsNotNull(filter);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                T item = list[i];
                if (filter.Invoke(item) == false)
                {
                    list.RemoveAt(i);
                }
            }

            return list;
        }

        public static List<ObjectType> GetSubAssetsOfType<ObjectType>(this Object asset)
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

            List<ObjectType> results = new List<ObjectType>();

            foreach (Object o in objs)
            {
                if (o is ObjectType oo)
                {
                    results.Add(oo);
                }
            }

            return results;
        }
    }
}