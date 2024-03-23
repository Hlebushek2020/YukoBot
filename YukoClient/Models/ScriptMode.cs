using System;
using System.Reflection;
using YukoClient.Attributes.Enum;

namespace YukoClient.Models
{
    public class ScriptMode
    {
        public Enums.ScriptMode Mode { get; }

        public string Title
        {
            get
            {
                Type type = Mode.GetType();
                MemberInfo[] memInfo = type.GetMember(Mode.ToString());
                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(TitleAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        return ((TitleAttribute)attrs[0]).Value;
                    }
                }
                return Mode.ToString();
            }
        }

        public string Description
        {
            get
            {
                Type type = Mode.GetType();
                MemberInfo[] memInfo = type.GetMember(Mode.ToString());
                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        return ((DescriptionAttribute)attrs[0]).Value;
                    }
                }
                return Mode.ToString();
            }
        }

        public ScriptMode(Enums.ScriptMode mode) { Mode = mode; }
    }
}