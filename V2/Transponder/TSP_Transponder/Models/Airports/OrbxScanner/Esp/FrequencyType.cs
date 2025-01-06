namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// Which radio communications type this frequency represents.
    /// </summary>
    public enum FrequencyType
    {
        Atis = 0x01,
        Multicom = 0x02,
        Unicom = 0x03,
        Ctaf = 0x04,
        Ground = 0x05,
        Tower = 0x06,
        Clearance = 0x07,
        Approach = 0x08,
        Departure = 0x09,
        Center = 0x0A,
        Fss = 0x0B,
        Awos = 0x0C,
        Asos = 0x0D,
        ClearancePreTaxi = 0x0E,
        RemoteClearanceDelivery = 0x0F
    }
}
