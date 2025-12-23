import { useEffect, useState } from "react";
import type { BucketConfigDTO } from "../types/BucketConfigDTO";
import apiClient from "../api/apiClient";
import DataTable from "../components/DataTable";
import { Box, CircularProgress, Container, Typography } from "@mui/material";

function ApiKeysPage() {
  const [apiKeys, setApiKeys] = useState<Record<string, BucketConfigDTO>>({});
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const res = await apiClient.get("/v1/api-keys");
        setApiKeys(res.data);
      } catch (err) {
        console.error("Error loading keys", err);
        setError("Failed to load API keys");
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <Container maxWidth="lg" sx={{ pt: 4 }}>
      {loading && (
        <Box
          display="flex"
          mt={4}
          flexDirection="column"
          alignItems="center"
          gap={2}
        >
          <Typography fontFamily="monospace">
            Getting API keys…
          </Typography>
          <CircularProgress />
        </Box>
      )}

      {!loading && error && (
        <Typography color="error" mt={2}>
          {error}
        </Typography>
      )}

      {!loading && !error && (
        <DataTable rows={apiKeys} />
      )}
    </Container>
  );
}

export default ApiKeysPage