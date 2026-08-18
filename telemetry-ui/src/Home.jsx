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
import {
  Navbar,
  Nav,
  Container,
} from "react-bootstrap";
import {
  BrowserRouter,
  Routes,
  Route,
  Link,
} from "react-router-dom";

function Home() {
  const [sessions, SetSessions] = useState([]);

  useEffect(() => {
    fetch("http://localhost:5000/api/sessions").then((response) => response.json()).then((data) => {
      SetSessions(data);
    }).catch((error) => {
      console.error("Error fetching sessions:", error);
    });
  }, []);

  return (
    <>
      <div
        style={{
          position: "absolute",
          top: "20px",
          right: "20px"
        }}
      >
      </div>
      <div>
        <h1>Sessions</h1>

        {sessions.map((session) => (
          <div key={session.name}>
            {session.name}
          </div>
        ))}
      </div>
    </>
  );
}
export default Home;