using System;
using MultiplayerMod.Platform.Direct.Network.Messaging;

namespace MultiplayerMod.Platform.Direct.Network;

public static class Configuration
{
    public const int SERVER_PORT = 27015;

    private const int defaultBufferSize = 10485760; // 10 MiB

    //public static SteamNetworkingConfigValue_t SendBufferSize(int size = defaultBufferSize) => new()
    //{
    //    m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize,
    //    m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
    //    m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = size }
    //};

    public const int MaxMessageSize = 524288; // 512 KiB
    public static readonly int MaxFragmentDataSize = GetFragmentDataSize();

    private static int GetFragmentDataSize()
    {
        using var serialized = NetworkSerializer.Serialize(new NetworkMessageFragment(0, Array.Empty<byte>()));
        return MaxMessageSize - (int) serialized.Size;
    }

}
