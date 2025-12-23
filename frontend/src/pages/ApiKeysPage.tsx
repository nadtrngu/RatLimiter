import { useEffect, useState } from "react";
import type { BucketConfigDTO } from "../types/BucketConfigDTO";
import apiClient from "../api/apiClient";
import DataTable from "../components/DataTable";
import { Box, CircularProgress, Container, Modal, Typography } from "@mui/material";
import EditForm from "../components/EditForm";
import type { KeyEditValue } from "../types/KeyEditValue";

function ApiKeysPage() {
  const [apiKeys, setApiKeys] = useState<Record<string, BucketConfigDTO>>({});
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [editKey, setEditKey] = useState<KeyEditValue | null>(null);

  const refreshKeys = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get("/v1/api-keys");
      setApiKeys(res.data);
    }
    catch (e: any) {
      setError(e);
    }
    finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshKeys();
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
        <DataTable rows={apiKeys} setEditKey={setEditKey} />
      )}
      {editKey && (<Modal
        open={editKey !== null}
        onClose={() => setEditKey(null)}
      >
        <EditForm
          apiKey={editKey.apiKey}
          formValues={editKey.valuesToUpdate}
          onClose={() => setEditKey(null)}
          onUpdated={refreshKeys}
        />
      </Modal>
      )}
    </Container>
  );
}

export default ApiKeysPage