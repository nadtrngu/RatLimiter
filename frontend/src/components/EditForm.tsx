import { Box, Button, FormControl, FormControlLabel, FormLabel, Radio, RadioGroup, Stack, TextField } from "@mui/material";
import { useState } from "react";
import apiClient from "../api/apiClient";
import type { LimitUpdateRequest } from "../types/LimitUpdateRequest";

type EditFormProps = {
  apiKey: string;
  formValues: LimitUpdateRequest;
  onClose: () => void;
  onUpdated: () => Promise<void>;
};

function EditForm({ apiKey, formValues, onClose, onUpdated }: EditFormProps) {
  const [capacity, setCapacity] = useState<string>(formValues.Capacity.toString());
  const [refillRate, setRefillRate] = useState<string>(formValues.RefillRate.toString());
  const [saving, setSaving] = useState<boolean>(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      const payload: LimitUpdateRequest = {
        Capacity: Number(capacity),
        RefillRate: Number(refillRate),
        Algorithm: 0
      };

      apiClient.put(`/v1/api-keys/${apiKey}/limits`, payload);

      await onUpdated();
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

export default EditForm