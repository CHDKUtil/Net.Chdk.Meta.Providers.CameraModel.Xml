using Net.Chdk.Meta.Generators.Platform;
using Net.Chdk.Meta.Providers.CameraModel.Xml.Model;
using Net.Chdk.Meta.Providers.Platform;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Net.Chdk.Meta.Providers.CameraModel.Xml
{
    sealed class XmlPlatformProvider : PlatformProvider
    {
        public XmlPlatformProvider(IPlatformGenerator platformGenerator)
            : base(platformGenerator)
        {
        }

        protected override IEnumerable<KeyValuePair<string, string>> DoGetPlatforms(Stream stream)
        {
            return ReadModelIdTag(stream)
                .Values
                .Select(GetValue);
        }

        private static Tag ReadModelIdTag(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(TagInfo));
            var tagInfo = (TagInfo)serializer.Deserialize(stream);
            var table = tagInfo.Tables.Single(t => t.Name == "CanonRaw::Main");
            return table.Tags.Single(t => t.Name == "CanonModelID");
        }

        private static KeyValuePair<string, string> GetValue(Key key)
        {
            var id = uint.Parse(key.Id);
            return new KeyValuePair<string, string>($"0x{id:x}", key.Value);
        }
    }
}
