using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace LongXinTool
{
    public class RefWindowCache : ScriptableObject
    {
       //private const string ROOT_DIR = "Packages/com.longxin.refwindow/Editor";
        private const string ROOT_DIR = "Packages/com.longxin.refwindow/Editor";
        private const string CachePath = ROOT_DIR + "/SO/RefWindowCache.asset";
        public const string XMLPath = ROOT_DIR + "/UI/RefWindow.uxml";

        public List<Object> refList;
        public List<string> objNameList;
        public string searchFIeldValue;
        public int columnCount = 1;
        private static RefWindowCache instance;
        public static RefWindowCache Instance
        {
            get
            {
                if (instance == null)
                {
                    ReadFile();
                    if (instance == null)
                    {
                        Create();
                    }
                }
                return instance;
            }
        }
        private static void Create()
        {
            instance = CreateInstance<RefWindowCache>();
            instance.refList = new List<Object>();
            instance.objNameList = new List<string>();
            AssetDatabase.CreateAsset(instance, CachePath);
        }
        private static void ReadFile()
        {
            var objs = InternalEditorUtility.LoadSerializedFileAndForget(CachePath);
            RefWindowCache context = (objs.Length > 0 ? objs[0] : null) as RefWindowCache;
            if (context != null && !context.Equals(null))
                instance = context;
        }
        public void SaveData()
        {
            for (int i = 0; i < refList.Count; i++)
            {
                if (refList[i] == null)
                {
                    refList.Remove(refList[i]);
                    i--;
                }
            }
            for (int i = 0; i < objNameList.Count; i++)
            {
                if (string.IsNullOrEmpty(objNameList[i]))
                {
                    objNameList.Remove(objNameList[i]);
                    i--;
                }
            }
            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { this }, CachePath, true);
        }
        public void AddRefList()
        {
            refList.Add(null);
        }
        public void AddRefList(Object obj)
        {
            refList.Add(obj);
        }  
        public void AddObjNameList(string name)
        {
            objNameList.Add(name);
        }
    }

}