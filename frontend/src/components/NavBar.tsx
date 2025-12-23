import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Container from '@mui/material/Container';
import Button from '@mui/material/Button';
import PestControlRodentIcon from '@mui/icons-material/PestControlRodent';
import { useNavigate } from "react-router-dom";

const pages = [{name: 'Api Keys', path: '/api-keys' } , { name: 'Telemetry', path: '/telemetry'}];

function NavBar() {
  const navigate = useNavigate();

  return (
    <AppBar
      position="static"
      color="transparent"
      elevation={0}
      sx={{
        backgroundColor: '#111827', // dark gray/near-black
        color: '#F9FAFB',            // off-white text
      }}
    >
      <Container maxWidth="lg" disableGutters>
        <Toolbar>
          <PestControlRodentIcon sx={{ mr: 1 }} />
          <Typography
            variant="h6"
            noWrap
            component="a"
            onClick={() => navigate('/')}
            sx={{
              mr: 4,
              fontFamily: 'monospace',
              fontWeight: 700,
              letterSpacing: '.3rem',
              color: 'inherit',
              textDecoration: 'none',
            }}
          >
            RatLimiter
          </Typography>

          <Box sx={{ flexGrow: 1 }} />

          {pages.map((page) => (
            <Button
              key={page.name}
              sx={{ ml: 2, color: 'inherit' }}
              onClick={() => navigate(page.path)}
            >
              {page.name.toUpperCase()}
            </Button>
          ))}
        </Toolbar>
      </Container>
    </AppBar>
  );
}

export default NavBar;
