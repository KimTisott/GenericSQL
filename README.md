# GenericSQL

Minimalist and easy to use API to access and manage databases.

By combining EntityFramework capabilities with Dapper performance, this project aims to build complex queries with ease, that run fast and are easy to maintain.

WARNING: this is not ready for production, use it at your own risk.

## Features

* Object-oriented API
* Clean and fast coding
* Easy maintenance
* Entirely asynchronous
* Databases:
  * MySQL
* Languages:
  * C# 9.0
* Operations:
  * Add : insert one object
  * Del : delete one object
  * Get : select one object
  * GetAll : select all objects
  * Set : update one object
  * Sum : count all objects
* Parameters:
  * Select : define object properties
  * Where : define simple conditions

## Roadmap

* Include new operations:
  * AddAll : insert all objects
  * DelAll : delete all objects
  * SetAll : update all objects
  * Run : execute stored procedure
* Improve parameters:
  * Select : foreign properties
  * Where : complex conditions
* Add support for other databases:
  * MSSQL
  * Oracle

## Tutorial

Declaration of table and columns
```c#
[Table(nameof(City))]
public record City
{
    [Column(nameof(ID)), PrimaryKey]
    public int ID { get; set; }
    [Column(nameof(Name))]
    public string Name { get; set; }
    [Column(nameof(CountryCode))]
    public string CountryCode { get; set; }
    [Column(nameof(District))]
    public string District { get; set; }
    [Column(nameof(Population))]
    public int Population { get; set; }
    [Column(nameof(Country))]
    public Country Country { get; set; }
}
```

Usage with examples
```c#
int insertedRowCount = await db.Query<City>().Add(new City { Name = "Generic City", Population = 1234 });
City genericCity = await db.Query<City>().Where(x => x.Name == "Generic City" && x.Population == 1234).Select(x => new { x.ID, x.Name, x.Population }).Get();
IEnumerable<City> smallCitiesIds = await db.Query<City>().Select(x => new { x.ID }).Where(x => x.Population < 10000).GetAll();
int updatedRowCount = await db.Query<City>().Where(x => x.ID == genericCity.ID).Set(genericCity with { Name = "Updated Generic City" });
int updatedGenericCitiesCount = await db.Query<City>().Where(x => x.Name == "Updated Generic City").Sum();
int deletedRowCount = await db.Query<City>().Where(x => x.Name == "Generic City").Del();
```
