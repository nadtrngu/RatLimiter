export interface NewApiKeyRequest {
  Name: string;
  Description?: string | null;
  Status: number;
  Algorithm?: 0;
  RefillRate?: number;
  Capacity?: number;
}