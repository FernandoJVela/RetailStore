export { shippingApi } from './api/shipping.api';
export type * from './api/shipping.dto';
export type { 
    Shipment, 
    ShipmentDetail, 
    ShipmentItem, 
    ShippingAddress, 
    ShipmentStatus, 
    AssignCarrierData } from './domain/shipping.model';
export { shipmentStatusVariant, STATUS_STEPS } from './domain/shipping.model';