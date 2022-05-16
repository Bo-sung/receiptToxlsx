using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class HUDPoolManager : PoolManager<HUDPoolManager>, IHUDPool
    {
        private IHUDContainer hudContainer;
        private Transform hudRoot;

        protected override void Awake()
        {
            base.Awake();

            hudContainer = AssetManager.Instance;
            hudRoot = UIManager.Instance.HUDRoot;
        }

        protected override Transform GetOriginal(string key)
        {
            GameObject go = hudContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return go.transform;
        }

        protected override PoolObject Create(string key)
        {
            GameObject go = hudContainer.Get(key);

            if (go == null)
                throw new System.ArgumentException($"존재하지 않는 GameObject 입니다: {nameof(key)} = {key}");

            return Instantiate(go).AddMissingComponent<HUDObject>();
        }

        private T SpawnHUD<T>(string key, Transform anchor, bool isFollow)
            where T : HUDObject
        {
            HUDObject obj = Spawn(key, hudRoot, worldPositionStays: false) as HUDObject;
            obj.SetAnchor(anchor, isFollow);
            return obj as T;
        }

        Damage IHUDPool.SpawnDamage(Transform anchor)
        {
            return SpawnHUD<Damage>("Damage", anchor, isFollow: false);
        }

        TotalDamage IHUDPool.SpawnTotalDamage(Transform anchor)
        {
            return SpawnHUD<TotalDamage>("Total Damage", anchor, isFollow: false);
        }

        ElementalDamageBonus IHUDPool.SpawnElementalDamageBonus(Transform anchor)
        {
            return SpawnHUD<ElementalDamageBonus>("Elemental Damage Bonus", anchor, isFollow: false);
        }

        Heal IHUDPool.SpawnHeal(Transform anchor)
        {
            return SpawnHUD<Heal>("Heal", anchor, isFollow: false);
        }

        HpGaugeBar IHUDPool.SpawnHPBar(Transform anchor)
        {
            return SpawnHUD<HpGaugeBar>("HP Bar", anchor, isFollow: true);
        }

        UITouchEffect IHUDPool.SpawnTouchEffect(Vector3 position)
        {
            var obj = SpawnHUD<UITouchEffect>("UI_FX_Click_2", UIManager.Instance.HUDRoot, false);
            obj.SetPosition(position, true);
            //obj.CachedTransform.localPosition = position;
            return obj;
        }

        ComingSurprise IHUDPool.SpawnSurprise(Transform anchor)
        {
            return SpawnHUD<ComingSurprise>("ComingSurprise", anchor, isFollow: true);
        }

        PlainText IHUDPool.SpawnPlainText(Transform anchor)
        {
            return SpawnHUD<PlainText>("PlainText", anchor, isFollow: false);
        }

        SpeechBalloon IHUDPool.SpawnSpeechBalloon(Transform anchor, SpeechBalloon.BalloonType type, string text)
        {
            SpeechBalloon speechBalloon = SpawnHUD<SpeechBalloon>("Speech Balloon", anchor, isFollow: true);
            speechBalloon.Initialize(type, text);
            return speechBalloon;
        }

        ChatBalloon IHUDPool.SpawnChatBalloon(Transform anchor, string text, float duration)
        {
            ChatBalloon chatBalloon = SpawnHUD<ChatBalloon>("Chat Balloon", anchor, isFollow: true);
            chatBalloon.Initialize(text, duration);
            return chatBalloon;
        }

        SkillBalloon IHUDPool.SpawnSkillBalloon(Transform anchor)
        {
            return SpawnHUD<SkillBalloon>("Skill Balloon", anchor, isFollow: true);
        }

        SkillBalloon IHUDPool.SpawnSkillImageBalloon(Transform anchor)
        {
            return SpawnHUD<SkillBalloon>("Skill Image Balloon", anchor, isFollow: true);
        }

        LobbyChatBallon IHUDPool.SpawnLobbyChatBalloon(Transform anchor, string text, float duration)
        {
            LobbyChatBallon chatBalloon = SpawnHUD<LobbyChatBallon>("LobbyChatBalloon", anchor, isFollow: true);
            chatBalloon.Initialize(text, duration);
            return chatBalloon;
        }

        NpcChatBalloon IHUDPool.SpawnNpcChatBalloon(Transform anchor, string text)
        {
            NpcChatBalloon npcChatBalloon = SpawnHUD<NpcChatBalloon>("NpcChatBalloon", anchor, isFollow: true);
            npcChatBalloon.Initialize(text);
            return npcChatBalloon;
        }

        LobbyPrivateStoreBalloon IHUDPool.SpawnLobbyPrivateStoreBalloon(Transform anchor)
        {
            LobbyPrivateStoreBalloon balloon = SpawnHUD<LobbyPrivateStoreBalloon>("UIPrivateStoreBalloon", anchor, isFollow: true);
            return balloon;
        }

        HudUnitName IHUDPool.SpawnUnitName(Transform anchor)
        {
            return SpawnHUD<HudUnitName>("Unit Name", anchor, isFollow: true);
        }

        HudCharacterName IHUDPool.SpawnCharacterName(Transform anchor)
        {
            return SpawnHUD<HudCharacterName>("Character Name", anchor, isFollow: true);
        }

        HudMazeMonsterName IHUDPool.SpawMazeMonsterName(Transform anchor)
        {
            return SpawnHUD<HudMazeMonsterName>("MazeMonster Name", anchor, isFollow: true);
        }

        SimpleHudUnitName IHUDPool.SpawnSimpleHudUnitName(Transform anchor)
        {
            return SpawnHUD<SimpleHudUnitName>("Simple Hud Name", anchor, isFollow: true);
        }

        HUDObject IHUDPool.SpawnRockPaperScissors(Transform anchor)
        {
            return SpawnHUD<EmptyHudObject>("RockPaperScissors", anchor, isFollow: true);
        }

        HudWayPoint IHUDPool.SpawnWayPoint(Transform anchor)
        {
            return SpawnHUD<HudWayPoint>("HudWayPointName", anchor, isFollow: true);
        }

        HudCircleTimer IHUDPool.SpawnCircleTimer(Transform anchor)
        {
            return SpawnHUD<HudCircleTimer>("Circle Timer", anchor, isFollow: true);
        }

        HudMazeItem IHUDPool.SpawnMazeItem(Transform anchor)
        {
            return SpawnHUD<HudMazeItem>("MazeItem", anchor, isFollow: true);
        }

        HudUnitInfo IHUDPool.SpawnUnitInfo(Transform anchor)
        {
            return SpawnHUD<HudUnitInfo>("UnitInfo", anchor, isFollow: true);
        }

        HudCharacterInfo IHUDPool.SpawnCharacterInfo(Transform anchor)
        {
            return SpawnHUD<HudCharacterInfo>("CharacterInfo", anchor, isFollow: true);
        }

        HUDObject IHUDPool.SpawnMazeLobbyField(Transform anchor, int chapter)
        {
            return SpawnLobbyField(anchor, MazeLobbyField.Type.Chapter, chapter);
        }

        HUDObject IHUDPool.SpawnMazeLobbyEventField(Transform anchor, int eventId)
        {
            return SpawnLobbyField(anchor, MazeLobbyField.Type.Event, eventId);
        }

        HUDObject IHUDPool.SpawnMazeLobbyForestMaze(Transform anchor, int id)
        {
            return SpawnLobbyField(anchor, MazeLobbyField.Type.ForestMaze, id);
        }

        HUDObject IHUDPool.SpawnMazeLobbyGateMaze(Transform anchor, int gateId)
        {
            return SpawnLobbyField(anchor, MazeLobbyField.Type.Gate, gateId);
        }

        HUDObject IHUDPool.SpawnNpcSign(Transform anchor, NpcType npcType, Action<NpcType> onClick)
        {
            NpcSign npcSign = SpawnHUD<NpcSign>("NpcSign", anchor, isFollow: true);
            npcSign.Initialize(npcType, onClick);
            return npcSign;
        }

        HUDObject IHUDPool.SpawnNpcQuestAlarm(Transform anchor)
        {
            return SpawnHUD<EmptyHudObject>("NpcQuestAlarm", anchor, isFollow: true);
        }

        HUDObject IHUDPool.SpawnEmotion(Transform anchor, EmotionType type)
        {
            HudEmotion hudEmotion = SpawnHUD<HudEmotion>("HudEmotion", anchor, isFollow: true);
            hudEmotion.Initialize(type);
            return hudEmotion;
        }

        private MazeLobbyField SpawnLobbyField(Transform anchor, MazeLobbyField.Type type, int mazeId)
        {
            MazeLobbyField mazeLobbyField = SpawnHUD<MazeLobbyField>("MazeLobbyField", anchor, isFollow: true);
            mazeLobbyField.Initialize(type, mazeId);
            return mazeLobbyField;
        }
    }
}