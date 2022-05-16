using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class InspectorFinderTools
    {
        // 재귀함수의 중복을 막기 위해 추가
        private static readonly BetterList<int> instanceIds = new BetterList<int>();

        public static void FindFields(params Object[] targets)
        {
            Component[] arrayComponent = System.Array.ConvertAll(targets, t => t as Component);
            for (int i = 0; i < arrayComponent.Length; i++)
            {
                if (AutoFind(arrayComponent[i]))
                {
                    EditorUtility.SetDirty(arrayComponent[i]);
                }
            }

            instanceIds.Clear();
        }

        private static bool AutoFind(Component component)
        {
            int instanceId = component.GetInstanceID();
            if (instanceIds.Contains(instanceId))
                return false;

            instanceIds.Add(instanceId);

            // Auto 먼저 찾음
            bool isDirty = AutoFindFields(component);

            // 수동 찾음
            if (component is IInspectorFinder inspectorFinder)
            {
                if (inspectorFinder.Find())
                {
                    isDirty = true;
                }
            }

            return isDirty;
        }

        private static bool AutoFindFields(Component component)
        {
            System.Type type = component.GetType();
            bool isDirty = FindField(type, component);

            // 상속 받는 Component
            while (type.BaseType != null)
            {
                type = type.BaseType;
                if (FindField(type, component))
                {
                    isDirty = true;
                }
            }

            return isDirty;
        }

        private static bool FindField(System.Type type, Component component)
        {
            const BindingFlags serializeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            bool isDirty = false;
            foreach (FieldInfo field in type.GetFields(serializeFlags))
            {
                if (field.FieldType.IsArray)
                {
                    // Component 배열
                    if (field.FieldType.GetElementType().IsSubclassOf(typeof(Component)))
                    {
                        if (FindComponentArray(field, component))
                        {
                            isDirty = true;
                        }
                    }
                }
                else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition().Equals(typeof(List<>)))
                {
                    // Component List
                    if (field.FieldType.GetGenericArguments()[0].IsSubclassOf(typeof(Component)))
                    {
                        if (FindComponentList(field, component))
                        {
                            isDirty = true;
                        }
                    }
                }
                else
                {
                    if (field.FieldType.Equals(typeof(GameObject)))
                    {
                        if (FindGameObject(field, component))
                        {
                            isDirty = true;
                        }
                    }
                    else if (field.FieldType.Equals(typeof(Transform)))
                    {
                        if (FindTransform(field, component))
                        {
                            isDirty = true;
                        }
                    }
                    else if (field.FieldType.IsSubclassOf(typeof(Component)))
                    {
                        if (FindComponent(field, component))
                        {
                            isDirty = true;
                        }
                    }
                }
            }

            return isDirty;
        }

        private static bool FindGameObject(FieldInfo field, Component component)
        {
            // 이미 값이 있을 경우
            if (field.GetValue(component) != null)
            {
                // Check Missing Object
                GameObject value = field.GetValue(component) as GameObject;

                // Missing 이 아닐 경우 Skip
                if (value)
                    return false;
            }

            string childName = ToTrimLower(field.Name);
            Transform child = FindRecursivelyLowerName(component.transform, childName);

            // 아직 못 찾음
            if (!child)
            {
                const string PREFIX = "go"; // 접두사
                if (childName.CustomStartsWith(PREFIX))
                {
                    childName = childName.Substring(PREFIX.Length);
                    child = FindRecursivelyLowerName(component.transform, childName);
                }
            }

            if (!child)
                return false;

            field.SetValue(component, child.gameObject);
            return true;
        }

        private static bool FindTransform(FieldInfo field, Component component)
        {
            // 이미 값이 있을 경우
            if (field.GetValue(component) != null)
            {
                // Check Missing Object
                Transform value = field.GetValue(component) as Transform;

                // Missing 이 아닐 경우 Skip
                if (value)
                    return false;
            }

            string childName = ToTrimLower(field.Name);
            Transform child = FindRecursivelyLowerName(component.transform, childName);

            // 아직 못 찾음
            if (!child)
            {
                const string PREFIX = "ft"; // 접두사
                if (childName.CustomStartsWith(PREFIX))
                {
                    childName = childName.Substring(PREFIX.Length);
                    child = FindRecursivelyLowerName(component.transform, childName);
                }
            }

            if (!child)
                return false;

            field.SetValue(component, child);
            return true;
        }

        private static bool FindComponent(FieldInfo field, Component component)
        {
            // 이미 값이 있을 경우 Skip
            if (field.GetValue(component) != null)
                return false;

            Component[] findedComponents = component.GetComponentsInChildren(field.FieldType, includeInactive: true);

            bool isDirty = false;
            string fieldName, componentName;
            foreach (Component item in findedComponents)
            {
                fieldName = ToTrimLower(field.Name);
                componentName = ToTrimLower(item.name);

                // 이름 비교
                if (fieldName.Equals(componentName))
                {
                    isDirty = true;
                    field.SetValue(component, item);
                }

                // 만약 해당 Component가 IAutoInspectorFinder 일 경우 AutoFind 재귀 호출
                if (item is IAutoInspectorFinder)
                {
                    if (AutoFind(item))
                    {
                        isDirty = true;
                    }
                }
            }

            return isDirty;
        }

        private static bool FindComponentArray(FieldInfo field, Component component)
        {
            if (!field.FieldType.IsArray)
                return false;

            System.Type fieldType = field.FieldType.GetElementType();
            Component[] findedComponents = component.GetComponentsInChildren(fieldType, includeInactive: true);

            // Array 생성
            bool isDirty = false;
            System.Array array = System.Array.CreateInstance(fieldType, findedComponents.Length);
            for (int i = 0; i < findedComponents.Length; i++)
            {
                array.SetValue(findedComponents[i], i);

                // 만약 해당 Component가 IAutoInspectorFinder 일 경우 AutoFind 재귀 호출
                if (findedComponents[i] is IAutoInspectorFinder)
                {
                    if (AutoFind(findedComponents[i]))
                    {
                        isDirty = true;
                    }
                }
            }

            // 이미 같은 값일 경우
            System.Array preArray = field.GetValue(component) as System.Array;
            if (ArrayEquals(preArray, array))
                return isDirty;

            field.SetValue(component, array);
            return true;
        }

        private static bool FindComponentList(FieldInfo field, Component component)
        {
            bool isList = field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition().Equals(typeof(List<>));
            if (!isList)
                return false;

            System.Type fieldType = field.FieldType.GetGenericArguments()[0];
            Component[] findedComponents = component.GetComponentsInChildren(fieldType, includeInactive: true);

            // List 생성
            bool isDirty = false;
            IList list = System.Activator.CreateInstance(typeof(List<>).MakeGenericType(fieldType)) as IList;
            for (int i = 0; i < findedComponents.Length; i++)
            {
                list.Add(findedComponents[i]);

                // 만약 해당 Component가 IAutoInspectorFinder 일 경우 AutoFind 재귀 호출
                if (findedComponents[i] is IAutoInspectorFinder)
                {
                    if (AutoFind(findedComponents[i]))
                    {
                        isDirty = true;
                    }
                }
            }

            // 이미 같은 값일 경우
            IList preList = field.GetValue(component) as IList;
            if (ListEquals(preList, list))
                return isDirty;

            Debug.Log("Use array instead, please!");

            field.SetValue(component, list);
            return true;
        }

        /// <summary>
        /// LowerName을 찾아줍니다
        /// </summary>
        private static Transform FindRecursivelyLowerName(Transform tf, string name)
        {
            if (ToTrimLower(tf.name).Equals(name))
                return tf;

            // 재귀함수를 통하여 모든 Transform 의 name 을 찾음
            for (int i = 0; i < tf.childCount; ++i)
            {
                Transform child = FindRecursivelyLowerName(tf.GetChild(i), name);

                if (child)
                    return child;
            }

            return null;
        }

        private static bool ArrayEquals(System.Array array1, System.Array array2)
        {
            if (ReferenceEquals(array1, array2))
                return true;

            if (array1 == null || array2 == null)
                return false;

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                    return false;
            }

            return true;
        }

        private static bool ListEquals(IList list1, IList list2)
        {
            if (ReferenceEquals(list1, list2))
                return true;

            if (list1 == null || list2 == null)
                return false;

            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                    return false;
            }

            return true;
        }

        private static string ToTrimLower(string inputText)
        {
            return inputText.Replace("_", string.Empty).Trim().ToLower();
        }
    }
}