using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class BattleOptionCheckWindow : EditorWindow, IHasCustomMenu
    {
        private const float FOOTER_HEIGHT = 40f;

        MultiViewer multiViwer;

        private PassiveBattleOptionList options;

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

                using (var scope = new GUILayout.HorizontalScope())
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

            if (options == null)
                options = new PassiveBattleOptionList();
        }

        private void ResetData()
        {
            multiViwer.Clear();
        }

        private void ShowCurrentClientInfo()
        {
            // 버프 아이템
            options.AddRange(Entity.player.battleBuffItemInfo);
            BattleOption[] buffItemOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 장착 아이템
            options.AddRange(Entity.player.battleItemInfo);
            BattleOption[] itemOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 패시브 스킬
            options.AddRange(Entity.player.battleSkillInfo);
            BattleOption[] skillOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 길드 스킬
            options.AddRange(Entity.player.battleGuildSkillInfo);
            BattleOption[] guildSkillOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 동료
            options.AddRange(Entity.player.battleAgentOptionInfo);
            BattleOption[] agentOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 동료 도감
            options.AddRange(Entity.player.battleAgentBookOptionInfo);
            BattleOption[] agentBookOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 쉐어포스 스탯
            options.AddRange(Entity.player.battleShareForceStatusOptionInfo);
            BattleOption[] shareForceStatusOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 도감
            options.AddRange(Entity.player.battleBookOptionInfo);
            BattleOption[] bookOptions = options.ToArrayForServerCheck();
            options.Clear();

            // 아이템 스탯
            BattleItemInfo itemValue = Entity.player.battleItemInfo;

            multiViwer[0].SetText(GetText(buffItemOptions, itemOptions, skillOptions, guildSkillOptions, agentOptions, agentBookOptions, shareForceStatusOptions, bookOptions, itemValue)); // 클라이언트 View 에 세팅
        }

        private void SendProtocol()
        {
            SendProtocolAsync().WrapNetworkErrors();
        }

        private async Task SendProtocolAsync()
        {
            Response response = await Protocol.DEBUG_BATTLE_OPTION_LIST.SendAsync();

            // 버프 아이템
            BattleOption[] buffItemOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("1"), ConverterToBattleOption);

            // 장착 아이템
            BattleOption[] itemOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("2"), ConverterToBattleOption);

            // 패시브 스킬
            BattleOption[] skillOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("3"), ConverterToBattleOption);

            // 길드 스킬
            BattleOption[] guildSkillOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("4"), ConverterToBattleOption);

            // 동료
            BattleOption[] agentOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("13"), ConverterToBattleOption);

            // 동료 도감
            BattleOption[] agentBookOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("14"), ConverterToBattleOption);

            // 쉐어포스 스탯
            BattleOption[] shareForceStatusOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("17"), ConverterToBattleOption);

            // 도감
            BattleOption[] bookOptions = System.Array.ConvertAll(response.GetPacketArray<BattleOptionPacket>("18"), ConverterToBattleOption);

            // 아이템 스탯
            ItemStatusValuePacket itemStatusValue = response.GetPacket<ItemStatusValuePacket>("6");

            multiViwer[1].SetText(GetText(buffItemOptions, itemOptions, skillOptions, guildSkillOptions, agentOptions, agentBookOptions, shareForceStatusOptions, bookOptions, itemStatusValue)); // 서버 View 에 세팅
        }

        private string GetText(BattleOption[] buffItemOptions
            , BattleOption[] itemOptions
            , BattleOption[] skillOptions
            , BattleOption[] guildSkillOptions
            , BattleOption[] agentOptions
            , BattleOption[] agentBookOptions
            , BattleOption[] shareForceStatusOptions
            , BattleOption[] bookOptions
            , BattleItemInfo.IValue itemValue)
        {
            var sb = StringBuilderPool.Get();

            sb.Append(GetBattleOptionText("버프 아이템", buffItemOptions));
            sb.Append(GetBattleOptionText("장착 아이템", itemOptions));
            sb.Append(GetBattleOptionText("패시브 스킬", skillOptions));
            sb.Append(GetBattleOptionText("길드 스킬", guildSkillOptions));
            sb.Append(GetBattleOptionText("동료", agentOptions));
            sb.Append(GetBattleOptionText("동료 도감", agentBookOptions));
            sb.Append(GetBattleOptionText("쉐어포스 스탯", shareForceStatusOptions));
            sb.Append(GetBattleOptionText("도감", bookOptions));

            sb.Append($"[아이템 스탯]"); // Header
            sb.AppendLine().Append("\t[Total Atk] ").Append(itemValue.TotalItemAtk);
            sb.AppendLine().Append("\t[Total Matk] ").Append(itemValue.TotalItemMatk);
            sb.AppendLine().Append("\t[Total Def] ").Append(itemValue.TotalItemDef);
            sb.AppendLine().Append("\t[Total Mdef] ").Append(itemValue.TotalItemMdef);

            return sb.Release();
        }

        private BattleOption ConverterToBattleOption(BattleOptionPacket packet)
        {
            return new BattleOption(packet.battle_option_type, packet.value_1, packet.value_2);
        }

        private string GetBattleOptionText(string header, BattleOption[] buffItemOptions)
        {
            System.Array.Sort(buffItemOptions, SortByBattleOptionType); // Sort

            var sb = StringBuilderPool.Get();
            sb.Append($"[{header}] ({buffItemOptions.Length})");
            sb.AppendLine();
            foreach (var item in buffItemOptions)
            {
                if (item.battleOptionType.IsConditionalOption())
                {
                    sb.Append("\t").Append($"[{item.battleOptionType}:{item.value2}] {item.value1}");
                }
                else
                {
                    sb.Append("\t").Append($"[{item.battleOptionType}] {item.value1}, {item.value2}");
                }
                sb.AppendLine();
            }

            return sb.Release();
        }

        private int SortByBattleOptionType(BattleOption a, BattleOption b)
        {
            int result1 = a.battleOptionType.CompareTo(b.battleOptionType); // battleOptionType
            int result2 = result1 == 0 ? a.value2.CompareTo(b.value2) : result1; // value2
            return result2;
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("초기화"), false, ResetData);
            menu.ShowAsContext();
        }
    }
}