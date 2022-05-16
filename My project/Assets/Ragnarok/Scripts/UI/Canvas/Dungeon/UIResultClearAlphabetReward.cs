using UnityEngine;
using System.Collections;

namespace Ragnarok
{
    public class UIResultClearAlphabetReward : MonoBehaviour
    {
        [SerializeField] GameObject root;
        
        public void SetData(UIDuelAlphabet alphabet, char character)
        {
            UIDuelAlphabet comp = root.GetComponentInChildren<UIDuelAlphabet>(true);
            if (comp != null)
                Destroy(comp.gameObject);
            comp = Instantiate(alphabet);
            comp.transform.parent = root.transform;
            comp.transform.localPosition = Vector3.zero;
            comp.transform.localScale = Vector3.one;
            comp.gameObject.SetActive(true);
            comp.SetData(0, character, null);
        }
    }
}