using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;

namespace Lostbyte.Toolkit.Localization
{
    [Tag("Localization")]
    [SupportedFactTypes(typeof(string))]
    public class LocaleChangeReaction : FactReaction
    {
        public override FactReaction Copy() => new LocaleChangeReaction();
        public override void OnLoad(object data) => OnValueChanged(null, Value);
        protected override void OnValueChanged(object oldValue, object newValue)
        {
            LocalizationSettings.Database.ChangeLocaleSync((string)newValue);
        }
    }
}
