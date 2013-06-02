using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Platform
{
    public abstract class ClassRegistry
    {
        public abstract void RegisterPlatform();
        private static Dictionary<string, object> _items = new Dictionary<string, object>();
        protected static void RegisterClass<TType>(TType Pclass)
        {
            if (typeof(TType).BaseType.ToString() != typeof(PlatformDependentClassBaseType).ToString())
                throw new Exception("Not supported class.");
            if (_items.ContainsKey(typeof(TType).ToString()))
                _items[typeof(TType).ToString()] = Pclass;
            else
                _items.Add(typeof(TType).ToString(), Pclass);
        }
        public static TType GetClass<TType>()
        {
            if (typeof(TType).BaseType.ToString() != typeof(PlatformDependentClassBaseType).ToString())
                throw new Exception("Not supported class.");
            if (_items.ContainsKey(typeof(TType).ToString()))
                return (TType)_items[typeof(TType).ToString()];
            throw new Exception("No active class registered.");
        }
        public class PlatformDependentClassBaseType
        {
        }
    }
}
