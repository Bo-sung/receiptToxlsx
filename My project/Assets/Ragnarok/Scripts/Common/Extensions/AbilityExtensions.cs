using UnityEngine;

namespace Ragnarok
{
    public static class AbilityExtensions
    {
        public static void SetStatValue(this UILabelValue label, int totalStatus, int originStatus, int delta)
        {
            int buffStatus = totalStatus - (originStatus + delta);

            var sb = StringBuilderPool.Get();
            sb.Append("[c][4C4A4D]");

            if (delta > 0)
                sb.Append("[809CE5]");

            sb.Append(totalStatus);

            if (delta > 0)
                sb.Append("[-]");

            if (delta > 0 || buffStatus != 0)
            {
                sb.Append(" (");
                sb.Append(originStatus);

                if (delta > 0)
                    sb.AppendFormat("+[809CE5]{0}[-]", delta);

                if (buffStatus != 0)
                {
                    sb.Append(buffStatus > 0 ? '+' : '-');
                    sb.AppendFormat("[908F90]{0}[-]", Mathf.Abs(buffStatus));
                }

                sb.Append(")");
            }

            sb.Append("[-][/c]");
            label.Value = sb.ToString();
            sb.Release();
        }

        public static void SetPercentStatValue(this UILabelValue label, int totalStatus, int originStatus, int delta)
        {
            int buffStatus = totalStatus - (originStatus + delta);

            var sb = StringBuilderPool.Get();
            sb.Append("[c][4C4A4D]");

            if (delta > 0)
                sb.Append("[809CE5]");

            sb.AppendFormat("{0:0.00}", MathUtils.ToPercentValue(totalStatus));

            if (delta > 0)
                sb.Append("[-]");

            if (delta > 0 || buffStatus != 0)
            {
                sb.Append(" (");
                sb.Append(MathUtils.ToPercentValue(originStatus).ToString("F1"));

                if (delta > 0)
                    sb.AppendFormat("+[809CE5]{0:0.00}[-]", MathUtils.ToPercentValue(delta));

                if (buffStatus != 0)
                {
                    sb.Append(buffStatus > 0 ? '+' : '-');
                    sb.AppendFormat("[908F90]{0:0.00}[-]", MathUtils.ToPercentValue(buffStatus));
                }

                sb.Append(")");
            }

            sb.Append("[-][/c]");
            label.Value = sb.ToString();
            sb.Release();
        }
    }
}