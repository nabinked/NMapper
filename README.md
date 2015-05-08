# nMapper
A very basic Micro-ORM .NET library for directly mapping POCO/DTO objects to database tables.

###Available on Nuget
#####Install-Package NMapper

###Features
- Easily Map database tables with POCO classes.
- No need to work with heavy DataTables any more.
- Works with any database provider that implements IdbCommand.
 


###How does it work

- Decorate your POCO class properties with the column names atributes. This requires you to reference the NMapperAttributes Library thats available on Nuget.
- Instantiate a new Mapper Class.
- Pass in the Idb command parameter and get the Object(s).


###Code Example

Suppose we have the fowllowing table in our database.

user_id | user_name | password | email | first_name | last_name | audit_ts
--- | --- | --- |---| --- | --- | ---
1 | nabinked | mypassword | nabin@outlook.com| nabin | karki | 2012-12-12 00:00:00
2 | robert | isnotmyname |robert@gmail.com| robert | hobert | 2012-09-12 00:00:00
23 | mike | eatsmyears | mike@tyson.com|mike | tyson | 2012-09-12 23:23:23

Now in order to map this table to POCO classes using NMapper we do the following

```C#
///This is the POCO class for the user Table
//The ColunmName Attribute class is defined in NMapperAttributes. This should be referenced if not done automatically while installing NMapper From Nuget.
public class User
    {
        [ColumnName("user_id")]
        public long UserId { get; set; }
        
        [ColumnName("user_name")]
        public string UserName { get; set; }

        [ColumnName("password")]
        public string Password { get; set; }

        [ColumnName("email")]
        public string Email { get; set; }

        [ColumnName("first_name")]
        public string FirstName { get; set; }

        [ColumnName("last_name")]
        public string LastName { get; set; }
        
        [ColumnName("audit_ts")]
        public DateTime AuditTs { get; set; }

    }

//Now in order to map the table values to a collection of the User Object inside any function. we do the follwoing.

public void GetUsersCollection()
{
  var sqlConnection = Connection;//This should be the connection object.
  var sql = "SELECT * FROM public.users;"
  using(var cmd = new SqlCommand(sql, Connection))
  {
    var mapper = new Mapper<User>();
    var users = mapper.GetObjects(cmd);
    //now do any thing with the users object
  }

}

public void GetSingleUser()
{
  var sqlConnection = Connection;//This should be the connection object.
  var sql = "SELECT * FROM public.users WHERE user_id = 1;"
  using(var cmd = new SqlCommand(sql, Connection))
  {
    var mapper = new Mapper<User>();
    var singleUser = mapper.GetObject(cmd);//Note : This calls the GetObject Function of the Mapper class
    //now do any thing with the singleUser object
  }

}
