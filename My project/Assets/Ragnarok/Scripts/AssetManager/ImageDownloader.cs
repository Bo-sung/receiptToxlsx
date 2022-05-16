using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ImageDownloader
    {
        private readonly Dictionary<string, Texture> cachedTextureDic;

        public ImageDownloader()
        {
            cachedTextureDic = new Dictionary<string, Texture>(System.StringComparer.Ordinal);
        }

        public void Clear()
        {
            if (cachedTextureDic.Count > 0)
            {
                foreach (var item in cachedTextureDic.Values)
                {
                    Object.Destroy(item);
                }

                cachedTextureDic.Clear();
            }
        }

        public Texture Get(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            return cachedTextureDic.ContainsKey(url) ? cachedTextureDic[url] : null;
        }

        public void Cache(string url, Texture texture)
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (texture == null)
                return;

            // 기존 url 이 존재할 경우
            if (cachedTextureDic.ContainsKey(url))
            {
                Object.Destroy(cachedTextureDic[url]); // 기존 Texture 제거
                cachedTextureDic.Remove(url);
            }

            cachedTextureDic.Add(url, texture);
        }
    }
}