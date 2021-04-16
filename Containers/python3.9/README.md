# Сборка python 3.9 с помощью afl++

Команда сборки:
```shell
docker build -t --build-arg AFL_VERSION=3.12c --build-arg PYTHON_VERSION=3.9.4 python_afl ./
```
Аргументы `AFL_VERSION` и `PYTHON_VERSION` - опциональны. 

Комнада для тестового запуска фаззинга

```shell
mkdir "in"
echo a=1 > in/1
mkdir out
docker run -v $PWD/in:/in -v $PWD/out:/out --privileged -it --name=python_fuzz python_afl afl-fuzz -i /in -o /out -m none -- ./python/python @@
```