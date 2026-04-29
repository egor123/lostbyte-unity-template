using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.Common
{
    [System.Flags]
    public enum PlatformConstraints
    {
        None = 0,
        Editor = 1 << 0,
        Web = 1 << 1,
        Mobile = 1 << 2,
        Desktop = 1 << 3,
        Console = 1 << 4,
        All = ~0
    }
    public class DisableOnPlatform : MonoBehaviour
    {
        [SerializeField] private PlatformConstraints m_disableOnPlatforms = PlatformConstraints.None;

        private void Awake()
        {
            if (ShouldDisableOnCurrentPlatform(m_disableOnPlatforms))
            {
                gameObject.SetActive(false);
                return;
            }
        }
        public static bool ShouldDisableOnCurrentPlatform(PlatformConstraints constraints)
        {
            if (constraints == PlatformConstraints.None) return false;

            // Editor Check
            if (Application.isEditor && constraints.HasFlag(PlatformConstraints.Editor)) return true;

            // WebGL Check
            if (Application.platform == RuntimePlatform.WebGLPlayer && constraints.HasFlag(PlatformConstraints.Web)) return true;

            // Mobile Check
            if ((Application.platform == RuntimePlatform.Android ||
                 Application.platform == RuntimePlatform.IPhonePlayer) &&
                 constraints.HasFlag(PlatformConstraints.Mobile)) return true;

            // Desktop Check
            if ((Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.LinuxPlayer) &&
                 constraints.HasFlag(PlatformConstraints.Desktop)) return true;

            // Console Check
            if ((Application.platform == RuntimePlatform.PS4 ||
                 Application.platform == RuntimePlatform.PS5 ||
                 Application.platform == RuntimePlatform.XboxOne ||
                 Application.platform == RuntimePlatform.Switch) &&
                 constraints.HasFlag(PlatformConstraints.Console)) return true;

            return false;
        }
    }
}
