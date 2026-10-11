[ENG](README_ENG.md)
<h1 align="center">jellyfin-plugin-shikimori</h1>

# О плагине

Этот плагин загружает метаданные с сайта [Shikimori](https://shikimori.io)

Плагин поддерживает загрузку метадаты для аниме фильмов и сериалов. Загружает
рейтинг, описание, постеры, жанры, студии.

Для лучшего поиска можно указать в названии медиафайла или корневой папки тэг вида `[shikimori-12345]`,
где число - это id с сайта Shikimori.

# Установка

1. Зайти в Администрирование -> Панель управления -> Плагины -> Управление репозиториями -> Новый репозиторий.
2. Ввести название, в поле "URL репозитория" ввести:
```
https://raw.githubusercontent.com/DevLn737/jellyfin-plugin-shikimori/main/manifest.json
```
3. Установить плагин через каталог.

# Сборка

1. Установить [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
2. Склонировать этот репозиторий
3. Скомпилировать плагин следующей командой:
```bash
dotnet publish Jellyfin.Plugin.Shikimori/Jellyfin.Plugin.Shikimori.csproj --configuration Release --output artifacts/publish
```
4. Создать директорию `shikimori` в папке с плагинами джелифина (Она находится в [основной директории jellyfin](https://jellyfin.org/docs/general/administration/configuration/#data-directory))
5. Переместить `Jellyfin.Plugin.Shikimori.dll`, `Newtonsoft.Json.dll` и `meta.json` из `artifacts/publish` в созданную директорию. Остальные DLL из publish принадлежат серверу и не входят в пакет плагина.

# Совместимость 12.2

Версия 6.0.0.0 предназначена для Jellyfin 12.2 и .NET 10. Она сохраняет GUID,
ключи настроек, имя `Jellyfin.Plugin.Shikimori.xml`, поиск по Shikimori ID и
сохранение идентификаторов других провайдеров из версии 5.0.1.0. Перед заменой
бинарников остановить Jellyfin; сохранить файл настроек в `plugins/configurations`
и убрать старую директорию бинарников Shikimori.

Публичный `manifest.json` содержит только опубликованные релизы. Локальную сборку
6.0.0.0 устанавливать из проверенного пакета. Добавлять её в публичный каталог
можно после публикации ZIP и проверки его адреса и контрольной суммы.

Проверенные команды тестирования:

```bash
dotnet test Jellyfin.Plugin.Shikimori.sln --configuration Release
```

Тесты проверяют восстановление старых XML-настроек, поиск ID, неизменность GUID и
имени настроек, обработку метаданных и результатов поиска фильмов/сериалов без
потери TMDb, TVDB, AniList, AniDB, IMDb и пользовательских идентификаторов.
