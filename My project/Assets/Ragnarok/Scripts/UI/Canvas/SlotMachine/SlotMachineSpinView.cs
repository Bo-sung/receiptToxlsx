using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class SlotMachineSpinView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        SkillData[] skillDatas;

        [SerializeField] float strength = 5f;
        // [SerializeField] Vector3 pos = Vector3.down; // * wrapper.Grid.cellHeight * (this.skillDatas.Length - 6);

        SpringPanel sp;

        public void Awake()
        {
            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnElementRefresh);
        }

        public void SetData(SkillData[] skillDatas)
        {
            this.skillDatas = skillDatas;
            wrapper.Resize(this.skillDatas.Length);
            wrapper.SetProgress(0f);
        }

        void OnElementRefresh(GameObject go, int index)
        {
            SlotMachineSpinSlot slot = go.GetComponent<SlotMachineSpinSlot>();
            slot.SetData(skillDatas[index]);
        }

        public void Play()
        {
            float loss = Random.Range(-0.4f, 0.4f);
            sp = SpringPanel.Begin(wrapper.ScrollView.gameObject, Vector3.down * wrapper.Grid.cellHeight * (this.skillDatas.Length - 6 - 1) , strength);
        }

        //void OnValidate()
        //{
        //    wrapper.SetProgress(0f);
        //    Play();
        //}

    }
}