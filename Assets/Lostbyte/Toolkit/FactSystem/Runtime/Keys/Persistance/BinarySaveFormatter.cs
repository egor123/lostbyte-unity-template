using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Persistance
{
    public class BinarySaveFormatter : ISaveFormatter
    {
        private static IFormatter _formatter;
        private static IFormatter Formatter
        {
            get
            {
                if (_formatter != null) return _formatter;

                BinaryFormatter formatter = new();
                SurrogateSelector selector = new();
                StreamingContext context = new(StreamingContextStates.All);

                //TODO auto load !!!!!!
                selector.AddSurrogate(typeof(Vector2), context, new Vector2Surrogate());
                selector.AddSurrogate(typeof(Vector3), context, new Vector3Surrogate());
                selector.AddSurrogate(typeof(Vector4), context, new Vector4Surrogate());
                selector.AddSurrogate(typeof(Quaternion), context, new QuaternionSurrogate());
                selector.AddSurrogate(typeof(Color), context, new ColorSurrogate());
                selector.AddSurrogate(typeof(Color32), context, new Color32Surrogate());
                selector.AddSurrogate(typeof(Rect), context, new RectSurrogate());
                selector.AddSurrogate(typeof(Bounds), context, new BoundsSurrogate());

                formatter.SurrogateSelector = selector;
                _formatter = formatter;
                return _formatter;

            }
        }
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _formatter = null;
        }
#endif

        public object Deserialize(Stream serializationStream) => Formatter.Deserialize(serializationStream);
        public void Serialize(Stream serializationStream, object graph) => Formatter.Serialize(serializationStream, graph);
    }

    public class Vector2Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Vector2)obj;
            info.AddValue("x", v.x);
            info.AddValue("y", v.y);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Vector2(
                info.GetSingle("x"),
                info.GetSingle("y")
            );
        }
    }

    public class Vector3Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Vector3)obj;
            info.AddValue("x", v.x);
            info.AddValue("y", v.y);
            info.AddValue("z", v.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Vector3(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("z")
            );
        }
    }

    public class Vector4Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Vector4)obj;
            info.AddValue("x", v.x);
            info.AddValue("y", v.y);
            info.AddValue("z", v.z);
            info.AddValue("w", v.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Vector4(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("z"),
                info.GetSingle("w")
            );
        }
    }

    public class QuaternionSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var q = (Quaternion)obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Quaternion(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("z"),
                info.GetSingle("w")
            );
        }
    }

    public class ColorSurrogate : ISerializationSurrogate
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

    public class Color32Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var c = (Color32)obj;
            info.AddValue("r", c.r);
            info.AddValue("g", c.g);
            info.AddValue("b", c.b);
            info.AddValue("a", c.a);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Color32(
                info.GetByte("r"),
                info.GetByte("g"),
                info.GetByte("b"),
                info.GetByte("a")
            );
        }
    }

    public class RectSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var r = (Rect)obj;
            info.AddValue("x", r.x);
            info.AddValue("y", r.y);
            info.AddValue("w", r.width);
            info.AddValue("h", r.height);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Rect(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("w"),
                info.GetSingle("h")
            );
        }
    }

    public class BoundsSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var b = (Bounds)obj;
            info.AddValue("center", b.center);
            info.AddValue("size", b.size);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return new Bounds(
                (Vector3)info.GetValue("center", typeof(Vector3)),
                (Vector3)info.GetValue("size", typeof(Vector3))
            );
        }
    }
}