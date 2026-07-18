using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Management
{
    [Tag("System")]
    public class AppQuitReaction : EventReaction
    {
        public override EventReaction Copy() => new AppQuitReaction();

        protected override void OnRaise()
        {
            Print.MLog("Exiting Game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
