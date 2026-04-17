using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.Localization;
using UnityEngine;

namespace Localization
{
    public static class Strings
    {
        public static LocalizedString GetTestKey(string gender, int count) => new LocalizedString("test_key", gender, count);       
    }
}