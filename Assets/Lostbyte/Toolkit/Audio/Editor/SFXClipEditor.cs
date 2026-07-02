using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Lostbyte.Toolkit.CustomEditor;

namespace Lostbyte.Toolkit.Audio.Editor
{
    [UnityEditor.CustomEditor(typeof(SFXClip))]
    public class SFXClipEditor : UnityEditor.Editor
    {
        private SerializedProperty _clips;
        private SerializedProperty _minVolume, _maxVolume, _minPitch, _maxPitch;
        private SerializedProperty _spatialBlend, _steroPan, _reverb;
        private SerializedProperty _minDist, _maxDist, _rolloff, _spread, _dopler;

        private SerializedProperty GetProp(string propName) => serializedObject.FindProperty($"<{propName}>k__BackingField");

        private void OnEnable()
        {
            _clips = GetProp(nameof(SFXClip.Clips));

            _minVolume = GetProp(nameof(SFXClip.MinVolume));
            _maxVolume = GetProp(nameof(SFXClip.MaxVolume));
            _minPitch = GetProp(nameof(SFXClip.MinPitch));
            _maxPitch = GetProp(nameof(SFXClip.MaxPitch));

            _spatialBlend = GetProp(nameof(SFXClip.SpatialBlend));
            _steroPan = GetProp(nameof(SFXClip.StereoPan));
            _reverb = GetProp(nameof(SFXClip.ReverbZoneMix));

            _minDist = GetProp(nameof(SFXClip.MinDistance));
            _maxDist = GetProp(nameof(SFXClip.MaxDistance));
            _rolloff = GetProp(nameof(SFXClip.RolloffMode));
            _spread = GetProp(nameof(SFXClip.Spread));
            _dopler = GetProp(nameof(SFXClip.DopplerLevel));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            var clipsCard = CreateTogglableCard("Audio Clips");
            var listView = new ListView
            {
                bindingPath = _clips.propertyPath,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = false,
                showBoundCollectionSize = false,
                showBorder = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = { marginTop = 0, flexGrow = 1 },
                makeItem = () => new PropertyField { style = { marginTop = 0, marginBottom = 0, paddingRight = 0 } },
                bindItem = (element, i) =>
                    {
                        var propField = (PropertyField)element;
                        propField.BindProperty(_clips.GetArrayElementAtIndex(i));
                        propField.label = string.Empty;
                    }
            };

            clipsCard.Add(listView);
            root.Add(clipsCard);

            var dynamicsCard = CreateTogglableCard("Audio Dynamics");
            dynamicsCard.Add(CreateMinMaxSlider("Volume", _minVolume, _maxVolume, 0f, 1f));
            dynamicsCard.Add(CreateMinMaxSlider("Pitch", _minPitch, _maxPitch, -3f, 3f));
            root.Add(dynamicsCard);

            var spatialCard = CreateTogglableCard("Spatial Settings");
            spatialCard.Add(CreateCompactProperty(_spatialBlend));
            spatialCard.Add(CreateCompactProperty(_steroPan));
            spatialCard.Add(CreateCompactProperty(_reverb));
            root.Add(spatialCard);

            var spatial3DCard = CreateTogglableCard("3D Settings");
            spatial3DCard.Add(CreateCompactProperty(_dopler));
            spatial3DCard.Add(CreateCompactProperty(_spread));
            spatial3DCard.Add(CreateCompactProperty(_rolloff));

            var distanceGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 0, marginBottom = 0 } };
            distanceGroup.Add(new Label("Distance (Min/Max)") { style = { width = 140, unityTextAlign = TextAnchor.MiddleLeft } });

            distanceGroup.Add(new PropertyField(_minDist, "") { style = { flexGrow = 1 } });
            distanceGroup.Add(new Label("-") { style = { width = 12, unityTextAlign = TextAnchor.MiddleCenter } });
            distanceGroup.Add(new PropertyField(_maxDist, "") { style = { flexGrow = 1 } });

            spatial3DCard.Add(distanceGroup);
            root.Add(spatial3DCard);

            spatial3DCard.style.display = _spatialBlend.floatValue > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            spatial3DCard.TrackPropertyValue(_spatialBlend, prop =>
            {
                spatial3DCard.style.display = prop.floatValue > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            });
            var previewBtn = new Button(PreviewAudio)
            {
                text = "Play Preview",
                style = { height = 22, marginTop = 2, unityFontStyleAndWeight = FontStyle.Bold }
            }.SetBorderRadius(4);
            root.Add(previewBtn);
            return root;
        }

        private void ApplyCardStyle(VisualElement element)
        {
            element.SetBackgroundColor(new Color(0f, 0f, 0f, 0.1f))
                .SetBorderRadius(4)
                .SetPadding(2, 3, 0)
                .SetMargin(0, 0, 3, 0);
        }

        private Foldout CreateTogglableCard(string title, bool defaultState = true)
        {
            var foldout = new Foldout { text = title, value = defaultState, viewDataKey = $"SFXClipEditor_{title.Replace(" ", "")}" };
            ApplyCardStyle(foldout);
            var toggle = foldout.Q<Toggle>();
            if (toggle != null)
            {
                toggle.SetMargin(2, 0).SetPadding(0, 0, 2);
                toggle.style.borderBottomWidth = 1;
                toggle.style.borderBottomColor = new Color(0f, 0f, 0f, 0.2f);
                var label = toggle.Q<Label>();
                if (label != null) label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            return foldout;
        }

        private VisualElement CreateCompactProperty(SerializedProperty prop) => new PropertyField(prop) { style = { marginBottom = 0, marginTop = 0 } };

        private VisualElement CreateMinMaxSlider(string labelText, SerializedProperty minProp, SerializedProperty maxProp, float minLimit, float maxLimit)
        {
            var container = new VisualElement().MakeRow().SetMargin(0);
            var label = new Label(labelText) { style = { width = 140, unityTextAlign = TextAnchor.MiddleLeft } };
            var minField = new FloatField { bindingPath = minProp.propertyPath, style = { width = 45 } };
            var maxField = new FloatField { bindingPath = maxProp.propertyPath, style = { width = 45 } };
            var slider = new MinMaxSlider(minProp.floatValue, maxProp.floatValue, minLimit, maxLimit) { style = { flexGrow = 1, paddingLeft = 2, paddingRight = 2 } };
            slider.RegisterValueChangedCallback(evt =>
            {
                minProp.floatValue = (float)System.Math.Round(evt.newValue.x, 2);
                maxProp.floatValue = (float)System.Math.Round(evt.newValue.y, 2);
                serializedObject.ApplyModifiedProperties();
            });
            slider.TrackPropertyValue(minProp, prop => slider.value = new Vector2(prop.floatValue, slider.value.y));
            slider.TrackPropertyValue(maxProp, prop => slider.value = new Vector2(slider.value.x, prop.floatValue));
            container.Add(label);
            container.Add(minField);
            container.Add(slider);
            container.Add(maxField);
            return container;
        }

        private void PreviewAudio()
        {
            SFXClip clip = (SFXClip)target;
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                Vector3 viewPos = SceneView.lastActiveSceneView.camera.transform.position;
                clip.Play(viewPos);
            }
            else
            {
                clip.Play(Vector3.zero);
            }
        }
    }
}