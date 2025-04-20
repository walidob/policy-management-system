namespace PolicyManagementApp.Api.Models.ApiModels;

public class ErrorResponseModel
{
    public string Message { get; set; } = "An error occurred.";
    public string? TraceId { get; set; }
    public string? StackTrace { get; set; }
    public int StatusCode { get; set; } = 500;
}
