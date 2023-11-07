# DatabaseGPT

[![Lint Code Base](https://github.com/marcominerva/DatabaseGPT/actions/workflows/linter.yml/badge.svg)](https://github.com/marcominerva/DatabaseGPT/actions/workflows/linter.yml)
[![CodeQL](https://github.com/marcominerva/DatabaseGPT/actions/workflows/codeql.yml/badge.svg)](https://github.com/marcominerva/DatabaseGPT/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/marcominerva/TinyHelpers/blob/master/LICENSE)

Query a database using natural language.

Thanks to [Adam Buckley](https://github.com/happyadam73/tsql-chatgpt) for the [original inspiration](https://www.linkedin.com/pulse/query-your-data-azure-sql-using-natural-language-chatgpt-adam-buckley/) for this project.

![](https://raw.githubusercontent.com/marcominerva/DatabaseGPT/master/assets/DatabaseGptConsole.gif)

### Usage

Currently, SQL Server and PostgreSQL databases are supported.

#### Using DatabaseGpt in your project

If you want to use **DatabaseGpt** as a library in your application, you can reference the `DatabaseGpt/DatabaseGpt.csproj` project and the one that contains the specific implementation for your DBMS, available as `DatabaseGpt.<DBMS>/DatabaseGpt.<DBMS>.csproj`:

Database|Project to include
-|-
SQL Server|DatabaseGpt.SqlServer/DatabaseGpt.SqlServer.csproj
PostgreSQL|DatabaseGpt.Npgsql/DatabaseGpt.Npgsql.csproj

After referencing the proper projects, you can easily initialize **DatabaseGpt** at the startup of your application.

```csharp
// ...

builder.Services.AddDatabaseGpt(database =>
{
    // For SQL Server.
    database.UseConfiguration(context.Configuration)
            .UseSqlServer(context.Configuration.GetConnectionString("SqlConnection"));

    // For PostgreSQL.
    //database.UseConfiguration(context.Configuration)
    //        .UseNpgsql(context.Configuration.GetConnectionString("NpgsqlConnection"));
},
chatGpt =>
{
    chatGpt.UseConfiguration(context.Configuration);
});
```

#### Using the console test application

The [DatabaseGptConsole](https://github.com/marcominerva/DatabaseGPT/tree/master/samples/DatabaseGptConsole) project is a .NET console application that can be used to test the library. It requires .NET 7.0 SDK or later. If you just want to run the application, you can safely download the binaries from the [Releases section](https://github.com/marcominerva/DatabaseGPT/releases).

You need to set the required values in the [appsettings.json](https://github.com/marcominerva/DatabaseGPT/blob/master/src/DatabaseGptConsole/appsettings.json) file:

```
"ConnectionStrings": {
    "SqlConnection": "" // The database connection string
},
"ChatGPT": {
    "Provider": "OpenAI",           // Optional. Allowed values: OpenAI (default) or Azure
    "ApiKey": "",                   // Required
    "Organization": "",             // Optional, used only by OpenAI
    "ResourceName": "",             // Required when using Azure OpenAI Service
    "AuthenticationType": "ApiKey", // Optional, used only by Azure OpenAI Service. Allowed values: ApiKey (default) or ActiveDirectory
    "DefaultModel": "my-model"      // Required  
},
"DatabaseGptSettings": {
    "ExcludedTables": [ ],          // Array of table names to exclude (in the form of "schema.table")
    "ExcludedColumns": [ ],         // Array of column names to exclude
    "MaxRetries": 3                 // Max retries when the query fails
}
```

For more information about how to configure the ChatGPT integration, refer to the documentation of [ChatGptNet](https://github.com/marcominerva/ChatGptNet).

> **Note**
If possible, use GPT-4 models. Current experiments demonstrate that they are more accurate than GPT-3 models when generating queries.

### Configuration

The system works by using an OpenAI model to generate a SQL query from a natural language question, reading the list of the available tables with their structure. If table names and columns are well defined, the library should be able to automatically determine what tables to use and how to join them. For example:

```sql
CREATE TABLE dbo.Categories(
	Id INT IDENTITY(1,1) NOT NULL,
	CategoryName NVARCHAR(15) NOT NULL
)

CREATE TABLE dbo.Suppliers(
	Id INT IDENTITY(1,1) NOT NULL,
	CompanyName NVARCHAR(40) NOT NULL,
	ContactName NVARCHAR(30) NULL
)

CREATE TABLE dbo.Products(
	Id INT IDENTITY(1,1) NOT NULL,
	ProductName NVARCHAR(40) NOT NULL,
	SupplierId INT NULL,
	CategoryId INT NULL,
	QuantityPerUnit NVARCHAR(20) NULL,
	UnitPrice MONEY NULL,
	UnitsInStock SMALLINT NULL,
	UnitsOnOrder SMALLINT NULL,
	Discontinued BIT NOT NULL
)
```

Giving this schema, the model will be able to infer the following information, for example:

- If the user wants the name of the products, the column `ProductName` of the table `Products` must be used.
- The `SupplierId` column in the `Products` table is a foreign key to the `Id` column in the `Suppliers` table.
- The `CategoryId` column in the `Products` table is a foreign key to the `Id` column in the `Categories` table.

If in the schema there are tables and columns and you never want to be used, you can exclude them from the query generation process by adding them to the `ExcludedTables` and `ExcludedColumns` arrays in the [appsettings.json](https://github.com/marcominerva/DatabaseGPT/blob/master/src/DatabaseGptConsole/appsettings.json#L17-L18) file. For example:

```
"DatabaseSettings": {
    "ExcludedTables": [ "dbo.CheckView" ],       
    "ExcludedColumns": [ "Timestamp" ]         // Exclude the Timestamp column from all tables
}
```

On the other hand, if you want to use only a particular set of tables, you can add them to the `IncludedTables` array in the [appsettings.json](https://github.com/marcominerva/DatabaseGPT/blob/master/src/DatabaseGptConsole/appsettings.json#L16) file. For example:

```json
"DatabaseSettings": {
    "IncludedTables": [ "Production.Product", "Sales.SalesOrderHeader" "Sales.SalesOrderDetail" ]
}
```

In some cases, some columns might contains values that have a particular meaning. For example:

```sql
CREATE TABLE dbo.Attachments(
	Id INT IDENTITY(1,1) NOT NULL,
	Name NVARCHAR(40) NOT NULL,
	Status INT NOT NULL,
	Path NVARCHAR(MAX) NOT NULL
)
```

In this case, the `Status` column contains an integer value that represents the status of the attachment. In order to make the query generation process more accurate, you need to tell the library that the `Status` column is an enumeration. You can do this by adding some comments in the [SystemMessage.txt](https://github.com/marcominerva/DatabaseGPT/blob/master/src/DatabaseGptConsole/SystemMessage.txt) file, using a natural language:

```
- If the 'Status' column of table 'dbo.Attachments' is equals to 0, it means that the attachment has not been processed yet.
- If the 'Status' column of table 'dbo.Attachments' is equals to 1, it means that the attachment has been processed and approved.
- If the 'Status' column of table 'dbo.Attachments' is equals to 2, it means that the attachment has been rejected.
```

You can add as many indications as you need. The library will use this information to generate the query.

#### Retry strategy

As we know, GPT models are not perfect. Sometimes, the generated query is not valid. In this case, the library will retry to generate the query, using a different approach. The number of retries is configured in the [appsettings.json](https://github.com/marcominerva/DatabaseGPT/blob/master/src/DatabaseGptConsole/appsettings.json#L19) file:

```json
"DatabaseSettings": {
    "MaxRetries": 3
}
```

The retry strategy is handled using [Polly](https://github.com/App-vNext/Polly).

## Contribute

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can. 
