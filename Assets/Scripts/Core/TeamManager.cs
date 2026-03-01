using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace SpaceFighter
{
    public class TeamManager : NetworkBehaviour
    {
        public readonly SyncList<int> BlueTeamIds = new();
        public readonly SyncList<int> RedTeamIds = new();

        private readonly SyncDictionary<int, ShipClass> _classSelections = new();

        [Server]
        public void AssignTeam(NetworkConnection conn, Team team)
        {
            int connId = conn.ClientId;

            BlueTeamIds.Remove(connId);
            RedTeamIds.Remove(connId);

            if (team == Team.Blue)
                BlueTeamIds.Add(connId);
            else if (team == Team.Red)
                RedTeamIds.Add(connId);
        }

        [Server]
        public void SetClassSelection(NetworkConnection conn, ShipClass shipClass)
        {
            int connId = conn.ClientId;
            Team team = GetTeam(connId);

            if (team == Team.None)
                return;

            if (!ValidateClassSelection(team, shipClass))
                return;

            _classSelections[connId] = shipClass;
        }

        public Team GetTeam(int connId)
        {
            if (BlueTeamIds.Contains(connId))
                return Team.Blue;
            if (RedTeamIds.Contains(connId))
                return Team.Red;
            return Team.None;
        }

        public int GetTeamCount(Team team)
        {
            if (team == Team.Blue)
                return BlueTeamIds.Count;
            if (team == Team.Red)
                return RedTeamIds.Count;
            return 0;
        }

        public int GetClassCount(Team team, ShipClass shipClass)
        {
            int count = 0;
            SyncList<int> ids = team == Team.Blue ? BlueTeamIds : RedTeamIds;

            foreach (int connId in ids)
            {
                if (_classSelections.TryGetValue(connId, out ShipClass selected) && selected == shipClass)
                    count++;
            }

            return count;
        }

        public bool ValidateClassSelection(Team team, ShipClass shipClass)
        {
            if (shipClass == ShipClass.Vanguard && GetClassCount(team, ShipClass.Vanguard) >= GameConstants.MaxVanguards)
                return false;
            if (shipClass == ShipClass.Disruptor && GetClassCount(team, ShipClass.Disruptor) >= GameConstants.MaxDisruptors)
                return false;
            return true;
        }

        public ShipClass GetClassSelection(int connId)
        {
            if (_classSelections.TryGetValue(connId, out ShipClass shipClass))
                return shipClass;
            return ShipClass.Striker;
        }
    }
}
