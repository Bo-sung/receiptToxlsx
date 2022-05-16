using Sfs2X.Entities.Data;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class AttackPowerCheckWindow : EditorWindow, IHasCustomMenu
    {
        private const float FOOTER_HEIGHT = 40f;

        MultiViewer multiViwer;

        void OnEnable()
        {
            Initialize();
        }

        void OnGUI()
        {
            Rect mainRect = new Rect(Vector2.zero, position.size);
            mainRect.height -= FOOTER_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
            multiViwer.OnGUI(mainRect);

            Rect bottom = mainRect;
            bottom.y = mainRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            bottom.width = position.width;
            bottom.height = FOOTER_HEIGHT;
            using (new GUILayout.AreaScope(bottom))
            {
                GUILayout.FlexibleSpace();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledGroupScope(!EditorApplication.isPlaying))
                    {
                        if (GUILayout.Button("Send Protocol", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                        {
                            ResetData();
                            ShowCurrentClientInfo();
                            SendProtocol();
                        }
                    }

                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void Initialize()
        {
            if (multiViwer == null)
            {
                multiViwer = new MultiViewer(MultiViewer.DivisionType.Horizontal, multiCount: 2);
                multiViwer[0].SetTitle("클라이언트");
                multiViwer[1].SetTitle("서버");
            }
        }

        private void ResetData()
        {
            multiViwer.Clear();
        }

        private void ShowCurrentClientInfo()
        {
            int totalAP = Entity.player.GetTotalAttackPower();
            int characterAP = Entity.player.attackPowerInfo.GetCharacterAttackPower();
            int equipmentAP = Entity.player.attackPowerInfo.GetEquipmentAttackPower();
            int cardAP = Entity.player.attackPowerInfo.GetCardAttackPower();
            int skillAP = Entity.player.attackPowerInfo.GetSkillAttackPower();
            int cupetAP = Entity.player.attackPowerInfo.GetCupetAttackPower();
            int haveAgentAP = Entity.player.attackPowerInfo.GetHaveAgentAttackPower();
            float equippedAgentAp = Entity.player.attackPowerInfo.GetEquippedAgentAttackPower();

            int equipmentAtk = Entity.player.battleItemInfo.TotalItemAtk;
            int equipmentDef = Entity.player.battleItemInfo.TotalItemDef;
            int equipmentMatk = Entity.player.battleItemInfo.TotalItemMatk;
            int equipmentMdef = Entity.player.battleItemInfo.TotalItemMdef;

            multiViwer[0].SetText(GetText(characterAP, equipmentAP, cardAP, skillAP, cupetAP, haveAgentAP, equippedAgentAp, totalAP, equipmentAtk, equipmentDef, equipmentMatk, equipmentMdef)); // 클라이언트 View 에 세팅
        }

        private void SendProtocol()
        {
            SendProtocolAsync().WrapNetworkErrors();
        }

        private async Task SendProtocolAsync()
        {
            Response response = await Protocol.DEBUG_BATTLE_OPTION_LIST.SendAsync();

            var equipmentInfo = response.GetSFSObject("6");
            int totalAP = response.GetInt("7");
            int characterAP = response.GetInt("8");
            int equipmentAP = response.GetInt("9");
            int skillAP = response.GetInt("10");
            int cardAP = response.GetInt("11");
            int cupetAP = response.GetInt("12");
            int haveAgentAP = response.GetInt("15");
            float equippedAgentAp = response.GetFloat("16");

            int equipmentAtk = equipmentInfo.GetInt("1");
            int equipmentMatk = equipmentInfo.GetInt("2");
            int equipmentDef = equipmentInfo.GetInt("3");
            int equipmentMdef = equipmentInfo.GetInt("4");

            multiViwer[1].SetText(GetText(characterAP, equipmentAP, cardAP, skillAP, cupetAP, haveAgentAP, equippedAgentAp, totalAP, equipmentAtk, equipmentDef, equipmentMatk, equipmentMdef)); // 서버 View 에 세팅
        }

        private string GetText(int characterAP, int equipmentAP, int cardAP, int skillAP, int cupetAP, int haveAgentAp, float equippedAgentAp, int totalAP
            , int equipmentAtk, int equipmentDef, int equipmentMatk, int equipmentMdef)
        {
            var sb = StringBuilderPool.Get();

            sb.Append($"[총 전투력] {totalAP}");
            sb.AppendLine();
            sb.Append("\t").Append($"[캐릭터 전투력] {characterAP}");
            sb.AppendLine();
            sb.Append("\t").Append($"[장비 전투력] {equipmentAP}");
            sb.AppendLine();
            sb.Append("\t\t").Append($"[ATK] {equipmentAtk}");
            sb.AppendLine();
            sb.Append("\t\t").Append($"[DEF] {equipmentDef}");
            sb.AppendLine();
            sb.Append("\t\t").Append($"[MATK] {equipmentMatk}");
            sb.AppendLine();
            sb.Append("\t\t").Append($"[MDEF] {equipmentMdef}");
            sb.AppendLine();
            sb.Append("\t").Append($"[카드인챈트 전투력] {cardAP}");
            sb.AppendLine();
            sb.Append("\t").Append($"[스킬 전투력] {skillAP}");
            sb.AppendLine();
            sb.Append("\t").Append($"[큐펫 전투력] {cupetAP}");
            sb.AppendLine();
            sb.Append("\t").Append($"[보유동료 전투력] {haveAgentAp}");
            sb.AppendLine();
            sb.Append("\t").Append($"[장착동료 전투력] {equippedAgentAp}");

            return sb.Release();
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("초기화"), false, ResetData);
            menu.ShowAsContext();
        }
    }
}