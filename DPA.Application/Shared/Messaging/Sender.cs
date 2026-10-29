namespace DPA.Application.Shared.Messaging;

public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;
    
    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, 
        CancellationToken cancellationToken) 
    {
       // 1 + 2 Build IRequestHandler<TheRealQueryType, TResponse>
       var handlerType = typeof(IRequestHandler<,>)
           .MakeGenericType(request.GetType(), typeof(TResponse));
       
       // 3 We ask the DI for it and crash loudly if nobody registered one
       var handler = _serviceProvider.GetService(handlerType)
                     ?? throw new InvalidOperationException(
                         $"No handler registered for {request.GetType().Name}");
       
       // Call the Handle through reflection
       var handleMethod = handlerType.GetMethod("Handle")
           ?? throw new InvalidOperationException
                ($"{handlerType.Name} has no Handle method");


       var result = handleMethod.Invoke(handler, [request, cancellationToken])
                    ?? throw new InvalidOperationException
                        ($"{handler.GetType().Name}. Handle returned null ");
      
       return await (Task<TResponse>)result;
    }
}