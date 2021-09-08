FROM ubuntu:18.04 AS base_image

WORKDIR /build

RUN apt update && \
    apt-get -y install libunwind8 libicu60 curl git clang-3.9 cmake make libc6-dev libssl-dev libkrb5-dev libcurl4-openssl-dev zlib1g-dev locales gcc wget apt-transport-https \
    build-essential python3-dev automake git flex bison libglib2.0-dev libpixman-1-dev python3-setuptools lld llvm llvm-dev clang

RUN apt-get -y install gcc-$(gcc --version|head -n1|sed 's/.* //'|sed 's/\..*//')-plugin-dev libstdc++-$(gcc --version|head -n1|sed 's/.* //'|sed 's/\..*//')-dev

RUN locale-gen en_US.UTF-8

RUN git clone --depth 1 -b release/3.1 --single-branch --progress --verbose https://github.com/dotnet/corefx.git
RUN cd corefx && \
    ./build.sh -allconfigurations --configuration release

WORKDIR /afl
RUN wget https://github.com/AFLplusplus/AFLplusplus/archive/refs/tags/3.14c.tar.gz && \
    tar -xvf 3.14c.tar.gz && \
    cd AFLplusplus-3.14c && \
    make

WORKDIR /app
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-3.1 dotnet-runtime-3.1

ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global SharpFuzz.CommandLine

COPY CoreFX.csproj NuGet.Config ./

RUN package_version=$(find /build/corefx/artifacts/packages/ -name "Microsoft.Private.CoreFx.NETCoreApp.*.nupkg" | head -n 1 | grep -Po '\d\.\d\.\d-dev\.\d+\.\d' | awk '{$1=$1};1') && sed -i "s/package_version_for_replace/${package_version}/g" CoreFX.csproj

RUN dotnet restore -r linux-x64

ARG FUZZ_TARGET_FOLDER

COPY ${FUZZ_TARGET_FOLDER}/ ./

RUN dotnet publish -r linux-x64 --no-restore -c release -o out

ENV AFL_SKIP_BIN_CHECK=true
ENV SHARPFUZZ_PRINT_INSTRUMENTED_TYPES=true
ENV AFL_I_DONT_CARE_ABOUT_MISSING_CRASHES=true

ARG FUZZ_TARGET_LIBRARY

RUN sharpfuzz out/${FUZZ_TARGET_LIBRARY}

CMD if [ -f "samples.dict" ]; then /afl/AFLplusplus-3.14c/afl-fuzz -i Testcases -o findings -t 5000 -m 10000 -x samples.dict ./out/CoreFX; else /afl/AFLplusplus-3.14c/afl-fuzz -i Testcases -o findings -t 5000 -m 10000 ./out/CoreFX; fi
