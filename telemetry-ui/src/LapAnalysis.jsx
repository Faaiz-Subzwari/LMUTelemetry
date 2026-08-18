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

function LapAnalysis() {

      const [telemetryData, setTelemetryData] = useState([]);
      const [lapNumber, setLapNumber] = useState(1)
      const [laps, setLaps] = useState({});
      const [selectedLap, setSelectedLap] = useState(1);
    
      const handleFileUpload = async (event) => {
        const file = event.target.files[0];
    
        if (!file) return;
    
        const text = await file.text();
        const telemetry = text.split("\n").filter(line => line.trim() !== "").map(line => JSON.parse(line));
    
        console.log(telemetry);
    
        setTelemetryData(telemetry);
    
        const groupedLaps = {};
    
        telemetry.forEach(sample => {
          if (!groupedLaps[sample.lap]) {
            groupedLaps[sample.lap] = [];
          }
    
          groupedLaps[sample.lap].push(sample);
        });
    
        setLaps(groupedLaps);
    
      };

      const formatTime = (totalSeconds) =>{
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = Math.floor(totalSeconds % 60);
        const milliseconds = Math.floor((totalSeconds * 1000) % 1000);        
      }
    
      useEffect(() => {
        console.log("WEBSOCKET EFFECT RAN");
    
        const ws = new WebSocket("ws://localhost:8181");
    
        ws.onerror = (error) => {
          // This triggers the generic Event object you saw
          console.error('WebSocket Error observed:', error);
        };
    
        ws.onclose = (event) => {
          // This provides more detail on why it closed
          console.log(`Code: ${event.code}, Reason: ${event.reason}`);
        };
    
        ws.onmessage = async (event) => {
          const text = await event.data;
    
          const telemetry = JSON.parse(text);
    
          setLapNumber((prev) => {
            const next = telemetry.lap;
    
            if (next === undefined || next === null) {
              return prev; // ignore bad updates
            }
    
            return next;
          });
    
          setTelemetryData((prev) => {
            const MAX_POINTS = 50;
            const nextPoint = {
              time: prev.length,
              throttle: telemetry.throttle,
              brake: telemetry.brake,
              speed: telemetry.speed
            };
    
            if (prev.length >= MAX_POINTS) {
              return [...prev.slice(1), nextPoint];
            }
    
            return [...prev, nextPoint];
          });
        };
    
        ws.onerror = (err) => {
          console.error(err);
        };
    
        return () => {
          console.log("WEBSOCKET CLEANUP");
          ws.close();
        };
      }, []);

    return (
        <>
            <div className="LapAnalysis">
                <h1>Lap Analysis</h1>
                <input
                    type="file"
                    accept=".txt"
                    onChange={handleFileUpload}
                />

                <input
                    type="number"
                    value={selectedLap}
                    onChange={(e) => setSelectedLap(Number(e.target.value))}
                />

                <h2> Laps: {Object.keys(laps).length} </h2>
                <h2> Lap time: </h2>
            </div>

            <LineChart width={900} height={300} data={laps[selectedLap]}>
                <YAxis domain={[0, 100]} />
                <Tooltip cursor={true} />
                <Line
                    type="linear"
                    dataKey="throttle"
                    stroke="#00ff00"
                    dot={false}
                    isAnimationActive={false}
                    label={(props) => {
                        const isLastPoint = props.index === telemetryData.length - 1;
                        if (!isLastPoint) return null;
                        return (
                            <text
                                x={props.x - 20}
                                y={props.y + 20}
                                textAnchor="middle"
                                stroke="#00ff00"
                            >
                                {props.value.toFixed(1)}
                            </text>
                        );
                    }}
                />
            </LineChart>
                <LineChart width={900} height={300} data={laps[selectedLap]}>
                <YAxis domain={[0, 100]} />
                <Tooltip cursor={true} />
                <Line
                    type="linear"
                    dataKey="brake"
                    stroke="#ea0000"
                    dot={false}
                    isAnimationActive={false}
                    label={(props) => {
                        const isLastPoint = props.index === telemetryData.length - 1;
                        if (!isLastPoint) return null;
                        return (
                            <text
                                x={props.x - 20}
                                y={props.y + 20}
                                textAnchor="middle"
                                stroke="#00ff00"
                            >
                                {props.value.toFixed(1)}
                            </text>
                        );
                    }}
                />
            </LineChart>

                            <LineChart width={900} height={300} data={laps[selectedLap]}>
                <YAxis domain={[0, 100]} />
                <Tooltip cursor={true} />
                <Line
                    type="linear"
                    dataKey="speed"
                    stroke="#0000ff"
                    dot={false}
                    isAnimationActive={false}
                    label={(props) => {
                        const isLastPoint = props.index === telemetryData.length - 1;
                        if (!isLastPoint) return null;
                        return (
                            <text
                                x={props.x - 20}
                                y={props.y + 20}
                                textAnchor="middle"
                                stroke="#00ff00"
                            >
                                {props.value.toFixed(1)}
                            </text>
                        );
                    }}
                />
            </LineChart>

        </>
        
    )
}

export default LapAnalysis;