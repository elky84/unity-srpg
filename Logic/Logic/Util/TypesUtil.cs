using System;
using Logic.Common;

namespace Logic.Util
{
    public static class TypesUtil
    {
        public static int Code<T>(this T e) where T : struct
        {
            return (int)(object)e;
        }

        public static Nullable<T> FromDescription<T>(string description) where T : struct
        {
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
            {
                Enum eValue = (Enum)Enum.ToObject(typeof(T), e);
                if (eValue.GetDescription() == description)
                {
                    return e;
                }
            }

            return null;
        }

        public static Nullable<T> FromName<T>(string name) where T : struct
        {
            name = name.ToUpper();
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
            {
                if (e.ToString() == name)
                {
                    return e;
                }
            }

            return null;
        }
    }
}
