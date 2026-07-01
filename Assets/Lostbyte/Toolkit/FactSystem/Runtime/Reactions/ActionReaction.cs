using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.FactSystem
{
    [Tag("Events")]
    public class ActionReaction : FactReaction
    {
        public override FactReaction Copy() => new LogReaction() { };
        protected override void OnValueChanged(object newValue) => Print.Log($"{Key.name}[{Fact.name}] = {newValue}");
    }
}
