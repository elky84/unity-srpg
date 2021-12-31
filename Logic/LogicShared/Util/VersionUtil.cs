using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Logic.Util
{
    public static class VersionUtil
    {
        public static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        private static List<Type> GetTypes(string targetNamespace)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.FullName.StartsWith("LogicShared")).FirstOrDefault();
            if (assembly == null)
            {
                return new List<Type>();
            }
            return assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(targetNamespace)).ToList();
        }

        public static UInt64 ProtocolVersion()
        {
            UInt64 hash = 0;
            foreach (var t in GetTypes("Logic.Protocols"))
            {
                hash += CalculateHashType(t);
            }

            foreach (var t in GetTypes("Logic.Types"))
            {
                hash += CalculateHashType(t);
            }
            return hash;
        }

        private static UInt64 CalculateHashEnum<T>() where T : struct
        {
            UInt64 hash = 0;
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
            {
                hash += CalculateHash(e.ToString());
            }
            return hash;
        }

        private static UInt64 CalculateHashType(Type type)
        {
            UInt64 hash = 0;
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                hash += CalculateHash(field.MemberType.GetType().ToString());
                hash += CalculateHash(field.Name);
            }
            return hash;
        }
    }
}
