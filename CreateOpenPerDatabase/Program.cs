// See https://aka.ms/new-console-template for more information
using CreateOpenPerDatabase;

Console.WriteLine("Create openPer Database");

// We are creating the sqlite database to support the open source version of ePer that I've called openPer
// We will process three files, one is an MS Access database and two are encoded files containing VIN data
// The Access database will be transferred over to sqlite pretty much as-is including indices.
// The VIN data will be transformed to make it easier to process later.  We will also do some
// data corrections on the way through.


// Access database first
AccessDatabaseTransfer.ProcessAccessTables(@"c:\temp\openper.sqlite");