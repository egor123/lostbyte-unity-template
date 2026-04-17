using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Localization;

namespace Lostbyte.Toolkit.SaveSystem
{
    public static class Formatter
    {
        public static BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter formatter = new();
            SurrogateSelector selector = new();
            selector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), new ColorSerializationSurrogate());
            selector.AddSurrogate(typeof(LocalizedString), new StreamingContext(StreamingContextStates.All), new LocalizedStringSurrogate());

            formatter.SurrogateSelector = selector;
            return formatter;
        }
    }
    public class ColorSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Color color = (Color)obj;
            info.AddValue("r", color.r);
            info.AddValue("g", color.g);
            info.AddValue("b", color.b);
            info.AddValue("a", color.a);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Color color = (Color)obj;
            color.r = (float)info.GetValue("r", typeof(float));
            color.g = (float)info.GetValue("g", typeof(float));
            color.b = (float)info.GetValue("b", typeof(float));
            color.a = (float)info.GetValue("a", typeof(float));
            return color;
        }
    }
    public class LocalizedStringSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            LocalizedString localizedString = (LocalizedString)obj;
            localizedString.GetLocalizedString();
            info.AddValue("tableReference", localizedString.TableReference.TableCollectionNameGuid.ToString());
            info.AddValue("entryReference", localizedString.TableEntryReference.KeyId);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            // LocalizedString localizedString = (LocalizedString)obj;
            LocalizedString localizedString = new(Guid.Parse(info.GetString("tableReference")), info.GetInt64("entryReference"));
            // {
            //     TableReference = Guid.Parse(info.GetString("tableReference")),
            //     TableEntryReference = info.GetInt64("entryReference")
            // };
            // //FIXME now it displays properly in inspector, but "System.NullReferenceException: Object reference not set to an instance of an object" when it isused
            return localizedString;
        }
    }
}