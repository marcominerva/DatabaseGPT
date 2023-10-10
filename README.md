# DatabaseGPT
Query a generic SQL Server database using natural language.

Thanks to [Adam Buckley](https://github.com/happyadam73/tsql-chatgpt) for the [original inspiration](https://www.linkedin.com/pulse/query-your-data-azure-sql-using-natural-language-chatgpt-adam-buckley/) for this project.

### Configuration

The [DatabaseGptConsole](https://github.com/marcominerva/DatabaseGPT/tree/master/src/DatabaseGptConsole) project is a .NET console application that can be used to test the library.

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
"DatabaseSettings": {
    "ExcludedTables": [ ],          // Array of table names to exclude (in the form of "schema.table")
    "ExcludedColumns": [ ],         // Array of column names to exclude
    "MaxRetries": 3                 // Max retries when the query fails
}
```

### Usage

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

If in the schema there are tables and columns and you never want to be used, you can exclude these tables and columns from the query generation process by adding them to the `ExcludedTables` and `ExcludedColumns` arrays in the [appsettings.json](https://github.com/marcominerva/DatabaseGPT/blob/master/src/DatabaseGptConsole/appsettings.json) file. For example:

```
"DatabaseSettings": {
    "ExcludedTables": [ "dbo.CheckView" ],       
    "ExcludedColumns": [ "Timestamp" ]         // Exclude the Timestamp column from all tables
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

## Contribute

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can. 