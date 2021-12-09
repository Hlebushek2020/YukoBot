using System;
using YukoClient.Attributes.Enum;

namespace YukoClient.Enums
{
    [Flags]
    public enum ScriptMode : int
    {
        [Title("Одно")]
        [Description("Получить вложение из сообщения по Id")]
        One = 0,

        [Title("После")]
        [Description("Получить вложения из сообщений после заданного через Id")]
        After = 1,

        [Title("До")]
        [Description("Получить вложения из сообщений перед заданным через Id")]
        Before = 2,

        [Title("Последние")]
        [Description("Получить вложения(е) из сообщений(я) с конца. (Для получения вложения из последнего сообщения в количестве указать 1)")]
        End = 4,

        [Title("Все")]
        [Description("Получить вложения из всех сообщений в данном канале")]
        All = 8
    }
}