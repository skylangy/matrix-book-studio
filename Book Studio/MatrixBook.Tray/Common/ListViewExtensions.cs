using System.Reflection;

namespace MatrixBook.Tray.Common;
public static class ListViewExtensions
{
    public static void SetDoubleBuffered(this ListView listView, bool doubleBuffered = true)
    {
        listView
            .GetType()
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?
            .SetValue(listView, doubleBuffered, null);
    }
}
