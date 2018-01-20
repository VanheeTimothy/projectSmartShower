# smartShower API

languages, libraries, tools and technologies used
De API is een c# .NET cloud Microsoft.Azure Functions gebouwd in een visual studio 2017 omgeving.
Volgende assemblies, dependencies zijn gebruikt geweest voor de opbouw:
* using System;
* using System.Collections.Generic;
* using System.Data;
* using System.Data.SqlClient;
* using System.Diagnostics;
* using System.Linq;
* using System.Net;
* using System.Net.Http;
* using System.Threading.Tasks;
* using Microsoft.Azure.Documents.Client;
* using Microsoft.Azure.WebJobs;
* using Microsoft.Azure.WebJobs.Extensions.Http;
* using Microsoft.Azure.WebJobs.Host;
* using Newtonsoft.Json;

Azure is gelinkt aan github  voor synchronisatie van de code.
De entity framework is gebruikt om data op te halen en weg te schrijven naar SQL database: SmartShowerDb. 
Alle data die gelogt wordt door de senoren, die aan de Arduino hangen, wordt doorgestuurd naar een cosmosDb die online staat op Azure. 
		
## Overzicht van de API
Communicatie verloopt over HTTP
Base-url: https://smartshowerfunctions.azurewebsites.net/
Functions.cs  class neemt de volledige communciatie op zich en volgende calls zijn mogelijk:


### POST RegisterUser
Hier wordt een controle gedaan of het email adres al dan niet bestaat in de database. 
Indien dit het geval is, wordt een Forbidden status code meegeven. 
Wanneer het emailadres niet gekend is, wordt een nieuwe gebruiker aangemaakt in de database. 
Men krijgt dan status 200 OK.
https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/Reg

### POST RegisterShower
Deze functie is geschreven om zelf een douche te registreren. 
Bij het prototype van de smartShower is de IdShower hardcode in de arduino. 
Dit wil zeggen dat er van deze functie momenteel geen gebruik wordt gemaakt. 
Toch kan deze functie in productie handig zijn.  
Er wordt verwacht dat men in de body een idShower en waterCost meegeeft. 
Standaard staat de kostprijs per liter op 0.005€.
RETURN 200 OK indien de functie correct uitgevoerd werd. 
Indien de douche al in de database zit, krijgt men een Forbidden terug.
https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Shower/Reg

### POST RegisterUserShower
Bij deze call wordt een gebruiker aan een douche gelinkt. 
In de body wordt een idUser en IdShower meegegeven. 
RETURN indien er geen douche gevonden is, een not Found statuscode terug. 
Men krijgt een 200 bij succes alsook het idUser en idShower.
https://smartshowerfunctions.azurewebsites.net/api/SmartShower/UserShower/Reg

### POST GetAvailableColors
Er wordt een idShower meegestuurd in de body. 
Door een joinquery worden alle kleuren opgevraagd die al in gebruik zijn. 
Vervolgens worden ze uit een list met alle kleuren verwijderd. 
De kleuren die overblijven zijn de kleuren die nog beschikbaar zijn. 
Noot: voor het datatype van de kleuren is  gebruikt gemaakt van integers
RETURN bij succes statuscode 200, en een list met kleuren. 
Bij een failure wordt een internalServerError weergegeven
https://smartshowerfunctions.azurewebsites.net/api/SmartShower/GetSessions
### POST RegisterShower
```
Deze functie is geschreven om zelf een douche te registreren. Bij het prototype van de smartShower is de IdShower hardcode in de arduino. Dit wil zeggen dat er van deze functie momenteel geen gebruik wordt gemaakt. Toch kan deze functie in productie handig zijn.  Er wordt verwacht dat men in de body een idShower en waterCost meegeeft. Standaard staat de kostprijs per liter op 0.005€.
RETURN 200 OK indien de functie correct uitgevoerd werd. Indien de douche al in de database zit, krijgt men een Forbidden terug.
https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Shower/Reg
```
### POST RegisterShower
```
Deze functie is geschreven om zelf een douche te registreren. Bij het prototype van de smartShower is de IdShower hardcode in de arduino. Dit wil zeggen dat er van deze functie momenteel geen gebruik wordt gemaakt. Toch kan deze functie in productie handig zijn.  Er wordt verwacht dat men in de body een idShower en waterCost meegeeft. Standaard staat de kostprijs per liter op 0.005€.
RETURN 200 OK indien de functie correct uitgevoerd werd. Indien de douche al in de database zit, krijgt men een Forbidden terug.
https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Shower/Reg
```

What things you need to install the software and how to install them

```
Give examples
```

### Installing

A step by step series of examples that tell you have to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Billie Thompson** - *Initial work* - [PurpleBooth](https://github.com/PurpleBooth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc

