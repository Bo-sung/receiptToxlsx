namespace SheetViewer
{ 
    public interface IView
    {

    }
    public interface ISheetView : IView
    {
        void URLMake();
        void Find();
        void AddContents();
        void Print();
    }
    public interface IViewPresenter
    {
        void AddEvent();
        void RemoveEvent();
    }
}
