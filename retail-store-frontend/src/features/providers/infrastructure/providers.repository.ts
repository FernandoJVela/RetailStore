import { providersApi, mapProviderDto, mapProviderDetailDto } from '@features/providers';
import type { Provider, ProviderDetail, RegisterProviderData, UpdateProviderData } from '@features/providers';
 
/** Repository: calls API, maps through mapper, returns domain models. */
export const providersRepository = {
  async getAll(params?: { search?: string; isActive?: boolean }): Promise<Provider[]> {
    const { data } = await providersApi.getAll(params);
    return data.map(mapProviderDto);
  },
 
  async getById(id: string): Promise<ProviderDetail> {
    const { data } = await providersApi.getById(id);
    return mapProviderDetailDto(data);
  },
 
  async register(input: RegisterProviderData): Promise<string> {
    const { data } = await providersApi.register({
      companyName: input.companyName,
      contactName: input.contactName,
      email: input.email,
      phone: input.phone || null,
    });
    return data;
  },
 
  async update(id: string, input: UpdateProviderData): Promise<void> {
    await providersApi.update(id, {
      companyName: input.companyName,
      contactName: input.contactName,
      phone: input.phone || null,
    });
  },
 
  async changeEmail(id: string, newEmail: string): Promise<void> {
    await providersApi.changeEmail(id, { newEmail });
  },
 
  async associateProduct(providerId: string, productId: string): Promise<void> {
    await providersApi.associateProduct(providerId, productId);
  },
 
  async dissociateProduct(providerId: string, productId: string): Promise<void> {
    await providersApi.dissociateProduct(providerId, productId);
  },
 
  async deactivate(id: string): Promise<void> {
    await providersApi.deactivate(id);
  },
 
  async reactivate(id: string): Promise<void> {
    await providersApi.reactivate(id);
  },
};