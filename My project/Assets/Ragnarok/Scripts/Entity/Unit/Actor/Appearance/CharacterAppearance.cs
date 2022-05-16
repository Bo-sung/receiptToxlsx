using UnityEngine;

namespace Ragnarok
{
    public sealed class CharacterAppearance : UnitAppearance
    {
        ICharacterPool characterPool;
        IItemPool itemPool;

        private Job job;
        private Gender gender;
        private EquipmentClassType weaponType;

        private PoolObject body;
        private PoolObject weapon;
        private PoolObject weaponEffect;
        private GameObject skinedMash;

        private bool isPrivateStoreSelling;
        private bool isShadowCharacter;

        private bool isUI;

        private bool isPlayEmotion;

        private string weaponPrefabName;
        private string weaponEffectName;

        private PoolObject costumeHat, costumeFace, costumeCape;
        private FollowTarget costumePet;
        private PoolObject costumeHatEffect, costumeFaceEffect, costumeCapeEffect, costumePetEffect, costumeBodyEffect;
        private string costumeHatPrefabName, costumeFacePrefabName, costumeCapePrefabName, costumePetPrefabName, costumeBodyPrefabName;
        private string costumeHatEffectName, costumeFaceEffectName, costumeCapeEffectName, costumePetEffectName, costumeBodyEffectName;
        private CostumeData costumeData;

        protected override void Awake()
        {
            base.Awake();

            characterPool = CharacterPoolManager.Instance;
            itemPool = ItemPoolManager.Instance;
        }

        public override void PlayEmotion(bool isPlay, float remainTime = default)
        {
            // 이모션 애니메이션 동작 중에는 무기 숨김
            if (isPlay)
            {
                // 상점 모드일 경우에는 무기 숨김 예외
                if (!IsHideCharacter())
                {
                    isPlayEmotion = true;
                    SetActiveItem(weapon, false);

                    Invoke("FinishEmotion", remainTime);
                }
            }
            else
            {
                FinishEmotion();
            }
        }

        public void Initialize(Job job, Gender gender, bool isPrivateStoreSelling, bool isShadowCharacter)
        {
            this.job = job;
            this.gender = gender;
            this.isPrivateStoreSelling = isPrivateStoreSelling;
            this.isShadowCharacter = isShadowCharacter;
            ApplyBody();
        }

        public void SetJob(Job job)
        {
            this.job = job;
            ApplyBody();
        }

        public void SetGender(Gender gender)
        {
            this.gender = gender;
            ApplyBody();
        }

        public void SetWeaponType(string weaponPrefabName, EquipmentClassType weaponType, string effectName)
        {
            // 같은 외형의 무기 && 같은 이펙트 장착 시도시 리턴
            if (string.Equals(this.weaponPrefabName, weaponPrefabName) && string.Equals(this.weaponEffectName, effectName))
                return;

            this.weaponPrefabName = weaponPrefabName;
            this.weaponType = weaponType;
            this.weaponEffectName = effectName;
            ApplyWeapon();
        }

        /// <summary>
        /// 모자 코스튬
        /// </summary>
        /// <param name="prefabName"></param>
        public void SetCostumeHat(string prefabName, CostumeData data, string effectName)
        {
            if (string.Equals(costumeHatPrefabName, prefabName) && string.Equals(costumeHatEffectName, effectName))
                return;

            costumeHatPrefabName = prefabName;
            costumeData = data;
            costumeHatEffectName = effectName;
            ApplyCostume(ref costumeHat, CostumeType.Hat, prefabName);
            ApplyEffect(ref costumeHatEffect, costumeHat, costumeHatEffectName);
        }

        /// <summary>
        /// 얼굴 코스튬
        /// </summary>
        public void SetCostumeFace(string prefabName, string effectName)
        {
            if (string.Equals(costumeFacePrefabName, prefabName) && string.Equals(costumeFaceEffectName, effectName))
                return;

            costumeFacePrefabName = prefabName;
            costumeFaceEffectName = effectName;
            ApplyCostume(ref costumeFace, CostumeType.Face, prefabName);
            ApplyEffect(ref costumeFaceEffect, costumeFace, costumeFaceEffectName);
        }

        /// <summary>
        /// 망토 코스튬
        /// </summary>
        public void SetCostumeCape(string prefabName, string effectName)
        {
            if (string.Equals(costumeCapePrefabName, prefabName) && string.Equals(costumeCapeEffectName, effectName))
                return;

            costumeCapePrefabName = prefabName;
            costumeCapeEffectName = effectName;
            ApplyBody();
            ApplyCostume(ref costumeCape, CostumeType.Garment, prefabName);
            ApplyEffect(ref costumeCapeEffect, costumeCape, costumeCapeEffectName);
        }

