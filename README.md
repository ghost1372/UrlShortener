[![Total downloads](https://img.shields.io/github/downloads/ghost1372/UrlShortener/total.svg)](https://github.com/ghost1372/UrlShortener/releases)
[![GitHub version](https://badge.fury.io/gh/ghost1372%2FUrlShortener.svg)](https://badge.fury.io/gh/ghost1372%2FUrlShortener)

# UrlShortener
A simple program to shorten links

![](https://file.soft98.ir/uploads/mahdi72/2018/10/26_10-zxc.png)


## If you know the other shortening service that supports API, let us know!

Any contributions will be accepted

# Linux and Mac Version
CrossPlatform version Written by DotNet Core<br>
you can access Linux and Mac version via two way!<br>
1.Precompiled Binary [Download it from [here](https://github.com/ghost1372/UrlShortener/releases))]<br>
2.Compile Project on Linux or Mac

## How to Run downloaded binary on linux or mac?
unzip binary and go to directoy then execute following command:
```
./UrlShortenerCrossPlatform.dll
```
#### Simple usage
-s is optional argument that is a integer for selecting service (you can find services in help menu)<br>
-l is required argument that is long url you want to shot it.
```
./UrlShortenerCrossPlatform.dll -s 3 -l https://github.com/ghost1372
```
## Compile Project on Linux or Mac
First Step: you must install prerequisites library<br>
Second Step: clone project
```
git clone https://github.com/ghost1372/UrlShortener.git
cd UrlShortener
cd UrlShortenerCrossPlatform
```
Third Step: Compile it
```
dotnet build -c release
```


### Prerequisites for .NET Core on Linux
#### Ubuntu
Ubuntu distributions require the following libraries installed:<br>

liblttng-ust0<br>
libcurl3<br>
libssl1.0.0<br>
libkrb5-3<br>
zlib1g<br>
libicu52 (for 14.x)<br>
libicu55 (for 16.x)<br>
libicu57 (for 17.x)<br>
libicu60 (for 18.x)<br>
libunwind8<br>
libuuid1<br>

#### CentOS and Fedora
CentOS distributions require the following libraries installed:<br>

lttng-ust<br>
libcurl<br>
openssl-libs<br>
krb5-libs<br>
libicu<br>
zlib<br>
libunwind<br>
libuuid<br>

Fedora users: If your openssl's version >= 1.1, you'll need to install compat-openssl10.

### Prerequisites for .NET Core on Mac
Download and install the .NET Core SDK from [.NET Downloads](https://www.microsoft.com/net/download/core)).


