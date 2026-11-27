[ENG](README_ENG.md)
<h1 align="center">jellyfin-plugin-shikimori</h1>

# О плагине

Этот плагин загружает метаданные с сайта [Shikimori](https://shikimori.io)

Плагин поддерживает загрузку метадаты для аниме фильмов и сериалов. Загружает
рейтинг, описание, постеры, жанры, студии.

Для лучшего поиска можно указать в названии медиафайла или корневой папки тэг вида `[shikimori-12345]`,
где число - это id с сайта Shikimori.

# Установка

Этот форк публикуется как **Shikimori Next** в репозитории
[jellyfin-plugins](https://github.com/DevLn737/jellyfin-plugins).

1. Зайти в Администрирование -> Панель управления -> Плагины -> Управление репозиториями -> Новый репозиторий.
2. Ввести название, в поле "URL репозитория" ввести:
```
https://devln737.github.io/jellyfin-plugins/manifest.json
```
3. Установить Shikimori Next через каталог.

# Сборка

1. Установить [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
2. Склонировать этот репозиторий
3. Скомпилировать плагин следующей командой:
```bash
dotnet publish Jellyfin.Plugin.Shikimori/Jellyfin.Plugin.Shikimori.csproj --configuration Release --output artifacts/publish
```
4. Создать директорию `shikimori` в папке с плагинами джелифина (Она находится в [основной директории jellyfin](https://jellyfin.org/docs/general/administration/configuration/#data-directory))
5. Переместить `Jellyfin.Plugin.Shikimori.dll`, `Newtonsoft.Json.dll` и `meta.json` из `artifacts/publish` в созданную директорию. Остальные DLL из publish принадлежат серверу и не входят в пакет плагина.

# Shikimori Next и Jellyfin 12.2

Версия 6.1.0.0 предназначена для Jellyfin 12.2 и .NET 10. С версии 6.1.0.0 у
плагина своё имя и GUID: **Shikimori Next**, `42730b4b-b30e-45dc-bee0-8f351eadb690`,
поэтому в каталоге сервера он не конфликтует с оригинальным Shikimori. Всё, от чего
зависит библиотека, осталось прежним: провайдер метаданных по-прежнему называется
`Shikimori`, у элементов сохраняются идентификаторы `Shikimori`, настройки лежат в
`plugins/configurations/Jellyfin.Plugin.Shikimori.xml`.

Переход с оригинального плагина (или с версии 6.0.0.0 и старше этого форка):
удалить Shikimori в Панели управления -> Плагины, перезапустить Jellyfin,
установить Shikimori Next из каталога и перезапустить ещё раз. Настройки
библиотек, сохранённые ID и настройки плагина сохраняются. Не держать оба плагина
одновременно: они регистрируют один и тот же провайдер.

Проверенные команды тестирования:

```bash
dotnet test Jellyfin.Plugin.Shikimori.sln --configuration Release
```

Тесты проверяют восстановление старых XML-настроек, поиск ID, GUID и
имя файла настроек, обработку метаданных и результатов поиска фильмов/сериалов без
потери TMDb, TVDB, AniList, AniDB, IMDb и пользовательских идентификаторов.
