using UnityEngine;

namespace Ragnarok
{
    #region 프리팹

    public interface ICharacterContainer
    {
        GameObject Get(string name);
    }

    public interface IMonsterContainer
    {
        GameObject Get(string name);
    }

    public interface IItemContainer
    {
        GameObject Get(string name);
    }

    public interface IUIContainer
    {
        GameObject GetUI(string name);
    }

    public interface IEffectContainer
    {
        GameObject Get(string name);
    }

    public interface IHUDContainer
    {
        GameObject Get(string name);
    }

    public interface INpcContainer
    {
        GameObject Get(string name);
    }

    #endregion

    #region 애니메이션 클립

    public interface ICharacterAniContainer
    {
        AnimationClip[] GetAniClips();
    }

    #endregion

    #region 오디오 클립

    public interface IBgmContainer
    {
        AudioClip Get(string name);
    }

    public interface ISfxContainer
    {
        AudioClip Get(string name);
    }

    public interface IUiSfxContainer
    {
        AudioClip Get(string name);
    }

    #endregion

    #region ScriptableObject

    public interface ISkillSettingContainer
    {
        SkillSetting Get(int name);
    }

    public interface IProjectileSettingContainer
    {
        ProjectileSetting Get(string name);
    }

    public interface ISpecialRouletteConfigContainer
    {
        SpecialRouletteConfig.Config Get(int index);
    }

    #endregion

    public interface IFontContainer
    {
        Font Get(string name);
    }
}