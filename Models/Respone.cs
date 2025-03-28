namespace OMS.Core.Commands
{
    public sealed class CommandResponse
    {
        public ResponseStatus Status { get; private set; }

        public string ErrorCode { get; private set; }

        public string ErrorMessage { get; private set; }

        private CommandResponse(ResponseStatus status, string errorMessage)
        {
            Status = status;
            ErrorCode = "Unknown";
            ErrorMessage = errorMessage;
        }

        private CommandResponse(ResponseStatus status, string errorCode, string errorMessage)
        {
            Status = status;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static CommandResponse Error(string errorMessage) => new CommandResponse(ResponseStatus.Error, errorMessage);

        public static CommandResponse Error(string errorCode, string errorMessage) => new CommandResponse(ResponseStatus.Error, errorCode, errorMessage);

        public static CommandResponse Success = new CommandResponse(ResponseStatus.Success, string.Empty);

        public static CommandResponse Conflict = new CommandResponse(ResponseStatus.Conflict, string.Empty);
    }

    public sealed class CommandResponse<T>
    {
        public ResponseStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }
        public string Message { get; private set; }
        public T? Data { get; private set; }

        private CommandResponse(ResponseStatus status, string errorMessage, T? data, string message = "success")
        {
            Status = status;
            ErrorMessage = errorMessage;
            Data = data;
            Message = message;
        }

        public static CommandResponse<T> Error(string errorMessage)
            => new CommandResponse<T>(ResponseStatus.Error, errorMessage, default, string.Empty);

        public static CommandResponse<T> Error(T? data)
            => new CommandResponse<T>(ResponseStatus.Error, string.Empty, data, string.Empty);

        public static CommandResponse<T> Success(T? data)
            => new CommandResponse<T>(ResponseStatus.Success, string.Empty, data);

        public static CommandResponse<T> Success(T? data, string errorMessage)
            => new CommandResponse<T>(ResponseStatus.Success, errorMessage, data);

        public static CommandResponse<T> Conflict = new CommandResponse<T>(ResponseStatus.Conflict, string.Empty, default);
    }

    public enum ResponseStatus
    {
        Success = 0,
        Error = 1,
        Conflict = 2,
    }
}
