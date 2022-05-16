using UnityEngine;

namespace Ragnarok
{
    public static class AvatarTools
    {
        /// <summary>
        /// 캐릭터 아바타 생성
        /// 모델링을 합칠때 FBX 파일의 Read/Write Enabled 체크 필수
        /// </summary>
        /// <param name="character">캐릭터 본</param>
        /// <param name="skinnedMeshRenderer">캐릭터 파츠(머리,얼굴,몸)</param>      
        /// <returns></returns>
        public static GameObject Create(PoolObject character, params PoolObject[] skinnedMeshRenderer)
        {
            Buffer<Transform> bones = new Buffer<Transform>();
            Buffer<BoneWeight> boneWeights = new Buffer<BoneWeight>();
            Buffer<CombineInstance> combineInstances = new Buffer<CombineInstance>();
            Buffer<SkinnedMeshRenderer> smRenderers = new Buffer<SkinnedMeshRenderer>();
            Buffer<Matrix4x4> bindPoses = new Buffer<Matrix4x4>();
            Buffer<Material> materials = new Buffer<Material>();

            // 본                   
            Transform[] transforms = character.GetComponentsInChildren<Transform>();

            // 병합할 스킨메쉬리스트
            for (int i = 0; i < skinnedMeshRenderer.Length; i++)
            {
                smRenderers.AddRange(skinnedMeshRenderer[i].GetComponentsInChildren<SkinnedMeshRenderer>());
                skinnedMeshRenderer[i].Release();
            }

            int boneOffset = 0;
            for (int s = 0; s < smRenderers.size; s++)
            {
                SkinnedMeshRenderer smr = smRenderers[s];

                BoneWeight[] meshBoneweight = smr.sharedMesh.boneWeights;

                for (int i = 0; i < meshBoneweight.Length; ++i)
                {
                    BoneWeight bWeight = meshBoneweight[i];

                    bWeight.boneIndex0 += boneOffset;
                    bWeight.boneIndex1 += boneOffset;
                    bWeight.boneIndex2 += boneOffset;
                    bWeight.boneIndex3 += boneOffset;                   

                    boneWeights.Add(bWeight);
                }

                boneOffset += smr.bones.Length;
                Transform[] meshBones = smr.bones;

                for (int i = 0; i < meshBones.Length; ++i)
                {
                    foreach (var trans in transforms)
                    {
                        if (trans.name != meshBones[i].name)
                            continue;
                        bones.Add(trans);
                    }
                    bindPoses.Add(smr.sharedMesh.bindposes[i] * smr.transform.worldToLocalMatrix);
                }

                foreach (var material in smr.sharedMaterials)
                {
                    materials.Add(material);
                }

                CombineInstance ci = new CombineInstance();
                ci.mesh = smr.sharedMesh;
                ci.transform = smr.transform.localToWorldMatrix;
                combineInstances.Add(ci);
            }

            SkinnedMeshRenderer r = new GameObject("SkinnedMesh").AddComponent<SkinnedMeshRenderer>();
            r.gameObject.layer = character.CachedGameObject.layer;
            r.transform.SetParent(character.transform);
            r.sharedMesh = new Mesh();
            r.sharedMesh.CombineMeshes(combineInstances.GetBuffer(isAutoRelease: true), mergeSubMeshes: false);
            r.sharedMaterials = materials.GetBuffer(isAutoRelease: true);
            r.bones = bones.GetBuffer(isAutoRelease: true);
            r.sharedMesh.boneWeights = boneWeights.GetBuffer(isAutoRelease: true);
            r.sharedMesh.bindposes = bindPoses.GetBuffer(isAutoRelease: true);
            r.sharedMesh.RecalculateBounds();

            smRenderers.Clear();
            materials.Clear();

            return r.gameObject;
        }
    }
}
