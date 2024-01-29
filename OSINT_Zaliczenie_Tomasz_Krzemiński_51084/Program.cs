// See https://aka.ms/new-console-template for more information
using OSINT_Zaliczenie_Tomasz_Krzemiński_51084;

Console.WriteLine("Witaj w Aplikacji pobierającej dane z KNF");
while (true)
{
    Console.WriteLine("Wpisz ile rekordów chciałbyś uzyskać: ");

    var limit = Console.ReadLine();

    Console.WriteLine($"Pobieranie pierwszych {limit} rekordów ze szczegółami");

    var records = await KnfService.GetRecords(Convert.ToInt32(limit));

    Console.WriteLine();
    Console.WriteLine($"W bazie znajduje sie {KnfService.TotalRecords} rekordów");
    Console.WriteLine($"Pobrano: {records.Count()}");
    Console.WriteLine();
    foreach (var record in records)
    {
        Console.WriteLine($"Id: {record.Id}");
        Console.WriteLine($"Nazwa Firmy: {record.Details.CompanyName}");
        Console.WriteLine($"NIP Firmy: {record.Details.NIP}");
        Console.WriteLine($"Imię Właściciela: {record.Details.FirstName}");
        Console.WriteLine($"Nazwisko Właściciela: {record.Details.LastName}");
        Console.WriteLine();
        Console.WriteLine("Dane Rejestru:");
        Console.WriteLine($"Typ Rejestru: {record.Details.RegistryType}");
        Console.WriteLine($"Numer Wpisu do Rejestru: {record.Details.RegistryNumber}");
        Console.WriteLine();
        Console.WriteLine("ADRES");
        Console.WriteLine($"Miasto: {record.Details.City}");
        Console.WriteLine($"Ulica: {record.Details.StreetAddress}");
        Console.WriteLine($"Nr. Domu: {record.Details.HouseNumber}");

        // Można uzyskać wszystkie wartości jakie posiada dany rekord ale trzeba dodawać ręcznie w GetDetails w KnfService.cs :/
        Console.WriteLine();
    }
}