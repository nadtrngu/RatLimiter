import { useState } from "react";
import type { NewApiKeyRequest } from "../types/NewApiKeyRequest";
import apiClient from "../api/apiClient";
import Box from "@mui/material/Box";
import { Button, FormControl, FormControlLabel, FormLabel, Radio, RadioGroup, Stack, TextField } from "@mui/material";

type CreateKeyFormProps = {
  onClose: () => void;
  onCreate: () => Promise<void>;
};

function CreateKeyForm({ onClose, onCreate }: CreateKeyFormProps) {
  const [name, setName] = useState<string>("");
  const [status, setStatus] = useState<string>("0");
  const [description, setDescription] = useState<string>("");
  const [capacity, setCapacity] = useState<string>('100');
  const [refillRate, setRefillRate] = useState<string>('5');
  const [saving, setSaving] = useState<boolean>(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      const payload: NewApiKeyRequest = {
        Capacity: Number(capacity),
        RefillRate: Number(refillRate),
        Algorithm: 0,
        Name: name,
        Status: Number(status),
        Description: description.trim() === "" ? null : description.trim()
      };

      await apiClient.post(`/v1/api-keys/`, payload);

      await onCreate();
      onClose();
    }
    catch (err) {
      console.error("Failed to update ", err);
    }
    finally {
      setSaving(false);
    }
  }
  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{
        position: "absolute",
        top: "50%",
        left: "50%",
        transform: "translate(-50%, -50%)",
        bgcolor: "background.paper",
        p: 4,
        borderRadius: 2,
        boxShadow: 24,
        minWidth: 360,
      }}
    >
      <Stack spacing={2}>
        <TextField
          label="Name"
          type="text"
          required
          fullWidth
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <TextField
          label="Description"
          multiline
          fullWidth
          rows={4}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <TextField
          label="Capacity"
          type="number"
          required
          fullWidth
          value={capacity}
          InputProps={{
            inputProps: {
              min: 0
            }
          }}
          onChange={(e) => setCapacity(e.target.value)}
        />
        <TextField
          label="Refill rate (per second)"
          type="number"
          required
          fullWidth
          value={refillRate}
          InputProps={{
            inputProps: {
              min: 0
            }
          }}
          onChange={(e) => setRefillRate(e.target.value)}
        />

        <FormControl>
          <FormLabel>Algorithm</FormLabel>
          <RadioGroup value="0">
            <FormControlLabel
              value="0"
              control={<Radio />}
              label="Token Bucket"
            />
          </RadioGroup>
        </FormControl>
        <FormControl>
          <FormLabel>Status</FormLabel>
          <RadioGroup
            value={status}
            onChange={(e) => setStatus(e.target.value)}
          >
            <FormControlLabel value="0" control={<Radio />} label="Active" />
            <FormControlLabel value="1" control={<Radio />} label="Disabled" />
          </RadioGroup>
        </FormControl>
        <Stack direction="row" justifyContent="flex-end" spacing={2}>
          <Button onClick={onClose} disabled={saving}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={saving}>
            {saving ? "Saving…" : "Save"}
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}

export default CreateKeyForm