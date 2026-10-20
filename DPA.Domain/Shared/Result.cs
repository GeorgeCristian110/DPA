namespace DPA.Domain.Shared;

public class Result
{
    public bool IsSuccess { get;  }
    public Error? Error { get;  }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    
    public static Result Failure(ErrorType type, string message) => new(false, new Error(type, message));
}