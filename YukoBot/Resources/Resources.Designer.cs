﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YukoBot {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("YukoBot.Resources.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Привет, для того что бы узнать больше информации обо мне выполни команду `{0}info`..
        /// </summary>
        internal static string BotDescription {
            get {
                return ResourceManager.GetString("BotDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, эта команда доступна для зарегистрированных пользователей!.
        /// </summary>
        internal static string RegisteredUserCommand_AccessError {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.AccessError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to К сожалению причина бана не была указана!.
        /// </summary>
        internal static string RegisteredUserCommand_BanReason_NotBanReason {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.BanReason.NotBanReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Хост.
        /// </summary>
        internal static string RegisteredUserCommand_Settings_FieldHost_Title {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Settings.FieldHost.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Порт.
        /// </summary>
        internal static string RegisteredUserCommand_Settings_FieldPort_Title {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Settings.FieldPort.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Вы не забанены!.
        /// </summary>
        internal static string RegisteredUserCommand_Settings_NotBan {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Settings.NotBan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to **Алиасы:**.
        /// </summary>
        internal static string UserCommand_Help_AliasesSection {
            get {
                return ResourceManager.GetString("UserCommand.Help.AliasesSection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to **Аргументы:**.
        /// </summary>
        internal static string UserCommand_Help_ArgumentsSection {
            get {
                return ResourceManager.GetString("UserCommand.Help.ArgumentsSection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Необязательно, по умолчанию: {0}.
        /// </summary>
        internal static string UserCommand_Help_OptionalArgument {
            get {
                return ResourceManager.GetString("UserCommand.Help.OptionalArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to **__Вариант {0}__**.
        /// </summary>
        internal static string UserCommand_Help_OptionsSection {
            get {
                return ResourceManager.GetString("UserCommand.Help.OptionsSection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Привет, я Юко. Бот созданный для быстрого скачивания картинок с каналов серверов (гильдий) дискорда. Так же я могу составлять коллекции из сообщений с картинками (или ссылками на картинки) для последующего скачивания этих коллекций..
        /// </summary>
        internal static string UserCommand_Info_EmbedDescription {
            get {
                return ResourceManager.GetString("UserCommand.Info.EmbedDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Данный функционал доступен только зарегистрированным пользователям. Для просмотра всех доступных команд управления коллекциями воспользуйся командой `{0}help {1}`..
        /// </summary>
        internal static string UserCommand_Info_FieldCollectionManagement_Description {
            get {
                return ResourceManager.GetString("UserCommand.Info.FieldCollectionManagement.Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Управление коллекциями.
        /// </summary>
        internal static string UserCommand_Info_FieldCollectionManagement_Title {
            get {
                return ResourceManager.GetString("UserCommand.Info.FieldCollectionManagement.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ссылки.
        /// </summary>
        internal static string UserCommand_Info_FieldLinks_Title {
            get {
                return ResourceManager.GetString("UserCommand.Info.FieldLinks.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Премиум доступ позволяет заранее сохранять необходимые данные (при добавлении сообщения в коллекцию) для скачивания вложений из сообщения. Это в разы уменьшает время получения ссылок клиентом для скачивания вложений. На данный момент выдается моим хозяином..
        /// </summary>
        internal static string UserCommand_Info_FieldPremiumAccess_Description {
            get {
                return ResourceManager.GetString("UserCommand.Info.FieldPremiumAccess.Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Премиум доступ.
        /// </summary>
        internal static string UserCommand_Info_FieldPremiumAccess_Title {
            get {
                return ResourceManager.GetString("UserCommand.Info.FieldPremiumAccess.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Новый пароль от учетной записи отправлен в ЛС..
        /// </summary>
        internal static string UserCommand_Register_Description_PasswordChange {
            get {
                return ResourceManager.GetString("UserCommand.Register.Description.PasswordChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Регистрация прошла успешно! Пароль и логин от учетной записи отправлены в ЛС..
        /// </summary>
        internal static string UserCommand_Register_Description_Register {
            get {
                return ResourceManager.GetString("UserCommand.Register.Description.Register", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пароль сменен!.
        /// </summary>
        internal static string UserCommand_Register_DmTitle_PasswordChange {
            get {
                return ResourceManager.GetString("UserCommand.Register.DmTitle.PasswordChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Регистрация прошла успешно!.
        /// </summary>
        internal static string UserCommand_Register_DmTitle_Register {
            get {
                return ResourceManager.GetString("UserCommand.Register.DmTitle.Register", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Используй **{0}** или **{1}**.
        /// </summary>
        internal static string UserCommand_Register_FieldDmDescription_Login {
            get {
                return ResourceManager.GetString("UserCommand.Register.FieldDmDescription.Login", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Логин.
        /// </summary>
        internal static string UserCommand_Register_FieldDmTitle_Login {
            get {
                return ResourceManager.GetString("UserCommand.Register.FieldDmTitle.Login", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Новый пароль.
        /// </summary>
        internal static string UserCommand_Register_FieldDmTitle_NewPassword {
            get {
                return ResourceManager.GetString("UserCommand.Register.FieldDmTitle.NewPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пароль.
        /// </summary>
        internal static string UserCommand_Register_FieldDmTitle_Password {
            get {
                return ResourceManager.GetString("UserCommand.Register.FieldDmTitle.Password", resourceCulture);
            }
        }
    }
}
