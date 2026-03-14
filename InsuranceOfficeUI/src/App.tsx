import { useState, useRef, useEffect } from "react";
import "./App.css";

interface Message {
  role: "user" | "assistant";
  content: string;
}

const API_BASE = "http://localhost:5100";

const SUGGESTIONS = [
  "→ Quanto costa assicurare un'auto da €25.000 per un 35enne?",
  "→ Confronta le coperture vita delle 3 compagnie",
  "→ Qual è la compagnia più conveniente per una casa da €300.000?",
  "→ Cosa copre The Three Lines per l'auto?",
];

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, loading]);

  const adjustTextarea = () => {
    const ta = textareaRef.current;
    if (!ta) return;
    ta.style.height = "auto";
    ta.style.height = Math.min(ta.scrollHeight, 160) + "px";
  };

  const sendMessage = async (text: string) => {
    if (!text.trim() || loading) return;

    const userMsg: Message = { role: "user", content: text.trim() };
    const newMessages = [...messages, userMsg];
    setMessages(newMessages);
    setInput("");
    setLoading(true);

    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
    }

    try {
      const res = await fetch(`${API_BASE}/api/chat`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          message: text.trim(),
          history: messages.map((m) => ({ role: m.role, content: m.content })),
        }),
      });

      if (!res.ok) throw new Error(`HTTP ${res.status}`);

      const data = await res.json();
      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: data.reply },
      ]);
    } catch (err) {
      setMessages((prev) => [
        ...prev,
        {
          role: "assistant",
          content:
            "⚠ Errore di connessione. Assicurati che InsuranceOfficeApi e il Proxy siano in esecuzione.",
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage(input);
    }
  };

  return (
    <div className="app">
      {/* Header */}
      <header className="header">
        <div className="header-left">
          <h1>Insurance Office</h1>
          <div className="tagline">Multi-Company Assistant</div>
        </div>
        <div className="status-row">
          <div className="status-dot" />
          <span>3 compagnie connesse</span>
        </div>
      </header>

      {/* Companies bar */}
      <div className="companies-bar">
        <span className="company-pill">The Lion Insurance</span>
        <span className="company-pill">The Blue Company</span>
        <span className="company-pill">The Three Lines</span>
      </div>

      {/* Messages */}
      {messages.length === 0 && !loading ? (
        <div className="welcome">
          <div className="welcome-icon">🏛</div>
          <h2>Come posso aiutarti?</h2>
          <p>
            Chiedi un preventivo, confronta le coperture o scopri quale
            compagnia è più adatta alle tue esigenze.
          </p>
          <div className="suggestions">
            {SUGGESTIONS.map((s, i) => (
              <button
                key={i}
                className="suggestion"
                onClick={() => sendMessage(s.replace("→ ", ""))}
              >
                {s}
              </button>
            ))}
          </div>
        </div>
      ) : (
        <div className="messages">
          {messages.map((msg, i) => (
            <div key={i} className={`message ${msg.role}`}>
              <div className="message-role">
                {msg.role === "user" ? "Tu" : "Assistente"}
              </div>
              <div
                className="message-bubble"
                dangerouslySetInnerHTML={{
                  __html: msg.content
                    .replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>")
                    .replace(/\n/g, "<br/>"),
                }}
              />
            </div>
          ))}

          {loading && (
            <div className="message assistant">
              <div className="message-role">Assistente</div>
              <div className="thinking">
                <div className="thinking-dots">
                  <span />
                  <span />
                  <span />
                </div>
                Consultando le compagnie...
              </div>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>
      )}

      {/* Input */}
      <div className="input-area">
        <div className="input-row">
          <div className="input-wrapper">
            <textarea
              ref={textareaRef}
              value={input}
              onChange={(e) => {
                setInput(e.target.value);
                adjustTextarea();
              }}
              onKeyDown={handleKeyDown}
              placeholder="Scrivi un messaggio..."
              rows={1}
              disabled={loading}
            />
          </div>
          <button
            className="send-btn"
            onClick={() => sendMessage(input)}
            disabled={!input.trim() || loading}
            title="Invia (Enter)"
          >
            ↑
          </button>
        </div>
        <div className="input-hint">
          Enter per inviare · Shift+Enter per andare a capo
        </div>
      </div>
    </div>
  );
}
