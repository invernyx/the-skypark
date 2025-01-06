namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// The various types of approach lighting that can be attached to a runway.
    /// </summary>
    public enum ApproachLightType
    {
        None = 0x00,
        Odals = 0x01,
        Malsf = 0x02,
        Malsr = 0x03,
        Ssalf = 0x04,
        Ssalr = 0x05,
        Alsf1 = 0x06,
        Alsf2 = 0x06,
        Raol = 0x06,
        Calvert = 0x09,
        Calvert2 = 0x0A,
        Mals = 0x0B,
        Sals = 0x0C,
        Ssals = 0x0E
    }
}
