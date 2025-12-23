import { useEffect, useState } from "react";
import type { BucketConfigDTO } from "../types/BucketConfigDTO";
import apiClient from "../api/apiClient";

function ApiKeysPage() {
    const [apiKeys, setApiKeys] = useState<Record<string, BucketConfigDTO>>({});

    useEffect(() => {
    const load = async () => {
      try {
        const res = await apiClient.get("/v1/api-keys");
        setApiKeys(res.data);
      } catch (err) {
        console.error("Error loading keys", err);
      }
    };

    load();
    console.log(apiKeys);
    }, []);

    return (
        <>
        </>
    );
}

export default ApiKeysPage