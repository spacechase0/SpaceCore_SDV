using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Utilities
{
    public class Reflect
    {
        // [DefenTheNation]
        // Changed to use new Reflection interface


        // Originally I was just going to do this directly myself, but since SMAPI
        // caches these things I might as well take advantage of it.
        public static T getField<T>(object obj, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetField<T>(obj, field).GetValue();
        }

        public static T getField<T>(Type type, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetField<T>(type, field).GetValue();
        }

        public static T getProperty<T>(object obj, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetProperty<T>(obj, field).GetValue();
        }

        public static T getProperty<T>(Type type, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetProperty<T>(type, field).GetValue();
        }

        public static void setField<T>(object obj, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetField<T>(obj, field).SetValue(value);
        }

        public static void setField<T>(Type type, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetField<T>(type, field).SetValue(value);
        }

        public static void setProperty<T>(object obj, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetProperty<T>(obj, field).SetValue(value);
        }

        public static void setProperty<T>(Type type, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetProperty<T>(type, field).SetValue(value);
        }

        public static void callMethod(object obj, string method, object[] args)
        {
            SpaceCore.instance.Helper.Reflection.GetMethod(obj, method).Invoke(args);
        }

        public static T callMethod< T >( object obj, string method, object[] args )
        {
            return SpaceCore.instance.Helper.Reflection.GetMethod(obj, method).Invoke< T >(args);
        }

        public static void callMethod(Type type, string method, object[] args)
        {
            SpaceCore.instance.Helper.Reflection.GetMethod(type, method).Invoke(args);
        }

        public static T callMethod<T>(Type type, string method, object[] args)
        {
            return SpaceCore.instance.Helper.Reflection.GetMethod(type, method).Invoke<T>(args);
        }
    }
}
