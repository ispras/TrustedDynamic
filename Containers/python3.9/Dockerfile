# Base container with aflplusplus
FROM fedora:33 AS afl
ARG AFL_VERSION=3.12c
WORKDIR /root/afl/
RUN dnf install -y make gcc clang llvm llvm-devel lld gcc-plugin-devel wget
RUN wget https://github.com/AFLplusplus/AFLplusplus/archive/refs/tags/$AFL_VERSION.tar.gz && tar -zxvf $AFL_VERSION.tar.gz && \
    PREFIX=/usr make --directory=AFLplusplus-$AFL_VERSION && PREFIX=/usr make install --directory=AFLplusplus-$AFL_VERSION && \
    rm -rf /root/afl/*

# Stage for build python
FROM afl AS build
ARG PYTHON_VERSION=3.9.4
WORKDIR /root/python
RUN dnf install dnf-plugins-core xz -y
RUN dnf builddep python3 -y

RUN wget https://www.python.org/ftp/python/$PYTHON_VERSION/Python-$PYTHON_VERSION.tar.xz && tar -xvf Python-$PYTHON_VERSION.tar.xz
WORKDIR Python-$PYTHON_VERSION
RUN export CC=afl-clang-fast && \
    export CXX=afl-clang-fast++ && \
    # Флаги взяты из сборки python для fedora 33
    export CFLAGS="-O2  -fexceptions -g -grecord-gcc-switches -pipe -Wall -Werror=format-security -Wp,-D_FORTIFY_SOURCE=2 -Wp,-D_GLIBCXX_ASSERTIONS  -fstack-protector-strong   -m64 -mtune=generic -fasynchronous-unwind-tables -fstack-clash-protection -fcf-protection -fuse-ld=lld -D_GNU_SOURCE -fPIC -fwrapv  -Wno-unused-value -Wno-empty-body -Qunused-arguments" && \
    export CFLAGS_NODIST="-fexceptions -g -grecord-gcc-switches -pipe -Wall -Werror=format-security -Wp,-D_FORTIFY_SOURCE=2 -Wp,-D_GLIBCXX_ASSERTIONS -specs=/usr/lib/rpm/redhat/redhat-hardened-cc1 -fstack-protector-strong -specs=/usr/lib/rpm/redhat/redhat-annobin-cc1  -m64 -mtune=generic -fasynchronous-unwind-tables -fstack-clash-protection -fcf-protection -fuse-ld=lld -D_GNU_SOURCE -fPIC -fwrapv " && \
    export CXXFLAGS=" -O2  -fexceptions -g -grecord-gcc-switches -pipe -Wall -Werror=format-security -Wp,-D_FORTIFY_SOURCE=2 -Wp,-D_GLIBCXX_ASSERTIONS  -fstack-protector-strong   -m64 -mtune=generic -fasynchronous-unwind-tables -fstack-clash-protection -fcf-protection -fuse-ld=lld -D_GNU_SOURCE -fPIC -fwrapv  -Wno-unused-value -Wno-empty-body -Qunused-arguments" && \
    export CPPFLAGS="$(pkg-config --cflags-only-I libffi) -fuse-ld=lld  -Wno-unused-value -Wno-empty-body -Qunused-arguments" && \
    export OPT="-O2  -fexceptions -g -grecord-gcc-switches -pipe -Wall -Werror=format-security -Wp,-D_FORTIFY_SOURCE=2 -Wp,-D_GLIBCXX_ASSERTIONS  -fstack-protector-strong   -m64 -mtune=generic -fasynchronous-unwind-tables -fstack-clash-protection -fcf-protection -fuse-ld=lld -D_GNU_SOURCE -fPIC -fwrapv" && \
    export CFLAGS="$CFLAGS $(pkg-config --cflags openssl)" && \
    export LDFLAGS=" -Wl,-z,relro -Wl,--as-needed  -Wl,-z,now -fuse-ld=lld -g $(pkg-config --libs-only-L openssl)" && \
    export LDFLAGS_NODIST="-Wl,-z,relro -Wl,--as-needed  -Wl,-z,now -specs=/usr/lib/rpm/redhat/redhat-hardened-ld  -fuse-ld=lld -g $(pkg-config --libs-only-L openssl)" && \
    export RANLIB=llvm-ranlib && \
    export AR=llvm-ar && \
    export AS=llvm-as && \
    #export AFL_LLVM_LAF_ALL=1 && \
    export AFL_USE_UBSAN=1 && \
    export AFL_USE_ASAN=1 && \
    export ASAN_OPTIONS=detect_leaks=0 && \
    ./configure && make -j8

# final stage
FROM afl
ARG PYTHON_VERSION=3.9.4
WORKDIR /root/fuzz/
COPY --from=build /root/python/Python-$PYTHON_VERSION ./python