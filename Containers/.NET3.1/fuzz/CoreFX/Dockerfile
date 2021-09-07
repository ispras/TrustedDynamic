FROM ubuntu:18.04 AS base_image

WORKDIR /build

RUN apt update && \
    apt-get -y install libunwind8 libicu60 curl git clang-3.9 cmake make libc6-dev libssl-dev libkrb5-dev libcurl4-openssl-dev zlib1g-dev locales gcc wget apt-transport-https

RUN apt-get download libssl1.0-dev

RUN locale-gen en_US.UTF-8

RUN git clone --depth 1 -b release/3.1 --single-branch --progress --verbose https://github.com/dotnet/corefx.git
RUN cd corefx && \
    ./build.sh -allconfigurations --configuration release

WORKDIR /afl
RUN wget http://lcamtuf.coredump.cx/afl/releases/afl-latest.tgz && \
    tar -xvf afl-latest.tgz && \
    cd afl-2.52b && \
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

CMD if [ -f "samples.dict" ]; then /afl/afl-2.52b/afl-fuzz -i Testcases -o findings -t 5000 -m 10000 -x samples.dict ./out/CoreFX; else /afl/afl-2.52b/afl-fuzz -i Testcases -o findings -t 5000 -m 10000 ./out/CoreFX; fi
