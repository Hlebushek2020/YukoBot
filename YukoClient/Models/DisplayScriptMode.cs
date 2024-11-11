using System;
using System.Reflection;
using YukoClient.Attributes.Enum;
using YukoClient.Enums;

namespace YukoClient.Models
{
    public class DisplayScriptMode
    {
        public ScriptMode Mode { get; }

        public string Title
        {
            get
            {
                Type type = Mode.GetType();
                MemberInfo[] memInfo = type.GetMember(Mode.ToString());

                if (memInfo.Length <= 0)
                    return Mode.ToString();

                object[] attrs = memInfo[0].GetCustomAttributes(typeof(TitleAttribute), false);
                return attrs.Length > 0 ? ((TitleAttribute)attrs[0]).Value : Mode.ToString();
            }
        }

        public string Description
        {
            get
            {
                Type type = Mode.GetType();
                MemberInfo[] memInfo = type.GetMember(Mode.ToString());

                if (memInfo.Length <= 0)
                    return Mode.ToString();

                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attrs.Length > 0 ? ((DescriptionAttribute)attrs[0]).Value : Mode.ToString();
            }
        }

        public DisplayScriptMode(ScriptMode mode) { Mode = mode; }
    }
}