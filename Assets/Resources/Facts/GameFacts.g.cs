// AUTO-GENERATED FILE — DO NOT EDIT
using System;
using Lostbyte.Toolkit.FactSystem;
namespace GameFacts
{
    //------------- Key Refs -------------
    public static class Keys
    {
        private static KeyContainer _Settings = null;
        public static KeyContainer Settings = _Settings != null ? _Settings : _Settings = FactDatabase.Instance.GetKey("settings");
        private static KeyContainer _Env = null;
        public static KeyContainer Env = _Env != null ? _Env : _Env = FactDatabase.Instance.GetKey("env");
        private static KeyContainer _Game = null;
        public static KeyContainer Game = _Game != null ? _Game : _Game = FactDatabase.Instance.GetKey("game");
    }
    //------------- Fact Refs -------------
    public static class Facts
    {
        private static FactDefinition<System.Enum> _GameState = null;
        public static FactDefinition<System.Enum> GameState = _GameState != null ? _GameState : _GameState = (FactDefinition<System.Enum>) FactDatabase.Instance.GetFact("game_state");
        private static FactDefinition<System.Single> _MainVolume = null;
        public static FactDefinition<System.Single> MainVolume = _MainVolume != null ? _MainVolume : _MainVolume = (FactDefinition<System.Single>) FactDatabase.Instance.GetFact("main_volume");
        private static FactDefinition<System.Single> _MusicVolume = null;
        public static FactDefinition<System.Single> MusicVolume = _MusicVolume != null ? _MusicVolume : _MusicVolume = (FactDefinition<System.Single>) FactDatabase.Instance.GetFact("music_voulme");
        private static FactDefinition<System.Single> _SfxVolume = null;
        public static FactDefinition<System.Single> SfxVolume = _SfxVolume != null ? _SfxVolume : _SfxVolume = (FactDefinition<System.Single>) FactDatabase.Instance.GetFact("sfx_volume");
    }
    //------------- Events -------------
    public static class Events
    {
        private static EventDefinition _OnGameExit = null;
        public static EventDefinition OnGameExit = _OnGameExit != null ? _OnGameExit : _OnGameExit = (EventDefinition) FactDatabase.Instance.GetEvent("on_game_exit");
    }
    //------------- Enums -------------
    public static class Enums
    {
        public enum GameState { Menu, Game }
    }
}
