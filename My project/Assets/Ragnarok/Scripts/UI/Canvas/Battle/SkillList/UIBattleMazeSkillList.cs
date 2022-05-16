using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBattleMazeSkillList : UIBattleSkillList
    {
        public override int layer => Layer.UI_ExceptForCharZoom;

        public enum DraggableAreaType { None = 0, Stage = 1 }

        const string SkillPosX = "SkillPosX";
        const string SkillPosY = "SkillPosY";
        
        [SerializeField] UIDragItem dragItem;
        [SerializeField] UIWidget[] draggableAreas;

        private Rect curDraggableArea;

        protected override void OnShow(IUIData data = null)
        {
            base.OnShow(data);
            SetDraggableArea(DraggableAreaType.None);
            SetActivePlus(false);
        }

        protected override void OnInit()
        {
            base.OnInit();
            dragItem.OnDragStop += OnDragStop;
        }

        protected override void OnClose()
        {
            base.OnClose();

            dragItem.OnDragStop -= OnDragStop;
        }

        public override void SetCharacter(CharacterEntity entity)
        {
            base.SetCharacter(entity);

            if(PlayerPrefs.HasKey(SkillPosX) && PlayerPrefs.HasKey(SkillPosY))
                dragItem.transform.localPosition = new Vector2(PlayerPrefs.GetFloat(SkillPosX), PlayerPrefs.GetFloat(SkillPosY));
        }

        private void OnDragStop()
        {
            ClipToDragArea();
            PlayerPrefs.SetFloat(SkillPosX, dragItem.transform.localPosition.x);
            PlayerPrefs.SetFloat(SkillPosY, dragItem.transform.localPosition.y);
            PlayerPrefs.Save();
        }

        public void SetDraggableArea(DraggableAreaType areaType)
        {
            Timing.RunCoroutine(SetDraggableArea((int)areaType));
        }

        private IEnumerator<float> SetDraggableArea(int area)
        {
            yield return 0f;

            Vector3 lowerLeft = transform.localToWorldMatrix.MultiplyPoint3x4(draggableAreas[area].localCorners[0]);
            Vector3 topRight = transform.localToWorldMatrix.MultiplyPoint3x4(draggableAreas[area].localCorners[2]);

            var size = topRight - lowerLeft;
            curDraggableArea = new Rect(-size * 0.5f, size);
            ClipToDragArea();
        }

        private void ClipToDragArea()
        {
            Vector3 pos = dragItem.transform.position;

            if (!curDraggableArea.Contains(pos))
            {
                if (pos.x < curDraggableArea.xMin)
                    pos.x = curDraggableArea.xMin;
                if (pos.x > curDraggableArea.xMax)
                    pos.x = curDraggableArea.xMax;
                if (pos.y < curDraggableArea.yMin)
                    pos.y = curDraggableArea.yMin;
                if (pos.y > curDraggableArea.yMax)
                    pos.y = curDraggableArea.yMax;
            }

            dragItem.transform.position = pos;
        }
    }
}