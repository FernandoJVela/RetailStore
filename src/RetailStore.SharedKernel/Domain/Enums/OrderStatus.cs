namespace RetailStore.SharedKernel.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,
    Pending = 1,
    Confirmed = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5
}