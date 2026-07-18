// AUTO-GENERATED FILE — DO NOT EDIT
using System.Runtime.CompilerServices;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;

namespace Localization
{
    public static class GameTable
    {
        public const string Key = "game";
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetTestKeyRef(LocFloatArg arg1, LocArg arg2) => new("game", "test_key", arg1, arg2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetTestKeyString(float arg1, object arg2) => LocalizationDatabase.GetValue<System.String>("game", "test_key", arg1, arg2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String[]> GetTestArrayKeyRef() => new("game", "test_array_key");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String[] GetTestArrayKeyString() => LocalizationDatabase.GetValue<System.String[]>("game", "test_array_key");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<UnityEngine.TextAsset[]> GetFileRef() => new("game", "file");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static UnityEngine.TextAsset[] GetFileFile() => LocalizationDatabase.GetValue<UnityEngine.TextAsset[]>("game", "file");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<UnityEngine.TextAsset> GetTestRef() => new("game", "test");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static UnityEngine.TextAsset GetTestFile() => LocalizationDatabase.GetValue<UnityEngine.TextAsset>("game", "test");
    }

    public static class LocalesTable
    {
        public const string Key = "locales";
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetRuRURef() => new("locales", "ru-RU");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetRuRUString() => LocalizationDatabase.GetValue<System.String>("locales", "ru-RU");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetEnUSRef() => new("locales", "en-US");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetEnUSString() => LocalizationDatabase.GetValue<System.String>("locales", "en-US");
    }

    public static class UiTable
    {
        public const string Key = "ui";
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetStartBtnRef() => new("ui", "start_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetStartBtnString() => LocalizationDatabase.GetValue<System.String>("ui", "start_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetResumeBtnRef() => new("ui", "resume_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetResumeBtnString() => LocalizationDatabase.GetValue<System.String>("ui", "resume_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetMenuBtnRef() => new("ui", "menu_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetMenuBtnString() => LocalizationDatabase.GetValue<System.String>("ui", "menu_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetExitBtnRef() => new("ui", "exit_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetExitBtnString() => LocalizationDatabase.GetValue<System.String>("ui", "exit_btn");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetMainVolumeFieldRef() => new("ui", "main_volume_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetMainVolumeFieldString() => LocalizationDatabase.GetValue<System.String>("ui", "main_volume_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetMusicVolumeFieldRef() => new("ui", "music_volume_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetMusicVolumeFieldString() => LocalizationDatabase.GetValue<System.String>("ui", "music_volume_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetSfxVolumeFieldRef() => new("ui", "sfx_volume_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetSfxVolumeFieldString() => LocalizationDatabase.GetValue<System.String>("ui", "sfx_volume_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetMouseSensetivityFieldRef() => new("ui", "mouse_sensetivity_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetMouseSensetivityFieldString() => LocalizationDatabase.GetValue<System.String>("ui", "mouse_sensetivity_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetSaveFileFieldRef() => new("ui", "save_file_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetSaveFileFieldString() => LocalizationDatabase.GetValue<System.String>("ui", "save_file_field");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static LocalizedReference<System.String> GetLocaleFiledRef() => new("ui", "locale_filed");
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static System.String GetLocaleFiledString() => LocalizationDatabase.GetValue<System.String>("ui", "locale_filed");
    }
}