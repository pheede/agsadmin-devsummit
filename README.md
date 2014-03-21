# agsadmin-devsummit

Code and samples from "ArcGIS for Server Administration API for C# Developers" presentation given at 2013 and 2014 Esri Developer's Summit in Palm Springs, CA.

Built on VS2012/VS2013 and .NET Framework 4.5 with use of the JSON.NET NuGet package.

## Description

This small library demonstrates use of the ArcGIS Server administrative REST API from C# including the following key steps:

* securely getting an administrative token
* putting together the proper parameters
* making an HTTP request using HttpClient (GET or POST)
* parsing the result using JSON.NET

It includes example methods for:
* starting and stopping services
* monitoring service status
* uploading files for publishing new services from .sd files
* enumerating active services to discover their data sources (useful for finding which service has locked your dataset)

and several others.
