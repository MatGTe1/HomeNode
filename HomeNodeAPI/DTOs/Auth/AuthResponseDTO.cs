namespace HomeNodeAPI.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }

        public AuthResponseDTO(bool isSuccess, string? token, string? message)
        {
            IsSuccess = isSuccess;
            Token = token;
            Message = message;
        }
    }
}
