# DBMProject

Automate deployment of database changes



Database migrations

Database migration scripts are a way to automatically deploy changes to the database. A migration script is a set of commands that will take the target database from state A to state B. The scripts generally come in two flavors:
 - Schema changes or DDL (data definition language)  

 - Seeding scripts or DML (data modification language)

In most cases when we do a deployment of a new code package to an environment we would first apply DDL scripts to ensure that the schema is of the correct version and then run the DML scripts to seed new data or modify existing data. In order to enable automated deployment we need a tool that can automate this process. At a high level the tool would work as follows:

Inputs:

root folder of where the migration scripts are located. Using the root folder you can form the following two paths: 
 ${root_folder}/ddl

 ${root_folder)/dml

connection string of the target database
Algorithm:

obtain an inventory of all scripts in the source folder and cache in an appropriate data structure
compare the inventory to the list of already executed scripts. These are generally stored in a target database in a  "versions" table. This would be a new table and at a minimum it would need the script name and execution time stamp
Create a list of all scripts that have not yet been executed in the target database
Sort the scripts that they are executed in an appropriate order. The ordering is determined using 2 variables: - script type (DDL or DML). DDL scripts should be executed first - naming convention (something to be defined). A good example naming convention would be the following: "[ticket_name]_[execution order]-[description].sql".  Examples: MCRTECH_10101-udpate.sql, MCRTECH-10101.sql. Define a convention to skip a script from automatic execution: XMCRTECH-1111.sql. If the script does not comply with the naming convention then sort it alphabetically after complying scripts in each group (DDL/DML)
Each script may contain multiple "batches" separated by "GO" statements (https://docs.microsoft.com/en-us/sql/t-sql/language-elements/sql-server-utilities-statements-go?view=sql-server-2017) Since "go" is not actually an sql statement it cannot be executed as sql. It is a directive that this block of sql should be executed separately. So break the script by batch and executed each one separately
If the execution of all batches in a script is successful then write a record to the "Versions" table
If execution of any batch fails halt any further processing and output an error with the details (STDERR)
 

Outputs:

listing of scripts that will be applied to the target database in order outputted to STDOUT
messages from script execution (to STDOUT if successful or STDERR if failed)
database record of each script execution if successful - overall status of the batch to STDOUT or STERROR (if all successful the sucess else failure)
# Instruction

Cmd Usage

```
-r, --root      Required. Input file to read.

-c, --conn      Required. Connection String to SQL Server

-d, --dbname    Not Required. Specific name of Database that contains Version table

-s, --snapshot    Do you want to recover your data?

-v, --sub         Subtitute Variable

```

Cmd Example

```
dotnet run -r C:\\Users\\szhou\\Desktop\\script --conn "Data source=US-NY-8W1RQ32;Initial Catalog=Version_test;Integrated Security=True;"

```

```

dotnet run -r C:\\Users\\szhou\\Desktop\\script --conn "Data source=US-NY-8W1RQ32;Integrated Security=True;" --dbname Version_test

```
```
dotnet run -r C:\\Users\\szhou\\Desktop\\script --conn "Data source=US-NY-8W1RQ32;Integrated Security=True;" --dbname Version_test --snapshot
```
```
dotnet run -r C:\\Users\\szhou\\Desktop\\script --conn "Data source=US-NY-8W1RQ32;Integrated Security=True;" --sub var1:Version_test var2:test2  --dbname Version_test --snapshot
```
Substitute Variable

```
--sub var1:val1 var2:val2

In the script you would look for all instances of $var1$ and $var2$ and substitute with val1 and val2.
```

Version Table Sample Schema

```
CREATE TABLE [dbo].[version](
	
	[id] [int] IDENTITY(1,1) NOT NULL,
	
	[script_name] [varchar](50) NULL,
	
	[applied_date] [datetime] NULL
) 
ON [PRIMARY]

```
