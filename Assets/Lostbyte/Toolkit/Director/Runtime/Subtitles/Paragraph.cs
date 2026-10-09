using System;
using Lostbyte.Toolkit.CustomEditor.Graphs;
using Lostbyte.Toolkit.Localization;

namespace Lostbyte.Toolkit.Director
{
    [Serializable]
    public struct Paragraph
    {
        [GraphField("")] public LocalizedReference<string> String;
        [GraphField] public float Gap;
    }
}
