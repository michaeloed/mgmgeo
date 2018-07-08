set /p subver=Enter a subversion (if applicable):%=%
del /F readme.html
copy /Y InstallationSRC.xml Installation.xml
InstallationDirCreator.exe %subver%
copy /Y InstallationEXE.xml Installation.xml
InstallationDirCreator.exe %subver%
