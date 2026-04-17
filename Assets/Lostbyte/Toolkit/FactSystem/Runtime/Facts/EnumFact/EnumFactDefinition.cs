
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem
{
    public class EnumFactDefinition : FactDefinition<Enum>
    {
        [field: SerializeField, SerializeReference, SerializedEnum] public override Enum DefaultValue { get; set; }
        [field: SerializeField] public List<string> Values { get; private set; } = new(); //TODO internal
        public Type EnumType
        {
            get
            {
                if (DefaultValue != null)
                    return DefaultValue.GetType();

                string typeName = $"GameFacts.Enums+{FactUtils.MakeSafeIdentifier(name)}";

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType(typeName, false);
                    if (type != null)
                        return type;
                }

                return null;
            }
        }
        public Enum DefaultEnumValue
        {
            get
            {
                if (DefaultValue != null) return DefaultValue;
                var type = EnumType;
                if (type == null) return null;
                var arr = Enum.GetValues(EnumType);
                if (arr == null || arr.Length == 0) return null;
                return arr.GetValue(0) as Enum;
            }
        }
    }
}