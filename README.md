# IntrospectTokenCoreConsoleApp
The purpose of this .Net Core console app is to validate the access token through introspect endpoint using httpclient or Indentity Server 's IntrospectClient and show the output

## Requirements

* .NET CORE 1.1
* Identity Server 4 authority server 

## Usage

Open CMD and navigate to the folder of the project.  Available commnads:

* dotnet run help -- Displays all the available commands and their syntax
* dotnet run identity help -- Displays a list of arguments that are required for this command
* dotnet run identity serverAuthorityAddress scopeName scopePassword accessToken -- introspection of the access token using IntrospectionClient of Identity Server 4
* dotnet run httpclient help -- displays a list of arguments that are required for this command
* dotnet run httpclient erverAuthorityAddress scopeName scopePassword accessToken -- introspection of the access token using httpclient 
* dotnet run validate serverAuthorityAddress scopeName scopePassword accessToken userId expiresIn clientId -- introspection of the access token without connecting to introspection endpoint.In case of invalid token detailed errors are displayed. 
