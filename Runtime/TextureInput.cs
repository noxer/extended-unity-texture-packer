using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TexPacker
{
    public class TextureInput
    {
        public string label;

        public Texture2D texture;

        private Dictionary<TextureChannel, TextureChannelInput> _inputs = new Dictionary<TextureChannel, TextureChannelInput>();

        public bool inverted;

        public TextureInput()
        {
            _inputs[TextureChannel.ChannelRed]      = new TextureChannelInput(TextureChannel.ChannelRed);
            _inputs[TextureChannel.ChannelGreen]    = new TextureChannelInput(TextureChannel.ChannelGreen);
            _inputs[TextureChannel.ChannelBlue]     = new TextureChannelInput(TextureChannel.ChannelBlue);
            _inputs[TextureChannel.ChannelAlpha]    = new TextureChannelInput(TextureChannel.ChannelAlpha);
        }

        public TextureChannelInput GetChannelInput(TextureChannel channel)
        {
            return _inputs[channel];
        }

        public void SetChannelInput(TextureChannel channel, TextureChannelInput channelInput)
        {
            _inputs[channel] = channelInput;
        }
    }
}