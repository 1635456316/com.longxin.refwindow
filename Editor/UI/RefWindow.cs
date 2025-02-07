using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Reflection;


namespace LongXinTool
{
    internal class RefWindow : EditorWindow
    {
        //路径 
        private string ProjectDir;

        //控件 
        private RefWindowCache cache;
        private VisualElement root;
        private VisualElement menuIcon;
        private VisualElement RefField;
        private VisualElement objNameField;
        private ToolbarSearchField searchField;
        private Button searchBtn;
        private ObjectField inputField;
        private Button addBtn;
        private Button saveBtn;

        private string defaultLabel = "保存或退出后删除";
        private string inputFieldText = "拖到这里生成引用";

        [MenuItem("My Tool/引用窗口")]
        public static void ShowExample()
        {
            //Debug.Log("打开引用窗口");
            RefWindow wnd = GetWindow<RefWindow>();
            wnd.titleContent = new GUIContent("RefWindow");
        } 
        
        
        //[MenuItem("TestbedTool/关闭引用窗口")]
        public static void Closes()
        {
            bool wndOpened = HasOpenInstances<RefWindow>();
            if (wndOpened)
            {
                RefWindow wnd = GetWindow<RefWindow>();
                wnd.Close();
            }
        }
        private void OnDestroy()
        {
            if (cache)
            {
                cache.SaveData();
            }
        }
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;
            // Import UXML
            try
            {
                var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RefWindowCache.XMLPath);
                visualTree.CloneTree(root);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return;
            }
            InitDir();
            GetControls();
            InitElementStyle();
            RegisterEvt();
            Init();
        }
        private void InitDir()
        {
            ProjectDir = Application.dataPath + "/..";
        }
        private void GetControls()
        {
            menuIcon = root.Q<VisualElement>("menuIcon");
            RefField = root.Q<VisualElement>("RefField");
            objNameField = root.Q<VisualElement>("objNameField");
            searchField = root.Q<ToolbarSearchField>("searchField");
            searchBtn = root.Q<Button>("searchBtn");
            inputField = root.Q<ObjectField>("inputField");
            //addBtn = root.Q<Button>("addBtn");
            saveBtn = root.Q<Button>("BtnSave");
        }
        private void RegisterEvt()
        {
            menuIcon.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.RightMouse)
                {
                    ShowMenu();
                }
            });
            // addBtn.clicked += () =>
            // {
            //     cache.AddRefList();
            //     RefreshRefList();
            // };
            saveBtn.clicked += () =>
            {
                cache.SaveData();
                RefreshRefList();
                RefreshObjNameList();
            };
            inputField.objectType = typeof(Object);
            inputField.RegisterValueChangedCallback((evt) =>
            {
                if (AssetDatabase.IsMainAsset(evt.newValue))
                {
                    cache.AddRefList(evt.newValue);
                    RefreshRefList();
                }
                else
                {
                    AddObjNameList(evt.newValue);
                    RefreshObjNameList();
                }
                inputField.SetValueWithoutNotify(null);
                inputField.Q<Label>().text = inputFieldText;
            });
            searchField.RegisterCallback<KeyDownEvent>((evt) =>
            {
                if (//searchField.focusController.focusedElement == searchField &&
                        evt.keyCode == KeyCode.Return)
                {
                    OnSearch();
                }
            });
            searchBtn.clicked += OnSearch;
        }
        private void InitElementStyle()
        {
            inputField.Q<Label>().text = inputFieldText;
        }
        private void Init()
        {
            cache = RefWindowCache.Instance;
            RefreshRefList();
            RefreshObjNameList();
            searchField.value = cache.searchFIeldValue;
        }
        private void RefreshRefList()
        {
            RefField.Clear();
            List<Object> list = cache.refList;
            for (int i = 0; i < list.Count; i++)
            {
                ObjectField objfield = new ObjectField();
                objfield.style.width = new StyleLength(new Length(100f/cache.columnCount, LengthUnit.Percent));
                objfield.style.marginBottom = objfield.style.marginLeft =
                    objfield.style.marginRight = objfield.style.marginTop = 0;
                objfield.objectType = typeof(Object);

                //objfield.label = list[i] ? list[i].name : defaultLabel;
                objfield.value = list[i];

                int index = i;
                objfield.RegisterValueChangedCallback((evt) =>
                {
                    //Debug.Log("值改变");
                    list[index] = evt.newValue;
                    //objfield.label = evt.newValue ? evt.newValue.name : defaultLabel;
                });
                objfield.RegisterCallback<MouseDownEvent>((evt) =>
                {
                    if (evt.button == (int)MouseButton.RightMouse && evt.clickCount >= 2)
                    {
                        //这里触发了值改变函数
                        objfield.value = null;
                    }
                    if (evt.button == (int)MouseButton.MiddleMouse)
                    {
                        if (objfield.value)
                        {
                            searchField.SetValueWithoutNotify(objfield.value.name);
                            cache.searchFIeldValue = searchField.value;
                        }
                    }

                });
                RefField.Add(objfield);
            }
        }
        private void RefreshObjNameList()
        {
            objNameField.Clear();
            List<string> list = cache.objNameList;
            for (int i = 0; i < list.Count; i++)
            {
                Label label = new Label();
                label.text = GetHighRichName(list[i]);
                label.style.color = new Color(0.0627451f, 0.6352941f, 0.7647059f, 1);
                int index = i;
                label.RegisterCallback<MouseDownEvent>((evt) =>
                {
                    if (evt.button == (int)MouseButton.LeftMouse)
                    {
                        Search(list[index]);
                    }
                    if (evt.button == (int)MouseButton.RightMouse && evt.clickCount >= 2)
                    {
                        list[index] = null;
                        label.text = defaultLabel;
                    }
                    if (evt.button == (int)MouseButton.MiddleMouse)
                    {
                        if (!string.IsNullOrEmpty(list[index]))
                        {
                            searchField.SetValueWithoutNotify(list[index]);
                            cache.searchFIeldValue = searchField.value;
                        }
                    }
                });
                objNameField.Add(label);
            }
        }
        private string GetHighRichName(string str)
        {
            string[] arr = str.Split('/');
            string oldName = arr[arr.Length - 1];
            string newName = oldName ;
            return newName;
        }
        private void AddObjNameList(Object obj)
        {
            if (!(obj is GameObject))
            {
                return;
            }
            Transform trans = (obj as GameObject).transform;
            string allPath = trans.gameObject.name;
            int i = 0;
            //最多找15层
            while (trans.parent && i < 15)
            {
                trans = trans.parent;
                allPath = $"{trans.gameObject.name}/{allPath}";
                i++;
            }
            cache.AddObjNameList(allPath);
        }
        private void OnSearch()
        {
            cache.searchFIeldValue = searchField.value;
            Search(searchField.value);
        }
        private void Search(string path)
        {
            //完整路径找一次
            GameObject go = GameObject.Find(path);
            if (go)
            {
                EditorGUIUtility.PingObject(go);
                //Selection.activeGameObject = go;
            }
            else
            {
                //用名字直接找一次
                string[] arr = path.Split('/');
                string name = arr[arr.Length - 1];
                go = GameObject.Find(name);
                if (go)
                {
                    EditorGUIUtility.PingObject(go);
                }
                else
                {
                    SearchHierarchy(name);
                }

            }
        }
        private void SearchHierarchy(string name)
        {
            // 获取Hierarchy窗口的类型
            System.Type hierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

            // 获取Hierarchy窗口实例
            EditorWindow hierarchyWindow = EditorWindow.GetWindow(hierarchyWindowType);

            // 获取Hierarchy窗口中的搜索方法
            var searchMethod = hierarchyWindowType.GetMethod("SetSearchFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // 调用搜索方法
            searchMethod.Invoke(hierarchyWindow, new object[] { name, 0, false, false });
            //检查结果
            var getCurrentVisibleObjectsMethod = hierarchyWindowType.GetMethod("GetCurrentVisibleObjects",BindingFlags.Public | BindingFlags.Instance);
            if (getCurrentVisibleObjectsMethod == null)
            {
                CancelSearch(searchMethod, hierarchyWindow);
                return;
            }
            var searchResults = (string[])getCurrentVisibleObjectsMethod.Invoke(hierarchyWindow, null);

            if (searchResults.Length == 0)
            {
                // 结果为空，取消搜索
                CancelSearch(searchMethod, hierarchyWindow);
                return;
            }
            else
            {
                if (searchResults.Length <= 2)
                {
                    GameObject go = GameObject.Find(searchResults[searchResults.Length-1]);
                    if (go)
                    {
                        EditorGUIUtility.PingObject(go);
                        CancelSearch(searchMethod, hierarchyWindow);
                        return;
                    }
                }
            }
        }
        private void CancelSearch(MethodInfo searchMethod, EditorWindow hierarchyWindow)
        {
            searchMethod.Invoke(hierarchyWindow, new object[] { "", 0, false, false });
        }
        private void ShowMenu()
        {
            //Debug.Log("ShowMenu");
            GenericMenu genericMenu = new GenericMenu();
            if (cache.columnCount == 1)
            {
                genericMenu.AddItem(new GUIContent("切换双列"), false, () =>
                {
                    cache.columnCount = 2;
                    RefreshRefList();
                });
            }
            else
            {
                genericMenu.AddItem(new GUIContent("切换单列"), false, () =>
                {
                    cache.columnCount = 1;
                    RefreshRefList();
                });
            }
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("打开工程目录"), false, () => { Application.OpenURL(ProjectDir); });
            //genericMenu.AddSeparator("");
            //genericMenu.AddItem(new GUIContent("打开Excel文件夹"), false, OpenExcelDir);
            genericMenu.ShowAsContext();
        }
      
    }
}