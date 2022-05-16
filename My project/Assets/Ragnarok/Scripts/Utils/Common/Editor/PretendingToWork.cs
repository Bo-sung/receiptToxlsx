using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Ragnarok
{
    static class PretendingToWork
    {
        private static string companyCode;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnStartReload;
            AssemblyReloadEvents.afterAssemblyReload += OnFinishReload;
        }

        static void OnStartReload()
        {
            StopFinanceProgress();
        }

        static void OnFinishReload()
        {
            StartFinanceProgress();
        }

        [Shortcut("Ragnarok/Pretending To Work - Display Finance", KeyCode.F12, ShortcutModifiers.Action)]
        private static void DisplayFinance()
        {
            if (StopFinanceProgress())
            {
                companyCode = null;
                return;
            }

            FinanceWizard wizard = ScriptableWizard.DisplayWizard<FinanceWizard>(FinanceWizard.TITLE, FinanceWizard.BUTTON_TEXT);
            wizard.minSize = wizard.maxSize = new Vector2(480f, 200f);
            wizard.Focus();
            wizard.Repaint();
            wizard.Show();
        }

        private static void StartFinanceProgress()
        {
            CheckFinance().WrapNetworkErrors();
        }

        private static bool StopFinanceProgress()
        {
            if (AsyncProgressBar.Clear())
                return true;

            return false;
        }

        private static async Task CheckFinance()
        {
            FinanceJson financeJson;
            while (!string.IsNullOrEmpty(companyCode))
            {
                financeJson = await CheckFinance(companyCode);
                AsyncProgressBar.Display(financeJson.GetValueText(), 0f); // 시작

                await Task.Delay(4000);
            }
        }

        private class FinanceWizard : ScriptableWizard
        {
            public const string TITLE = "FinanceWizard";
            public const string BUTTON_TEXT = "Start";

            [SerializeField] string code;

            void Awake()
            {
                code = EditorLocalValue.CompanyCode;
            }

            void OnWizardUpdate()
            {
                isValid = !string.IsNullOrEmpty(code);
            }

            void OnWizardCreate()
            {
                StartFinance(code).WrapNetworkErrors();
            }

            private async Task StartFinance(string code)
            {
                FinanceJson financeJson = await CheckFinance(code);
                if (financeJson == null || financeJson.IsInvalid())
                {
                    EditorUtility.DisplayDialog(TITLE, StringBuilderPool.Get().Append("해당 code(").Append(code).Append(")가 존재하지 않습니다.").Release(), "확인");
                    return;
                }

                string companyName = financeJson.GetCompanyName();
                if (!EditorUtility.DisplayDialog(TITLE, StringBuilderPool.Get().Append(companyName).Append("(").Append(code).Append(")로 시작하시겠습니까?").Release(), "시작", "취소"))
                    return;

                EditorLocalValue.CompanyCode = code;
                companyCode = code;
                StartFinanceProgress();
            }
        }

        private static async Task<FinanceJson> CheckFinance(string code)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://polling.finance.naver.com/api/realtime?query=SERVICE_ITEM:" + code);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonUtility.FromJson<FinanceJson>(responseBody);
            }
        }

        [System.Serializable]
        private class FinanceJson
        {
            [System.Serializable]
            public class Result
            {
                public int pollingInterval;
                public Area[] areas;
                public long time;
            }

            [System.Serializable]
            public class Area
            {
                public string name;
                public Data[] datas;
            }

            [System.Serializable]
            public class Data
            {
                public string cd; // 코드
                public string nm; // 이름
                public int sv; // 전일종가
                public int nv;
                public int cv;
                public float cr;
                public string rf;
                public string mt;
                public string ms;
                public string tyn;
                public int pcv;
                public int ov; // 금일시가
                public int hv; // 금일변동(고가)
                public int lv; // 금일변동(저가)
                public int ul; // 상한가
                public int ll; // 하한가
                public int aq; // 거래량
                public int aa; // 거래대금
                public string nav;
                public int keps;
                public int eps;
                public float bps;
                public string cnsEps;
                public float dv; // 배당금
            }

            public string resultCode;
            public Result result;

            public bool IsInvalid()
            {
                if (result.areas.Length == 0)
                    return true;

                if (result.areas[0].datas.Length == 0)
                    return true;

                return false;
            }

            public string GetCompanyName()
            {
                if (IsInvalid())
                    return "null";

                return result.areas[0].datas[0].nm;
            }

            public string GetValueText()
            {
                if (IsInvalid())
                    return "null";

                int sv = result.areas[0].datas[0].sv;
                int nv = result.areas[0].datas[0].nv;
                int diff = nv - sv;
                float cr = result.areas[0].datas[0].cr;

                var sb = StringBuilderPool.Get();
                sb.Append(nv).Append(" (");
                if (diff < 0)
                {
                    sb.Append('▼');
                }
                else
                {
                    sb.Append('▲');
                }
                sb.Append(diff);
                sb.Append(") | ");
                if (diff < 0)
                {
                    sb.Append('-');
                }
                else
                {
                    sb.Append('+');
                }
                sb.Append(cr).Append('%');
                return sb.Release();
            }
        }
    }
}