namespace pdf_generator_service.Enums
{
    public enum ErrorCodeEnum
    {
        Unknown = 0,
        InvalidInput = 1,
        NotFound = 2,
        UnAuthentication = 3,
        UnAuthorization = 4,
        Forbidden = 5,
        Conflict = 6,
        InternalError = 7,
        ServiceUnavailable = 8
    }
}