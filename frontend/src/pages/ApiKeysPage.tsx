import { useEffect, useState } from "react";
import type { BucketConfigDTO } from "../types/BucketConfigDTO";
import apiClient from "../api/apiClient";
import DataTable from "../components/DataTable";
import { Box, Button, CircularProgress, Container, Modal, Typography } from "@mui/material";
import EditForm from "../components/EditForm";
import type { KeyEditValue } from "../types/KeyEditValue";
import CreateKeyForm from "../components/CreateKeyForm";

function ApiKeysPage() {
  const [apiKeys, setApiKeys] = useState<Record<string, BucketConfigDTO>>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editKey, setEditKey] = useState<KeyEditValue | null>(null);
  const [newKey, setNewKey] = useState(false);

  const refreshKeys = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiClient.get("/v1/api-keys");
      setApiKeys(res.data);
    } catch (e) {
      console.error(e);
      setError("Failed to load API keys");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshKeys();
  }, []);

  return (
    <Container maxWidth="lg" sx={{ pt: 4 }}>
      {loading && (
        <Box display="flex" mt={4} flexDirection="column" alignItems="center" gap={2}>
          <Typography fontFamily="monospace">Getting API keys…</Typography>
          <CircularProgress />
        </Box>
      )}

      {!loading && error && (
        <Typography color="error" mt={2}>
          {error}
        </Typography>
      )}

      {!loading && !error && (
        <Box>
          <Box display="flex" justifyContent="space-between" alignItems="flex-end" mb={2}>
            <Box>
              <Typography variant="h5" fontWeight={700}>
                API Keys
              </Typography>
              <Typography variant="body2" color="text.secondary">
                View and manage rate limits for each key.
              </Typography>
            </Box>

            <Button variant="contained" onClick={() => setNewKey(true)}>
              Create API Key
            </Button>
          </Box>

          <DataTable rows={apiKeys} setEditKey={setEditKey} />
        </Box>
      )}

      {newKey && (
        <Modal open onClose={() => setNewKey(false)}>
          <CreateKeyForm onClose={() => setNewKey(false)} onCreate={refreshKeys} />
        </Modal>
      )}

      {editKey && (
        <Modal open onClose={() => setEditKey(null)}>
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

export default ApiKeysPage;
