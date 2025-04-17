namespace PolicyManagementApp.Api.Models.ApiModels;

public class ErrorResponseModel
{
    public string Error { get; set; } = "An error occurred.";
    public int StatusCode { get; set; }
}
