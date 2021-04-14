
# Сборка подсистемы coreclr из состава .NET5.

Устанавливается компилятор clang-12, выполняется сборка AFLplusplus (протестировано на 3.12c), подменяются компиляторы для сборки собственно coreclr на -lto -компиляторы из состава AFL++, выполняется сборка coreclr с формированием слования лексем на этапе компиляции (AUTODICTIONARY из состава последнийх AFL++ - для самопроверки, что всё работает)

*docker build --build-arg cuid=$(id -u) --build-arg cgid=$(id -g) --build-arg cuidname=$(id -un) --build-arg cgidname=$(id -gn) -t net5_coreclr -f Dockerfile_net5_coreclr.txt .*

# Команда запуска контейнера

*docker run -it --name=net5 net5_coreclr*

# Команда тестового запуска фаззинга инструментированного бинарника дизассемблера (всё должно завестить, будут находится разумные сэмплы (MZ в начале))

 *mkdir -p test/in && echo 111 > test/in/sam && ../AFLplusplus/afl-fuzz -i test/in -o out -x clr.dict -- artifacts/bin/coreclr/Linux.x64.Debug/ildasm @@*
