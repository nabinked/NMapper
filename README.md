# nMapper
A very basic ORM .NET library for directly mapping POCO/DTO objects to database tables.

###Available on Nuget
#####Install-Package NMapper

###Features
- Easily Map database tables with POCO classes.
- No need to work with heavy DataTables any more.
- Works with any database provider that implements IdbCommand.
 


###How does it work

- Decorate you POCO classe properties with the column names. This requires you to reference the NMapperAttributes Library thats available on Nuget.
- Instantiate a new Mapper Class.
- Pass in the Idb command parameter and get the Object(s).


