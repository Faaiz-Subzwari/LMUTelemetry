import { useEffect, useState } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Brush,
} from "recharts";
import { Navbar,
         Nav,
         Container,
} from "react-bootstrap";
import {
  BrowserRouter,
  Routes,
  Route,
  Link,
} from "react-router-dom";

import LapAnalysis from "./LapAnalysis";
import Home from "./Home";

function App() {
  return (
      <BrowserRouter>
        <Navbar expand = "lg" className="bg-body-tertiary">
          <Container>
            <Navbar.Collapse id="basic-navbar-nav" />
            <Nav className="me-auto">
              <Nav.Link as={Link} to="/Home">
                Home
              </Nav.Link>
              <Nav.Link as={Link} to="/LapAnalysis">
                Lap Analysis
              </Nav.Link>
            </Nav>
          </Container>
        </Navbar>
        <Routes>
          <Route path="/Home" element={<Home />} />
          <Route path="/LapAnalysis" element={<LapAnalysis />} />
        </Routes>
      </BrowserRouter>
  );
}
export default App;