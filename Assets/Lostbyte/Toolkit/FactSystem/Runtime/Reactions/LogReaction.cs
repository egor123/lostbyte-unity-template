using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.FactSystem
{
    [Tag("System")]
    public class LogReaction : FactReaction
    {
        public override FactReaction Copy() => new LogReaction() { };
        protected override void OnValueChanged(object oldValue, object newValue) => Print.Log($"{Key.name}[{Fact.name}]: {oldValue} -> {newValue}");
    }
}
