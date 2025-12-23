import type { LimitUpdateRequest } from "./LimitUpdateRequest";

export interface KeyEditValue {
    apiKey: string,
    valuesToUpdate: LimitUpdateRequest
}