using CodeStage.AntiCheat.ObscuredTypes;
using Sfs2X.Entities.Data;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace Ragnarok
{
    public class ResourceVersion
    {
        public readonly ObscuredString typeName;
        public readonly ObscuredShort version;
        public readonly ObscuredInt size;
        private readonly ObscuredString hash;
        public readonly ObscuredString path;
        public readonly ObscuredBool hasType;

        public ResourceVersion(ISFSObject sfsObject)
        {
            hasType = Enum.IsDefined(typeof(ResourceType), sfsObject.GetByte("1"));
            typeName = ((ResourceType)sfsObject.GetByte("1")).ToString();
            version = sfsObject.GetShort("2");
            size = sfsObject.GetInt("4");
            hash = sfsObject.GetUtfString("5");
            path = $"{Application.persistentDataPath}/Data/{typeName}";           
        }

        /// <summary>
        /// 데이터파일 해쉬 검사
        /// </summary>
        /// <param name="md5Hash"></param>
        /// <returns></returns>
        public bool VerifyMd5Hash(MD5 md5Hash)
        {
            if (!File.Exists(path))
                return false;

            using (Stream stream = File.OpenRead(path))
            {
                string hashOfPath = BitConverter.ToString(md5Hash.ComputeHash(stream)).Replace("-", string.Empty);

                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
               
                if (0 == comparer.Compare(hashOfPath, hash))
                {
                    //Debug.Log($"typeName={typeName}, hashOfPath={hashOfPath}, true");
                    return true;
                }
                else
                {
                    //Debug.Log($"typeName={typeName}, hashOfPath={hashOfPath}, false");
                    return false;
                }
            }
        }

        public string GetUrl(string ip, int port)
        {
            return $"http://{ip}:{port}/resources/{typeName}_{version}";
        }

        public string GetUrl(string baseUrl)
        {
            return $"{baseUrl}/resources/{typeName}_{version}";
        }
    } 
}
