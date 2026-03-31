import { customersApi } from '@features/customers';
import type {
  Customer, CustomerDetail, RegisterCustomerData,
  UpdateCustomerData, UpdateAddressData,
} from '@features/customers';
import { mapCustomerDto, mapCustomerDetailDto } from '@features/customers/application/mappers/customers.mapper';
 
/** Repository: calls API service, maps through mapper, returns domain models. */
export const customersRepository = {
  async getAll(params?: { search?: string; isActive?: boolean }): Promise<Customer[]> {
    const { data } = await customersApi.getAll(params);
    return data.map(mapCustomerDto);
  },
 
  async getById(id: string): Promise<CustomerDetail> {
    const { data } = await customersApi.getById(id);
    return mapCustomerDetailDto(data);
  },
 
  async register(input: RegisterCustomerData): Promise<string> {
    const { data } = await customersApi.register({
      firstName: input.firstName,
      lastName: input.lastName,
      email: input.email,
      phone: input.phone || null,
      shippingAddress: input.shippingAddress || null,
    });
    return data;
  },
 
  async update(id: string, input: UpdateCustomerData): Promise<void> {
    await customersApi.update(id, {
      firstName: input.firstName,
      lastName: input.lastName,
      phone: input.phone || null,
    });
  },
 
  async changeEmail(id: string, newEmail: string): Promise<void> {
    await customersApi.changeEmail(id, { newEmail });
  },
 
  async updateAddress(id: string, address: UpdateAddressData): Promise<void> {
    await customersApi.updateAddress(id, address);
  },
 
  async deactivate(id: string): Promise<void> {
    await customersApi.deactivate(id);
  },
 
  async reactivate(id: string): Promise<void> {
    await customersApi.reactivate(id);
  },
};