        /// <summary>
        /// 펫 코스튬
        /// </summary>
        public void SetCostumePet(string prefabName, string effectName)
        {
            if (string.Equals(costumePetPrefabName, prefabName) && string.Equals(costumePetEffectName, effectName))
                return;

            costumePetPrefabName = prefabName;
            costumePetEffectName = effectName;
            ApplyCostumePet(ref costumePet, CostumeType.Pet, prefabName);
            ApplyEffectPet(ref costumePetEffect, costumePet, costumePetEffectName);
        }

        /// <summary>
        /// 몸 코스튬
        /// </summary>
        public void SetCostumeBody(string prefabName, string effectName)
        {
            if (string.Equals(costumeBodyPrefabName, prefabName) && string.Equals(costumeBodyEffectName, effectName))
                return;

            costumeBodyPrefabName = prefabName;
            costumeBodyEffectName = effectName;
            ApplyBody();
            ApplyEffect(ref costumeBodyEffect, body, costumeBodyEffectName);
        }

        public void SetPrivateStoreSelling(bool isPrivateStoreSelling)
        {
            this.isPrivateStoreSelling = isPrivateStoreSelling;
            ApplyBody();
        }

        public void SetIsUI(bool isUI)
        {
            this.isUI = isUI;
        }

        public override void OnReady(UnitEntity entity)
        {
            SetLayer(entity.Layer); // 레이어 세팅
        }

