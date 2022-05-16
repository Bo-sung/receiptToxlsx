using System;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="HUDPoolManager"/>
    /// </summary>
    public interface IHUDPool
    {
        Damage SpawnDamage(Transform anchor);

        TotalDamage SpawnTotalDamage(Transform anchor);

        ElementalDamageBonus SpawnElementalDamageBonus(Transform anchor);

        Heal SpawnHeal(Transform anchor);

        HpGaugeBar SpawnHPBar(Transform anchor);

		UITouchEffect SpawnTouchEffect(Vector3 position);

        ComingSurprise SpawnSurprise(Transform anchor);

        PlainText SpawnPlainText(Transform anchor);

        SpeechBalloon SpawnSpeechBalloon(Transform anchor, SpeechBalloon.BalloonType type, string text);

        ChatBalloon SpawnChatBalloon(Transform anchor, string text, float duration);

        SkillBalloon SpawnSkillBalloon(Transform anchor);

        SkillBalloon SpawnSkillImageBalloon(Transform anchor);

        LobbyChatBallon SpawnLobbyChatBalloon(Transform anchor, string text, float duration);

        NpcChatBalloon SpawnNpcChatBalloon(Transform anchor, string text);

        LobbyPrivateStoreBalloon SpawnLobbyPrivateStoreBalloon(Transform anchor);

        HudUnitName SpawnUnitName(Transform anchor);

        HudCharacterName SpawnCharacterName(Transform anchor);

        HudMazeMonsterName SpawMazeMonsterName(Transform anchor);

        SimpleHudUnitName SpawnSimpleHudUnitName(Transform anchor);

        HUDObject SpawnRockPaperScissors(Transform anchor);

        HudWayPoint SpawnWayPoint(Transform anchor);

        HudCircleTimer SpawnCircleTimer(Transform anchor);

        HudMazeItem SpawnMazeItem(Transform anchor);

        HudUnitInfo SpawnUnitInfo(Transform anchor);

        HudCharacterInfo SpawnCharacterInfo(Transform anchor);

        HUDObject SpawnMazeLobbyField(Transform anchor, int chapter);

        HUDObject SpawnMazeLobbyEventField(Transform anchor, int eventId);

        HUDObject SpawnMazeLobbyForestMaze(Transform anchor, int id);

        HUDObject SpawnMazeLobbyGateMaze(Transform anchor, int gateId);

        HUDObject SpawnNpcSign(Transform anchor, NpcType npcType, Action<NpcType> onClick);

        HUDObject SpawnNpcQuestAlarm(Transform anchor);

        HUDObject SpawnEmotion(Transform anchor, EmotionType type);
    }
}