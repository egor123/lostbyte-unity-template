using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.UIElements;
using System.Reflection;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.FactSystem.Editor;

namespace Lostbyte.Toolkit.Localization.Editor
{
    [UnityEditor.CustomEditor(typeof(LocalizationDatabase))]
    public class LocalizationDatabaseEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            Button reimportButton = new(() => Reimport())
            {
                text = "Reimport Tables"
            };
            reimportButton.style.marginBottom = 10;
            reimportButton.style.marginTop = 5;

            root.Add(reimportButton);
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }

        [MenuItem("Tools/Localization/Reimport")]
        public static void Reimport()
        {
            if (!LocalizationSchemaParser.UpdateScema()) return;
            if (!LocalizedTableParser.UpdateTables()) return;
            if (!LocalizationCodeGenerator.Generate()) return;
        }
    }
}