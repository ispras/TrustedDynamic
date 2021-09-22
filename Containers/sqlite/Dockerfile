#Basic Image (ubuntu 21.04 предоставляет наиболее актуальную версию llvm/clang для работы AFL++)
FROM ubuntu:21.04 AS base_image

#Сведения о контейнере
LABEL maintainer="a.irkhin@gardatech.ru"
LABEL version="1.0"
LABEL description="Докер контейнер для фаззинга sqlite с применением AFL++ на Ubuntu 21.04"

#запускаем обновление репозитория и устанавливаем с временной зоной tzdata (необходимо для следующих пакетов)
RUN apt-get update && DEBIAN_FRONTEND="noninteractive" TZ="Russia/Moscow" apt-get install -y tzdata

#ставим пакеты для AFL++. На данный момент рекомендуется версия >=11, в репозитории 12я
RUN apt-get -y install build-essential python3-dev automake git flex bison libglib2.0-dev libpixman-1-dev python3-setuptools \ 
    lld llvm llvm-dev clang  

RUN apt-get -y install gcc-$(gcc --version|head -n1|sed 's/.* //'|sed 's/\..*//')-plugin-dev libstdc++-$(gcc --version|head -n1|sed 's/.* //'|sed 's/\..*//')-dev

#ставим пакеты для sqlite
RUN apt-get -y install tcl locales wget

RUN locale-gen en_US.UTF-8

#ставим AFL++
WORKDIR /afl
RUN wget https://github.com/AFLplusplus/AFLplusplus/archive/refs/tags/3.14c.tar.gz && \
    tar -xf 3.14c.tar.gz && \
    cd AFLplusplus-3.14c && \
    make && make install
    #make ASAN_BUILD=1 && make install рекомендуется компилировать с опцией ASAN_BUILD=1 при сборке для санитайзеров

#настройка переменных окружения
ENV CC=afl-cc
ENV CXX=afl-cc
ENV LD=afl-cc
#ENV AFL_USE_ASAN=1 для компиляции с ASAN санитайзером

#сборка sqlite
WORKDIR /sourcecode
RUN git clone https://github.com/sqlite/sqlite && cd sqlite && \
    ./configure --disable-shared && make sqlite3.c

#сборка фаззинг обёртки для sqlite
WORKDIR /fuzzingapp
RUN cp /sourcecode/sqlite/sqlite3.[ch] .; cp /sourcecode/sqlite/tool/fuzzershell.c .
RUN afl-cc -O3 -o fuzzershell -DSQLITE_THREADSAFE=0 -DSQLITE_ENABLE_LOAD_EXTENSION=0 -DSQLITE_DEBUG -DSQLITE_ENABLE_FTS4 -DSQLITE_ENABLE_RTREE -DSQLITE_OMIT_RANDOMNESS -I. fuzzershell.c sqlite3.c -ldl

#передача тестовых образцов в докер контейнер
RUN mkdir Testcases
COPY Testcases/ ./Testcases


CMD if [ -f "samples.dict" ]; then afl-fuzz -i Testcases/ -o out -T sqlite -x samples.dict -- ./fuzzershell --oom --autovacuum; else afl-fuzz -i Testcases/ -o out -T sqlite -x /afl/AFLplusplus-3.14c/dictionaries/sql.dict -- ./fuzzershell --oom --autovacuum; fi
