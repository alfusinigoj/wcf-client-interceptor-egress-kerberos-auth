IF "%1"=="" @GOTO NO_ARG_PROVIDED
%INFO% Argument is the minor version of the publising package

nuget pack RouteServiceIwaWcfInterceptor.csproj -Version 1.0.%1-beta -OutputDirectory _publish

IF %ERRORLEVEL% NEQ 0 GOTO ERROR

nuget add _publish/Pivotal.WcfClient.Kerberos.Interceptor.1.0.%1-beta.nupkg -Source c:\MyLocalNugetRepo

IF %ERRORLEVEL% NEQ 0 GOTO ERROR
IF %ERRORLEVEL% EQU 0 GOTO SUCCESS

:NO_ARG_PROVIDED
@echo "*** No Argument(minor version) provided ***"
exit /B 9

:ERROR
@echo "*** Error occurred wile running the command n %computername% ***"
exit /B 9

:SUCCESS
cd %~dp0
@echo "**************************************************************
@echo "************** Process Completed Successfully ****************
@echo "**************************************************************
exit /B 9