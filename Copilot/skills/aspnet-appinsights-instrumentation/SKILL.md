---
name: aspnet-appinsights-instrumentation
description: 'Instrument an ASP.NET WebApp to send useful telemetry data to Azure App Insights'
---

# AppInsights instrumentation

This skill enables sending telemetry data of a webapp to Azure App Insights for better observability of the app's health.

## When to use this skill

Use this skill when the user wants to enable telemetry for their webapp.

## Prerequisites

The app in the workspace must be an ASP.NET Core app hosted in Azure

## Guidelines

### Collect context information

Find out the (programming language, application framework, hosting) tuple of the application the user is trying to add telemetry support in. This determines how the application can be instrumented. Read the source code to make an educated guess. Confirm with the user on anything you don't know. You must always ask the user where the application is hosted (e.g. on a personal computer, in an Azure App Service as code, in an Azure App Service as container, in an Azure Container App, etc.). 

### Prefer auto-instrument if possible

If the app is a C# ASP.NET Core app hosted in Azure App Service, use [AUTO guide](references/AUTO.md) to help user auto-instrument the app.

### Manually instrument

Manually instrument the app by creating the AppInsights resource and update the app's code. 

#### Modify application code

- If the app is an ASP.NET Core app, see [ASPNETCORE guide](references/ASPNETCORE.md) for how to modify the C# code.