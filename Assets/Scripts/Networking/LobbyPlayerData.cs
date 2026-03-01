using FishNet.Serializing;

namespace SpaceFighter
{
    public struct LobbyPlayerData
    {
        public int ConnectionId;
        public Team Team;
        public ShipClass ShipClass;
        public bool IsReady;
        public string PlayerName;
    }

    public static class LobbyPlayerDataSerializer
    {
        public static void WriteLobbyPlayerData(this Writer writer, LobbyPlayerData value)
        {
            writer.WriteInt32(value.ConnectionId);
            writer.WriteByte((byte)value.Team);
            writer.WriteByte((byte)value.ShipClass);
            writer.WriteBoolean(value.IsReady);
            writer.WriteString(value.PlayerName);
        }

        public static LobbyPlayerData ReadLobbyPlayerData(this Reader reader)
        {
            return new LobbyPlayerData
            {
                ConnectionId = reader.ReadInt32(),
                Team = (Team)reader.ReadByte(),
                ShipClass = (ShipClass)reader.ReadByte(),
                IsReady = reader.ReadBoolean(),
                PlayerName = reader.ReadString()
            };
        }
    }
}
