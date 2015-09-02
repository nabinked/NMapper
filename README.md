# nMapper
A very basic Micro-ORM .NET library for directly mapping POCO/DTO objects to database tables.

###Available on Nuget
#####Install-Package NMapper

###Features
- Easily Map database tables with POCO classes.
- No need to work with heavy DataTables any more.
- Works with any database provider that implements IdbCommand.
 


###How does it work

- ~~Decorate your POCO class properties with the column names atributes. This requires you to reference the NMapperAttributes Library thats available on Nuget.~~
- You dont require to put attributes around your classes. You can simply specify the NamingConvention of your database and your POCO classes by calling the public methods in NamingConvention Class at the start of your application. 
- If you havent followed a uniform naming convention then you can use the attributes as mentioned above in the striked bullet point.
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
        public long UserId { get; set; }
        
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public DateTime AuditTs { get; set; }

    }

   //This can go anywhere at the startup of your application
   void SomeMethodThaRunsOnApplicationStartup()
   {
     //Underscore case = 1
     //Pascalcase = 2
      NMapper.NamingConvention.SetDataBaseNamingConvention(1);
      NMapper.NamingConvention.SetObjectNamingConvention(2);
   }
   
   //By default the Database naming convention is underscore case and POCO naming convention is PascalCase.


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
