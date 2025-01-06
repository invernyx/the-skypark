namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// Different surfaces that can be used for runways, taxiways, and aprons.
    /// </summary>
    public enum Surface
    {
        Concrete = 0x0000,
        Grass = 0x0001,
        Water = 0x0002,
        Asphalt = 0x0004,
        Clay = 0x0007,
        Snow = 0x0008,
        Ice = 0x0009,
        Dirt = 0x000C,
        Coral = 0x000D,
        Gravel = 0x000E,
        OilTreated = 0x000F,
        SteelMats = 0x0010,
        Bituminous = 0x0011,
        Brick = 0x0012,
        Macadam = 0x0013,
        Planks = 0x0014,
        Sand = 0x0015,
        Shale = 0x0016,
        Tarmac = 0x0017,
        Unknown = 0x00FE
    }
}
