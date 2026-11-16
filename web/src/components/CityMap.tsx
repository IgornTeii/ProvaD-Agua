import type { ReactNode } from "react";
import { CircleMarker, MapContainer, Popup, TileLayer, Tooltip } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { TONE_COLOR, type Tone } from "../status";

export interface MapPoint {
  id: string;
  latitude: number;
  longitude: number;
  tone: Tone;
  label: string;
  selected?: boolean;
  popup?: ReactNode;
  onSelect?: () => void;
}

const SAO_PAULO: [number, number] = [-23.55, -46.58];

export default function CityMap({ points, height = "55vh" }: { points: MapPoint[]; height?: string }) {
  return (
    <div className="map" style={{ height }}>
      <MapContainer center={SAO_PAULO} zoom={11} scrollWheelZoom style={{ height: "100%" }}>
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {points.map((point) => (
          <CircleMarker
            key={point.id}
            center={[point.latitude, point.longitude]}
            radius={point.selected ? 16 : 11}
            pathOptions={{
              color: point.selected ? "#0b2545" : "#ffffff",
              weight: point.selected ? 4 : 2,
              fillColor: TONE_COLOR[point.tone],
              fillOpacity: 0.9,
            }}
            eventHandlers={point.onSelect ? { click: point.onSelect } : undefined}
          >
            <Tooltip direction="top" offset={[0, -10]}>{point.label}</Tooltip>
            {point.popup && <Popup>{point.popup}</Popup>}
          </CircleMarker>
        ))}
      </MapContainer>
    </div>
  );
}
