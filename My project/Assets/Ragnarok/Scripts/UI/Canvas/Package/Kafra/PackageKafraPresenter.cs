namespace Ragnarok
{
    public class PackageKafraPresenter : PackageCatCoinPresenter
    {
        /******************** Models ********************/

        /******************** Repositories ********************/

        /******************** Event ********************/

        int kafraBuffRate;

        public PackageKafraPresenter()
        {
            kafraBuffRate = BasisType.KAFRA_POINT_RATE.GetInt();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public string GetKafraBuffRateText()
        {
            return $"+{MathUtils.ToPermyriadText(kafraBuffRate)}";
        }
    }
}