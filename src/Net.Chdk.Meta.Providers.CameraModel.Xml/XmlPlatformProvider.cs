using Net.Chdk.Meta.Generators.Platform;
using Net.Chdk.Meta.Model.CameraModel;
using Net.Chdk.Meta.Providers.CameraModel.Xml.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Net.Chdk.Meta.Providers.CameraModel.Xml
{
    sealed class XmlPlatformProvider : IPlatformProvider
    {
        private IPlatformGenerator PlatformGenerator { get; }

        public XmlPlatformProvider(IPlatformGenerator platformGenerator)
        {
            PlatformGenerator = platformGenerator;
        }

        public IDictionary<string, PlatformData> GetPlatforms(Stream stream)
        {
            var tag = ReadModelIdTag(stream);
            var keys = tag.Values;
            var values = keys
                .Where(k => k.Value != "EOS D30")
                .Concat(new[]
                {
                    new Key
                    {
                        Id = $"{0x3380000}",
                        Value = "PowerShot N Facebook"
                    }
                }
            ).Select(GetValue);

            return GetPlatforms(values);
        }

        private IDictionary<string, PlatformData> GetPlatforms(IEnumerable<KeyValuePair<string, string>> values)
        {
            var platforms = new SortedDictionary<string, PlatformData>();
            foreach (var kvp in values)
            {
                var models = GetCameraModels(kvp.Value);
                if (models.First() != null)
                {
                    foreach (var model in models)
                    {
                        var platform = GetPlatform(kvp.Key, model);
                        platforms.Add(model.Platform, platform);
                    }
                }
            }
            return platforms;
        }

        private static PlatformData GetPlatform(string modelId, CameraModel model)
        {
            return new PlatformData
            {
                ModelId = modelId,
                Names = model.Names
            };
        }

        private IEnumerable<CameraModel> GetCameraModels(string value)
        {
            var models = GetModels(value)
                .Select(m => m.TrimEnd(" (new)"))
                .ToArray();

            return GetModelMatrix(models)
                .Select(GetCameraModel);
        }

        private CameraModel GetCameraModel(string[] names)
        {
            var platform = GetPlatform(names);
            if (platform == null)
                return null;

            return new CameraModel
            {
                Names = names.Select(n => $"Canon {n}").ToArray(),
                Platform = platform
            };
        }

        private string GetPlatform(string[] names)
        {
            return PlatformGenerator.GetPlatform(names);
        }

        private static IEnumerable<string> GetModels(string value)
        {
            int index;
            var startIndex = 0;
            while ((index = value.IndexOf(" / ", startIndex)) >= 0)
            {
                yield return value.Substring(startIndex, index - startIndex);
                startIndex = index + " / ".Length;
            }

            yield return value.Substring(startIndex);
        }

        private static string[][] GetModelMatrix(string[] models)
        {
            if (models[0].Contains('/'))
            {
                var models2 = models
                    .Select(Split)
                    .ToArray();
                return models.Length > 1
                    ? Transpose(models2)
                    : models2;
            }
            return new[] { models };
        }

        private static string[] Split(string model)
        {
            var split = model.Split(' ');
            var index = GetIndex(split);
            var submodels = split[index].Split('/');
            var result = new string[submodels.Length];
            for (int i = 0; i < submodels.Length; i++)
            {
                var split2 = new string[split.Length];
                Array.Copy(split, split2, split.Length);
                split2[index] = submodels[i];
                result[i] = string.Join(" ", split2);
            }
            return result;
        }

        private static string[][] Transpose(string[][] m)
        {
            var t = new string[m[0].Length][];
            for (int i = 0; i < m[0].Length; i++)
            {
                t[i] = new string[m.Length];
                for (int j = 0; j < m.Length; j++)
                    t[i][j] = m[j][i];
            }

            return t;
        }

        private static int GetIndex(string[] split)
        {
            for (int i = 0; i < split.Length; i++)
                if (split[i].Contains('/'))
                    return i;
            return -1;
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
