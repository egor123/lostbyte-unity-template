using System.Threading.Tasks;
using UnityEngine;

namespace Lostbyte.Toolkit.Scenes
{
    public abstract class LoadingScreenBase : MonoBehaviour
    {
        public abstract bool InTransition { get; }
        public abstract Task FadeIn();
        public abstract Task FadeOut();
        public abstract void SetFadeIn(float progress);
        public abstract void SetFadeOut(float progress);
        public abstract void Skip();
    }
}
