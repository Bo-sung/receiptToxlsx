using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// VisualFX의 ParticleSystem, AnimationClip을 Editor Mode에서 재생해준다.
    /// </summary>
    public sealed class VfxEditorModePlayer : IEditorModePlayer
    {
        private class Setting
        {
            private static readonly AnimationCurve DEFAULT = AnimationCurve.Linear(0f, 0f, 0f, 0f);

            private readonly SkillSetting.Vfx vfx;
            private readonly SkillSetting.Projectile projectile;
            private readonly ProjectileSetting.Start start;
            private readonly ProjectileSetting.Loop loop;
            private readonly ProjectileSetting.End end;

            public Setting(SkillSetting.Vfx vfx)
            {
                this.vfx = vfx;
            }

            public Setting(SkillSetting.Projectile projectile, ProjectileSetting.Start start)
            {
                this.projectile = projectile;
                this.start = start;
            }

            public Setting(SkillSetting.Projectile projectile, ProjectileSetting.Loop loop)
            {
                this.projectile = projectile;
                this.loop = loop;
            }

            public Setting(SkillSetting.Projectile projectile, ProjectileSetting.End end)
            {
                this.projectile = projectile;
                this.end = end;
            }

            public float GetActiveTime()
            {
                if (start != null) return 0f; // 바로 켜짐
                if (loop != null) return 0f; // 바로 켜짐
                if (end != null) return (projectile.duration - end.overlapTime) * 0.01f;
                return vfx.time * 0.01f;
            }

            public string GetName()
            {
                if (start != null) return start.name;
                if (loop != null) return loop.name;
                if (end != null) return end.name;
                return vfx.name;
            }

            public bool HasDuration()
            {
                if (start != null) return false; // Duration 없음
                if (loop != null) return projectile.duration > 0 || loop.delayDestory > 0;
                if (end != null) return false; // Duration 없음
                return vfx.duration > 0;
            }

            public float GetDuration()
            {
                if (start != null) return 0f;
                if (loop != null) return (projectile.duration + loop.delayDestory) * 0.01f;
                if (end != null) return 0f;
                return vfx.duration * 0.01f;
            }

            public bool GetToTarget()
            {
                // TODO: 테스트 코드 (원래는 loop가 끝나는 지점에서 보이는 것이 정석)
                if (end != null) return true; // Projectile의 마지막 경우에는 무조건 toTarget: false

                return vfx != null ? vfx.toTarget : false; // Projectile의 경우에는 무조건 toTarget: false
            }

            public string GetNodeName()
            {
                return vfx != null ? vfx.node : projectile.node;
            }

            public string GetDestinationNodeName()
            {
                if (start != null) return null;
                if (loop != null) return loop.node;
                if (end != null) return null;
                return null;
            }

            public bool GetIsAttach()
            {
                return vfx != null ? vfx.isAttach : false; // Projectile의 경우에는 무조건 isAttach: false
            }

            public Vector3 GetOffset()
            {
                return vfx != null ? vfx.offset : projectile.offset;
            }

            public Vector3 GetRotate()
            {
                return vfx != null ? vfx.rotate : projectile.rotate;
            }

            public AnimationCurve GetHeightCurve()
            {
                return loop != null ? loop.heightCurve : DEFAULT;
            }

            public AnimationCurve GetMoveCurve()
            {
                return loop != null ? loop.moveCurve : DEFAULT;
            }

            public AnimationCurve GetSideDirCurve()
            {
                return loop != null ? loop.sideDirCurve : DEFAULT;
            }
        }

        private readonly PrefabContainer container;
        private readonly Setting setting;

        GameObject vfx;

        Transform unit, target;
        Vector3 sourcePos, targetPos;

        GameObject animationGameObject;
        ParticleSystem[] particles;
        Animator animator;
        Animation animation;
        AnimationClip animationClip;

        double lastFrameEditorTime;
        float currentTime;
        bool isActive = true;

        public VfxEditorModePlayer(SkillSetting.Vfx vfx) : this()
        {
            setting = new Setting(vfx);
            Initialize();
        }

        public VfxEditorModePlayer(SkillSetting.Projectile projectile, ProjectileSetting.Start start) : this()
        {
            setting = new Setting(projectile, start);
            Initialize();
        }

        public VfxEditorModePlayer(SkillSetting.Projectile projectile, ProjectileSetting.Loop loop) : this()
        {
            setting = new Setting(projectile, loop);
            Initialize();
        }

        public VfxEditorModePlayer(SkillSetting.Projectile projectile, ProjectileSetting.End end) : this()
        {
            setting = new Setting(projectile, end);
            Initialize();
        }

        private VfxEditorModePlayer()
        {
            EditorUtility.DisplayProgressBar("Loading VfxEditorModePlayer", "Battle Effect", 1f);
            container = AssetDatabase.LoadAssetAtPath<PrefabContainer>(SkillPreviewWindow.BATTLE_EFFECT_ASSET_PATH);
            EditorUtility.ClearProgressBar();
        }

        private void Initialize()
        {
            string savedName = vfx ? vfx.name : string.Empty;
            string name = setting.GetName();

            if (!savedName.Equals(name))
            {
                Release();
                Create(name);
            }

            Link();
        }

        public void Release()
        {
            NGUITools.Destroy(vfx);

            vfx = null;

            particles = null;
            animation = null;
            animator = null;
            animationClip = null;
        }

        public void SetActive(bool isActive)
        {
            if (this.isActive == isActive)
                return;

            this.isActive = isActive;

            if (vfx == null)
                return;

            vfx.SetActive(isActive);

            if (isActive)
            {
                Initialize();

                sourcePos = vfx.transform.position;
                if (target == null)
                {
                    targetPos = Vector3.zero;
                }
                else
                {
                    Transform node = GetNode(target, setting.GetDestinationNodeName());
                    targetPos = node == null ? target.position : node.position;
                }

                lastFrameEditorTime = EditorApplication.timeSinceStartup;
                currentTime = 0f;

                if (animationGameObject != null && animationClip != null)
                    animationClip.SampleAnimation(animationGameObject, currentTime);

                if (particles != null)
                {
                    foreach (var item in particles)
                    {
                        if (item == null)
                            continue;

                        item.Simulate(0f, true);
                        item.Play();

                        var emission = item.emission;
                        emission.enabled = true;
                    }
                }
            }
            else
            {
                Transform transform = vfx.transform;

                if (transform.parent)
                    transform.SetParent(null);

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
        }

        public void Play(float t)
        {
            float activeTime = setting.GetActiveTime();

            bool isActive = t >= activeTime;
            SetActive(isActive);

            if (!isActive)
                return;

            double timeSinceStartup = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(timeSinceStartup - lastFrameEditorTime);
            lastFrameEditorTime = timeSinceStartup;
            currentTime += deltaTime;

            bool hasDuration = setting.HasDuration();
            if (hasDuration)
            {
                float duration = setting.GetDuration();
                float normalizedTime = currentTime / duration;

                AnimationCurve heightCurve = setting.GetHeightCurve();
                AnimationCurve moveCurve = setting.GetMoveCurve();
                AnimationCurve sideDirCurve = setting.GetSideDirCurve();

                Vector3 toTarget = targetPos - sourcePos;
                Vector3 move = toTarget * moveCurve.Evaluate(normalizedTime);

                move.y += heightCurve.Evaluate(normalizedTime);

                float sideDir = sideDirCurve.Evaluate(normalizedTime);

                if (!Mathf.Approximately(sideDir, 0f))
                {
                    Vector3 right = Vector3.Cross(toTarget.normalized, Vector3.up);
                    move += right * sideDir;
                }

                SetPosition(sourcePos + move);

                if (t > duration)
                {
                    if (particles != null)
                    {
                        foreach (var item in particles)
                        {
                            if (item == null)
                                continue;

                            var emission = item.emission;
                            emission.enabled = false;
                        }
                    }
                }
            }

            if (animationGameObject != null && animationClip != null)
                animationClip.SampleAnimation(animationGameObject, currentTime);

            if (particles != null)
            {
                foreach (var item in particles)
                {
                    if (item == null)
                        continue;

                    item.Simulate(deltaTime, true, false, false);
                    //item.Play();
                }
            }
        }

        public void SetUnit(GameObject goUnit)
        {
            unit = goUnit ? goUnit.transform : null;
        }

        public void SetTarget(GameObject goTarget)
        {
            target = goTarget ? goTarget.transform : null;
        }

        private void Link()
        {
            if (vfx == null)
                return;

            Transform transform = vfx.transform;

            Vector3 offset = setting.GetOffset();
            Vector3 rotate = setting.GetRotate();
            //bool isEmptyOffset = offset.Equals(Vector3.zero) && rotate.Equals(Vector3.zero);

            Transform source = setting.GetToTarget() ? target : unit; // ToTarget
            Transform node = GetNode(source, setting.GetNodeName()); // Node
            transform.SetParent(node ?? source, worldPositionStays: false); // Attach

            transform.localPosition = offset; // Offset
            transform.localRotation = Quaternion.Euler(rotate); // Rotate

            // 다시 빼주는 작업
            if (!setting.GetIsAttach())
            {
                transform.SetParent(null, worldPositionStays: true);
                transform.localScale = Vector3.one;
            }
        }

        private void SetPosition(Vector3 pos)
        {
            if (vfx == null)
                return;

            Transform transform = vfx.transform;

            transform.position = pos;

            if (target)
                transform.LookAt(target.position);
        }

        private void Create(string name)
        {
            GameObject obj = GetEffect(name);

            if (obj == null)
                return;

            EditorUtility.DisplayProgressBar("Create Effect", "Battle Effect", 1f);
            vfx = PrefabUtility.InstantiatePrefab(obj) as GameObject;
            EditorUtility.ClearProgressBar();

            ClearParticleSystemRandom(vfx);
            particles = GetParentParticleSystem(vfx).ToArray();

            animator = vfx.GetComponent<Animator>();

            if (animator)
            {
                var clips = GetAnimationClipOfAnimator(animator);

                if (clips != null)
                    animationClip = clips[0];

                animationGameObject = animator.gameObject;
            }

            animation = vfx.GetComponent<Animation>();

            if (animation == null)
                animation = vfx.GetComponentInChildren<Animation>();

            if (animation != null)
            {
                animationGameObject = animation.gameObject;
                animationClip = animation.clip;
            }
        }

        private List<T> GetComponentsInChild<T>(Transform parent, int depth) where T : Component
        {
            List<T> list = new List<T>();

            if (depth == 0)
                return list;

            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>() as T;
                if (component != null)
                {
                    list.Add(component);
                }

                var childList = GetComponentsInChild<T>(child, depth - 1);
                list.AddRange(childList);
            }

            return list;
        }

        private List<ParticleSystem> GetParentParticleSystem(GameObject go)
        {
            List<ParticleSystem> list = new List<ParticleSystem>();
            var psArray = go.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in psArray)
            {
                if (IsRoot(ps))
                {
                    list.Add(ps);
                }
            }

            return list;
        }

        private bool IsRoot(ParticleSystem ps)
        {
            var parent = ps.transform.parent;

            if (parent == null)
                return true;

            return parent.GetComponent<ParticleSystem>() == false;
        }

        private void ClearParticleSystemRandom(GameObject go)
        {
            var psArray = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psArray)
            {
                ps.useAutoRandomSeed = false;
            }
        }

        private AnimationClip[] GetAnimationClipOfAnimator(Animator animator)
        {
            if (animator == null) return null;
            if (animator.runtimeAnimatorController == null) return null;
            if (animator.runtimeAnimatorController.animationClips == null) return null;
            if (animator.runtimeAnimatorController.animationClips.Length == 0) return null;

            return animator.runtimeAnimatorController.animationClips;
        }

        private GameObject GetEffect(string name)
        {
            foreach (var item in container.GetArray())
            {
                if (item.name.Equals(name))
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Find Recursive
        /// </summary>
        private Transform GetNode(Transform transform, string name)
        {
            if (transform == null)
                return null;

            if (string.IsNullOrEmpty(name))
                return null;

            if (transform.name.Equals(name))
                return transform;

            // 재귀함수를 통하여 모든 Transform 의 name 을 찾음
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = GetNode(transform.GetChild(i), name);

                if (child)
                    return child;
            }

            return null;
        }
    }
}