using MEC;
using MsgPack;
using MsgPack.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Ragnarok
{
    public class GoogleSheet
    {
        private static readonly string KEY = "AIzaSyDHPwhJBbWpZ7EM6gx2X_0PKWEw8JN_cOU";

        private static readonly string SHEED_ID = "1QBYEYGP2_CwSdUTzA2mKHvWgE3HZGqfyjg9IoBhZcVA";
        private static readonly string URL = $"https://sheets.googleapis.com/v4/spreadsheets/{SHEED_ID}/values/Localize?key={KEY}";
        private static readonly string FILE_PATH = $"{Application.dataPath}/Ragnarok/Resources/LanguageData.bytes";

        private static readonly string EDITOR_SHEED_ID = "1Tt2u3ms3ST-ZJzbe8KEdORzgl4-nnz5DOU9r0XrIteY";
        private static readonly string EDITOR_URL = $"https://sheets.googleapis.com/v4/spreadsheets/{EDITOR_SHEED_ID}/values/LocalizeComplete?key={KEY}";
        private static readonly string EDITOR_FILE_PATH = $"{Application.dataPath}/Editor Default Resources/LanguageData.bytes";

        private static readonly string LOCALIZEKEY_PATH = $"{Application.dataPath}/Ragnarok/Scripts/Config/LocalizeKey.cs";

        /// <summary>
        /// 에디터용 언어 데이터 사용 여부
        /// </summary>
        public static bool IsUseEditorLocalize
        {
            get { return EditorPrefs.GetBool(nameof(IsUseEditorLocalize), defaultValue: false); }
            set { EditorPrefs.SetBool(nameof(IsUseEditorLocalize), value); }
        }

        [MenuItem("라그나로크/로컬테이블/에디터 언어데이터 사용")]
        private static void ToggleEditorLocalize()
        {
            IsUseEditorLocalize = !IsUseEditorLocalize;
        }

        [MenuItem("라그나로크/로컬테이블/에디터 언어데이터 사용", validate = true)]
        private static bool ToggleEditorLocalizeValidate()
        {
            Menu.SetChecked("라그나로크/로컬테이블/에디터 언어데이터 사용", IsUseEditorLocalize);
            return true;
        }

        [MenuItem("라그나로크/로컬테이블/바이너리저장")]
        private static void UpdateFromSpreadsheet()
        {
            Timing.RunCoroutine(_UpdateFromSpreadsheet(URL, FILE_PATH, maxId: 999), Segment.EditorUpdate);
        }

        [MenuItem("라그나로크/로컬테이블/에디터 바이너리저장")]
        private static void UpdateFromSpreadsheetEditor()
        {
            Timing.RunCoroutine(_UpdateFromSpreadsheet(EDITOR_URL, EDITOR_FILE_PATH, maxId: 99999), Segment.EditorUpdate);
        }

        [MenuItem("라그나로크/로컬테이블/LocalizeKey.cs 동기화")]
        private static void UpdateLocalizeKeyFromSpreadSheet()
        {
            Timing.RunCoroutine(_YieldUpdateLocalizeKeyFromSpreadsheet(EDITOR_URL), Segment.EditorUpdate);
        }

        private static IEnumerator<float> _UpdateFromSpreadsheet(string url, string path, int maxId)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log("로컬 언어테이블 다운로드 중");
                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    Debug.Log($"Progress : {asyncOp.progress}");
                    yield return Timing.WaitForOneFrame;
                }
                Debug.Log("로컬 언어테이블 다운로드 완료");

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    var json = Json.Deserialize(request.downloadHandler.text);
                    var rows = (List<object>)((Dictionary<string, object>)json)["values"];
                    var cells = rows.Cast<List<object>>()
                        .Select(_ => _.Cast<string>().ToArray())
                        .ToArray();

                    using (var stream = new MemoryStream())
                    {
                        var packer = Packer.Create(stream);
                        ReadData(cells, packer, maxId);
                        byte[] bytes = stream.ToArray();

                        using (var fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    AssetDatabase.Refresh();
                    Debug.Log("로컬 언어테이블 생성");
                }
            }
        }

        private static IEnumerator<float> _YieldUpdateLocalizeKeyFromSpreadsheet(string url)
        {
            string csText = File.ReadAllText(LOCALIZEKEY_PATH);
            Regex regex = new Regex(@"(^.*\#region\s*LocalizeData.*?\r\n).*(\r\n\s*\#endregion.*$)", options: RegexOptions.Singleline | RegexOptions.Compiled);
            var mc = regex.Match(csText);
            if (!mc.Success)
            {
                Debug.LogError("RegEx Match Failed.");
                yield break;
            }

            var sb = StringBuilderPool.Get();
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log("LocalizeKey 다운로드 중");
                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    Debug.Log($"Progress : {asyncOp.progress}");
                    yield return Timing.WaitForOneFrame;
                }
                Debug.Log("LocalizeKey 다운로드 완료");

                if (request.isNetworkError || request.isHttpError)
                    yield break;

                var json = Json.Deserialize(request.downloadHandler.text);
                var rows = (List<object>)((Dictionary<string, object>)json)["values"];
                var cells = rows.Cast<List<object>>()
                    .Select(_ => _.Cast<string>().ToArray())
                    .ToArray();

                ReadDataLocalizeKey(cells, sb);
            }

            File.WriteAllText(LOCALIZEKEY_PATH, $"{mc.Groups[1].Value}\r\n{sb.Release()} {mc.Groups[2].Value}", Encoding.UTF8);
        }

        private static void ReadData(string[][] cells, Packer packer, int maxId)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (i == 0)
                    continue;

                string[] arrText = cells[i];
                if (!int.TryParse(arrText[0], out int result))
                {
                    Debug.Log($"string => int 실패 Index={i}, {arrText[0]}");
                    continue;
                }

                if (result > maxId)
                    continue;

                packer.Pack(new LanguageData.EditorTuple(result, arrText));
            }
        }

        private static void ReadDataLocalizeKey(string[][] cells, StringBuilder sb)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                int length = cells[i].Length;

                if (i == 0 || length < 1 || string.IsNullOrEmpty(cells[i][0]))
                    continue;

                if (!int.TryParse(cells[i][0], out int result))
                {
                    Debug.Log($"string => int 실패 Index={i}, {cells[i][0]}");
                    continue;
                }

                if (result > 99999)
                    continue;

                int id = result;
                string korean = length > 1 ? cells[i][1] : string.Empty;

                if (string.IsNullOrWhiteSpace(korean))
                    continue;

                // public const int _1 = 1; // 확인
                sb.Append($"        public const int _{id} = {id}; // {korean}\r\n");
            }
        }
    }
}