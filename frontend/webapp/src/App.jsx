import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

export default function App() {
  const [messages, setMessages] = useState([]);
  const [status, setStatus] = useState("desconectado");

  useEffect(() => {
    const url = import.meta.env.VITE_SIGNALR_URL || "https://localhost:5001/hub";
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .build();

    connection.on("NewMessage", (msg) => {
      console.log("NewMessage", msg);
      setMessages((m) => [msg, ...m]);
    });

    connection.start()
      .then(() => setStatus("conectado"))
      .catch((err) => {
        console.error(err);
        setStatus("erro");
      });

    return () => connection.stop();
  }, []);

  return (
    <div style={{ padding: 20 }}>
      <h2>Painel WhatsApp Proxy (MVP 1)</h2>
      <p>Status: <strong>{status}</strong></p>
      <div>
        {messages.length === 0 && <p>Nenhuma mensagem recebida ainda.</p>}
        {messages.map((m, i) => (
          <div key={i} style={{ border: "1px solid #ddd", padding: 8, marginBottom: 8 }}>
            <div><strong>{m.contactName || m.contactNumber}</strong> ({m.contactNumber})</div>
            <div style={{ marginTop: 6 }}>{m.text}</div>
            <div style={{ color: "#666", marginTop: 6, fontSize: 12 }}>{new Date(m.timestamp).toLocaleString()}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
