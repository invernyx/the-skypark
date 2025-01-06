namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// The style of VASI attached to a runway.
    /// </summary>
    public enum VasiType
    {
        Vasi21 = 0x01,
        Vasi31 = 0x02,
        Vasi22 = 0x03,
        Vasi32 = 0x04,
        Vasi23 = 0x05,
        Vasi33 = 0x06,
        Papi2 = 0x07,
        Papi4 = 0x08,
        TriColor = 0x09,
        PVasi = 0x0A,
        TVasi = 0x0B,
        Ball = 0x0C,
        Apap = 0x0D
    }
}
