using System.Text;
using System.Text.Json;

namespace OSINT_Zaliczenie_Tomasz_Krzemiński_51084;
internal static class KnfService
{
    private static readonly string Url = "https://rpkip.knf.gov.pl/JSON";
    public static int TotalRecords { get; private set; }

    public static async Task<IEnumerable<RecordDto>> GetRecords(int limit = 10)
    {
        string payload = $@"{{
            ""cmd"": ""get"",
            ""search"": [],
            ""limit"": {limit},
            ""offset"": {limit},
            ""method"": ""Default"",
            ""sort"": [
                {{""field"": ""REGISTRY_NUMBER"", ""direction"": ""asc""}}
            ],
            ""searchLogic"": ""AND"",
            ""searchValue"": """"
        }}";

        List<RecordDto> recordsDto = new();
        using (HttpClient client = new HttpClient())
        {
            var content = new StringContent($"request={Uri.EscapeDataString(payload)}", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await client.PostAsync(Url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var rootRaw = JsonSerializer.Deserialize<BaseRoot>(responseContent);

                TotalRecords = rootRaw!.total;

                recordsDto = MapRecords(rootRaw!).ToList();

                foreach (var record in recordsDto)
                {
                    record.Details = await GetDetails(record.Id);
                }
            }
            else
            {
                Console.WriteLine($"Błąd: {response.StatusCode}");
            }
        }

        return recordsDto;
    }

    public static async Task<RecordDetailsDto> GetDetails(int id)
    {
        string payload = @$"{{""cmd"":""get-properties"",""search"":[],""limit"":100,""offset"":0,""method"":""RHATable"",""recid"":{id}}}";

        DetailsRecordRoot detailsRoot = new DetailsRecordRoot();
        using (HttpClient client = new HttpClient())
        {
            var content = new StringContent($"request={Uri.EscapeDataString(payload)}", Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await client.PostAsync(Url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                detailsRoot = JsonSerializer.Deserialize<DetailsRecordRoot>(responseContent)!;
            }
        }

        var recordDetails = new RecordDetailsDto()
        {
            FirstName = detailsRoot.records.FirstOrDefault(x => x.field == "CI_SURNAME")?.value.ToString()!,
            LastName = detailsRoot.records.FirstOrDefault(x => x.field == "SURNAME")?.value.ToString()!,
            City = detailsRoot.records.FirstOrDefault(x => x.field == "AS_LOCALITY")?.value.ToString()!,
            StreetAddress = detailsRoot.records.FirstOrDefault(x => x.field == "AS_STREET")?.value.ToString()!,
            HouseNumber = detailsRoot.records.FirstOrDefault(x => x.field == "AS_STREET_NUMBER")?.value.ToString()!,
            NIP = detailsRoot.records.FirstOrDefault(x => x.field == "NIP")?.value.ToString()!,
            CEIDG = detailsRoot.records.FirstOrDefault(x => x.field == "CEIDG")?.value.ToString()!,
            CompanyName = detailsRoot.records.FirstOrDefault(x => x.field == "COMPANY_OUT")?.value.ToString()!,
            RegistryType = detailsRoot.records.FirstOrDefault(x => x.field == "ENT_TYP")?.value?.ToString()!,
            RegistryNumber = detailsRoot.records.FirstOrDefault(x => x.field == "REGISTRY_NUMBER_RPH_OUT")?.value?.ToString()!,
            // More can be added
        };

        return recordDetails;
    }

    private static IEnumerable<RecordDto> MapRecords(BaseRoot baseRoot)
    {
        List<RecordDto> records = new();

        foreach (var record in baseRoot.records)
        {
            records.Add(new()
            {
                Id = record.ID,
                CompanyName = record.COMPANY,
                RegistryType = record.ENT_TYP,
                Broker_Status = record.BROKER_STATUS,
                H_Broker_Status = record.H_BROKER_STATUS,
                Localization = record.AS_LOCALITY,
                NIP = record.NIP
            });
        }

        return records;
    }
}

public class BaseRecord
{
    public string BROKER_STATUS { get; set; } = default!;
    public string ENT_TYP { get; set; } = default!;
    public string H_ENT_TYP { get; set; } = default!;
    public string NIP { get; set; } = default!;
    public string REGISTRY_NUMBER { get; set; } = default!;
    public int H_ENT { get; set; }
    public string AS_LOCALITY { get; set; } = default!;
    public int ID { get; set; }
    public string H_BROKER_STATUS { get; set; } = default!;
    public int recid { get; set; }
    public string COMPANY { get; set; } = default!;
}

public class BaseRoot
{
    public int total { get; set; }
    public List<BaseRecord> records { get; set; } = default!;
    public string status { get; set; } = default!;
}

public class RecordDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = default!;
    public string Broker_Status { get; set; } = default!;
    public string H_Broker_Status { get; set; } = default!;
    public string RegistryType { get; set; } = default!;
    public string Localization { get; set; } = default!;
    public string NIP { get; set; } = default!;
    public RecordDetailsDto Details { get; set; } = default!;
}

public class RecordDetailsDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string City { get; set; } = default!;
    public string StreetAddress { get; set; } = default!;
    public string HouseNumber { get; set; } = default!;
    public string NIP { get; set; } = default!;
    public string CEIDG { get; set; } = default!;
    public string CompanyName { get; set; } = default!;
    public string RegistryNumber { get; set; } = default!;
    public string RegistryType { get; set; } = default!;
}

class DetailsRecordRoot
{
    public int total { get; set; }
    public List<DetailsRecord> records { get; set; } = default!;
    public string status { get; set; } = default!;
}

class DetailsRecord
{
    public int id { get; set; }
    public string field { get; set; } = default!;
    public string name { get; set; } = default!;
    public object value { get; set; } = default!;
}
