using System;
using Lostbyte.Toolkit.Common;

namespace Lostbyte.Toolkit.Localization
{
    public static class SubscriptionGroupLocalizationExtentions
    {
        public static void SubscribeLocalizationChange(this SubscriptionGroup goup, Action<Enum> action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke(LocalizationSettings.Locale);
            goup.Subscribe(LocalizationSettings.AddListenerOnLocaleChange, LocalizationSettings.RemoveListenerOnLocaleChange, action);
        }
        public static void SubscribeLocalizationChange(this SubscriptionGroup goup, Action action, bool invokeImidiate = false)
        {
            if (invokeImidiate) action.Invoke();
            goup.Subscribe(LocalizationSettings.AddListenerOnLocaleChange, LocalizationSettings.RemoveListenerOnLocaleChange, (Enum _) => action.Invoke());
        }
    }
}
