namespace SpaceFighter
{
    public enum ShipClass { Vanguard, Striker, Disruptor, Flanker }
    public enum Team { None, Blue, Red }
    public enum GameState { WaitingForPlayers, Lobby, RoundActive, RoundEnd, Overtime, MatchEnd }
    public enum AbilitySlot { Ability1, Ability2 }
    public enum RoundEndReason { Elimination, CoreCapture, Disconnect }
}
