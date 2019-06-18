call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat x86"
set path=tools;%path%;C:\Windows\Microsoft.NET\Framework\v4.0.30319;c:\windows\system32
set
nant > build.txt 2>&1

