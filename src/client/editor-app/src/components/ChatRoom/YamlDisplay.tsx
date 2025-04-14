import React from "react";
import { useWebSocketContext,ValidationError,WebSocketRoom } from "shared";
import "./YamlDisplay.css";

function renderYamlLine(line: string) {
  const level = line.search(/\S/);
  const indentMatch = line.match(/^(\s*)(.*)$/);
  const indent = indentMatch ? indentMatch[1] : "";
  const rest = indentMatch ? indentMatch[2] : line;

  const colorLevel = ((level - 1) % 8) + 1;
  const indentSpan = indent.length > 0 ? (
    <span
      className={`yaml-indent yaml-indent-level-${colorLevel}`}
      style={{ paddingLeft: `${indent.length}ch` }}
    ></span>
  ) : null;

  // Try to split rest into key: value (split at first colon)
  const colonIdx = rest.indexOf(":");
  if (colonIdx !== -1) {
    const key = rest.slice(0, colonIdx);
    const value = rest.slice(colonIdx + 1);
    return (
      <>
        {indentSpan}
        <span className={`ident${colorLevel}`}>{key}:</span>
        {value.length > 0 && (
          <span className={`yaml-value yaml-value-level-${colorLevel}`}>{value}</span>
        )}
      </>
    );
  }

  return (
    <>
      {indentSpan}
      <span>{rest}</span>
    </>
  );
}

interface GraphOfChartRoomProps {
  roomName: string;
  onErrorCountChange?: (count: number) => void;
}

const YamlDisplay: React.FC<GraphOfChartRoomProps> = ({ roomName, onErrorCountChange }) => {
  const { rooms } = useWebSocketContext();

  // Find the room with the exact roomName
  const room: WebSocketRoom | undefined = rooms.find(
    (r) => r.Name === roomName
  );
  
  // Split YAML into lines for inline error display
  const errors: ValidationError[] = room?.Errors ?? [];
  const yamlContent: string = room?.Yaml ?? ""; // Default to empty string if room is undefined

  // Build a single list of all errors with display line number
  const allErrors = React.useMemo(() => {
    return (errors ?? [])
      .map((e, i) => ({
        ...e,
        _displayIndex: i,
        _line: typeof e.LineNumber === "number" && e.LineNumber >= 1 ? e.LineNumber : 0
      }))
      .sort((a, b) => a._line - b._line);
  }, [errors]);

  const [selectedErrorIdx, setSelectedErrorIdx] = React.useState(0);

  // Refs for each error line
  const errorRefs = React.useRef<(HTMLDivElement | null)[]>([]);

  // Scroll to selected error when it changes
  React.useEffect(() => {
    if (allErrors.length === 0) return;
    const ref = errorRefs.current[selectedErrorIdx];
    if (ref) {
      ref.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  }, [selectedErrorIdx, allErrors.length]);

  React.useEffect(() => {
    if (onErrorCountChange) {
      onErrorCountChange(errors.length);
    }
  }, [errors.length, onErrorCountChange]);

  // Navigation handlers
  const goPrevError = () => {
    setSelectedErrorIdx(idx => (idx > 0 ? idx - 1 : allErrors.length - 1));
  };
  const goNextError = () => {
    setSelectedErrorIdx(idx => (idx < allErrors.length - 1 ? idx + 1 : 0));
  };

  // Reset selected error if errors change
  React.useEffect(() => {
    setSelectedErrorIdx(0);
  }, [roomName, errors.length]);

  return (
    <div className="full-page-container">
      <div className="yaml-container">
        {errors.length > 0 && (
          <div className="yaml-error-toolbar">
            <span className="toolbar-icon" title="YAML Errors">⚠️</span>
            <span className="toolbar-title">YAML Errors</span>
            <span className="toolbar-divider" />
            <span className="toolbar-count">
              {errors.length} error{errors.length === 1 ? "" : "s"}
            </span>
            <button className="toolbar-btn" onClick={goPrevError} title="Previous Error" disabled={allErrors.length === 0}>⬆️</button>
            <button className="toolbar-btn" onClick={goNextError} title="Next Error" disabled={allErrors.length === 0}>⬇️</button>
            <span className="toolbar-index">
              {allErrors.length > 0 ? `Error ${selectedErrorIdx + 1} of ${allErrors.length}` : ""}
            </span>
            <span className="toolbar-tip">Review highlighted lines below.</span>
          </div>
        )}
        {yamlContent.trim().length === 0 ? (
          <div className="no-yaml">No YAML found.</div>
        ) : (
          <pre className="yaml-inline-error-block">
            {/* Top errors at line 0 */}
            {allErrors.filter(e => e._line === 0).map((error, i) => {
              const isSelected = selectedErrorIdx === allErrors.findIndex(e2 => e2 === error);
              return (
                <div
                  key={`top-error-${i}`}
                  className={`yaml-line-container has-error yaml-top-error-line${isSelected ? " selected-error" : ""}`}
                  ref={el => { errorRefs.current[allErrors.findIndex(e2 => e2 === error)] = el; }}
                  style={isSelected ? { outline: "2px solid #b59a00", background: "#fffbe6", color: "#000" } : undefined}
                >
                  <span className="yaml-line-number">0</span>
                  <span className="yaml-line">
                    <span className="error-msg">{error.Message}</span>
                  </span>
                </div>
              );
            })}
            {/* YAML lines */}
            {(() => {
              let prevIndentLen = 0;
              return yamlContent.split('\n').map((line, idx) => {
                const indentMatch = line.match(/^(\s*)/);
                const currIndentLen = indentMatch ? indentMatch[1].length : 0;
                const lineNumber = idx + 1;
                const lineErrors = allErrors.filter(e => e._line === lineNumber);
                const isSelected = lineErrors.some(e => selectedErrorIdx === allErrors.findIndex(e2 => e2 === e));
                const rendered = (
                  <div
                    key={`yaml-line-${idx}`}
                    className={`yaml-line-container${lineErrors.length > 0 ? " has-error" : ""}${isSelected ? " selected-error" : ""}`}
                    ref={el => {
                      if (lineErrors.length > 0) {
                        // Assign ref to the first error on this line
                        const displayIdx = allErrors.findIndex(e2 => e2 === lineErrors[0]);
                        if (displayIdx !== -1) {
                          errorRefs.current[displayIdx] = el;
                        }
                      }
                    }}
                    style={isSelected ? { outline: "2px solid #b59a00", background: "#fffbe6", color: "#000" } : undefined}
                  >
                    <span className="yaml-line-number">{lineNumber}</span>
                    <span className="yaml-line">
                      {renderYamlLine(line)}
                    </span>
                    {/* Render errors for this line */}
                    {lineErrors.map((error, i) => (
                      <div key={i} className="yaml-error-message">
                        <span className="error-msg">{error.Message}</span>
                        {typeof error.CharPosition === "number" && (
                          <span className="error-pos"> (Char {error.CharPosition})</span>
                        )}
                      </div>
                    ))}
                  </div>
                );
                prevIndentLen = currIndentLen;
                return rendered;
              });
            })()}
          </pre>
        )}
      </div>
    </div>
  );
}
export default YamlDisplay;