        public override void OnRelease()
        {
            base.OnRelease();

            Release();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public override float GetRadius()
        {
            return body == null ? 0f : GetRadius(body.CachedTransform);
        }

        private void ApplyBody()
        {
            UnlinkWeapon(); // Weapon 링크 해제
            UnlinkCostume(); // 코스튬 링크 해제

            if (skinedMash != null)
            {
                NGUITools.Destroy(skinedMash);
            }

            if (body != null)
            {
                body.Release();
                body = null;
            }

            if (job == default || gender == default)
                return;

            if (isPrivateStoreSelling) // 상점 개설시 포링
            {
                int randomFormIndex = Random.Range(0, 4);
                body = characterPool.Spawn(Constants.Trade.GetPrivateStoreFormPrefabName(randomFormIndex));
            }
            else if (isShadowCharacter)
            {
                body = characterPool.SpawnShadow();
            }
            else
            {
                Avatar avatar = Avatar.Get(job, gender);
                body = characterPool.Spawn("Character");

                string bodyName = avatar.body;

                if (!string.IsNullOrEmpty(costumeBodyPrefabName))
                    bodyName = costumeBodyPrefabName;

                var goBody = characterPool.Spawn(bodyName);
                var goFace = characterPool.Spawn(avatar.face);
                var goHair = characterPool.Spawn(avatar.hair);

                // 기본망토 표시 && 망토 코스튬 미장착 && 몸코스튬 미장착
                if (!string.IsNullOrEmpty(avatar.cape) && string.IsNullOrEmpty(costumeCapePrefabName) && string.IsNullOrEmpty(costumeBodyPrefabName))
                {
                    var goCape = characterPool.Spawn(avatar.cape);
                    skinedMash = AvatarTools.Create(body, goBody, goFace, goHair, goCape);
                }
                else
                {
                    skinedMash = AvatarTools.Create(body, goBody, goFace, goHair);
                }
            }

            LinkBody(body.CachedGameObject);

            // 상점 개설 시 무기, 코스튬 숨기기
            if (IsHideCharacter())
            {
                SetActiveItem(weapon, false);
                SetActiveItem(costumeHat, false);
                SetActiveItem(costumeFace, false);
                SetActiveItem(costumeCape, false);
                SetActiveItem(costumePet, false);
            }
            else
            {
                SetActiveItem(weapon, true);
                SetActiveItem(costumeHat, true);
                SetActiveItem(costumeFace, true);
                SetActiveItem(costumeCape, true);
                SetActiveItem(costumePet, true);

                LinkWeapon(); // 무기 링크
                LinkCostume(); // 코스튬 링크
            }
        }

        private void ApplyWeapon()
        {
            if (weapon != null)
            {
                weapon.Release();
                weapon = null;
            }

            if (weaponEffect != null)
            {
                weaponEffect.Release();
                weaponEffect = null;
            }

            if (!string.IsNullOrEmpty(weaponPrefabName))
                weapon = itemPool.SpawnWeapon(weaponPrefabName);

            ApplyEffect(ref weaponEffect, weapon, weaponEffectName);

            if (IsHideCharacter())
            {
                SetActiveItem(weapon, false);
            }
            else
            {
                LinkWeapon(); // 무기 링크
            }
        }

        private void UnlinkWeapon()
        {
            if (weapon == null)
                return;

            weapon.CachedTransform.SetParent(null);
        }

        private void LinkWeapon()
        {
            if (weapon == null || body == null)
                return;

            int layer = body.CachedGameObject.layer;
            Transform tfChild = weapon.CachedTransform;

            // 레이어 세팅
            if (layer != weapon.CachedGameObject.layer)
            {
                weapon.CachedGameObject.layer = layer;
                tfChild.SetChildLayer(layer);
            }

            Transform parent = itemPool.GetParent(body.transform, weaponType);

            if (parent == null)
            {
                Debug.LogError("parent를 찾을 수 없습니다");
                return;
            }

            tfChild.SetParent(parent, worldPositionStays: false);
            tfChild.localPosition = Vector3.zero;
            tfChild.localRotation = Quaternion.identity;
            tfChild.localScale = Vector3.one;
        }

        private void FinishEmotion()
        {
            if (isPlayEmotion)
            {
                isPlayEmotion = false;
                SetActiveItem(weapon, true);
            }
        }

        /// <summary>
        /// 코스튬 적용 (모자, 얼굴, 망토) 
        /// 무기 코스튬은 무기 장착에서 같이 처리
        /// </summary>
        private void ApplyCostume(ref PoolObject costume, CostumeType type, string name)
        {
            if (costume != null)
            {
                costume.Release();
                costume = null;
            }

            // prefab 이름이 없을 경우
            if (string.IsNullOrEmpty(name))
                return;

            costume = itemPool.SpawnWeapon(name);

            if (IsHideCharacter())
            {
                SetActiveItem(costume, false);
            }
            else
            {
                if (type == CostumeType.Hat && costumeData != null)
                {
                    LinkCostume(costume, type, costumeData.GetOffset(job, gender)); // 코스튬 링크
                }
                else
                {
                    LinkCostume(costume, type); // 코스튬 링크
                }
            }
        }

        private void ApplyCostumePet(ref FollowTarget costume, CostumeType type, string name)
        {
            if (costume != null)
            {
                costume.Release();
                costume = null;
            }

            // prefab 이름이 없을 경우
            if (string.IsNullOrEmpty(name))
                return;

            costume = itemPool.SpawnCostumePet(name, transform.position);
            if (costume != null)
            {
                costume.SetTarget(transform);
            }

            if (IsHideCharacter())
            {
                SetActiveItem(costume, false);
            }
            else
            {
                LinkCostumePet(costume);
            }
        }

        private void UnlinkCostume()
        {
            // 모자 코스튬 링크 해제
            if (costumeHat != null)
                costumeHat.CachedTransform.SetParent(null);

            // 얼굴 코스튬 링크 해제
            if (costumeFace != null)
                costumeFace.CachedTransform.SetParent(null);

            // 망토 코스튬 링크 해제
            if (costumeCape != null)
                costumeCape.CachedTransform.SetParent(null);

            // 펫 코스튬 링크 해제
            if (costumePet != null && isUI)
                costumePet.CachedTransform.SetParent(null);
        }

        private void LinkCostume()
        {
            if (costumeData != null)
                LinkCostume(costumeHat, CostumeType.Hat, costumeData.GetOffset(job, gender)); // 모자 코스튬 링크 // 오프셋 필요                     
            LinkCostume(costumeFace, CostumeType.Face); // 얼굴 코스튬 링크
            LinkCostume(costumeCape, CostumeType.Garment); // 망토 코스튬 링크
            LinkCostumePet(costumePet);
        }

        void LinkCostumePet(PoolObject poolObject)
        {
            if (poolObject == null || body == null)
                return;

            int layer = body.CachedGameObject.layer;
            Transform tfChild = poolObject.CachedTransform;

            // 레이어 세팅
            if (layer != poolObject.CachedGameObject.layer)
            {
                poolObject.CachedGameObject.layer = layer;
                tfChild.SetChildLayer(layer);
            }

            if (!isUI)
                return;

            Transform parent = body.transform;

            if (parent == null)
            {
                Debug.LogError("parent를 찾을 수 없습니다");
                return;
            }

            tfChild.SetParent(parent, worldPositionStays: false);
            tfChild.localPosition = new Vector3(1.5f, -1, 0);
            tfChild.localRotation = Quaternion.identity;
            tfChild.localScale = Vector3.one;
        }

        void LinkCostume(PoolObject poolObject, CostumeType type, float offset = 0)
        {
            if (poolObject == null || body == null)
                return;

            int layer = body.CachedGameObject.layer;
            Transform tfChild = poolObject.CachedTransform;

            // 레이어 세팅
            if (layer != poolObject.CachedGameObject.layer)
            {
                poolObject.CachedGameObject.layer = layer;
                tfChild.SetChildLayer(layer);
            }

            Transform parent = itemPool.GetParent(body.transform, type);

            if (parent == null)
            {
                Debug.LogError("parent를 찾을 수 없습니다");
                return;
            }

            tfChild.SetParent(parent, worldPositionStays: false);
            tfChild.localPosition = new Vector3(offset, 0, 0);
            tfChild.localRotation = Quaternion.identity;
            tfChild.localScale = Vector3.one;
        }

        private void ApplyEffectPet(ref PoolObject effect, FollowTarget parent, string effectName)
        {
            if (effect != null)
            {
                effect.Release();
                effect = null;
            }

            // prefab 이름이 없을 경우 || 부모프리팹이 없는 경우
            if (isUI || string.IsNullOrEmpty(effectName) || parent == null || parent.GetEffectParent() == null)
                return;

            effect = itemPool.Spawn(effectName, parent.GetEffectParent(), false);

            int layer = parent.CachedGameObject.layer;
            Transform tfChild = effect.CachedTransform;

            // 레이어 세팅
            if (layer != effect.CachedGameObject.layer)
            {
                effect.CachedGameObject.layer = layer;
                tfChild.SetChildLayer(layer);
            }

            tfChild.localPosition = Vector3.zero;
            tfChild.localRotation = Quaternion.identity;
            tfChild.localScale = Vector3.one;
        }

        private void ApplyEffect(ref PoolObject effect, PoolObject parent, string effectName)
        {
            if (effect != null)
            {
                effect.Release();
                effect = null;
            }

            // prefab 이름이 없을 경우 || 부모프리팹이 없는 경우
            if (isUI || string.IsNullOrEmpty(effectName) || parent == null)
                return;

            effect = itemPool.Spawn(effectName, parent.CachedTransform, false);

            int layer = parent.CachedGameObject.layer;
            Transform tfChild = effect.CachedTransform;

            // 레이어 세팅
            if (layer != effect.CachedGameObject.layer)
            {
                effect.CachedGameObject.layer = layer;
                tfChild.SetChildLayer(layer);
            }

            tfChild.localPosition = Vector3.zero;
            tfChild.localRotation = Quaternion.identity;
            tfChild.localScale = Vector3.one;
        }

        public void Release()
        {
            if (skinedMash != null)
            {
                NGUITools.Destroy(skinedMash);
            }

            if (body)
            {
                body.Release();
                body = null;
            }

            if (weapon)
            {
                weapon.Release();
                weapon = null;
                weaponPrefabName = null;
            }

            if (weaponEffect)
            {
                weaponEffect.Release();
                weaponEffect = null;
                weaponEffectName = null;
            }

            if (costumeHat)
            {
                costumeHat.Release();
                costumeHat = null;
                costumeHatPrefabName = null;
            }

            if (costumeHatEffect)
            {
                costumeHatEffect.Release();
                costumeHatEffect = null;
                costumeHatEffectName = null;
            }

            if (costumeFace)
            {
                costumeFace.Release();
                costumeFace = null;
                costumeFacePrefabName = null;
            }

            if (costumeFaceEffect)
            {
                costumeFaceEffect.Release();
                costumeFaceEffect = null;
                costumeFaceEffectName = null;
            }

            if (costumeCape)
            {
                costumeCape.Release();
                costumeCape = null;
                costumeCapePrefabName = null;
            }

            if (costumeCapeEffect)
            {
                costumeCapeEffect.Release();
                costumeCapeEffect = null;
                costumeCapeEffectName = null;
            }

            if (costumePet)
            {
                costumePet.Release();
                costumePet = null;
                costumePetPrefabName = null;
            }

            if (costumePetEffect)
            {
                costumePetEffect.Release();
                costumePetEffect = null;
                costumePetEffectName = null;
            }

            if (costumeBodyEffect)
            {
                costumeBodyEffect.Release();
                costumeBodyEffect = null;
                costumeBodyEffectName = null;
            }
        }

        public override void OnSetParent()
        {
            if (costumePet != null)
            {
                costumePet.CachedTransform.SetParent(null, true);
            }
        }

        private void SetActiveItem(PoolObject poolObject, bool isActive)
        {
            if (poolObject != null)
            {
                if (isActive)
                {
                    poolObject.Show();
                }
                else
                {
                    poolObject.Hide();
                }
            }
        }

        private bool IsHideCharacter()
        {
            return isPrivateStoreSelling || isShadowCharacter;
        }
    }
}