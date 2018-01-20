# smartShower API

	languages, libraries, tools and technologies used
		De API is een c# .NET cloud Microsoft.Azure Functions gebouwd in een visual studio 2017 omgeving.
		Volgende assemblies, dependencies zijn gebruikt geweest voor de opbouw:
		•	using System;
		•	using System.Collections.Generic;
		•	using System.Data;
		•	using System.Data.SqlClient;
		•	using System.Diagnostics;
		•	using System.Linq;
		•	using System.Net;
		•	using System.Net.Http;
		•	using System.Threading.Tasks;
		•	using Microsoft.Azure.Documents.Client;
		•	using Microsoft.Azure.WebJobs;
		•	using Microsoft.Azure.WebJobs.Extensions.Http;
		•	using Microsoft.Azure.WebJobs.Host;
		•	using Newtonsoft.Json;

	Azure is gelinkt aan github  voor synchronisatie van de code.
	De entity framework is gebruikt om data op te halen en weg te schrijven naar SQL database: SmartShowerDb. Alle data die gelogt wordt door de senoren, die aan de Arduino hangen, wordt doorgestuurd naar een cosmosDb die online staat op Azure. 

	Overzicht van de API
		Communicatie verloopt over HTTP
		Base-url: https://smartshowerfunctions.azurewebsites.net/
		Functions.cs  class neemt de volledige communciatie op zich en volgende calls zijn mogelijk:
			•	POST RegisterUser
				o	Hier wordt een controle gedaan of het email adres al dan niet bestaat in de database. Indien dit het geval is, wordt een Forbidden status code meegeven. Wanneer het emailadres niet gekend is, wordt een nieuwe gebruiker aangemaakt in de database. Men krijgt dan status 200 OK.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/Reg
			•	POST RegisterShower
				o	Deze functie is geschreven om zelf een douche te registreren. Bij het prototype van de smartShower is de IdShower hardcode in de arduino. Dit wil zeggen dat er van deze functie momenteel geen gebruik wordt gemaakt. Toch kan deze functie in productie handig zijn.  Er wordt verwacht dat men in de body een idShower en waterCost meegeeft. Standaard staat de kostprijs per liter op 0.005€.
				o	RETURN 200 OK indien de functie correct uitgevoerd werd. Indien de douche al in de database zit, krijgt men een Forbidden terug.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Shower/Reg
			•	POST RegisterUserShower
				o	Bij deze call wordt een gebruiker aan een douche gelinkt. In de body wordt een idUser en IdShower meegegeven. 
				o	RETURN indien er geen douche gevonden is, een not Found statuscode terug. Men krijgt een 200 bij succes alsook het idUser en idShower.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/UserShower/Reg
			•	POST GetAvailableColors
				o	Er wordt een idShower meegestuurd in de body. Door een joinquery worden alle kleuren opgevraagd die al in gebruik zijn. Vervolgens worden ze uit een list met alle kleuren verwijderd. De kleuren die overblijven zijn de kleuren die nog beschikbaar zijn. 
					Noot: voor het datatype van de kleuren is  gebruikt gemaakt van integers
				o	RETURN bij succes statuscode 200, en een list met kleuren. Bij een failure wordt een internalServerError weergegeven
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/GetSessions
			•	POST LoginUser
				o	Hier worden een email en (gehashed ) wachtwoord meegeven. Er wordt een controle gedaan of deze login gegevens in de database zitten en correct zijn.
				o	RETURN een status code 200 en de user zelf bij succes. De gegevens van de user worden meegestuurd zodat zijn persoonlijke instellingen kunnen worden weergeven in de applicatie. Men krijgt een Unauthorized indien de login credentials niet correct zijn.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/login
			•	POST AddSessionToCosmosDb
				o	Wordt gebruikt om de data, die de Raspberry Pi van de Arduino krijgt, door te sturen naar de CosmosDb. 
				o	RETURN alle data die doorgestuurd werd bij succes. Bij een faillure krijgt men een InternalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/AddSession
			•	GET GetSessionFromCosmosDb
				o	Aan de hand van een idSession wordt de data van deze sessie uit de cosmosdb gehaald. 
				o	RETURN een list met sessieData bij succes. Internalserver error bij failure.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/getSession/{idSession}
			•	 GET CalculateSession
				o	Aan de hand van een idSession wordt alle data uit de cosmosdb gehaald. Hier wordt dus een GET GetSessionFromCosmosDb uitgevoerd. Met de sessiedata worden berekingen gedaan zoals gemiddelde temperatuur, waterverbruik, totale duurtijd, hoeveel geld bespaard en de ecoscore van één sessie. Nadat de berekeningen gebeurd zijn worden deze doorgestuurd naar de SQLdb in de tabel Session. Er wordt ook een timestamp toegevoegd ter controle. Aangezien een Iduser aangemaakt wordt in de applicatie zelf en niet in arduino, wordt de Iduser gelinkt aan het profielnummer die wel in de arduino bepaald wordt. Een user heeft een idUser en een kleur, de waarde van die kleur (int) is gelinkt aan het profielnummer. Zo weet men welke sessie van welke gebruiker is.
					Noot: deze HTTP GET wordt aangeroepen in het pythonscript en moet asynchronious lopen, gezien de tijdsduur van deze bewerking.
				o	RETURN statuscode 200 indien geslaagd. Indien er geen gebruiker kon gelinkt worden aan de het profielnummer krijgt men een NotFound error. Indien er problemen zijn aan de server kant krijgt men opnieuw een InternalServerError. 
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/calculateSession/{idSession}
			•	POST GetSessions
				o	In de body wordt een idUser meegegeven en een datalength (int). Afhankelijk van de datalength (0-2) wordt uit de database de sessie van de gebruiker weergegeven, per dag(0), per week(1) of per maand(2)
				o	RETURN bij succes status code 200 en een list met sessions. Bij een failure wordt een internalServerError weergegeven. 
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/getAllGroupsFromUser
			•	POST GetUserInfo
				o	Door in de body een idUser mee te geven, krijgt men alle info van deze gebruiker terug die opgeslaan is in de tabel Users.
				o	RETURN status code 200 en de userInfo. Indien geen success, wordt een InternalServerError terug gegeven. 
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/login
			•	 PUT UpdateUser
				o	In de body wordt een object user meegegeven met daarin al zijn gegevens vervolgens worden deze gegevens van de user geupdated.
				o	RETURN bij succes statuscode 200. Bij een failure krijgt men een internalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/Update/
			•	PUT UpdateShower
				o	In de body wordt een idShower meegeven en een float waterCost. Vervolgens worden de gegevens geupdate in de database.
				o	RETURN status code 200 bij succes, internalServerError bij een fout
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/Update/
			•	POST MakeGroup
				o	Hier wordt een idGroup en idUser meegeven. Pending van de gebruiker staat automatisch op 0. Dit wil zeggen dat wanneer een gebruiker een groep aanmaakt, hij zichzelf niet hoeft te accepeteren in zijn eigen groep. 
				o	RETURN statuscode OK, true bij succes. Bij failure krijgt de gebruiker een internalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Group/new/
			•	DELETE DeleteGroup
				o	In de body wordt een IdGroup meegegeven. Vervolgens wordt een query uitgevoerd die de groep met het desbetreffende id verwijderd.
				o	RETURN statuscode OK, true bij succes. Bij failure krijgt de gebruiker een internalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Group/delete/
			•	DELETE DeleteUserFromGroup
				In de body wordt een idGroup en Iduser meegeven. Vervolgens zal de rij uit de database verwijderd worden waar idGroup en IdUser gelinkt zijn. 
				Noot: deleteUserFromGroup en DeclineInvite hebben allebei dezelfde functie. Vandaar er enkel DeleteUserFromGroup aangemaakt is.
				o	RETURN statuscode OK, true bij succes. Bij failure krijgt de gebruiker een internalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Group/user/delete
			•	POST SendGroupInvite
				o	Het verzenden van GroupInvites gebeurd via het email adres van een gebruiker. Van deze gebruiker worden gegevens opgehaald zoals, idUser, naam, kleur en foto. Indien een gebruiker is gevonden in de database wordt de idUser wordt gelinkt aan de idGroup. 
				o	RETURN bij succes statuscode 200 en de idUser, naam, kleur en foto meegestuurd. Indien er geen gebruiker gevonden werd wordt een statuscode NotFound meegestuurd. Bij een andere failure krijgt men een internalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Group/invite

			•	PUT AcceptInvite
				o	Zet de pending van de meegegeven (id)User op 0. Dit betekent dat de gebruiker een uitnodiging van een bepaalde groep heeft goedgekeurd.
				o	RETURN true indien de HTTP verb correct is uitgevoerd. InternalServerError indien de operatie niet geslaagd is.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/Group/invite/accept
			•	POST GetAllGroupsFromUser
				o	Nadat een idUser is meegeven in de body worden alle idGroups waar de gebruiker in zit teruggegeven. 
				o	RETURN bij succes een list met Usergroups. Deze bevat objecten met properties: IdGroup en IdUser. Bij een failure wordt een internalServerError meegeven.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/getAllGroupsFromUser
			•	POST GetAllFriendsFromUser
				o	Opnieuw wordt  in de body een idUser meegegeven. In de database wordt gekeken in welke groepen deze idUser terug te vinden is. Vervolgens wordt in deze groepen gekeken welke andere gebruikers erin zitten. Deze personen worden beschouwd als vrienden van de gebruiker. 
				o	RETURN bij succes statuscode 200 en een list met users die een gemeenschappelijke groep delen. Bij failure krijgt de gebruiker een internalServerError.
				o	https://smartshowerfunctions.azurewebsites.net/api/SmartShower/getAllGroupsFromUser
		klassen
			bij de API horen volgende klassen:
				•	Session.cs
				•	SessionCosmosDb
				•	Shower.cs
				•	User.cs
				•	UserGroup.cs
				•	UserShower.cs
			De klasses zijn terug te vinden in het project SmartShowerFunctions in de map Model.
			Configuratie
		Om de azure functions lokaal te kunnen draaien moeten de rode variabelen correct worden toegevoegd in de local.settings.json:
		{
			"IsEncrypted": false,
		  "Values": {
			"AzureWebJobsStorage": "",
			"AzureWebJobsDashboard": "",
			"ConnectionString": "",
			"COSMOSHOST": "",
			"COSMOSKEY": "",
			"COSMOSDATABASE": "",
			"COSMOSCOLLECTIONID": ""

		  }
		}
		Waneer men alles online wilt testen mag men niet vergeten de connectionString toe te voegen aan de azure function. Ook aan de cosmosDb moeten zoals de host, key, databaseName en cosmoscollectionId worden gecontroleerd zodat alle waarden juist zijn.

		Databases
		MSSQL: SmartShowerDb
		ERD-schema
		 
		ERD-schema SmartShower
