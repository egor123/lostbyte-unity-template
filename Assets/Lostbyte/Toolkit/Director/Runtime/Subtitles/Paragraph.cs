using System;
using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.Localization;
using UnityEngine;

namespace Lostbyte.Toolkit.Director
{
    [Serializable]
    public struct Paragraph
    {
        [GraphField("")] public LocalizedReference<string> String;
        [GraphField] public float Gap;
    }
}
