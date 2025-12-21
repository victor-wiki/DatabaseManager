namespace DatabaseManager.Core.Model
{
    public class DataCompareOption
    {
        public DataCompareDisplayMode DisplayMode { get; set; } = DataCompareDisplayMode.None;
    }

    public enum DataCompareDisplayMode
    {
        None = 0,
        Different = 2,
        OnlyInSource = 4,
        OnlyInTarget = 8,
        Indentical = 16
    }
}
