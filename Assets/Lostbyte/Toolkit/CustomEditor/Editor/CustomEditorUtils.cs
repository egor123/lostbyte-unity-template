using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    public static class CustomEditorUtils
    {
        public static Rect GetNextSegment(this Rect rect, float nextWidth, Rect parentRect)
        {
            rect.x += rect.width;
            rect.width = nextWidth;
            parentRect.xMax = Mathf.Min(parentRect.xMax);
            return rect;
        }
        public static Rect GetFirtstSegment(this Rect rect, float width)
        {
            rect.width = Mathf.Min(width, rect.width);
            return rect;
        }
    }
}
