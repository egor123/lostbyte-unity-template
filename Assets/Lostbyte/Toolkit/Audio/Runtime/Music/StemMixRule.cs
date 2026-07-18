using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem;
using UnityEngine;

namespace Lostbyte.Toolkit.Audio.Music
{
    [Flags]
    public enum StemDriveMode
    {
        ConditionToggle = 0x1,
        EquationDriven = 0x2,
    }
    [Serializable]
    public struct StemMixRule
    {
        [Tooltip("The audio layer this rule overrides")]
        public int TargetLayer;

        public StemDriveMode DriveMode;

        [Tooltip("Used if DriveMode is ConditionToggle.")]
        [EnumShowIf(nameof(DriveMode), StemDriveMode.ConditionToggle)]
        public Condition ActivationCondition;
        [EnumShowIf(nameof(DriveMode), true, StemDriveMode.ConditionToggle)]
        [Range(0f, 1f)] public float ActiveVolume;
        [Tooltip("Equation should output a float between 0 and 1.")]
        [EnumShowIf(nameof(DriveMode), StemDriveMode.EquationDriven)]
        public NumericEquation VolumeEquation;
        [Tooltip("How fast (in seconds) the system interpolates to the new volume.")]
        [Min(0)] public float FadeSmoothTime;

        public readonly float? EvaluateTargetVolume()
        {
            if (HasFlag(DriveMode, StemDriveMode.ConditionToggle))
                if (ActivationCondition != null && !ActivationCondition.IsMet)
                    return null;
            float volume = ActiveVolume;
            if (HasFlag(DriveMode, StemDriveMode.EquationDriven))
                if (VolumeEquation != null)
                    return VolumeEquation.Value;
            return volume;
        }
        private static bool HasFlag(StemDriveMode op, StemDriveMode checkflag) => (op & checkflag) == checkflag;
    }
}
