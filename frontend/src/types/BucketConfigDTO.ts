export interface BucketConfigDTO {
  Name: string;
  Algorithm: string;
  Capacity: number;
  Status: string;
  RefillRate: number;
  CreatedAt: string; // ISO string from DateTimeOffset
  Description: string;
}