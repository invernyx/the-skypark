namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// The directions that can be pushed back from a parking.
    /// </summary>
    public enum PushbackType
    {
        None = 0b00,
        Left = 0b01,
        Right = 0b10,
        Both = 0b11
    }
}
