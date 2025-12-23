import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Paper from "@mui/material/Paper";
import type { BucketConfigDTO } from "../types/BucketConfigDTO";

type DataTableProps = {
  rows: Record<string, BucketConfigDTO>;
};

function DataTable({ rows }: DataTableProps) {
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
                  ? new Date(cfg.CreatedAt).toLocaleString()
                  : "-"}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

export default DataTable;
