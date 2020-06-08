using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TexPacker
{
    public class TexturePackerWindow : EditorWindow
    {
        private readonly string _windowTitle = "Channel Packer";
        private readonly Vector2 _windowSize = new Vector2(300, 450);
        private readonly int _maxInputCount = 4;
        private readonly int _textureSupportedResolutionMin = 64;
        private readonly int _textureSupportedResolutionMax = 8192;

        Vector2 _windowScrollPos;

        private TextureFormat _textureFormat = TextureFormat.PNG;

        private TexturePacker _texturePacker = new TexturePacker();

        private List<int> _textureResolutions = new List<int>();
        private List<string> _textureResolutionsNames = new List<string>();

        private bool isPreset = true;
        private string presetName;

        private List<TextureItem> _items = new List<TextureItem>();
        private TexturePreview _preview;

        [MenuItem("Window/Channel Packer/Custom")]
        static void OpenDefault()
        {
            TexturePackerWindow window = GetWindow<TexturePackerWindow>();
            window.Initialize();
        }

        [MenuItem("Window/Channel Packer/HDRP Mask Map")]
        static void OpenMaskMap() {
            TexturePackerWindow window = GetWindow<TexturePackerWindow>();
            window.Initialize("HDRP Mask Map");

            window.isPreset = true;

            window.AddPredefined(
                "Metallic",
                new Dictionary<TextureChannel, TextureChannelInput>(){ { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelRed, true) } },
                false
            );
            window.AddPredefined(
                "Occlusion",
                new Dictionary<TextureChannel, TextureChannelInput>(){ { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelGreen, true) } },
                false
            );
            window.AddPredefined(
                "Detail Mask",
                new Dictionary<TextureChannel, TextureChannelInput>(){ { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelBlue, true) } },
                false
            );
            window.AddPredefined(
                "Smoothness",
                new Dictionary<TextureChannel, TextureChannelInput>(){ { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelAlpha, true) } },
                false
            );
        }

        [MenuItem("Window/Channel Packer/HDRP Detail Map")]
        static void OpenDetailMap() {
            TexturePackerWindow window = GetWindow<TexturePackerWindow>();
            window.Initialize("HDRP Detail Map");

            window.isPreset = true;

            window.AddPredefined(
                "Desaturated Albedo",
                new Dictionary<TextureChannel, TextureChannelInput>() { { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelRed, true) } },
                false
            );
            window.AddPredefined(
                "Normal Map",
                new Dictionary<TextureChannel, TextureChannelInput>() {
                    { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelAlpha, true) },
                    { TextureChannel.ChannelGreen, new TextureChannelInput(TextureChannel.ChannelGreen, true) }
                },
                false
            );
            window.AddPredefined(
                "Smoothness",
                new Dictionary<TextureChannel, TextureChannelInput>() { { TextureChannel.ChannelRed, new TextureChannelInput(TextureChannel.ChannelBlue, true) } },
                false
            );
        } 

        public void Initialize(string title = null)
        {
            minSize = _windowSize;
            titleContent = new GUIContent(string.IsNullOrEmpty(title) ? _windowTitle : title);

            for(int i = _textureSupportedResolutionMin; i <= _textureSupportedResolutionMax; i *= 2)
            {
                _textureResolutions.Add(i);
                _textureResolutionsNames.Add(i.ToString());
            }

            _texturePacker.Initialize();
            _preview = new TexturePreview();
        }

        private void RefreshItems()
        {
            if (_items.Count == 0)
                return;

            var toDeleteItems = _items.Where(x => x.toDelete==true).ToList();
            foreach (var item in toDeleteItems)
            {
                _texturePacker.Remove(item.input);
                _items.Remove(item);
            }
        }

        private void AddPredefined(string label, Dictionary<TextureChannel, TextureChannelInput> channels, bool inverted)
        {
            TextureInput entry = new TextureInput();
            entry.label = label;
            entry.inverted = inverted;

            foreach (KeyValuePair<TextureChannel, TextureChannelInput> kv in channels)
                entry.SetChannelInput(kv.Key, kv.Value);

            _texturePacker.Add(entry);
            _items.Add(new TextureItem(entry));
        }

        private void OnGUI()
        {
            _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos, false, false);

            RefreshItems();

            GUILayout.Label(_windowTitle, TexturePackerStyles.Heading);
            GUILayout.BeginVertical(TexturePackerStyles.Section);

            GUILayout.Label("Inputs", TexturePackerStyles.Heading);
            foreach (TextureItem item in _items)
            {
                item.Draw();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = _items.Count < _maxInputCount;

            if (GUILayout.Button("+"))
            {
                TextureInput entry = new TextureInput();
                _texturePacker.Add(entry);
                _items.Add(new TextureItem(entry));
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            int prevRes = _texturePacker.resolution;
            _texturePacker.resolution = 128;

            _preview.Draw(_texturePacker);

            _texturePacker.resolution = prevRes;

            GUILayout.Label("Options", TexturePackerStyles.Heading);
            GUILayout.BeginVertical(TexturePackerStyles.Section);
            _textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("> Format:", _textureFormat);
            _texturePacker.resolution = EditorGUILayout.IntPopup("> Resolution:", _texturePacker.resolution, _textureResolutionsNames.ToArray(), _textureResolutions.ToArray());
            GUILayout.EndVertical();

            if (GUILayout.Button("Generate Texture", TexturePackerStyles.Button))
            {
                string savePath = EditorUtility.SaveFilePanel("Save", Application.dataPath, "texture.png", _textureFormat.ToString());
                if (savePath != string.Empty)
                {
                    Texture2D output = _texturePacker.Create();

                    if(_textureFormat == TextureFormat.JPG)
                        File.WriteAllBytes(savePath, output.EncodeToJPG());
                    else if(_textureFormat == TextureFormat.PNG)
                        File.WriteAllBytes(savePath, output.EncodeToPNG());
                    else
                        File.WriteAllBytes(savePath, output.EncodeToEXR());

                    AssetDatabase.Refresh();
                }
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}
