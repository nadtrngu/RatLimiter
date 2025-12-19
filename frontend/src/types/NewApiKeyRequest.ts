export interface NewApiKeyRequest {
  name: string;
  description?: string | null;
  status: 'ACTIVE' | 'DISABLED';
  algorithm?: 'TokenBucket';
  refillRate?: number;
  capacity?: number;
}