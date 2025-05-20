using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TcpSocketServer.Services;

public class CarApiService : ICarApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CarApiService> _logger;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public CarApiService(ILogger<CarApiService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _apiKey = configuration["CarApi:ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey));
        _apiUrl = configuration["CarApi:Url"] ?? "https://api.api-ninjas.com/v1/cars";

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }

    public async Task<string> GetCarInfoAsync(string brand, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_apiUrl}?make={Uri.EscapeDataString(brand.ToLower())}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API returned error: {StatusCode}", response.StatusCode);
                return $"ERROR CarApiError: {response.StatusCode}";
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("API Response: {JsonResponse}", jsonString);

            var carData = JsonSerializer.Deserialize<List<CarInfo>>(jsonString, _jsonOptions);

            if (carData == null || carData.Count == 0)
            {
                return $"ERROR NoCarDataFound for brand: {brand}";
            }

            var firstCar = carData[0];

            var output = new List<string>
            {
                $"CAR INFO: {firstCar.make} {firstCar.model} {firstCar.year}"
            };

            if (!string.IsNullOrEmpty(firstCar.class_name))
                output.Add($"Class: {firstCar.class_name}");

            if (firstCar.displacement.HasValue)
                output.Add($"Engine: {firstCar.displacement}L");

            if (firstCar.cylinders.HasValue)
                output.Add($"{firstCar.cylinders}cyl");

            if (!string.IsNullOrEmpty(firstCar.transmission))
                output.Add($"Transmission: {firstCar.transmission}");

            if (!string.IsNullOrEmpty(firstCar.drive))
                output.Add($"Drive: {firstCar.drive}");

            return string.Join(", ", output);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Car API request timed out");
            return "ERROR CarApiTimeout";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching car info for brand: {Brand}", brand);
            return "ERROR CarApiError";
        }
    }
}
