using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class DuplicateWithNumbering 
{
    [MenuItem("GameObject/Duplicate With Numbering %#d")]
    public static void DupWithNumbering()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable))
        {
            GameObject newObject = null;

            PrefabType pt = PrefabUtility.GetPrefabType(t.gameObject);
            if (pt == PrefabType.PrefabInstance || pt == PrefabType.ModelPrefabInstance)
            {
                // it's an instance of a prefab! Create a new instance of the same prefab!
                Object prefab = PrefabUtility.GetPrefabParent(t.gameObject);
                newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                // we've got a brand new prefab instance, but it doesn't have the same overrides as our original. Fix that...
                PropertyModification[] overrides = PrefabUtility.GetPropertyModifications(t.gameObject);
                PrefabUtility.SetPropertyModifications(newObject, overrides);

                //// okay, prefab is instantiated (or if it's not a prefab, we've just cloned it in the scene)
                //// Make sure it's got the same parent and position as the original!
                newObject.transform.parent = t.parent;
                newObject.transform.position = t.position;
                newObject.transform.rotation = t.rotation;
                newObject.transform.localScale = t.localScale;
            }
            else
            {
                // not a prefab... so just instantiate it!
                newObject = Object.Instantiate(t.gameObject);

                //always first object name of end with _01
                string originalName = t.name;
                int curNumber = 0;
                int originalNumberLength = 0;
                Regex regex = new Regex(@"(?<name>.*?)(?<number>\d+)$");
                if (regex.IsMatch(originalName))
                {
                    var match = regex.Match(originalName);
                    originalName = match.Groups["name"].Value;
                    curNumber = int.Parse(match.Groups["number"].Value);
                    originalNumberLength = match.Groups["number"].Value.Length;
                }

                int newNumber = curNumber + 1;
                // 기존 넘버링의 prefix를 따름 (001, 002, ...)
                string prefix = string.Empty;
                if (originalNumberLength > newNumber.ToString().Length)
                {
                    for (int i = 0; i < originalNumberLength - newNumber.ToString().Length; ++i)
                    {
                        prefix += "0";
                    }
                }
                string newName = originalName + prefix + newNumber.ToString();
                newObject.name = newName;

                newObject.transform.parent = t.parent;
                newObject.transform.position = t.position;
                newObject.transform.rotation = t.rotation;
                newObject.transform.localScale = t.localScale;
            }
            // tell the Undo system we made this, so you can undo it if you did it by mistake
            Undo.RegisterCreatedObjectUndo(newObject, "DuplicateWithNumbering");
            Selection.objects = new Object[] { newObject };
        }
    }

    [MenuItem("GameObject/Duplicate With Numbering %#d", true)]
    public static bool ValidateDuplicateWithNumbering()
    {
        return Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable).Length > 0;
    }
}
