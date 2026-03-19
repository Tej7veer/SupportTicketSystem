namespace SupportTicketDesktop.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

public class Ticket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TicketStatusHistory
{
    public int Id { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = "";
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Remarks { get; set; }
}

public class TicketComment
{
    public int Id { get; set; }
    public string CommentText { get; set; } = "";
    public bool IsInternal { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedByRole { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketDetailResponse
{
    public Ticket Ticket { get; set; } = new();
    public List<TicketStatusHistory> History { get; set; } = new();
    public List<TicketComment> Comments { get; set; } = new();
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}

public static class AppSession
{
    public static string Token { get; set; } = "";
    public static int UserId { get; set; }
    public static string Username { get; set; } = "";
    public static string FullName { get; set; } = "";
    public static string Role { get; set; } = "";
    public static bool IsAdmin => Role == "Admin";

    public static void Clear()
    {
        Token = ""; UserId = 0; Username = ""; FullName = ""; Role = "";
    }
}