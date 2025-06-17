using MediatR;

namespace AideMemoire.Domain.Events;

public record MemoryUpdated(Memory Memory) : INotification;
