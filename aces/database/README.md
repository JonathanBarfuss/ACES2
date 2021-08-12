To create the database execute the AntiCheatDatabaseScript.sql
**IMPORTANT**
For the script to work the FILENAME for both PRIMARY and LOG must be set for the correct path on the server.

**Example**
ON  PRIMARY 
( NAME = N'AntiCheat', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS2\MSSQL\DATA\AntiCheat.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'AntiCheat_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS2\MSSQL\DATA\AntiCheat_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
**End Example**

Most paths will be similar but the ...\MSSQL15.SQLEXPRESS2\... was specific to test the script on my local database.

After the database is installed the "ConnectionStrings:" in appsettings.json must be updated to connect to the database.  By default the connection string will be
the one we used to run the database on titan server.  To test the database on your local machine the connection string will be like the following example:

**Example**
"ConnectionStrings": {
    "ACESContext": "Server=ERICPC\\SQLEXPRESS2;Database=AntiCheat;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
**End Example**

Replace the Server=ERICPC\\SQLEXPRESS2 with Server={your local database path};  be sure to use \\ instead of \ for an escape character.  
