# Yuko / Юко (Discord File Loader Bot)
Бот предназначен для скачивания вложений с каналов Discord-а, доступных боту и вам.
## Содержание
- [Yuko Client](#yuko-client)
  - [Что за правила?](#что-за-правила)
  - [Как это работает?](#как-это-работает)
  - **[ИСПОЛЬЗОВАНИЕ ПРИЛОЖЕНИЯ](#использование-приложения)**
- [Yuko Collection Client](#yuko-collection-client)
  - [Команды для работы с сообщениями и коллекциями](#команды-для-работы-с-сообщениями-и-коллекциями)
  - **[ИСПОЛЬЗОВАНИЕ ПРИЛОЖЕНИЯ](#использование-приложения-1)**
- [Команды](#команды)
  - [Дополнительные возможности](#дополнительные-возможности)
- [Дополнительные сведения](#дополнительные-сведения)
- [Первый запуск приложения](#первый-запуск-приложения)
## Yuko Client
Стандартный клиент, позволяет скачивать вложения из сообщений посредством правил.
### Что за правила?
Правила описывают из каких сообщений нужно получить вложения. Правила содержат следующие настройки: канал, тип получения (указывает из каких сообщений достать вложения), id сообщения, количество сообщений для обработки.
### Как это работает?
1) Вы в клиентском приложении задаете правила
2) Бот вытаскивает ссылки на эти вложения (если они есть) и отправляет эти ссылки вам в приложение
3) Вы скачиваете файлы по этим ссылкам (можно выбрать в каком количестве скачивать файлы за условную единицу времени) в приложении
### Использование приложения
Запускаем приложение и логинемся ***(YukoClient.exe)***.  
***Если вы впервые используете любое из двух приложений, сначала прочитайте главу: [Первый запуск приложения](#первый-запуск-приложения).***  
После того как мы залогинились открывается главное окно приложения.
#### Описание главного окна
![Главное окно](https://user-images.githubusercontent.com/63193749/206501825-79196e21-033f-445e-9add-2b5ceac05342.png)
1) Шапка программы. Слева находится информация о текущем пользователе, справа - кнопка настроек приложения.
2) Список серверов, на которых есть данный пользователь и к которым бот имеет доступ
3) Список правил для выбранного сервера
4) Список ссылок для выбранного сервера и выполненных (смотрите меню списка правил) правил
#### Описание окна добавления правила
![Окно добавления правила](https://user-images.githubusercontent.com/63193749/206503087-5fe3eaaa-b528-4af3-93c7-83fed10d6240.png)  
- Канал - канал, с которого будут браться сообщения для вытаскивания ссылок  
- Получить - указывает из каких сообщений взятых с канала (указанного выше) следует вытащить ссылки (под полем отображается описание режима)  
- Сообщение (Id) - id сообщения, из которого нужно достать ссылки  
- Количество - количество сообщений, из которых надо вытащить ссылки
## Yuko Collection Client
Клиент, для скачивания вложений из сообщений сгруппированных по коллекциям.
### Команды для работы с сообщениями и коллекциями
**Эти команды доступны для зарегистрированных и не забаненых на сервере (где используется команда) пользователей!**
Команда|Алиасы|Описание
-|-|-
add|-|Добавляет вложенное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию
add-by-id|-|Добавляет сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию
add-collection|-|Создает новую коллекцию
clear-collection|-|Удаляет все сообщения из коллекции
end|-|Задает конечное сообщение (входит в промежуток) и добавляет промежуток в заданную коллекцию
remove-collection|rm-collection|Удаляет коллекцию
remove-item|rm-item|Удалить сообщение из коллекции
rename-collection|-|Переименовывает указанную коллекцию
show-collections|collections|Показывает список коллекций
show-items|items|Показывает последние 25 сообщений коллекции
start|-|Задает начальное сообщение (входит в промежуток)
### Использование приложения
Запускаем приложение и логинемся ***(YukoCollectionClient.exe)***.  
***Если вы впервые используете любое из двух приложений, сначала прочитайте главу: [Первый запуск приложения](#первый-запуск-приложения).***  
После того как мы залогинились открывается главное окно приложения.
#### Описание главного окна
![Главное окно](https://user-images.githubusercontent.com/63193749/206527915-1b013330-262e-40d4-af39-1e0f7dbca631.png)
1) Шапка программы. Слева находится информация о текущем пользователе, справа - кнопка настроек приложения.
2) Поле для фильтрации списка коллекций по названию
3) Список коллекций
4) Список сообщений выбранной коллекции
5) Список ссылок из сообщений выбранной коллекции 
## Команды
Команда|Алиасы|Описание|Владелец бота|Aдминистратор</br>сервера</br>(гильдии)|Остальные</br>участники</br>сервера</br>(гильдии)
-|-|-|-|-|-
add-command-response|add-response|Отключает сообщение об успешности выполнения команды [add](#команды-для-работы-с-сообщениями-и-коллекциями) на сервере, взамен сообщение будет приходить в ЛС|:heavy_check_mark:|:heavy_check_mark:|:x:
app|-|Ссылка на скачивание актуальной версии клиента|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
ban|-|Запрещает пользователю скачивать с этого сервера (гильдии)|:heavy_check_mark:|:heavy_check_mark:|:x:
ban-reason|reason|Причина бана на текущем сервере|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
bag-report|-|Позволяет сообщить об ошибке|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
info-message-pm|-|Отключает или включает отправку информационных сообщений в личные сообщения (работает для команды [add](#команды-для-работы-с-сообщениями-и-коллекциями))|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
member-ban-reason|m-reason|Причина бана участника сервера|:heavy_check_mark:|:heavy_check_mark:|:x:
password-reset|password|Сброс пароля|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
register|reg|Регистрация|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
set-app|-|Устанавливает новую ссылку для команды: app|:heavy_check_mark:|:x:|:x:
set-art-channel|-|Устанавливает канал для поиска сообщений при использовании комманд категории "Управление коллекциями"|:heavy_check_mark:|:heavy_check_mark:|:x:
set-premium|-|Предоставляет пользователю [дополнительные возможности](#дополнительные-возможности)|:heavy_check_mark:|:x:|:x:
settings|-|Данные для подключения|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
shutdown|sd|Выключить бота|:heavy_check_mark:|:x:|:x:
status|-|Отображает сборку, дату запуска и время работы бота|:heavy_check_mark:|:x:|:x:
unban|-|Удаляет пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера (гильдии))|:heavy_check_mark:|:heavy_check_mark:|:x:
### Дополнительные возможности
1) Вытаскивание ссылок на вложения при добавлении сообщений в коллекцию, а не по запросу [клиента](#yuko-collection-client)
## Дополнительные сведения
1) Для удаления сообщения бота в ЛС поставьте реакцию ` :negative_squared_cross_mark: ` на это сообщение
2) Для получения ссылки для добавления бота на сервер обращаться к владельцу репозитория (Discord: Hlebushek#4209)
## Первый запуск приложения
Запускаем нужное нам приложение (YukoClient.exe или YukoCollectionClient.exe). Перед нами появится окно входа, для получения логина и пароля выполняем команду `reg` бота, после чего логин и пароль придет в личные сообщения. Вводим данные, если мы нажмем на кнопку "Войти" нам выдастся следующее сообщение "Сначала настройте программу! Значок в правом нижнем углу", следуем этому указанию (если у вас не появляется такое сообщение или появляется другое, то все равно выполните действия описанные далее).  
Нажимаем на значок шестерёнки в правом нижнем углу. Открывается окно с настройками, все параметры кроме параметров в разделе "Подключение" настраиваем под свое усмотрение (либо не трогаем).  
Для настройки параметров в разделе "Подключение" выполняем команду `settings` бота и заполняем поля соответствующими значениями. Нажимаем кнопку "Применить" и выполняем вход.
