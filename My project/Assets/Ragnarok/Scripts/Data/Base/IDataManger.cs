namespace Ragnarok
{
    /// <summary>
    /// 모든 데이터 테이블 매니저 인터페이스를 상속받아야 한다.
    /// <see cref="LanguageDataManager"/>
    /// </summary>
    public interface IDataManger
    {
        ResourceType DataType { get; }

        void ClearData();
        void LoadData(byte[] bytes);
        void Initialize();
        void VerifyData();
    }
}