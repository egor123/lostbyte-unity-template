using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    public abstract class LoadingScreenBase : MonoBehaviour
    {
        public bool InTransition { get; protected set; }
        public abstract void FadeIn();
        public abstract void FadeOut();
        public abstract void Skip();
    }
}
