# Yuko / Юко (Discord File Loader Bot)
Бот предназначен для скачивания вложений с каналов Discord-а, доступных боту и вам.
## Yuko Client
Стандартный клиент, позволяет скачивать вложения из сообщений посредством правил.
### Что за правила?
Правила описывают из каких сообщений нужно получить вложения. Правила содержат следующие настройки: канал, тип получения (указывает из каких сообщений достать вложения), id сообщения, количество сообщений для обработки.
### Как это работает?
1) Вы в клиентском приложении задаете правила
2) Бот вытаскивает ссылки на эти вложения (если они есть) и отправляет эти ссылки вам в приложение
3) Вы скачиваете файлы по этим ссылкам (можно выбрать в каком количестве скачивать файлы за условную единицу времени) в приложении
## Yuko Collection Client
Клиент, для скачивания вложений из сообщений сгруппированных по коллекциям.
### Команды для работы с сообщениями и коллекциями
**Эти команды доступны для зарегистрированных и не забаненых на сервере (где используется команда) пользователей!**
Команда|Алиасы|Описание
-|-|-
add|-|Добавляет вложенное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию
add-by-id|-|Добавляет сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию
add-collection|-|Создает новую коллекцию
add-range|-|Добавляет сообщения (имеющие вложения) из заданного промежутка в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию
clear-collection|-|Удаляет все сообщения из коллекции
remove-collection|rm-collection|Удаляет коллекцию
remove-item|rm-item|Удалить сообщение из коллекции
rename-collection|-|Переименовывает указанную коллекцию
show-collections|collections|Показывает список коллекций
show-items|items|Показывает последние 25 сообщений коллекции
## Команды
Команда|Алиасы|Описание|Владелец бота|Aдминистратор</br>сервера</br>(гильдии)|Остальные</br>участники</br>сервера</br>(гильдии)
-|-|-|-|-|-
add-command-response|add-response|Отключает сообщение об успешности выполнения команды [add](#команды-для-работы-с-сообщениями-и-коллекциями) на сервере, взамен сообщение будет приходить в ЛС|:heavy_check_mark:|:heavy_check_mark:|:x:
app|-|Ссылка на скачивание актуальной версии клиента|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
ban|-|Запрещает пользователю скачивать с этого сервера (гильдии)|:heavy_check_mark:|:heavy_check_mark:|:x:
ban-reason|reason|Причина бана на текущем сервере|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
bag-report|report|Позволяет сообщить об ошибке|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
info-message-pm|-|Отключает или включает отправку информационных сообщений в личные сообщения (работает для команды [add](#команды-для-работы-с-сообщениями-и-коллекциями))|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
member-ban-reason|m-reason|Причина бана участника сервера|:heavy_check_mark:|:heavy_check_mark:|:x:
password-reset|password|Сброс пароля|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
register|reg|Регистрация|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
set-app|-|Устанавливает новую ссылку для команды: app|:heavy_check_mark:|:x:|:x:
set-art-channel|-|Устанавливает канал для поиска сообщений при использовании комманд категории "Управление коллекциями"|:heavy_check_mark:|:heavy_check_mark:|:x:
settings|-|Данные для подключения|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:
shutdown|sd|Выключить бота|:heavy_check_mark:|:x:|:x:
status|-|Отображает сборку, дату запуска и время работы бота|:heavy_check_mark:|:x:|:x:
unban|-|Удаляет пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера (гильдии))|:heavy_check_mark:|:heavy_check_mark:|:x:
## Дополнительные сведения
1) Для удаления сообщения бота в ЛС поставьте реакцию ` :negative_squared_cross_mark: ` на это сообщение
2) Для получения ссылки для добавления бота на сервер обращаться к владельцу репозитория (Discord: Hlebushek#4209)
