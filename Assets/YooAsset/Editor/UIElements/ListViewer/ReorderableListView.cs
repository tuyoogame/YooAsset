#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可排序列表视图，支持折叠、增删元素和自定义元素渲染
    /// </summary>
    public class ReorderableListView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ReorderableListView, UxmlTraits>
        {
        }
        
        /// <summary>
        /// 制作列表元素的委托
        /// </summary>
        /// <returns>创建的列表元素</returns>
        public delegate VisualElement MakeElementDelegate();

        /// <summary>
        /// 绑定数据到列表元素的委托
        /// </summary>
        /// <param name="element">待绑定的列表元素</param>
        /// <param name="index">数据源中的索引</param>
        public delegate void BindElementDelegate(VisualElement element, int index);

        private Foldout _foldout;
        private ListView _listView;
        private Label _headerLabel;
        private Button _addButton;
        private Button _removeButton;
        private string _headerName = nameof(ReorderableListView);

        /// <summary>
        /// 源数据
        /// </summary>
        public IList SourceData
        {
            set
            {
                if (value is ArrayList)
                    throw new NotSupportedException($"{nameof(SourceData)} does not support {nameof(ArrayList)}.");

                _listView.Clear();
                _listView.ClearSelection();
                _listView.itemsSource = value;
                _listView.Rebuild();
                RefreshFoldoutName();
                RefreshRemoveButton();
            }
            get
            {
                return _listView.itemsSource;
            }
        }

        /// <summary>
        /// 元素固定高度
        /// </summary>
        public float ElementHeight
        {
            set
            {
                _listView.fixedItemHeight = value;
                _listView.Rebuild();
            }
            get
            {
                return _listView.fixedItemHeight;
            }
        }

        /// <summary>
        /// 是否显示增加按钮
        /// </summary>
        public bool DisplayAdd
        {
            set
            {
                UIElementsTools.SetElementVisible(_addButton, value);
            }
            get
            {
                return _addButton.style.visibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// 是否显示移除按钮
        /// </summary>
        public bool DisplayRemove
        {
            set
            {
                UIElementsTools.SetElementVisible(_removeButton, value);
            }
            get
            {
                return _removeButton.style.visibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// 标题名称
        /// </summary>
        public string HeaderName
        {
            set
            {
                _headerName = value;
                RefreshFoldoutName();
            }
            get
            {
                return _headerName;
            }
        }

        /// <summary>
        /// 制作元素的回调
        /// </summary>
        public MakeElementDelegate MakeElementCallback { get; set; }

        /// <summary>
        /// 绑定元素的回调
        /// </summary>
        public BindElementDelegate BindElementCallback { get; set; }


        /// <summary>
        /// 创建可排序列表视图实例，默认启用折叠栏
        /// </summary>
        public ReorderableListView()
        {
            CreateView(true);
        }

        /// <summary>
        /// 创建可排序列表视图实例
        /// </summary>
        /// <param name="foldout">是否启用折叠栏</param>
        public ReorderableListView(bool foldout)
        {
            CreateView(foldout);
        }
        private void CreateView(bool foldout)
        {
            this.style.flexGrow = 1;
            this.style.flexShrink = 1;

            // 折叠栏
            if (foldout)
            {
                _foldout = new Foldout();
                _foldout.style.flexGrow = 1f;
                _foldout.style.flexShrink = 1f;
                _foldout.text = $"{nameof(ReorderableListView)}";
            }
            else
            {
                _headerLabel = new Label();
            }

            // 列表视图
            _listView = new ListView();
            _listView.style.flexGrow = 1;
            _listView.style.flexShrink = 1;
            _listView.reorderable = true;
            _listView.reorderMode = ListViewReorderMode.Animated;
            _listView.makeItem = MakeListViewElement;
            _listView.bindItem = BindListViewElement;
#if UNITY_2022_3_OR_NEWER
            _listView.selectionChanged += OnSelectionChanged;
#elif UNITY_2020_1_OR_NEWER
            _listView.onSelectionChange += OnSelectionChanged;
#else
            _listView.onSelectionChanged += OnSelectionChanged;
#endif

            // 按钮组
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.RowReverse;

            // 移除按钮
            _removeButton = new Button();
            _removeButton.text = " - ";
            _removeButton.clicked += OnClickRemoveButton;
            _removeButton.SetEnabled(false);
            buttonContainer.Add(_removeButton);

            // 增加按钮
            _addButton = new Button();
            _addButton.text = " + ";
            _addButton.clicked += OnClickAddButton;
            buttonContainer.Add(_addButton);

            // 组织页面
            if (foldout)
            {
                _foldout.Add(_listView);
                _foldout.Add(buttonContainer);
                this.Add(_foldout);
            }
            else
            {
                this.Add(_headerLabel);
                this.Add(_listView);
                this.Add(buttonContainer);
            }
        }
        private void OnClickAddButton()
        {
            if (_listView.itemsSource != null)
            {
                object defaultValue = GetElementDefaultValue();
                _listView.itemsSource.Add(defaultValue);
                _listView.Rebuild();
                RefreshFoldoutName();
                RefreshRemoveButton();
            }
            else
            {
                Debug.LogWarning("Source data is null.");
            }
        }
        private void OnClickRemoveButton()
        {
            if (_listView.itemsSource != null)
            {
                if (_listView.selectedIndex >= 0)
                {
                    _listView.itemsSource.RemoveAt(_listView.selectedIndex);
                    _listView.Rebuild();
                    RefreshFoldoutName();
                    RefreshRemoveButton();
                }
            }
            else
            {
                Debug.LogWarning("Source data is null.");
            }
        }
        private void OnSelectionChanged(IEnumerable<object> objs)
        {
            RefreshRemoveButton();
        }

        /// <summary>
        /// 生成元素
        /// </summary>
        private VisualElement MakeListViewElement()
        {
            if (MakeElementCallback != null)
            {
                return MakeElementCallback.Invoke();
            }

            Type elementType = GetElementType();
            if (elementType == typeof(string))
            {
                TextField textField = new TextField();
                textField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)textField.userData;
                    _listView.itemsSource[itemIndex] = textField.value;
                });
                return textField;
            }
            else if (elementType == typeof(int))
            {
                IntegerField intField = new IntegerField();
                intField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)intField.userData;
                    _listView.itemsSource[itemIndex] = intField.value;
                });
                return intField;
            }
            else if (elementType == typeof(long))
            {
                LongField longField = new LongField();
                longField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)longField.userData;
                    _listView.itemsSource[itemIndex] = longField.value;
                });
                return longField;
            }
            else if (elementType == typeof(float))
            {
                FloatField floatField = new FloatField();
                floatField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)floatField.userData;
                    _listView.itemsSource[itemIndex] = floatField.value;
                });
                return floatField;
            }
            else if (elementType == typeof(double))
            {
                DoubleField doubleField = new DoubleField();
                doubleField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)doubleField.userData;
                    _listView.itemsSource[itemIndex] = doubleField.value;
                });
                return doubleField;
            }
            else if (elementType == typeof(bool))
            {
                Toggle toggle = new Toggle();
                toggle.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)toggle.userData;
                    _listView.itemsSource[itemIndex] = toggle.value;
                });
                return toggle;
            }
            else if (elementType == typeof(Hash128))
            {
                Hash128Field hash128Field = new Hash128Field();
                hash128Field.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)hash128Field.userData;
                    _listView.itemsSource[itemIndex] = hash128Field.value;
                });
                return hash128Field;
            }
            else if (elementType == typeof(Vector2))
            {
                Vector2Field vector2Field = new Vector2Field();
                vector2Field.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)vector2Field.userData;
                    _listView.itemsSource[itemIndex] = vector2Field.value;
                });
                return vector2Field;
            }
            else if (elementType == typeof(Vector3))
            {
                Vector3Field vector3Field = new Vector3Field();
                vector3Field.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)vector3Field.userData;
                    _listView.itemsSource[itemIndex] = vector3Field.value;
                });
                return vector3Field;
            }
            else if (elementType == typeof(Vector4))
            {
                Vector4Field vector4Field = new Vector4Field();
                vector4Field.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)vector4Field.userData;
                    _listView.itemsSource[itemIndex] = vector4Field.value;
                });
                return vector4Field;
            }
            else if (elementType == typeof(Rect))
            {
                RectField rectField = new RectField();
                rectField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)rectField.userData;
                    _listView.itemsSource[itemIndex] = rectField.value;
                });
                return rectField;
            }
            else if (elementType == typeof(Bounds))
            {
                BoundsField boundsField = new BoundsField();
                boundsField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)boundsField.userData;
                    _listView.itemsSource[itemIndex] = boundsField.value;
                });
                return boundsField;
            }
            else if (elementType == typeof(Color))
            {
                ColorField colorField = new ColorField();
                colorField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)colorField.userData;
                    _listView.itemsSource[itemIndex] = colorField.value;
                });
                return colorField;
            }
            else if (elementType == typeof(Gradient))
            {
                GradientField gradientField = new GradientField();
                gradientField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)gradientField.userData;
                    _listView.itemsSource[itemIndex] = gradientField.value;
                });
                return gradientField;
            }
            else if (elementType == typeof(AnimationCurve))
            {
                CurveField curveField = new CurveField();
                curveField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)curveField.userData;
                    _listView.itemsSource[itemIndex] = curveField.value;
                });
                return curveField;
            }
            else if (elementType == typeof(UnityEngine.Object))
            {
                ObjectField objectField = new ObjectField();
                objectField.objectType = typeof(UnityEngine.Object);
                objectField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)objectField.userData;
                    _listView.itemsSource[itemIndex] = objectField.value;
                });
                return objectField;
            }
            else if (elementType.IsEnum)
            {
                EnumField enumField = new EnumField();
                enumField.RegisterValueChangedCallback(evt =>
                {
                    int itemIndex = (int)enumField.userData;
                    _listView.itemsSource[itemIndex] = enumField.value;
                });
                return enumField;
            }
            else
            {
                Label label = new Label();
                label.text = $"Not support element type : {elementType.Name}";
                return label;
            }
        }

        /// <summary>
        /// 绑定元素
        /// </summary>
        private void BindListViewElement(VisualElement listViewElement, int index)
        {
            if (BindElementCallback != null)
            {
                BindElementCallback.Invoke(listViewElement, index);
                return;
            }

            var elementValue = _listView.itemsSource[index];
            string elementName = GetElementName(index);
            Type elementType = GetElementType();
            if (elementType == typeof(string))
            {
                if (listViewElement is TextField textField)
                {
                    textField.userData = index;
                    textField.label = elementName;
                    textField.SetValueWithoutNotify(elementValue as string);
                    return;
                }
            }
            else if (elementType == typeof(int))
            {
                if (listViewElement is IntegerField intField)
                {
                    intField.userData = index;
                    intField.label = elementName;
                    intField.SetValueWithoutNotify((int)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(long))
            {
                if (listViewElement is LongField longField)
                {
                    longField.userData = index;
                    longField.label = elementName;
                    longField.SetValueWithoutNotify((long)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(float))
            {
                if (listViewElement is FloatField floatField)
                {
                    floatField.userData = index;
                    floatField.label = elementName;
                    floatField.SetValueWithoutNotify((float)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(double))
            {
                if (listViewElement is DoubleField doubleField)
                {
                    doubleField.userData = index;
                    doubleField.label = elementName;
                    doubleField.SetValueWithoutNotify((double)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(bool))
            {
                if (listViewElement is Toggle toggle)
                {
                    toggle.userData = index;
                    toggle.label = elementName;
                    toggle.SetValueWithoutNotify((bool)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Hash128))
            {
                if (listViewElement is Hash128Field hash128Field)
                {
                    hash128Field.userData = index;
                    hash128Field.label = elementName;
                    hash128Field.SetValueWithoutNotify((Hash128)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Vector2))
            {
                if (listViewElement is Vector2Field vector2Field)
                {
                    vector2Field.userData = index;
                    vector2Field.label = elementName;
                    vector2Field.SetValueWithoutNotify((Vector2)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Vector3))
            {
                if (listViewElement is Vector3Field vector3Field)
                {
                    vector3Field.userData = index;
                    vector3Field.label = elementName;
                    vector3Field.SetValueWithoutNotify((Vector3)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Vector4))
            {
                if (listViewElement is Vector4Field vector4Field)
                {
                    vector4Field.userData = index;
                    vector4Field.label = elementName;
                    vector4Field.SetValueWithoutNotify((Vector4)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Rect))
            {
                if (listViewElement is RectField rectField)
                {
                    rectField.userData = index;
                    rectField.label = elementName;
                    rectField.SetValueWithoutNotify((Rect)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Bounds))
            {
                if (listViewElement is BoundsField boundsField)
                {
                    boundsField.userData = index;
                    boundsField.label = elementName;
                    boundsField.SetValueWithoutNotify((Bounds)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Color))
            {
                if (listViewElement is ColorField colorField)
                {
                    colorField.userData = index;
                    colorField.label = elementName;
                    colorField.SetValueWithoutNotify((Color)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(Gradient))
            {
                if (listViewElement is GradientField gradientField)
                {
                    gradientField.userData = index;
                    gradientField.label = elementName;
                    gradientField.SetValueWithoutNotify((Gradient)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(AnimationCurve))
            {
                if (listViewElement is CurveField curveField)
                {
                    curveField.userData = index;
                    curveField.label = elementName;
                    curveField.SetValueWithoutNotify((AnimationCurve)elementValue);
                    return;
                }
            }
            else if (elementType == typeof(UnityEngine.Object))
            {
                if (listViewElement is ObjectField objectField)
                {
                    objectField.userData = index;
                    objectField.label = elementName;
                    objectField.SetValueWithoutNotify(elementValue as UnityEngine.Object);
                    return;
                }
            }
            else if (elementType.IsEnum)
            {
                if (listViewElement is EnumField enumField)
                {
                    enumField.userData = index;
                    enumField.label = elementName;
                    enumField.Init((Enum)elementValue);
                    enumField.SetValueWithoutNotify((Enum)elementValue);
                    return;
                }
            }

            Debug.LogException(new InvalidOperationException(
                $"BindListViewElement failed at index {index}: elementType={elementType.Name}, actual VisualElement type={listViewElement.GetType().Name}."));
        }

        private Type GetElementType()
        {
            Type elementType = _listView.itemsSource.GetType().GetGenericArguments()[0];
            return elementType;
        }
        private object GetElementDefaultValue()
        {
            Type type = GetElementType();
            if (type == typeof(string))
                return string.Empty;
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }
        private string GetElementName(int index)
        {
            return $"Element {index}";
        }

        private void RefreshRemoveButton()
        {
            if (_listView.itemsSource == null)
            {
                _removeButton.SetEnabled(false);
                return;
            }

            // 注意：数据列表移除元素的时候有可能会越界！
            if (_listView.selectedIndex >= _listView.itemsSource.Count)
                _listView.ClearSelection();

            if (_listView.selectedIndex >= 0)
                _removeButton.SetEnabled(true);
            else
                _removeButton.SetEnabled(false);
        }
        private void RefreshFoldoutName()
        {
            if (_listView.itemsSource == null)
            {
                if (_foldout != null)
                    _foldout.text = _headerName;
                if (_headerLabel != null)
                    _headerLabel.text = _headerName;
            }
            else
            {
                if (_foldout != null)
                    _foldout.text = _headerName + $" ({_listView.itemsSource.Count}) ";
                if (_headerLabel != null)
                    _headerLabel.text = _headerName + $" ({_listView.itemsSource.Count}) ";
            }
        }
    }
}
#endif