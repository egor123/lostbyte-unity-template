// AUTO-GENERATED FILE — DO NOT EDIT
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

namespace Localization
{
    public static class GameTable
    {
        public static LocalizedReference<System.String> GetTestKeyString(LocFloatArg arg1, LocArg arg2) => new("game", "test_key", arg1, arg2);
        public static LocalizedReference<System.String[]> GetTestArrayKeyString() => new("game", "test_array_key");
    }

    public static class LocalesTable
    {
        public static LocalizedReference<System.String> GetRuRUString() => new("locales", "ru-RU");
        public static LocalizedReference<System.String> GetEnUSString() => new("locales", "en-US");
    }

    public static class UiTable
    {
        public static LocalizedReference<System.String> GetStartBtnString() => new("ui", "start_btn");
        public static LocalizedReference<System.String> GetResumeBtnString() => new("ui", "resume_btn");
        public static LocalizedReference<System.String> GetMenuBtnString() => new("ui", "menu_btn");
        public static LocalizedReference<System.String> GetExitBtnString() => new("ui", "exit_btn");
        public static LocalizedReference<System.String> GetMainVolumeFieldString() => new("ui", "main_volume_field");
        public static LocalizedReference<System.String> GetMusicVolumeFieldString() => new("ui", "music_volume_field");
        public static LocalizedReference<System.String> GetSfxVolumeFieldString() => new("ui", "sfx_volume_field");
        public static LocalizedReference<System.String> GetMouseSensetivityFieldString() => new("ui", "mouse_sensetivity_field");
        public static LocalizedReference<System.String> GetSaveFileFieldString() => new("ui", "save_file_field");
        public static LocalizedReference<System.String> GetLocaleFiledString() => new("ui", "locale_filed");
    }
}