# Фаззинг стандартных библиотек dotnet

Проект по парсингу стандартных классов dotnet. Для фаззинга применяется AFL и [sharpfuzz](https://github.com/Metalnem/sharpfuzz)

Зависимости:
* Docker

## Формирование цели

Для формирования цели необходимо создать директорию в которой расположить следующие элементы:
* Testcases - [обязательно] директория с входным корпусом данных для фаззинга
* Program.cs - [обязательно] код, реализующий цель для фаззинга
* samples.dict - [не обязательно] словарь для фаззинга (если словать будет отсутствовать, то AFL будет запущен без `-x` параметра)

## Описание сборки Dockerfile

1. Производится сборка CoreFX (/build/corefx)
2. Производится сборка AFL (/afl/afl-2.52b)
3. Устанавливается dotnet
4. Устанавливается SharpFuzz (/root/.dotnet/tools)
5. В CoreFX.csproj подставляется номер сборки CoreFX с шага 1 (/app/CoreFX.csproj)
6. Восстанавливаем все зависимости для сборки цели (указаны в NuGet.Config). Зависимости беруться из сборки CoreFX с шага 1 (/app/NuGet.Config)
7. Производим сборку цели Program.cs (/app/Program.cs)
8. Инструментируем необходимую нам библиотеку из CoreFX (/app/out/...)
9. Запускаем фаззинг
   * /app/out/CoreFX - собранная цель
   * /app/Testcases - директория с входным корпусом данных для фаззинга
   * /app/findings - результаты работы AFL

Аргументы сборки:
* FUZZ_TARGET_LIBRARY - [обязательно] название библиотеки для инструментации
* FUZZ_TARGET_FOLDER - [обязательно] директория цели в которой располагаются Testcases, Program.cs, samples.dict

## Запуск фаззинга

Для примера возьмем цель XmlSerializer.Deserialize.

Производим сборку Docker контейнера:
``` bash
docker build -f Dockerfile --build-arg FUZZ_TARGET_LIBRARY=System.Private.Xml.dll --build-arg FUZZ_TARGET_FOLDER=XmlSerializer.Deserialize --tag=xmlserializer.deserialize .
```

Запускаем фаззинг:
``` bash
docker run -ti -v $(pwd)/findings:/app/findings xmlserializer.deserialize
```

Результаты фаззинга должны сохраниться в локальной директории findings, где был запущен контейнер.

## Полезные ссылки 

* https://github.com/Metalnem/sharpfuzz
* https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/dogfooding.md
* https://github.com/dotnet/corefx/blob/release/3.1/Documentation/building/unix-instructions.md#user-content-linux
* https://github.com/Metalnem/sharpfuzz/blob/master/docs/fuzzing-dotnet-core.md
* https://github.com/Metalnem/sharpfuzz-samples
