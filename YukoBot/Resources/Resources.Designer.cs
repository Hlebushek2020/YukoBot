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
        ///   Looks up a localized string similar to Простите, эта команда доступна для зарегистрированных и не забаненых (на этом сервере) пользователей!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AccessError {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AccessError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ой, у тебя уже есть такая коллекция! Id: {0}..
        /// </summary>
        internal static string ManagingСollectionsCommand_AddCollection_CollectionExists {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddCollection.CollectionExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Коллекция создана! Id: {0}..
        /// </summary>
        internal static string ManagingСollectionsCommand_AddCollection_Created {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddCollection.Created", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, название коллекции не может быть пустым!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddCollection_NameIsEmpty {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddCollection.NameIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, такой коллекции нет!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollection_CollectionNotFound {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollection.CollectionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, во время добавления сообщения в коллекцию &quot;{0}&quot; произошла ошибка. Сообщение не добавлено в коллекцию!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollection_ErrorAddingItem {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollection.ErrorAddingItem", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, данное сообщение уже добавлено в коллекцию &quot;{0}&quot;!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollection_ExistsInCollection {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollection.ExistsInCollection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сообщение добавлено в коллекцию &quot;{0}&quot;!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollection_IsSuccess {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollection.IsSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, нельзя добавлять сообщение в коллекцию если у него нет вложений!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollection_NoAttachments {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollection.NoAttachments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, нет вложенного сообщения!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollection_ReferencedMessageNotFound {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollection.ReferencedMessageNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, канал для поиска сообщений по умолчанию не найден, обратитесь к администратору сервера!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollectionById_ArtChannelNotFound {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollectionById.ArtChannelNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, я не смогла найти заданное сообщение в текущем канале!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollectionById_MessageNotFound {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollectionById.MessageNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, я не смогла найти заданное сообщение в текущем канале! Канал для поиска сообщений по умолчанию не установлен, пожалуйста обратитесь к администратору сервера!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollectionById_MessageNotFoundAndArtChannelNotSet {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollectionById.MessageNotFoundAndArtChannelNotSet" +
                        "", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, я не смогла найти заданное сообщение в канале для поиска сообщений!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollectionById_MessageNotFoundInArtChannel {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollectionById.MessageNotFoundInArtChannel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, у меня нет прав на чтение сообщений в канале для поиска сообщений!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollectionById_NoArtChannelAccess {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollectionById.NoArtChannelAccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, у меня нет прав на чтение сообщений в текущем канале!.
        /// </summary>
        internal static string ManagingСollectionsCommand_AddToCollectionById_NoChannelAccess {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.AddToCollectionById.NoChannelAccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, такой коллекции нет!.
        /// </summary>
        internal static string ManagingСollectionsCommand_DeleteCollection_CollectionNotFound {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.DeleteCollection.CollectionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Коллекция &quot;{0}&quot; удалена!.
        /// </summary>
        internal static string ManagingСollectionsCommand_DeleteCollection_Deleted {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.DeleteCollection.Deleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ой, начальное и конечное сообщение промежутка из разных каналов!.
        /// </summary>
        internal static string ManagingСollectionsCommand_End_DifferentChannels {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.End.DifferentChannels", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сообщения в коллекцию &quot;{0}&quot; успешно добавлены!.
        /// </summary>
        internal static string ManagingСollectionsCommand_End_EndOfExecution {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.End.EndOfExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, во время добавления сообщения в коллекцию &quot;{0}&quot; произошла ошибка. Добавлены не все сообщения!.
        /// </summary>
        internal static string ManagingСollectionsCommand_End_ErrorAddingItem {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.End.ErrorAddingItem", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, нет вложенного сообщения!.
        /// </summary>
        internal static string ManagingСollectionsCommand_End_NoReferencedMessage {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.End.NoReferencedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пожалуйста задайте начальное сообщение промежутка!.
        /// </summary>
        internal static string ManagingСollectionsCommand_End_StartMessageIsNotSet {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.End.StartMessageIsNotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пожалуйста подождите, после завершения операции я изменю это сообщение!.
        /// </summary>
        internal static string ManagingСollectionsCommand_End_StartOfExecution {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.End.StartOfExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, переименовываемая коллекция несуществует!.
        /// </summary>
        internal static string ManagingСollectionsCommand_RenameCollection_CollectionNotFound {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.RenameCollection.CollectionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, коллекция с названием &quot;{0}&quot; уже существует!.
        /// </summary>
        internal static string ManagingСollectionsCommand_RenameCollection_Exists {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.RenameCollection.Exists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Коллекция &quot;{0}&quot; переименована в &quot;{1}&quot;!.
        /// </summary>
        internal static string ManagingСollectionsCommand_RenameCollection_Renamed {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.RenameCollection.Renamed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, не удалось задать начальное сообщение!.
        /// </summary>
        internal static string ManagingСollectionsCommand_Start_IsNotSet {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.Start.IsNotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Начальное сообщение промежутка заданно!.
        /// </summary>
        internal static string ManagingСollectionsCommand_Start_IsSet {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.Start.IsSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, нет вложенного сообщения!.
        /// </summary>
        internal static string ManagingСollectionsCommand_Start_NoReferencedMessage {
            get {
                return ResourceManager.GetString("ManagingСollectionsCommand.Start.NoReferencedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, эта команда доступна только владельцу бота!.
        /// </summary>
        internal static string OwnerCommand_AccessError {
            get {
                return ResourceManager.GetString("OwnerCommand.AccessError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Хозяин! Пожалуйста, укажите единицу измерения для значения! {0}.
        /// </summary>
        internal static string OwnerCommand_ExtendPremium_IncorrectUnit {
            get {
                return ResourceManager.GetString("OwnerCommand.ExtendPremium.IncorrectUnit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Хозяин! Премиум доступ для {0} успешно продлен! {1}.
        /// </summary>
        internal static string OwnerCommand_ExtendPremium_IsSuccess {
            get {
                return ResourceManager.GetString("OwnerCommand.ExtendPremium.IsSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Хозяин! Данный участник сервера не зарегистрирован! {0}.
        /// </summary>
        internal static string OwnerCommand_ExtendPremium_MemberNotRegistered {
            get {
                return ResourceManager.GetString("OwnerCommand.ExtendPremium.MemberNotRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Хозяин! Ссылка на приложение установлена! {0}.
        /// </summary>
        internal static string OwnerCommand_SetApp_EmbedDescription {
            get {
                return ResourceManager.GetString("OwnerCommand.SetApp.EmbedDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Хорошо, хозяин! {0}.
        /// </summary>
        internal static string OwnerCommand_Shutdown_EmbedDescription {
            get {
                return ResourceManager.GetString("OwnerCommand.Shutdown.EmbedDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сборка.
        /// </summary>
        internal static string OwnerCommand_Status_FieldAssembly_Title {
            get {
                return ResourceManager.GetString("OwnerCommand.Status.FieldAssembly.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Дата запуска.
        /// </summary>
        internal static string OwnerCommand_Status_FieldLaunchDate_Title {
            get {
                return ResourceManager.GetString("OwnerCommand.Status.FieldLaunchDate.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}d, {1}h, {2}m, {3}s.
        /// </summary>
        internal static string OwnerCommand_Status_FieldWorkingHours_Description {
            get {
                return ResourceManager.GetString("OwnerCommand.Status.FieldWorkingHours.Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Время работы.
        /// </summary>
        internal static string OwnerCommand_Status_FieldWorkingHours_Title {
            get {
                return ResourceManager.GetString("OwnerCommand.Status.FieldWorkingHours.Title", resourceCulture);
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
        ///   Looks up a localized string similar to Ой, эта команда отключена!.
        /// </summary>
        internal static string RegisteredUserCommand_BugReport_Description_IsDisabled {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.BugReport.Description.IsDisabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Простите, нельзя отправлять пустой баг-репорт! Баг-репорт должен содержать описание и/или вложения и/или быть ответом на другое сообщение!.
        /// </summary>
        internal static string RegisteredUserCommand_BugReport_Description_IsEmpty {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.BugReport.Description.IsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Баг-репорт успешно отправлен!.
        /// </summary>
        internal static string RegisteredUserCommand_BugReport_Description_IsSuccess {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.BugReport.Description.IsSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Отключено!.
        /// </summary>
        internal static string RegisteredUserCommand_InfoMessagesInPM_Description_Disabled {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.InfoMessagesInPM.Description.Disabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Включено!.
        /// </summary>
        internal static string RegisteredUserCommand_InfoMessagesInPM_Description_Enabled {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.InfoMessagesInPM.Description.Enabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Отсутствуют.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldBanList_IsEmpty {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldBanList.IsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to не указана.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldBanList_ReasonNotSpecified {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldBanList.ReasonNotSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Список текущих банов:.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldBanList_Title {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldBanList.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Последний вход в приложение:.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldLastLogin_Title {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldLastLogin.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Отключены.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldOptionalNotifications_Disabled {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldOptionalNotifications.Disabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Включены.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldOptionalNotifications_Enabled {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldOptionalNotifications.Enabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Необязательные уведомления:.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldOptionalNotifications_Title {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldOptionalNotifications.Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Нет. Истек {0}.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldPremium_Expired {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldPremium.Expired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Есть. Истекает {0}.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldPremium_Expires {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldPremium.Expires", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Нет..
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldPremium_NotSet {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldPremium.NotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Премиум:.
        /// </summary>
        internal static string RegisteredUserCommand_Profile_FieldPremium_Title {
            get {
                return ResourceManager.GetString("RegisteredUserCommand.Profile.FieldPremium.Title", resourceCulture);
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
