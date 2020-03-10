using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Kernel
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum BlueMagic : byte
    {
        LaserEye,
        UltraWaves,
        Electrocute,
        LvDeath,
        Degenerator,
        AquaBreath,
        MicroMissiles,
        Acid,
        GatlingGun,
        FireBreath,
        BadBreath,
        WhiteWind,
        HomingLaser,
        MightyGuard,
        RayBomb,
        ShockwavePulsar,

        None = 0xFF,
    }
}