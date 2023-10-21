namespace YukoBot.Enums;

public enum ClientErrorCodes
{
    None = 0,

    //"Вы не авторизованы!"
    NotAuthorized = 1,

    //"Неверный логин или пароль!",
    InvalidCredentials = 2,

    //"Вас нет на этом сервере!"
    MemberNotFound = 3,

    //$"Вы забанены на этом сервере, для разбана обратитесь к администратору сервера.{reason}"
    MemberBanned = 4,

    //$"Следующие каналы были не найдены: {string.Join(',', channelNotFound)}.";
    ChannelNotFound = 5,

    //$"Следующие сообщения были не найдены: {string.Join(',', messageNotFound)}."
    MessageNotFound = 6
}