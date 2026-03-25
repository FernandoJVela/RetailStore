namespace RetailStore.Api.Features.Shipping.Domain;
 
public enum ShipmentStatus
{
    Pending,        // Created, waiting to be processed
    Processing,     // Being packed/prepared
    Shipped,        // Handed to carrier
    InTransit,      // Carrier confirmed pickup, en route
    Delivered,      // Delivered to customer
    Failed,         // Delivery attempt failed
    Returned,       // Returned to sender
    Cancelled       // Cancelled before shipping
}