using Ragnarok.View;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobGrowthSlot : UIView, IInspectorFinder
    {
        enum TextureType
        {
            None,
            Item,
            ContentsUnlock,
        }

        // 배경
        [SerializeField] GameObject shareBack, jobBack;

        [SerializeField] GameObject SelectedTween; // 현재 진행중인 슬롯 이펙트 표시..

        // 왼쪽
        [SerializeField] GameObject leftTextType;
        [SerializeField] GameObject leftImageType;

        [SerializeField] UILabelValue labelTitle;
        [SerializeField] UIGridHelper gridJobIcon;
        [SerializeField] UITextureHelper[] jobicons;

        [SerializeField] GameObject imageSkillPoint; // 텍스쳐
        [SerializeField] GameObject imageGuildSquare;

        // 중앙
        [SerializeField] GameObject centerTextType, centerImageType;

        [SerializeField] UIGridHelper jobLevelGrid;
        [SerializeField] UILabelHelper[] jobLabels;

        [SerializeField] UIJobGrowthCharacter[] charactersArray;

        // 오른쪽
        [SerializeField] GameObject iconSharevice1, iconSharevice2; // 텍스쳐
        [SerializeField] GameObject iconSlot, iconClone, iconFilter, iconGuildSquare; // 아틀라스

        [SerializeField] UILabelValue labelClone;
        [SerializeField] UILabelValue labelJob;
        [SerializeField] UILabelValue labelSkillpoint;

        [SerializeField] GameObject complete;

        JobGrowthPresenter presenter;

        JobGrowthState state;
        Gender gender;
        int levelUpShareviceId;
        int shareCloneId;
        int jobGrade;
        int jobLevel;
        int maxJobLevelByGroup;

        protected override void OnLocalize()
        {
        }

        public void SetData(JobGrowthPresenter presenter, int idx, bool activeTween)
        {
            this.presenter = presenter;
            state = (JobGrowthState)idx;

            gender = presenter.GetGender();
            levelUpShareviceId = presenter.GetLevelUpShareviceId();
            shareCloneId = presenter.GetShareCloneId();
            jobGrade = presenter.GetJobGrade();
            jobLevel = presenter.GetJobLevel();

            SetMain(); // 메인 이미지 셋팅(스킬의 경우 텍스트) // 최대 잡레벨, 스킬포인트 셋팅되는부분 !! 먼저 셋팅되어야함.

            SetTitle(); // 타이틀 셋팅(스킬, 길드의 경우 이미지)
            SetReward(); // 우측 셋팅

            SetComplete(); // 완료체크
            SetBackground(activeTween); // 배경, 진행도도 여기있음.
        }

        Job[] GetJobGradeCharacters(int grade)
        {
            return presenter.GetJobGradeCharacters(grade);
        }

        void SetBackground(bool activeTween)
        {
            switch (state)
            {
                case JobGrowthState.Sharevice1:
                case JobGrowthState.Sharevice2:
                case JobGrowthState.ShareClone:
                    shareBack.SetActive(true);
                    jobBack.SetActive(false);
                    break;

                default:
                    shareBack.SetActive(false);
                    jobBack.SetActive(true);
                    break;
            }

            SelectedTween.SetActive(activeTween);
        }

        void SetTitle()
        {
            // 타입 셋팅
            switch (state)
            {
                case JobGrowthState.SkillPoint1:
                case JobGrowthState.SkillPoint2:
                case JobGrowthState.SkillPoint3:
                    leftTextType.SetActive(false);
                    leftImageType.SetActive(true);

                    imageSkillPoint.SetActive(true);
                    SetTextures(imageSkillPoint, TextureType.Item);
                    imageGuildSquare.SetActive(false);
                    break;

                case JobGrowthState.GuildSquare:
                    leftTextType.SetActive(false);
                    leftImageType.SetActive(true);

                    imageSkillPoint.SetActive(false);
                    imageGuildSquare.SetActive(true);
                    break;

                default:
                    leftTextType.SetActive(true);
                    leftImageType.SetActive(false);
                    break;
            }

            // 텍스트 타입의 경우
            switch (state)
            {
                case JobGrowthState.Job1:
                    labelTitle.TitleKey = LocalizeKey._2002; // 1차 전직
                    labelTitle.Value = "";
                    break;

                case JobGrowthState.Job2:
                    labelTitle.TitleKey = LocalizeKey._2003; // 2차 전직
                    labelTitle.Value = "";
                    break;

                case JobGrowthState.Job3:
                    labelTitle.TitleKey = LocalizeKey._2016; // 3차 전직
                    labelTitle.Value = "";
                    break;

                case JobGrowthState.Sharevice1:
                    labelTitle.TitleKey = LocalizeKey._54301; // 쉐어바이스 레벨업
                    labelTitle.Value = LocalizeKey._2022.ToText().Replace(ReplaceKey.NAME, levelUpShareviceId.ToText()); // 시나리오 미궁 {NAME} 클리어 필요
                    break;

                case JobGrowthState.ShareClone:
                    labelTitle.TitleKey = LocalizeKey._54313; // 희망의 영웅
                    labelTitle.Value = LocalizeKey._2022.ToText().Replace(ReplaceKey.NAME, shareCloneId.ToText()); // 시나리오 미궁 {NAME} 클리어 필요
                    break;

                case JobGrowthState.Job4:
                    labelTitle.TitleKey = LocalizeKey._2017; // 4차 전직
                    labelTitle.Value = "";
                    break;

                case JobGrowthState.Sharevice2:
                    labelTitle.TitleKey = LocalizeKey._2021; // 2세대 쉐어바이스

                    var data = presenter.GetShareForceQuest();
                    labelTitle.Value = LocalizeKey._48255.ToText() // 타임패트롤 퀘스트 [{NUMBER}.{NAME}] 클리어 해야합니다.
                        .Replace("{NUMBER}", data.daily_group.ToString())
                        .Replace("{NAME}", data.name_id.ToText());
                    break;

                case JobGrowthState.Job5:
                    labelTitle.TitleKey = LocalizeKey._2018; // 5차 전직
                    labelTitle.ValueKey = LocalizeKey._2020; // Update
                    break;
            }
        }

        void SetMain()
        {
            // 잡레벨 리스트셋팅
            SetJobLevelList();

            // 타입 셋팅
            switch (state)
            {
                case JobGrowthState.SkillPoint1:
                case JobGrowthState.SkillPoint2:
                case JobGrowthState.SkillPoint3:
                    centerTextType.SetActive(true);
                    centerImageType.SetActive(false);
                    break;

                default:
                    centerTextType.SetActive(false);
                    centerImageType.SetActive(true);
                    break;
            }

            // 이미지의 경우
            switch (state)
            {
                case JobGrowthState.Job1:
                    var char1 = GetJobGradeCharacters(1);
                    GetJobCharacters().SetCharacterImages(char1.Select(x => x.ToString()).ToArray(), gender.ToString());
                    gridJobIcon.SetActive(true);
                    SetJobIcons(char1);
                    break;

                case JobGrowthState.Job2:
                    var char2 = GetJobGradeCharacters(2);
                    GetJobCharacters().SetCharacterImages(char2.Select(x => x.ToString()).ToArray(), gender.ToString());
                    gridJobIcon.SetActive(true);
                    SetJobIcons(char2);
                    break;

                case JobGrowthState.Job3:
                    var char3 = GetJobGradeCharacters(3);
                    GetJobCharacters().SetCharacterImages(char3.Select(x => x.ToString()).ToArray(), gender.ToString());
                    gridJobIcon.SetActive(true);
                    SetJobIcons(char3);
                    break;

                case JobGrowthState.Job4:
                    var char4 = GetJobGradeCharacters(4);
                    GetJobCharacters().SetCharacterImages(char4.Select(x => x.ToString()).ToArray(), gender.ToString());
                    gridJobIcon.SetActive(true);
                    SetJobIcons(char4);
                    break;

                case JobGrowthState.Job5:
                    GetJobCharacters().SetCharacterImages(new string[] { Job.Novice.ToString() }, gender.ToString());
                    gridJobIcon.SetActive(false);
                    break;

                case JobGrowthState.GuildSquare:
                    GetJobCharacters().SetItemImage();
                    gridJobIcon.SetActive(false);
                    break;

                case JobGrowthState.ShareClone:
                    GetJobCharacters();//.SetItemImage();
                    gridJobIcon.SetActive(false);
                    break;

                case JobGrowthState.Sharevice1:
                    GetJobCharacters().SetNpcImage();
                    gridJobIcon.SetActive(false);
                    break;

                case JobGrowthState.Sharevice2:
                    GetJobCharacters().SetNpcImage();
                    gridJobIcon.SetActive(false);
                    break;

                default:
                    gridJobIcon.SetActive(false);
                    return;
            }
        }

        void SetReward()
        {
            // 아이콘
            switch (state)
            {
                case JobGrowthState.Job1:
                case JobGrowthState.Job2:
                case JobGrowthState.ShareClone:
                    //case JobGrowthState.Job5: // 슬롯오픈 예정. 바뀔수 있음
                    iconSlot.SetActive(true);
                    break;

                default:
                    iconSlot.SetActive(false);
                    break;
            }

            iconClone.SetActive(state == JobGrowthState.Job4);
            iconFilter.SetActive(state == JobGrowthState.Job3);
            iconGuildSquare.SetActive(state == JobGrowthState.GuildSquare);

            iconSharevice1.SetActive(state == JobGrowthState.Sharevice1);
            if (state == JobGrowthState.Sharevice1) SetTextures(iconSharevice1, TextureType.ContentsUnlock);

            iconSharevice2.SetActive(state == JobGrowthState.Sharevice2);
            if (state == JobGrowthState.Sharevice2) SetTextures(iconSharevice2, TextureType.ContentsUnlock);

            // 레이블
            switch (state)
            {
                case JobGrowthState.Job1:
                    labelJob.SetActive(true);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.SkillPoint1:
                    labelJob.SetActive(false);
                    labelSkillpoint.SetActive(true);
                    break;

                case JobGrowthState.Job2:
                    labelJob.SetActive(true);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.Sharevice1:
                    labelJob.SetActive(false);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.SkillPoint2:
                    labelJob.SetActive(false);
                    labelSkillpoint.SetActive(true);
                    break;

                case JobGrowthState.Job3:
                    labelJob.SetActive(true);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.SkillPoint3:
                    labelJob.SetActive(false);
                    labelSkillpoint.SetActive(true);
                    break;

                case JobGrowthState.GuildSquare:
                    labelJob.SetActive(true);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.ShareClone:
                    labelJob.SetActive(false);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.Job4:
                    labelJob.SetActive(true);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.Sharevice2:
                    labelJob.SetActive(false);
                    labelSkillpoint.SetActive(false);
                    break;

                case JobGrowthState.Job5:
                    labelJob.SetActive(true);
                    labelSkillpoint.SetActive(false);
                    break;
            }

            labelClone.SetActive(state == JobGrowthState.ShareClone);

            // 텍스트
            switch (state)
            {
                case JobGrowthState.Job1:
                    labelJob.Title = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(1)); // JOB Lv. {LEVEL}
                    labelJob.Value = LocalizeKey._2019.ToText(); // 슬롯 오픈
                    break;

                case JobGrowthState.Job2:
                    labelJob.Title = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(2)); // JOB Lv. {LEVEL}
                    labelJob.Value = LocalizeKey._2019.ToText(); // 슬롯 오픈
                    break;

                case JobGrowthState.Job3:
                    labelJob.Title = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(3)); // JOB Lv. {LEVEL}
                    labelJob.Value = LocalizeKey._2024.ToText(); // 필터 오픈
                    break;

                case JobGrowthState.Job4:

                    labelJob.Title = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, BasisType.JOB_MAX_LEVEL.GetInt(4)); // JOB Lv. {LEVEL}
                    labelJob.Value = LocalizeKey._2029.ToText(); // 클론 쉐어
                    break;

                case JobGrowthState.Job5:
                    labelJob.Title = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, "?"); // JOB Lv. ?
                    labelJob.Value = "";// LocalizeKey._2019.ToText(); // 슬롯 오픈
                    break;

                case JobGrowthState.GuildSquare:
                    labelJob.Title = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, BasisType.GUILD_SQUARE_LIMIT_JOB_LEVEL.GetInt()); // JOB Lv. 
                    labelJob.Value = LocalizeKey._2025.ToText(); // 길드 스퀘어 오픈
                    break;

                case JobGrowthState.ShareClone:
                    labelClone.Title = "";// LocalizeKey._10242.ToText(); // 내 캐릭터
                    labelClone.Value = LocalizeKey._2019.ToText(); // 슬롯 오픈
                    break;

                case JobGrowthState.SkillPoint1:
                    labelSkillpoint.TitleKey = LocalizeKey._39014; // 스킬 포인트
                    labelSkillpoint.Value = string.Concat("+", presenter.GetRewardSkillPoint(1));
                    break;

                case JobGrowthState.SkillPoint2:
                    labelSkillpoint.TitleKey = LocalizeKey._39014; // 스킬 포인트
                    labelSkillpoint.Value = string.Concat("+", presenter.GetRewardSkillPoint(2));
                    break;

                case JobGrowthState.SkillPoint3:
                    labelSkillpoint.TitleKey = LocalizeKey._39014; // 스킬 포인트
                    labelSkillpoint.Value = string.Concat("+", presenter.GetRewardSkillPoint(3));
                    break;
            }
        }

        void SetComplete()
        {
            switch (state)
            {
                case JobGrowthState.Job1:
                    complete.SetActive(true);
                    break;

                case JobGrowthState.Job2:
                    complete.SetActive(2 <= jobGrade);
                    break;

                case JobGrowthState.Job3:
                    complete.SetActive(3 <= jobGrade);
                    break;

                case JobGrowthState.Sharevice1:
                    complete.SetActive(Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp, false));
                    break;

                case JobGrowthState.GuildSquare:
                    complete.SetActive(BasisType.GUILD_SQUARE_LIMIT_JOB_LEVEL.GetInt() <= jobLevel);
                    break;

                case JobGrowthState.ShareClone:
                    complete.SetActive(Entity.player.Quest.IsOpenContent(ContentType.ShareHope, false));
                    break;

                case JobGrowthState.SkillPoint1:
                case JobGrowthState.SkillPoint2:
                case JobGrowthState.SkillPoint3:
                    complete.SetActive(maxJobLevelByGroup <= jobLevel);
                    break;

                case JobGrowthState.Job4:
                    complete.SetActive(4 <= jobGrade);
                    break;

                case JobGrowthState.Sharevice2:
                    complete.SetActive(presenter.ActiveShareForce());
                    break;

                case JobGrowthState.Job5:
                    complete.SetActive(5 <= jobGrade);
                    break;

                default:
                    complete.SetActive(false); // 업데이트
                    break;
            }
        }

        // 현재 인덱스에 해당하는 이미지
        UIJobGrowthCharacter GetJobCharacters()
        {
            UIJobGrowthCharacter jobCharacters = null;
            foreach (var c in charactersArray)
            {
                if (c.name == state.ToString())
                    jobCharacters = c;
                else
                    c.SetActive(false);
            }

            return jobCharacters;
        }

        // 캐릭터 잡 아이콘 셋팅
        void SetJobIcons(Job[] chars)
        {
            gridJobIcon.SetValue(chars.Length);
            for (int i = 0; i < chars.Length; i++)
                jobicons[i].SetJobIcon(chars[i].GetJobIcon(), isAsync: false);
        }

        // 오브젝트 하위의 전체 텍스쳐 셋팅
        void SetTextures(GameObject go, TextureType textureType = TextureType.None, bool includeInactive = false)
        {
            foreach (var texture in go.GetComponentsInChildren<UITextureHelper>(includeInactive))
            {
                switch (textureType)
                {
                    case TextureType.Item:
                        texture.SetItem(texture.name, isAsync: false);
                        break;

                    case TextureType.ContentsUnlock:
                        texture.SetContentsUnlock(texture.name, isAsync: false);
                        break;

                    default:
                        texture.Set(texture.name, isAsync: false);
                        break;
                }
            }
        }

        // 스킬 포인트의 경우 메인 이미지(캐릭터) 대신 잡레벨 리스트 표시
        void SetJobLevelList()
        {
            // 잡레벨 리스트
            int[] jobLevelArray = presenter.GetRewardJobLevelsByGroup(state);

            if (jobLevelArray == default)
                return;

            maxJobLevelByGroup = jobLevelArray[jobLevelArray.Length - 1];

            // 스킬포인트 획득일 경우에만
            switch (state)
            {
                case JobGrowthState.SkillPoint1:
                case JobGrowthState.SkillPoint2:
                case JobGrowthState.SkillPoint3:
                    // 잡레벨 리스트 셋팅
                    jobLevelGrid.SetValue(jobLevelArray.Length);
                    for (int i = 0; i < jobLevelArray.Length; i++)
                    {
                        jobLabels[i].Text = LocalizeKey._3022.ToText().Replace(ReplaceKey.LEVEL, jobLevelArray[i]); // JOB Lv. {LEVEL}
                    }
                    break;
            }
        }

        bool IInspectorFinder.Find()
        {
            charactersArray = GetComponentsInChildren<UIJobGrowthCharacter>();

            if (gridJobIcon != null)
                jobicons = gridJobIcon.GetComponentsInChildren<UITextureHelper>();

            if (jobLevelGrid != null)
                jobLabels = jobLevelGrid.GetComponentsInChildren<UILabelHelper>();

            return true;
        }
    }
}