using System.Data;

namespace Ragnarok
{
    public class ProloguePresenter : ViewPresenter
    {
        DataTable prologueTable1, prologueTable2, prologueTable3;
        DataColumn idxColumn, talkerColumn, textColumn;
        DataRow row;

        public enum TalkerType
        {
            SelectPopup = -2,
            GainPopup = -1,

            OnlyText,
            AloneUser,
            AloneNPC,
            User,
            NPC
        }

        public enum ProgressType
        {
            None,
            Flow1_table, // 프롤로그 시작
            Flow2_blink,
            Flow3_table,
            Flow4_table,
            Flow5_effect
        }

        public ProgressType CurrentProgressType = ProgressType.None;

        public ProloguePresenter()
        {
            InitPrologueTable();
        }

        void InitPrologueTable()
        {
            // 프롤로그 데이터1
            talkerColumn = new DataColumn("talker", typeof(int));
            textColumn = new DataColumn("text", typeof(string));

            prologueTable1 = new DataTable();
            prologueTable1.Columns.Add(talkerColumn);
            prologueTable1.Columns.Add(textColumn);

            row = prologueTable1.NewRow();
            row[talkerColumn] = (int)TalkerType.AloneUser;
            row[textColumn] = LocalizeKey._905.ToText(); // 으음..
            prologueTable1.Rows.Add(row);


            // 프롤로그 데이터2
            talkerColumn = new DataColumn("talker", typeof(int));
            textColumn = new DataColumn("text", typeof(string));

            prologueTable2 = new DataTable();
            prologueTable2.Columns.Add(talkerColumn);
            prologueTable2.Columns.Add(textColumn);

            row = prologueTable2.NewRow();
            row[talkerColumn] = (int)TalkerType.AloneUser;
            row[textColumn] = LocalizeKey._906.ToText(); // 내가 왜 여기 있는 거지? 여긴 대체...?
            prologueTable2.Rows.Add(row);


            // 프롤로그 데이터3
            talkerColumn = new DataColumn("talker", typeof(int));
            textColumn = new DataColumn("text", typeof(string));

            prologueTable3 = new DataTable();
            prologueTable3.Columns.Add(talkerColumn);
            prologueTable3.Columns.Add(textColumn);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._907.ToText(); // 남아있는 모험가가 있었나?
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.User;
            row[textColumn] = LocalizeKey._908.ToText(); // 음... 제가 왜 여기 있는지 기억이 나질 않네요.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._909.ToText(); // 기억이 나지 않는다고...?\n잠깐. 분명 어디선가 본 적이 있는 얼굴인데.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._910.ToText(); // 아아... 알겠군. [62AEE4][C]희망의 영웅[/c][-]이 자네였나.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.SelectPopup;
            row[textColumn] = "";
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._911.ToText(); // 내 기억은 거짓말을 하지 않네.\n하필이면 지금 만나게 될 줄이야.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._912.ToText();// 갑작스럽겠지만, 꼭 받아줬으면 하는게 있네.\n기억을 잃은 것 같으니 이게 도움이 될거야.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.OnlyText;
            row[textColumn] = LocalizeKey._913.ToText(); // 의문의 남자에게 [62AEE4][C]정체 모를 물건[/c][-]을 받았다.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.GainPopup;
            row[textColumn] = "";
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._914.ToText(); // 혼란스럽겠지만, 이 물체가 자네가 해야 할 일을 알려줄 거야.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._915.ToText(); // 이 물체를 따라 자네가 잃어버린 걸 찾게.\n[62AEE4][C]기억[/c][-]도, [62AEE4][C]힘[/c][-]도 다시 찾을 수 있을거야.
            prologueTable3.Rows.Add(row);

            row = prologueTable3.NewRow();
            row[talkerColumn] = (int)TalkerType.NPC;
            row[textColumn] = LocalizeKey._916.ToText(); // 곧 다시 자네를 찾아가겠네. 지금은 해야할 일이 있으니 여기까지겠군. [62AEE4][C]빛이 제 길을 찾아가길 빌겠네.[/c][-]
            prologueTable3.Rows.Add(row);
        }

        public DataTable GetPrologueTable1()
        {
            return prologueTable1;
        }

        public DataTable GetPrologueTable2()
        {
            return prologueTable2;
        }

        public DataTable GetPrologueTable3()
        {
            return prologueTable3;
        }

        public string GetTalkerColumnName()
        {
            return talkerColumn.ColumnName;
        }

        public string GetTextColumnName()
        {
            return textColumn.ColumnName;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }
    }
}