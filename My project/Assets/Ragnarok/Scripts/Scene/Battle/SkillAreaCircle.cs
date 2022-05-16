using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public interface ISkillArea
    {
        void SetPos(Vector3 casterPos, Vector3 targetPos);
        void StartAnim(float chaseTime, float castTime);
        void StartHide();

        float SkillArea { get; }
        bool CheckCollision(Vector3 pos, float radius);
    }

    public class SkillAreaCollisionCheckFunctionality
    {
        private Vector3 lastPos;
        private Vector3 lastDir;
        private float rangeFrom;
        private float rangeTo;
        private EffectPointType pointType;
        private float halfAngle;
        private bool forceColl;
        private bool isCircle;

        public void Init(EffectPointType pointType, float rangeFrom, float rangeTo, float angle, Vector3 casterPos, Vector3 targetPos)
        {
            this.pointType = pointType;

            forceColl = false;
            lastPos = pointType == EffectPointType.Executer ? casterPos : targetPos;
            this.rangeFrom = rangeFrom;
            this.rangeTo = rangeTo;
            halfAngle = pointType == EffectPointType.Target ? Mathf.PI : angle * 0.5f * Mathf.Deg2Rad;
            SetDir(casterPos, targetPos);

            // target 타입의 스킬의 경우 원 형태를 갖고 있다고 가정합니다.
            isCircle = pointType == EffectPointType.Target || angle > 359.0f;
        }

        public void Init(SkillInfo skillInfo, Vector3 casterPos, Vector3 targetPos)
        {
            pointType = skillInfo.PointType;

            forceColl = skillInfo.GetSkillArea() == -1f;
            lastPos = pointType == EffectPointType.Executer ? casterPos : targetPos;
            rangeFrom = skillInfo.RangeFrom;
            rangeTo = skillInfo.RangeTo;
            halfAngle = pointType == EffectPointType.Target ? Mathf.PI : skillInfo.Angle * 0.5f * Mathf.Deg2Rad;
            SetDir(casterPos, targetPos);

            // target 타입의 스킬의 경우 원 형태를 갖고 있다고 가정합니다.
            isCircle = pointType == EffectPointType.Target || skillInfo.Angle > 359.0f;
        }

        public void SetPos(Vector3 casterPos, Vector3 targetPos)
        {
            lastPos = pointType == EffectPointType.Executer ? casterPos : targetPos;
            SetDir(casterPos, targetPos);
        }

        private void SetDir(Vector3 casterPos, Vector3 targetPos)
        {
            Vector3 diff = targetPos - casterPos;
            diff.y = 0;

            if (diff.sqrMagnitude < 0.00001f)
                lastDir = Vector3.zero;
            else
                lastDir = pointType == EffectPointType.Executer ? (targetPos - casterPos).normalized : Vector3.zero;
        }

        public bool CheckCollision(Vector3 pos, float radius)
        {
            if (forceColl)
                return true;

            Vector3 diff = pos - lastPos;
            diff.y = 0; // y 축은 제거
            float a = rangeTo + radius;
            float sqrMag = diff.sqrMagnitude;

            if (sqrMag > a * a)
                return false;

            if (radius < rangeFrom)
            {
                a = rangeFrom - radius;
                if (sqrMag < a * a)
                    return false;
            }

            if (isCircle || lastDir == Vector3.zero)
            {
                return true;
            }
            else
            {
                float mag = diff.magnitude;

                Vector3 tangent = Vector3.Cross(lastDir, Vector3.up);
                Vector3 localPos = new Vector3(Vector3.Dot(lastDir, diff), 0.0f, Vector3.Dot(tangent, diff));

                float cos = Mathf.Cos(halfAngle);
                float sin = Mathf.Sin(halfAngle);

                bool isPositiveSide = localPos.z > 0;

                Vector3 localTan = new Vector3(-sin, 0, cos);
                localTan.z *= isPositiveSide ? 1 : -1;

                if (mag > radius)
                {
                    var dot = Vector3.Dot(localPos, localTan);

                    if (dot > radius)
                        return false;
                }

                Vector3 fanEdgeDir = new Vector3(cos, 0, sin);
                fanEdgeDir.z *= isPositiveSide ? 1 : -1;

                float sqrRad = radius * radius;

                Vector3 vertexDiff = localPos - fanEdgeDir * rangeTo;
                if (Vector3.Dot(vertexDiff, fanEdgeDir) > 0 && Vector3.Dot(vertexDiff, localTan) > 0 && vertexDiff.sqrMagnitude > sqrRad)
                    return false;

                vertexDiff = localPos - fanEdgeDir * rangeFrom;
                if (Vector3.Dot(vertexDiff, fanEdgeDir) < 0 && Vector3.Dot(vertexDiff, localTan) > 0 && vertexDiff.sqrMagnitude > sqrRad)
                    return false;

                return true;
            }
        }
    }

    public class EmptySkillArea : ISkillArea
    {
        public float SkillArea { get; private set; }

        private SkillAreaCollisionCheckFunctionality checker = new SkillAreaCollisionCheckFunctionality();

        public EmptySkillArea(SkillInfo skillInfo, Vector3 casterPos, Vector3 targetPos)
        {
            SkillArea = skillInfo.GetSkillArea();
            checker.Init(skillInfo, casterPos, targetPos);
        }

        public void SetPos(Vector3 casterPos, Vector3 targetPos)
        {
            checker.SetPos(casterPos, targetPos);
        }

        public void StartAnim(float chaseTime, float castTime)
        {
        }

        public void StartHide()
        {
        }

        public bool CheckCollision(Vector3 pos, float radius)
        {
            return checker.CheckCollision(pos, radius);
        }
    }

    public class SkillAreaCircle : PoolObject, ISkillArea
    {
        public interface IAnimationProperty
        {
            Gradient BgColor { get; }
            Gradient ProgressColor { get; }
            AnimationCurve ProgressTint { get; }
            Gradient OutlineColor { get; }
            AnimationCurve OutlineTint { get; }
            AnimationCurve GlobalAlpha { get; }
            float ShowTime { get; }
            float HideTime { get; }
            float Duration { get; }
        }

        [System.Serializable]
        public class AnimationProperty : IAnimationProperty
        {
            [SerializeField] float showTime;
            [SerializeField] float hideTime;
            [SerializeField] AnimationCurve globalAlpha = AnimationCurve.Constant(0, 1, 1);
            [SerializeField] Gradient bgColor;
            [SerializeField] Gradient outlineColor;
            [SerializeField] AnimationCurve outlineTint = AnimationCurve.Constant(0, 1, 1);
            [SerializeField] Gradient progressColor;
            [SerializeField] AnimationCurve progressTint = AnimationCurve.Constant(0, 1, 1);

            public Gradient BgColor => bgColor;
            public Gradient ProgressColor => progressColor;
            public AnimationCurve ProgressTint => progressTint;
            public Gradient OutlineColor => outlineColor;
            public AnimationCurve OutlineTint => outlineTint;
            public AnimationCurve GlobalAlpha => globalAlpha;
            public float ShowTime => showTime;
            public float HideTime => hideTime;
            public virtual float Duration => 0;
        }

        [System.Serializable]
        public class AnimationPropertyWithDuration : AnimationProperty
        {
            [SerializeField] float duration;

            public override float Duration => duration;
        }

        [System.Serializable]
        public class AnimationContainer
        {
            [SerializeField] AnimationPropertyWithDuration noChaseNoCast;
            [SerializeField] AnimationProperty yesChaseNoCast;
            [SerializeField] AnimationProperty noChaseYesCast;
            [SerializeField] AnimationProperty yesChaseYesCast;

            public IAnimationProperty GetProperty(float chaseTime, float castTime)
            {
                if (chaseTime == 0)
                {
                    if (castTime == 0)
                        return noChaseNoCast;
                    else
                        return noChaseYesCast;
                }
                else
                {
                    if (castTime == 0)
                        return yesChaseNoCast;
                    else
                        return yesChaseYesCast;
                }
            }
        }

        [SerializeField] GameObject areaRoot;
        [SerializeField] SkinnedMeshRenderer areaRenderer;
        [SerializeField] AnimationContainer attackAnimContainer;
        [SerializeField] AnimationContainer recoveryAnimContainer;

        private MaterialPropertyBlock propertyBlock;
        private int progressColorProperty;
        private int progressTintProperty;
        private int outlineColorProperty;
        private int outlineTintProperty;
        private int bgColorProperty;
        private int outlineWidthProperty;
        private int angleProperty;
        private int rangeFromProperty;
        private int rangeToProperty;
        private int progressProperty;
        private int globalAlphaProperty;

        private SkillAreaCollisionCheckFunctionality checker = new SkillAreaCollisionCheckFunctionality();

        private EffectPointType pointType;
        private LifeCycle parentLifeCycle;
        private AnimationContainer animationContainerToUse;
        private IEnumerator animRoutine;
        private float currentAlpha;

        public float SkillArea { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            propertyBlock = new MaterialPropertyBlock();

            bgColorProperty = Shader.PropertyToID("_BGColor");
            progressColorProperty = Shader.PropertyToID("_ProgressColor");
            progressTintProperty = Shader.PropertyToID("_ProgressTint");
            outlineColorProperty = Shader.PropertyToID("_OutlineColor");
            outlineTintProperty = Shader.PropertyToID("_OutlineTint");

            outlineWidthProperty = Shader.PropertyToID("_Outline");
            angleProperty = Shader.PropertyToID("_Angle");
            rangeFromProperty = Shader.PropertyToID("_RangeFrom");
            rangeToProperty = Shader.PropertyToID("_RangeTo");
            progressProperty = Shader.PropertyToID("_Progress");
            globalAlphaProperty = Shader.PropertyToID("_GlobalAlpha");
        }

        protected override void OnDestroy()
        {
            areaRenderer.SetPropertyBlock(null);
            base.OnDestroy();
        }

        private void OnDisable()
        {
            animRoutine = null;
            if (!IsPooled)
                Release();
        }

        public override void OnCreate(IPooledDespawner despawner, string poolID)
        {
            base.OnCreate(despawner, poolID);

            CachedGameObject.layer = Layer.SKILLFX;
            CachedTransform.SetChildLayer(Layer.SKILLFX);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            if (parentLifeCycle != null)
            {
                parentLifeCycle.OnDispose -= OnParentDispose;
                parentLifeCycle = null;
            }
        }

        public void Initialize(EffectPointType pointType, float angle, float rangeFrom, float rangeTo, Vector3 casterPos, Vector3 targetPos)
        {
            this.pointType = pointType;
            SkillArea = 0;
            currentAlpha = 0;

            propertyBlock.SetFloat(rangeToProperty, rangeFrom);
            propertyBlock.SetFloat(rangeFromProperty, rangeTo);
            propertyBlock.SetFloat(angleProperty, pointType == EffectPointType.Target ? 2.0f : angle / 180.0f);
            propertyBlock.SetFloat(globalAlphaProperty, 0);
            propertyBlock.SetFloat(progressProperty, 0);

            CachedTransform.position = pointType == EffectPointType.Executer ? casterPos : targetPos;

            checker.Init(pointType, rangeFrom, rangeTo, angle, casterPos, targetPos);
        }

        public void Initialize(LifeCycle lifeCycle, SkillInfo skillInfo, Vector3 casterPos, Vector3 targetPos)
        {
            if (parentLifeCycle != null)
                parentLifeCycle.OnDispose -= OnParentDispose;

            parentLifeCycle = lifeCycle;
            parentLifeCycle.OnDispose += OnParentDispose;

            bool isAttackSkill = skillInfo.ActiveOptions.HasDamageValue || skillInfo.ActiveOptions.HasCrowdControl; // 공격형 스킬

            animationContainerToUse = isAttackSkill ? attackAnimContainer : recoveryAnimContainer;

            pointType = skillInfo.PointType;
            SkillArea = skillInfo.GetSkillArea();
            currentAlpha = 0;

            propertyBlock.SetFloat(rangeToProperty, skillInfo.RangeTo);
            propertyBlock.SetFloat(rangeFromProperty, skillInfo.RangeFrom);
            propertyBlock.SetFloat(angleProperty, pointType == EffectPointType.Target ? 2.0f : skillInfo.Angle / 180.0f);
            propertyBlock.SetFloat(globalAlphaProperty, 0);
            areaRenderer.SetPropertyBlock(propertyBlock);

            CachedTransform.position = pointType == EffectPointType.Executer ? casterPos : targetPos;

            checker.Init(skillInfo, casterPos, targetPos);

            areaRenderer.localBounds = new Bounds(Vector3.zero, new Vector3(1, 0, 1) * skillInfo.RangeTo * 2);
        }

        private void OnParentDispose()
        {
            parentLifeCycle.OnDispose -= OnParentDispose;
            parentLifeCycle = null;

            if (!IsPooled)
                StartHide();
        }

        public void SetPos(Vector3 casterPos, Vector3 targetPos)
        {
            CachedTransform.position = pointType == EffectPointType.Executer ? casterPos : targetPos;
            checker.SetPos(casterPos, targetPos);
            SetDir(casterPos, targetPos);
        }

        private void SetDir(Vector3 casterPos, Vector3 targetPos)
        {
            Vector3 diff = targetPos - casterPos;
            diff.y = 0;

            if (diff.sqrMagnitude > 0.00001f && pointType == EffectPointType.Executer)
                CachedTransform.forward = diff;
        }

        public void StartAnim(float chaseTime, float castTime)
        {
            if (animationContainerToUse == null)
                return;

            var property = animationContainerToUse.GetProperty(chaseTime, castTime);

            propertyBlock.SetFloat(progressProperty, castTime == 0 ? 1 : 0);
            propertyBlock.SetColor(progressColorProperty, property.ProgressColor.Evaluate(0));
            propertyBlock.SetFloat(progressTintProperty, property.ProgressTint.Evaluate(0));
            areaRenderer.SetPropertyBlock(propertyBlock);

            animRoutine = Anim(property, chaseTime, castTime);
        }

        public void StartHide()
        {
            animRoutine = HideAnim();
        }

        private void Update()
        {
            if (animRoutine != null)
                if (!animRoutine.MoveNext())
                    animRoutine = null;
        }

        private IEnumerator Anim(IAnimationProperty prop, float chaseTime, float castTime)
        {
            float totalProg = 0f;
            float baseTime = Time.realtimeSinceStartup;

            float cc = chaseTime + castTime;
            bool isShot = cc == 0;

            float animTotalDuration;
            float showTime;
            float hideTime;
            float hideStartTime;
            float castingStartTime;

            if (isShot)
            {
                showTime = prop.ShowTime;
                hideTime = prop.HideTime;
                animTotalDuration = showTime + prop.Duration + hideTime;
                hideStartTime = showTime + prop.Duration;
                castingStartTime = 0;
            }
            else
            {
                showTime = Mathf.Min(cc, prop.ShowTime);
                hideTime = prop.HideTime;
                animTotalDuration = cc + hideTime;
                hideStartTime = cc;
                castingStartTime = chaseTime;
            }

            int bgOutlineAnimPhase = 0;

            while (totalProg < 1)
            {
                float elapsedTime = Time.realtimeSinceStartup - baseTime;
                totalProg = Mathf.Clamp01(elapsedTime / animTotalDuration);

                if (bgOutlineAnimPhase == 0)
                {
                    float showProg = Mathf.Clamp01(elapsedTime / showTime);
                    propertyBlock.SetColor(bgColorProperty, prop.BgColor.Evaluate(showProg * 0.5f));
                    propertyBlock.SetColor(outlineColorProperty, prop.OutlineColor.Evaluate(showProg * 0.5f));
                    propertyBlock.SetFloat(outlineTintProperty, prop.OutlineTint.Evaluate(showProg * 0.5f));
                    currentAlpha = prop.GlobalAlpha.Evaluate(showProg * 0.5f);
                    propertyBlock.SetFloat(globalAlphaProperty, currentAlpha);

                    if (elapsedTime >= showTime)
                        bgOutlineAnimPhase = 1;
                }
                else if (bgOutlineAnimPhase == 1)
                {
                    if (elapsedTime >= hideStartTime)
                        bgOutlineAnimPhase = 2;
                }
                else if (bgOutlineAnimPhase == 2)
                {
                    float hideProg = Mathf.Clamp01((elapsedTime - hideStartTime) / hideTime);
                    propertyBlock.SetColor(bgColorProperty, prop.BgColor.Evaluate(hideProg * 0.5f + 0.5f));
                    propertyBlock.SetColor(outlineColorProperty, prop.OutlineColor.Evaluate(hideProg * 0.5f + 0.5f));
                    propertyBlock.SetFloat(outlineTintProperty, prop.OutlineTint.Evaluate(hideProg * 0.5f + 0.5f));
                    currentAlpha = prop.GlobalAlpha.Evaluate(hideProg * 0.5f + 0.5f);
                    propertyBlock.SetFloat(globalAlphaProperty, currentAlpha);
                }

                if (castTime > 0 && castingStartTime <= elapsedTime)
                {
                    var castProg = Mathf.Clamp01((elapsedTime - castingStartTime) / castTime);
                    propertyBlock.SetColor(progressColorProperty, prop.ProgressColor.Evaluate(castProg));
                    propertyBlock.SetFloat(progressTintProperty, prop.ProgressTint.Evaluate(castProg));
                    propertyBlock.SetFloat(progressProperty, castProg);
                }
                else if (castTime == 0)
                {
                    var castProg = Mathf.Clamp01((elapsedTime - hideStartTime) / hideTime);
                    propertyBlock.SetColor(progressColorProperty, prop.ProgressColor.Evaluate(castProg));
                    propertyBlock.SetFloat(progressTintProperty, prop.ProgressTint.Evaluate(castProg));
                }

                areaRenderer.SetPropertyBlock(propertyBlock);

                yield return null;
            }

            animRoutine = null;
            Release();
        }

        private IEnumerator HideAnim()
        {
            float baseTime = Time.realtimeSinceStartup;
            float prog = 0;

            while (prog < 1)
            {
                float elapsedTime = baseTime - Timing.DeltaTime;
                prog = Mathf.Clamp01(elapsedTime / 0.15f);
                currentAlpha = Mathf.Lerp(currentAlpha, 0, prog);

                propertyBlock.SetFloat(globalAlphaProperty, currentAlpha);
                areaRenderer.SetPropertyBlock(propertyBlock);

                yield return null;
            }

            animRoutine = null;
            Release();
        }

        public bool CheckCollision(Vector3 pos, float radius)
        {
            return checker.CheckCollision(pos, radius);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("라그나로크/스킬 범위 매시 생성")]
        private static void GenerateMesh()
        {
            Mesh mesh = new Mesh();

            int width = 64;
            Vector3[] vertices = new Vector3[4 + 4 * (width + 1) + 4];
            Vector2[] uvs = new Vector2[vertices.Length];

            vertices[0] = new Vector3(-1, -1, 1);
            vertices[1] = new Vector3(-1, 0, 1);
            vertices[2] = new Vector3(-1, 1, 1);
            vertices[3] = new Vector3(-1, 2, 1);

            vertices[vertices.Length - 4] = new Vector3(2, -1, 1);
            vertices[vertices.Length - 3] = new Vector3(2, 0, 1);
            vertices[vertices.Length - 2] = new Vector3(2, 1, 1);
            vertices[vertices.Length - 1] = new Vector3(2, 2, 1);

            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(0, 1 / 3.0f);
            uvs[2] = new Vector2(0, 2 / 3.0f);
            uvs[3] = new Vector2(0, 1);

            uvs[uvs.Length - 4] = new Vector2(1, 0);
            uvs[uvs.Length - 3] = new Vector2(1, 1 / 3.0f);
            uvs[uvs.Length - 2] = new Vector2(1, 2 / 3.0f);
            uvs[uvs.Length - 1] = new Vector2(1, 1);

            for (int i = 0; i <= width; ++i)
            {
                float u = (float)i / width;

                for (int j = 0; j < 4; ++j)
                {
                    int index = 4 + 4 * i + j;
                    vertices[index] = new Vector3(u, -1 + j, j == 0 || j == 3 ? 1 : 0);
                    uvs[index] = new Vector2((u + 1) / 3.0f, j / 3.0f);
                }
            }

            List<int> triangles = new List<int>();

            for (int i = 1; i <= (width + 2); ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int index = 4 * i + j;

                    if (j > 0)
                    {
                        triangles.Add(index - 5);
                        triangles.Add(index - 4);
                        triangles.Add(index - 1);
                        triangles.Add(index);
                        triangles.Add(index - 1);
                        triangles.Add(index - 4);
                    }
                }
            }

            triangles[0] = 0;
            triangles[1] = 5;
            triangles[2] = 4;
            triangles[3] = 0;
            triangles[4] = 1;
            triangles[5] = 5;

            int baseIndex = vertices.Length - 1 - 5;

            triangles[triangles.Count - 6] = baseIndex + 0;
            triangles[triangles.Count - 5] = baseIndex + 5;
            triangles[triangles.Count - 4] = baseIndex + 4;
            triangles[triangles.Count - 3] = baseIndex + 0;
            triangles[triangles.Count - 2] = baseIndex + 1;
            triangles[triangles.Count - 1] = baseIndex + 5;

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs;

            UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/SkillAreaMesh.mesh");
            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.CustomEditor(typeof(SkillAreaCircle))]
        public class Editor : UnityEditor.Editor
        {
            SkillAreaCircle comp;
            float animTo;
            float animTime;
            float animDuration;
            AnimationProperty colorContainer;
            Material sharedMaterial;
            Material instMat;
            IEnumerator anim;

            float chasingTime;
            float castingTime;

            private void OnEnable()
            {
                comp = target as SkillAreaCircle;
                chasingTime = 0;
                castingTime = 0;

                UnityEditor.EditorApplication.update += Update;
                if (comp.areaRenderer && comp.gameObject.scene.rootCount > 0)
                {
                    sharedMaterial = comp.areaRenderer.sharedMaterial;
                    instMat = new Material(Shader.Find("Custom/Mobile/SkillArea"));
                }
            }

            private void OnDisable()
            {
                UnityEditor.EditorApplication.update -= Update;
                if (comp.areaRenderer && sharedMaterial != null)
                    comp.areaRenderer.sharedMaterial = sharedMaterial;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                chasingTime = UnityEditor.EditorGUILayout.FloatField("ChasingTime", chasingTime);
                chasingTime = Mathf.Max(chasingTime, 0);

                castingTime = UnityEditor.EditorGUILayout.FloatField("CastingTime", castingTime);
                castingTime = Mathf.Max(castingTime, 0);

                if (GUILayout.Button("Simulate_Attack"))
                {
                    animDuration = 2;
                    animTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
                    animTo = animTime + animDuration;

                    anim = Anim(comp.attackAnimContainer.GetProperty(chasingTime, castingTime), chasingTime, castingTime);
                }

                if (GUILayout.Button("Simulate_Recovery"))
                {
                    animDuration = 2;
                    animTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
                    animTo = animTime + animDuration;

                    anim = Anim(comp.recoveryAnimContainer.GetProperty(chasingTime, castingTime), chasingTime, castingTime);
                }
            }

            private void Update()
            {
                if (anim == null || instMat == null)
                    return;

                if (!anim.MoveNext())
                {
                    anim = null;
                    comp.areaRenderer.sharedMaterial = sharedMaterial;
                }
            }

            private IEnumerator Anim(IAnimationProperty prop, float chaseTime, float castTime)
            {
                instMat.CopyPropertiesFromMaterial(sharedMaterial);
                comp.areaRenderer.sharedMaterial = instMat;
                instMat.SetFloat("_Progress", castTime == 0 ? 1 : 0);
                instMat.SetFloat("_GlobalAlpha", 0);
                instMat.SetColor("_ProgressColor", prop.ProgressColor.Evaluate(0));
                instMat.SetFloat("_ProgressTint", prop.ProgressTint.Evaluate(0));

                float duration = prop.Duration;

                float totalProg = 0f;
                float baseTime = (float)UnityEditor.EditorApplication.timeSinceStartup;

                float cc = chaseTime + castTime;
                bool isShot = cc == 0;

                float animTotalDuration;
                float showTime;
                float hideTime;
                float hideStartTime;
                float castingStartTime;

                if (isShot)
                {
                    showTime = prop.ShowTime;
                    hideTime = prop.HideTime;
                    animTotalDuration = showTime + duration + hideTime;
                    hideStartTime = showTime + duration;
                    castingStartTime = 0;
                }
                else
                {
                    showTime = Mathf.Min(cc, prop.ShowTime);
                    hideTime = prop.HideTime;
                    animTotalDuration = cc + hideTime;
                    hideStartTime = cc;
                    castingStartTime = chaseTime;
                }

                int bgOutlineAnimPhase = 0;

                while (totalProg < 1)
                {
                    float elapsedTime = (float)UnityEditor.EditorApplication.timeSinceStartup - baseTime;
                    totalProg = Mathf.Clamp01(elapsedTime / animTotalDuration);

                    if (bgOutlineAnimPhase == 0)
                    {
                        float showProg = Mathf.Clamp01(elapsedTime / showTime);
                        instMat.SetColor("_BGColor", prop.BgColor.Evaluate(showProg * 0.5f));
                        instMat.SetColor("_OutlineColor", prop.OutlineColor.Evaluate(showProg * 0.5f));
                        instMat.SetFloat("_OutlineTint", prop.OutlineTint.Evaluate(showProg * 0.5f));
                        instMat.SetFloat("_GlobalAlpha", prop.GlobalAlpha.Evaluate(showProg * 0.5f));

                        if (elapsedTime >= showTime)
                            bgOutlineAnimPhase = 1;
                    }
                    else if (bgOutlineAnimPhase == 1)
                    {
                        if (elapsedTime >= hideStartTime)
                            bgOutlineAnimPhase = 2;
                    }
                    else if (bgOutlineAnimPhase == 2)
                    {
                        float hideProg = Mathf.Clamp01((elapsedTime - hideStartTime) / hideTime);
                        instMat.SetColor("_BGColor", prop.BgColor.Evaluate(hideProg * 0.5f + 0.5f));
                        instMat.SetColor("_OutlineColor", prop.OutlineColor.Evaluate(hideProg * 0.5f + 0.5f));
                        instMat.SetFloat("_OutlineTint", prop.OutlineTint.Evaluate(hideProg * 0.5f + 0.5f));
                        instMat.SetFloat("_GlobalAlpha", prop.GlobalAlpha.Evaluate(hideProg * 0.5f + 0.5f));
                    }

                    if (castTime > 0 && castingStartTime <= elapsedTime)
                    {
                        var castProg = Mathf.Clamp01((elapsedTime - castingStartTime) / castTime);
                        instMat.SetColor("_ProgressColor", prop.ProgressColor.Evaluate(castProg));
                        instMat.SetFloat("_ProgressTint", prop.ProgressTint.Evaluate(castProg));
                        instMat.SetFloat("_Progress", castProg);
                    }
                    else
                    {
                        var castProg = Mathf.Clamp01((elapsedTime - hideStartTime) / hideTime);
                        instMat.SetColor("_ProgressColor", prop.ProgressColor.Evaluate(castProg));
                        instMat.SetFloat("_ProgressTint", prop.ProgressTint.Evaluate(castProg));
                    }

                    yield return null;
                }

                while ((float)UnityEditor.EditorApplication.timeSinceStartup - baseTime < animTotalDuration + 1)
                    yield return null;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.color = Color.white;
        }
#endif
    }
}