using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using VesselManagementClient.Model;

namespace VesselManagementClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl = "https://localhost:7042";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseApiUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        // OWNER
        public async Task<List<OwnerDto>?> GetOwnersAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/owners");
                response.EnsureSuccessStatusCode(); // !2xx
                return await response.Content.ReadFromJsonAsync<List<OwnerDto>>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"API Error getting owners: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error getting owners: {ex.Message}");
                return null;
            }
        }

        public async Task<OwnerDto?> GetOwnerByIdAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"api/owners/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<OwnerDto>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"API Error getting owner {id}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error getting owner {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<(OwnerDto? CreatedOwner, string? ErrorMessage)> CreateOwnerAsync(CreateOwnerDto owner)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/owners", owner);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return (null, $"API Error ({response.StatusCode}): {error}");
                }
                return (await response.Content.ReadFromJsonAsync<OwnerDto>(), null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error creating owner: {ex.Message}");
                return (null, $"Client Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteOwnerAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"api/owners/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (false, $"Owner with ID {id} not found.");
                    }
                    string error = await response.Content.ReadAsStringAsync();
                    return (false, $"API Error ({response.StatusCode}): {error}");
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error deleting owner {id}: {ex.Message}");
                return (false, $"Client Error: {ex.Message}");
            }
        }

        // SHIP

        public async Task<List<ShipDto>?> GetShipsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/ships");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ShipDto>>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"API Error getting ships: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error getting ships: {ex.Message}");
                return null;
            }
        }

        public async Task<ShipDetailsDto?> GetShipDetailsAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"api/ships/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ShipDetailsDto>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"API Error getting ship details {id}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error getting ship details {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<(ShipDetailsDto? CreatedShip, string? ErrorMessage)> CreateShipAsync(CreateShipDto ship)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/ships", ship);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return (null, $"API Error ({response.StatusCode}): {error}");
                }
                return (await response.Content.ReadFromJsonAsync<ShipDetailsDto>(), null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error creating ship: {ex.Message}");
                return (null, $"Client Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateShipAsync(int id, UpdateShipDto ship)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/ships/{id}", ship);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (false, $"Ship with ID {id} not found.");
                    }
                    string error = await response.Content.ReadAsStringAsync();
                    return (false, $"API Error ({response.StatusCode}): {error}");
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error updating ship {id}: {ex.Message}");
                return (false, $"Client Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteShipAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"api/ships/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (false, $"Ship with ID {id} not found.");
                    }
                    string error = await response.Content.ReadAsStringAsync();
                    return (false, $"API Error ({response.StatusCode}): {error}");
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected Error deleting ship {id}: {ex.Message}");
                return (false, $"Client Error: {ex.Message}");
            }
        }
    }
}
