using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 모델링을 합칠때 FBX 파일의 Read/Write Enabled 체크 필수
    /// </summary>
    public class SkinnedMeshCombiner : MonoBehaviour
    {     
        [ContextMenu("CreateSkinnedMesh")]
        public void CreateSkinnedMesh()
        {            
            List<Transform> bones = new List<Transform>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<CombineInstance> combineInstances = new List<CombineInstance>();           
            List<SkinnedMeshRenderer> smRenderers = new List<SkinnedMeshRenderer>();
            List<Matrix4x4> bindPoses = new List<Matrix4x4>();
            List<Material> materials = new List<Material>();

            // 기본 오브젝트 복사
            GameObject character = transform.Find("Character").gameObject;
            GameObject charGo = Instantiate(character);                 
            character.SetActive(false);
            charGo.SetActive(true);

            // 본
            Transform[] transforms = charGo.transform.Find("Root").GetComponentsInChildren<Transform>();            
            
            // 스킨 메쉬 병합
            GetComponentsInChildren<SkinnedMeshRenderer>(true, smRenderers);  // 병합할 스킨메쉬리스트
            int boneOffset = 0;
            for (int s = 0; s < smRenderers.Count; s++)
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
                smr.transform.parent.gameObject.SetActive(false);
            }          

            GameObject skin = charGo.transform.Find("SkinnedMesh").gameObject;   
            SkinnedMeshRenderer r = skin.AddComponent<SkinnedMeshRenderer>();
            r.sharedMesh = new Mesh();
            r.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, true);
            r.sharedMaterials = materials.ToArray();
            r.bones = bones.ToArray();
            r.sharedMesh.boneWeights = boneWeights.ToArray();
            r.sharedMesh.bindposes = bindPoses.ToArray();
            r.sharedMesh.RecalculateBounds();
        }

    }
}
