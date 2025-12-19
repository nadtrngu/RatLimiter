export interface BucketConfigDTO {
  name: string;
  algorithm: string;
  capacity: number;
  status: string;
  refillRate: number;
  createdAt: string; // ISO string from DateTimeOffset
  description: string;
}