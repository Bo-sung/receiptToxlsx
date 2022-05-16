using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public abstract class CheatRequestDrawer : IInitializable, System.IDisposable, System.IComparable<CheatRequestDrawer>
    {
        const bool MINIMALISTIC_LOOK = false;
        const bool DEFAULT_HEADER_STATE = true;

        private readonly BetterList<string> messageList = new BetterList<string>();
        private readonly BetterList<string> warningMessageList = new BetterList<string>();
        private readonly BetterList<string> errorMessageList = new BetterList<string>();

        public abstract int OrderNum { get; }

        public abstract string Title { get; }

        private Vector2 scrollPos;
        private Vector2 consoleScrollPos;

        public void Initialize()
        {
            Awake();
        }

        public void Dispose()
        {
            OnDestroy();
            ClearConsoleMessage();
        }

        protected abstract void Awake();

        protected abstract void OnDestroy();

        protected abstract void OnDraw();

        public void Draw(Rect rect)
        {
            const float CONSOLE_HEIGHT = 100f;

            if (HasConsoleMessage())
            {
                rect.height -= (CONSOLE_HEIGHT + EditorGUIUtility.standardVerticalSpacing);
            }

            using (new GUILayout.AreaScope(rect))
            {
                using (var gui = new GUILayout.ScrollViewScope(scrollPos))
                {
                    scrollPos = gui.scrollPosition;
                    OnDraw();
                }
            }

            if (HasConsoleMessage())
            {
                rect.y = rect.yMax + EditorGUIUtility.standardVerticalSpacing;
                rect.height = CONSOLE_HEIGHT;
                using (new GUILayout.AreaScope(rect, string.Empty, EditorStyles.textField))
                {
                    using (var gui = new GUILayout.ScrollViewScope(consoleScrollPos))
                    {
                        consoleScrollPos = gui.scrollPosition;

                        foreach (var item in errorMessageList)
                        {
                            EditorGUILayout.HelpBox(item, MessageType.Error);
                        }

                        foreach (var item in warningMessageList)
                        {
                            EditorGUILayout.HelpBox(item, MessageType.Warning);
                        }

                        foreach (var item in messageList)
                        {
                            EditorGUILayout.HelpBox(item, MessageType.None);
                        }
                    }
                }
            }
        }

        protected virtual bool IsInvalid()
        {
            // 플레이 상태가 아닐 때
            if (!EditorApplication.isPlaying)
                return true;

            // 데이터 로드 완료 상태가 아닐 때
            if (!AssetManager.IsAllAssetReady)
                return true;

            // GameMap 입장하지 않았을 때
            if (!PlayerEntity.IsJoinGameMap)
                return true;

            // Real 서버일 때
            return GameServerConfig.IsEditorRealServer;
        }

        protected void ShowDialog(string message)
        {
            EditorUtility.DisplayDialog("알림", message, "확인");
        }

        protected bool DrawHeader(string header)
        {
            return DrawHeader(header, header, false, MINIMALISTIC_LOOK);
        }

        protected bool DrawMiniHeader(string header)
        {
            return DrawHeader(header, header, false, true);
        }

        protected void DrawRequest(System.Action action)
        {
            using (new EditorGUI.DisabledGroupScope(IsInvalid()))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Request"))
                    {
                        ClearConsoleMessage();
                        action();
                    }
                }
            }
        }

        protected void AddErrorMessage(string text)
        {
            if (text == null)
                return;

            errorMessageList.Add(text);
        }

        protected void AddWarningMessage(string text)
        {
            if (text == null)
                return;

            warningMessageList.Add(text);
        }

        protected void AddMessage(string text)
        {
            if (text == null)
                return;

            messageList.Add(text);
        }

        #region 캐릭터
        protected void SendBaseLevel(int level)
        {
            const string CHEAT_TEXT = "/blv {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, level));
        }

        protected void SendJobLevel(int level)
        {
            const string CHEAT_TEXT = "/jlv {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, level));
        }

        protected void SendZeny(long zeny)
        {
            const string CHEAT_TEXT = "/zeny {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, zeny));
        }

        protected void SendCatCoin(long catCoin)
        {
            const string CHEAT_TEXT = "/catcoin {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, catCoin));
        }

        protected void SendRoPoint(long roPoint)
        {
            const string CHEAT_TEXT = "/ropoint {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, roPoint));
        }

        protected void SendPassExp(int passExp)
        {
            const string CHEAT_TEXT = "/pass_exp {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, passExp));
        }

        protected void SendOnBuffPassExp(int onbuffExp)
        {
            const string CHEAT_TEXT = "/onbuff_exp {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, onbuffExp));
        }
        #endregion

        #region 아이템
        protected void SendItem(int itemId, int count)
        {
            const string CHEAT_TEXT = "/item {VALUE} {COUNT}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, itemId)
                .Replace(ReplaceKey.COUNT, count));
        }
        #endregion

        #region 퀘스트
        protected void SendGuideQuestAllClear()
        {
            //const string CHEAT_TEXT = "/quest";
            //SendMessage(CHEAT_TEXT);
            SendGuideQuestClear(QuestDataManager.Instance.Get(QuestCategory.Main).Count - 1);
        }

        protected void SendGuideQuestClear(int seq)
        {
            const string CHEAT_TEXT = "/quest {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, seq));
        }

        protected void SendStage(int stageId)
        {
            const string CHEAT_TEXT = "/stage {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, stageId));
        }

        protected void SendMaze(int chapter)
        {
            const string CHEAT_TEXT = "/smclear {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, chapter));
        }

        protected void SendDuel(int chapter, int index)
        {
            const string CHEAT_TEXT = "/duel {VALUE} {INDEX}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, chapter)
                .Replace(ReplaceKey.INDEX, index));
        }
        #endregion

        #region 던전
        protected void SendFreeFightStart()
        {
            const string CHEAT_TEXT = "/ffstart";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendFreeFightEventStart()
        {
            const string CHEAT_TEXT = "/ffestart";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendTamingStart()
        {
            const string CHEAT_TEXT = "/tamingon";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendTamingEnd()
        {
            const string CHEAT_TEXT = "/tamingoff";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildAttackStart(int level)
        {
            const string CHEAT_TEXT = "/gastart {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, level));
        }

        protected void SendGuildAttackWin()
        {
            const string CHEAT_TEXT = "/gawin";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildAttackLose()
        {
            const string CHEAT_TEXT = "/galose";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildAttackDonation(int count)
        {
            const string CHEAT_TEXT = "/gacon {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, count));
        }

        protected void SendGuildAttackChangeRandomTime()
        {
            const string CHEAT_TEXT = "/gactime";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildAttackChangeTime(int minutes)
        {
            const string CHEAT_TEXT = "/gactime2 {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, minutes));
        }

        protected void SendGuildAttackDamage(int damage)
        {
            const string CHEAT_TEXT = "/gadam {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, damage));
        }

        protected void SendGuildAttackEmperiumLevel(int level)
        {
            const string CHEAT_TEXT = "/empellv {VALUE}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, level));
        }

        protected void SendEndlessTowerTicketResetTime()
        {
            const string CHEAT_TEXT = "/eticket";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildBattleReady()
        {
            const string CHEAT_TEXT = "/gvg_season_start";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildBattleStart()
        {
            const string CHEAT_TEXT = "/gvg_battle_start";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendGuildBattleEnd()
        {
            const string CHEAT_TEXT = "/gvg_battle_end";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendCupet(int cupetId, int count)
        {
            const string CHEAT_TEXT = "/guildpet {VALUE} {COUNT}";
            SendMessage(CHEAT_TEXT
                .Replace(ReplaceKey.VALUE, cupetId)
                .Replace(ReplaceKey.COUNT, count));
        }

        protected void SendArenaStart()
        {
            const string CHEAT_TEXT = "/arena_start";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendArenaEnd()
        {
            const string CHEAT_TEXT = "/arena_end";
            SendMessage(CHEAT_TEXT);
        }

        protected void SendArenaPoint(int count)
        {
            const string CHEAT_TEXT = "/arena_point {COUNT}";
            SendMessage(CHEAT_TEXT.Replace(ReplaceKey.COUNT, count));
        }
        #endregion

        private void SendMessage(string message)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", message);
            Protocol.REQUEST_CHANNEL_CHAT_MSG.SendAsync(sfs).WrapNetworkErrors();
        }

        private void ClearConsoleMessage()
        {
            errorMessageList.Release();
            warningMessageList.Release();
            messageList.Release();
        }

        private bool HasConsoleMessage()
        {
            return errorMessageList.size > 0 || warningMessageList.size > 0 || messageList.size > 0;
        }

        public int CompareTo(CheatRequestDrawer other)
        {
            return OrderNum.CompareTo(other.OrderNum);
        }

        private bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            bool state = EditorPrefs.GetBool(key, DEFAULT_HEADER_STATE); // state 기본 값을 false 로 둠

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }
    }
}