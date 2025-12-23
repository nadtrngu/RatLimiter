import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Paper from "@mui/material/Paper";
import type { BucketConfigDTO } from "../types/BucketConfigDTO";
import { EditOutlined } from "@mui/icons-material";
import { IconButton } from "@mui/material";
import type { KeyEditValue } from "../types/KeyEditValue";

type DataTableProps = {
  rows: Record<string, BucketConfigDTO>;
  setEditKey: React.Dispatch<React.SetStateAction<KeyEditValue | null>>;
};

function DataTable({ rows, setEditKey }: DataTableProps) {
  const entries = Object.entries(rows);

  if (entries.length === 0) {
    return (
      <Paper sx={{ p: 4, textAlign: "center" }}>
        No API keys yet. Create one to get started.
      </Paper>
    );
  }

  return (
    <TableContainer component={Paper}>
      <Table sx={{ minWidth: 650 }} aria-label="api keys table">
        <TableHead>
          <TableRow>
            <TableCell>API Key</TableCell>
            <TableCell>Name</TableCell>
            <TableCell align="right">Capacity</TableCell>
            <TableCell align="right">Refill / sec</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Algorithm</TableCell>
            <TableCell>Created</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {entries.map(([apiKey, cfg]) => (
            <TableRow key={apiKey} hover>
              <TableCell>{apiKey}</TableCell>
              <TableCell>{cfg.Name}</TableCell>
              <TableCell align="right">{cfg.Capacity}</TableCell>
              <TableCell align="right">{cfg.RefillRate}</TableCell>
              <TableCell>{cfg.Status}</TableCell>
              <TableCell>{cfg.Algorithm}</TableCell>
              <TableCell>
                {cfg.CreatedAt
                  ? new Date(cfg.CreatedAt).toLocaleDateString()
                  : "-"}
              </TableCell>
              <TableCell>
                <IconButton
                  size="small"
                  onClick={() =>
                    setEditKey({
                      apiKey,
                      valuesToUpdate: {
                        Capacity: cfg.Capacity,
                        RefillRate: cfg.RefillRate,
                        Algorithm: 0
                      }
                    })
                  }
                >
                  <EditOutlined fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

export default DataTable;
