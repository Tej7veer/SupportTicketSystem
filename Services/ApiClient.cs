using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SupportTicketDesktop.Models;

namespace SupportTicketDesktop.Services;

public class ApiClient
{
    private static readonly HttpClient _http = new();
    private const string BaseUrl = "http://localhost:5000/api";

    public static void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<ApiResponse<T>> SendAsync<T>(
        HttpMethod method, string endpoint, object? body = null)
    {
        try
        {
            var request = new HttpRequestMessage(method, $"{BaseUrl}/{endpoint}");
            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
            return result ?? new ApiResponse<T>
            {
                Success = false,
                Message = "Invalid response from server."
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Cannot connect to server. Is the API running?\n\n{ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    // Auth
    public static Task<ApiResponse<LoginResponse>> Login(string username, string password) =>
        SendAsync<LoginResponse>(HttpMethod.Post, "auth/login",
            new { username, password });

    // Tickets
    public static Task<ApiResponse<List<Ticket>>> GetTickets() =>
        SendAsync<List<Ticket>>(HttpMethod.Get, "tickets");

    public static Task<ApiResponse<TicketDetailResponse>> GetTicketDetail(int id) =>
        SendAsync<TicketDetailResponse>(HttpMethod.Get, $"tickets/{id}");

    public static Task<ApiResponse<Ticket>> CreateTicket(
        string subject, string description, string priority) =>
        SendAsync<Ticket>(HttpMethod.Post, "tickets",
            new { subject, description, priority });

    public static Task<ApiResponse<object>> AssignTicket(int ticketId, int adminUserId) =>
        SendAsync<object>(HttpMethod.Put, $"tickets/{ticketId}/assign",
            new { assignedToUserId = adminUserId });

    public static Task<ApiResponse<object>> UpdateStatus(
        int ticketId, string newStatus, string? remarks = null) =>
        SendAsync<object>(HttpMethod.Put, $"tickets/{ticketId}/status",
            new { newStatus, remarks });

    public static Task<ApiResponse<object>> AddComment(
        int ticketId, string commentText, bool isInternal = false) =>
        SendAsync<object>(HttpMethod.Post, $"tickets/{ticketId}/comments",
            new { commentText, isInternal });

    public static Task<ApiResponse<List<User>>> GetAdmins() =>
        SendAsync<List<User>>(HttpMethod.Get, "tickets/admins");
}