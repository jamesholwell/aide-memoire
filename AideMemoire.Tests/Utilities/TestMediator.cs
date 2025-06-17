using MediatR;

namespace AideMemoire.Tests.Utilities;

public class TestMediator : IMediator {
    private readonly List<INotification> _publishedNotifications = new();

    public IReadOnlyList<INotification> PublishedNotifications => _publishedNotifications.AsReadOnly();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("Send is not implemented in TestMediator");

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
        throw new NotImplementedException("Send is not implemented in TestMediator");

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("Send is not implemented in TestMediator");

    public Task Publish(object notification, CancellationToken cancellationToken = default) {
        if (notification is INotification notificationObj) {
            _publishedNotifications.Add(notificationObj);
        }

        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification {
        _publishedNotifications.Add(notification);

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("CreateStream is not implemented in TestMediator");

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("CreateStream is not implemented in TestMediator");
}
