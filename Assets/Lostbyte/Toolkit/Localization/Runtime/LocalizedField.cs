using System.Collections;
using System.Collections.Generic;
using Lostbyte.Toolkit.FactSystem;
using TMPro;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedField : MonoBehaviour
    {
        [field: SerializeField] public LocalizedString String { get; private set; }

        private TMP_Text _field;

        private void Awake()
        {
            _field = GetComponent<TMP_Text>();
            String.Subscribe(OnChange);
        }

        private void OnChange(string value)
        {
            _field.text = value;
        }
        private void OnDestroy()
        {
            String.Dispose();
        }
    }
}
