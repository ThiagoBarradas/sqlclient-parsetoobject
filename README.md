
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ThiagoBarradas_sqlclient-parsetoobject&metric=alert_status)](https://sonarcloud.io/dashboard?id=ThiagoBarradas_sqlclient-parsetoobject)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ThiagoBarradas_sqlclient-parsetoobject&metric=coverage)](https://sonarcloud.io/dashboard?id=ThiagoBarradas_sqlclient-parsetoobject)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SqlClient.ParseToObject.svg)](https://www.nuget.org/packages/SqlClient.ParseToObject/)
[![NuGet Version](https://img.shields.io/nuget/v/SqlClient.ParseToObject.svg)](https://www.nuget.org/packages/SqlClient.ParseToObject/)

# SqlClient.ParseToObject

Get a full object (with nested complex objects) from Microsoft.Data.SqlClient. Created with .NET Core support;

## Install via NuGet

```
PM> Install-Package SqlClient.ParseToObject
```

## How to use

Optional Packages (to run next code):
```
Install-Package Dapper 
```

Demo Models:
```
public class UserModel
{
	public string Id { get; set; }

	public string Name { get; set; }

	public int Weight { get; set; }

	public DateTime Birthdate{ get; set; }

	public AddressModel Address { get; set; }
}

public class AddressModel
{
	public string Line1 { get; set; }

	public string Line2 { get; set; }

	public string City { get; set; }
}
```

Code:
```

var query = @"SELECT 
				U.Cod AS Id,
				Name,
				Birthdate,
				Weight,
				A.Line1 as [Address.Line1],
				A.Line2 as [Address.Line2],
				A.City as [Address.City]
			  FROM User U INNER JOIN Address A ON U.AddressCod = A.Cod
			  WHERE Name = @Name";

var parameters = new 
{
	Name = "Thiago Barradas"
};

using (var sqlConnection = new SqlConnection(connectionString))
{
    var reader = sqlConnection.ExecuteReader(query, parameters);

    List<User> users = reader.GetResults<User>();
}

```

## How can I contribute?
Please, refer to [CONTRIBUTING](.github/CONTRIBUTING.md)

## Found something strange or need a new feature?
Open a new Issue following our issue template [ISSUE TEMPLATE](.github/ISSUE_TEMPLATE.md)

## Changelog
See in [nuget version history](https://www.nuget.org/packages/SqlClient.ParseToObject)

## Did you like it? Please, make a donate :)

if you liked this project, please make a contribution and help to keep this and other initiatives, send me some Satochis.

BTC Wallet: `1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX`

![1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX](https://i.imgur.com/mN7ueoE.png)
