import { apiClient } from './index';

describe('apiClient', () => {
  it('should work', () => {
    expect(apiClient()).toEqual('api-client');
  });
});
