// AUTO-GENERATED FILE — DO NOT EDIT
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

namespace Localization
{
    public static class GameTable
    {
        public static LocalizedString GetTestKeyString(LocArg<float> arg1, LocArg<object> arg2) => new("game", "test_key", arg1.Value, arg2.Value);
    }

    public static class LocalesTable
    {
        public static LocalizedString GetRuRUString() => new("locales", "RuRU");
        public static LocalizedString GetEnUSString() => new("locales", "EnUS");
    }

    public static class UiTable
    {
        public static LocalizedString GetStartBtnString() => new("ui", "start_btn");
        public static LocalizedString GetResumeBtnString() => new("ui", "resume_btn");
        public static LocalizedString GetMenuBtnString() => new("ui", "menu_btn");
        public static LocalizedString GetExitBtnString() => new("ui", "exit_btn");
        public static LocalizedString GetMainVolumeFieldString() => new("ui", "main_volume_field");
        public static LocalizedString GetMusicVolumeFieldString() => new("ui", "music_volume_field");
        public static LocalizedString GetSfxVolumeFieldString() => new("ui", "sfx_volume_field");
        public static LocalizedString GetMouseSensetivityFieldString() => new("ui", "mouse_sensetivity_field");
        public static LocalizedString GetSaveFileFieldString() => new("ui", "save_file_field");
        public static LocalizedString GetLocaleFiledString() => new("ui", "locale_filed");
    }
}