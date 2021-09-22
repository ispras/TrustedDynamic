# Фаззинг стандартных функций sqlite

Проект по фаззингу обработки SQL команд в sqlite. Для фаззинга применяется AFL++

Зависимости:
* Docker

Результаты тестирования расположены в папке [results](results)

## Формирование цели

Для формирования цели необходимо создать директорию в которой расположить следующие элементы:
* Testcases - [обязательно] директория с входным корпусом данных для фаззинга и анализа покрытия
* samples.dict - [не обязательно] словарь для фаззинга (если словать будет отсутствовать, то AFL++ будет запущен с обычным SQL словарём из коробки)

## Описание сборки Dockerfile

1. Производится сборка AFL++ (/afl/AFLplusplus-3.14c)
2. Производится сборка sqlite (/sourcecode)
3. Производится сборка модуля fuzzershell, реализующий цель для фаззинга
4. Запускаем фаззинг
   * /sourcecode/sqlite - исходники и собранный код sqlite
   * /fuzzingapp - собранный sqlite и его модуль fuzzershell
   * /fuzzingapp/Testcases - директория с входным корпусом данных для фаззинга
   * /fuzzingapp/out - результаты работы AFL


## Запуск фаззинга

Производим сборку Docker контейнера:
``` bash
docker build -f Dockerfile --tag=sqlite .
```

Запускаем фаззинг:
``` bash
docker run -ti -v $(pwd)/findings:/fuzzingapp/out sqlite
```

Результаты фаззинга должны сохраниться в локальной директории findings, где был запущен контейнер.

## Сборка покрытия

Производим сборку Docker контейнера:
``` bash
docker build -f Dockerfile-for-coverage --tag=sqlite-coverage .
```

Запускаем анализ покрытия:
``` bash
docker run -ti -v $(pwd)/coverage:/coverageapp/report-sqlite3 sqlite-coverage
```

## Особенности фаззинг сборки
Флаги при сборке sqlite
```
-DSQLITE_DEBUG для включения assert() проверок
-DSQLITE_OMIT_RANDOMNESS выключение ГПСЧ (иначе AFL++ будет считать каждый запуск уникальным)
-DSQLITE_THREADSAFE=0 и -DSQLITE_ENABLE_LOAD_EXTENSION=0 для корректной компановки
-DSQLITE_ENABLE_FTS4 для сборки с поддержкой FTS4 дополнения (полнотекстовый поиск), может быть необходимо для тестирования большего функционала
-DSQLITE_ENABLE_RTREE для сборки с поддержкой R*Tree модуля, может быть необходимо для тестирования большего функционала
-ldl обозначение библиотеки для компоновщика
```

Фаззинг обёртка имеет несколько дополнительных опций:
```
  --autovacuum          Включение AUTOVACUUM режима
  --database FILE       Использование файла базы данных вместо ОЗУ
  --disable-lookaside   Выключение lookaside памяти (описание в ссылках)
  --heap SZ MIN         Размер кучи
  --help                Вывод справочной информации
  --lookaside N SZ      Настройка lookaside
  --oom                 Запуск каждого теста несколько раз с симуляцией out of memory состояния
  --pagesize N          Настройка размера страницы
  --pcache N SZ         Настройка размера pagecache
  -q                    Уменьшение вывода в консоль
  --quiet               Уменьшение вывода в консоль (альтерантивная запись -q)
  --scratch N SZ        Настройка scratch памяти
  --unique-cases FILE   Запись уникальных тестовых случаев в файл FILE
  --utf16be             Установка кодировки UTF-16BE
  --utf16le             Установка кодировки UTF-16LE
  -v                    Увеличение вывода в консоль
  --verbose             Увеличение вывода в консоль (альтерантивная запись -v)
```
В данном случае используется только:
```
  --autovacuum          Тестирование как будет работать сжатие базы данных
  --oom                 Для тестирования в более сложных условиях
  --unique-cases FILE   Запись уникальных тестовых случаев в файл FILE (для возможного сравнения результатов)
  -v                    Для вывода большего количества информации о выполнении теста (при сборке покрытия)
```
Некоторые константы
* По умолчанию у `OOM_MAX` (определяет максимальное количество итераций при использовании --oom) стоит значение `625`

## Полезные ссылки 

* https://github.com/AFLplusplus/AFLplusplus - репозиторий AFL++
* https://github.com/sqlite/sqlite - репозиторий sqlite
* https://github.com/sqlite/sqlite/blob/master/tool/fuzzershell.c - код фаззинг обёртки
* https://www.sqlite.org/afl/doc/trunk/README.md - описание фаззинг тестирования sqlite через fuzzershell
* https://github.com/ispras/crusher/
* https://www.sqlite.org/malloc.html - описание работы с памятью (lookaside)
* https://www.sqlite.org/rtree.html - описание R*Tree модуля
* https://www.sqlite.org/fts3.html - описание FTS4 дополнения

Данный набор контейнеров подготовлен компанией ООО "Гарда Технологии"