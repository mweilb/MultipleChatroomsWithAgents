import React, { useState, useCallback, useRef, useEffect, useMemo, JSX } from "react";
import "./ChatRoom.scss";
import "./ChatRoom.css";
import { Button } from "primereact/button";
import { InputTextarea } from "primereact/inputtextarea";
import { Steps } from "primereact/steps";
import Logo from "./assets/peckham-logo-horizontal.png";
import ReactMarkdown from "react-markdown";
import { getColorByMessage } from "./utils";
import { useAppStateContext } from "./context-app/AppStateContext";

// Helper component for typing effect (for chatbot messages)
const TypingText = ({ text, speed = 50 }: { text: string; speed?: number }) => {
  const [displayedText, setDisplayedText] = useState<string>("");
  
  useEffect(() => {
    let currentIndex = 0;
    setDisplayedText(""); // Reset on text change.
    const interval = setInterval(() => {
      currentIndex++;
      setDisplayedText(text.substring(0, currentIndex));
      if (currentIndex === text.length) {
        clearInterval(interval);
      }
    }, speed);
    
    return () => clearInterval(interval);
  }, [text, speed]);

  return <span>{displayedText}</span>;
};

// Candidate script mapping by stage (activeChannel)
const candidateScript: { [key: number]: string } = {
  0: "Hi Journey, I am looking for a new job and could use some help.",
  1: "My name is Jennifer. I have a bachelor’s degree in accounting, I enjoy listening to audio books. I was a small business owner and I used to help people with their tax preparation, but I cannot do that job anymore. I like to help people and I’m interested in a desk job. I am blind so I think that I might need an accommodation if I am using a computer."
};

const ChatRoom = (): JSX.Element => {
  // activeChannel represents the current stage:
  // 0: About Peckham, 1: About You, 2: Peckham + You, etc.
  const { activeChannel, setActiveChannel } = useAppStateContext();

  // Mapping of stages to their initial chatbot message.
  const stageInitialMessages = [
    // Stage 0: About Peckham
    "Hi, I’m Journey, your career development guide. Let's start your career path! Would you like to learn more about Peckham? Or jump right into building your career profile?",
    // Stage 1: About You
    "To kick start this adventure, I’d like to learn more about you… Do you mind sharing your name? Also, tell me about your hobbies, what you do in your free time, your education and your past work experience?",
    // Stage 2: Peckham + You
    "Nice to meet you Jennifer. Based on what you have told me so far, I would recommend that you consider applying for a customer service position at one of our contact centers. In this role you would be trained to answer technical questions from members of the public who are calling a government agency. An accommodation that you may want to consider is using a screen reader in order to read your computer screen. This accommodation may work to make you independent in your new job. Would you like me to help you with an application?"
  ];

   // Mapping of stages to their initial chatbot message.
   const stageInitialMessages2 = [
    // Stage 0: About Peckham
    "Hi, I’m Journey, your career development guide. Let's start your career path! Would you like to learn more about Peckham? Or jump right into building your career profile?",
    // Stage 1: About You
    "Great! Let’s begin by getting to know you a little better. What’s your name? And could you tell me a bit about your background—like what you enjoy doing, your education, and any work experience you have?",
    // Stage 2: Peckham + You
    "Thanks for sharing, Jennifer! With your background in helping others and your interest in desk work, I suggest looking into one of our customer service roles at a Peckham contact center. You’d receive training to support callers with questions related to government services. For someone who’s blind, a screen reader is a great accommodation to explore—it can help you work independently and effectively. Would you like me to walk you through the application process? After that, we’ll connect you with an Intake Specialist to talk more about accommodations."
  ];

  // Local state for messages and input.
  type Message = { TransactionId: string; Content: string; AgentName: string };
  const [messages, setMessages] = useState<Message[]>([]);
  const [message, setMessage] = useState<string>("");
  const [translate, setTranslate] = useState<string>("");
  const [isHelp, setIsHelp] = useState<boolean>(false);

  // Flag to control auto-typing for candidate input.
  const [autoTypingActive, setAutoTypingActive] = useState<boolean>(false);

  // Ref to auto-scroll to the last message.
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Delay (in milliseconds) before transitioning to the next stage.
  const transitionDelay = 2000;

  // When the stage changes, reset the conversation with the initial chatbot message.
  useEffect(() => {
    setMessages([
      {
        TransactionId: crypto.randomUUID(),
        Content: stageInitialMessages2[activeChannel],
        AgentName: "Agent"
      }
    ]);
    // Reset candidate input when stage changes.
    setMessage("");
  }, [activeChannel]);

  // Toggle help modal.
  const toggleHelp = (): void => setIsHelp((prev) => !prev);

  // Steps for the stepper.
  const stepsItems = useMemo(
    () => [
      { label: "About Peckham" },
      { label: "About You" },
      { label: "Peckham + You" },
      { label: "Apply" }
    ],
    []
  );

  const colorMap = {
    0: "#003057",
    1: "#00758D",
    2: "#522A44",
    3: "#262626"
  };

  // Handle sending a candidate message.
  const handleClickSendMessage = useCallback((): void => {
    if (message.trim() !== "") {
      const candidateMessage = {
        TransactionId: crypto.randomUUID(),
        Content: message,
        AgentName: "Candidate"
      };

      setMessages((prev) => [...prev, candidateMessage]);
      setMessage("");
      setTranslate("");

      // Transition to the next stage after a delay.
      setTimeout(() => {
        if (activeChannel < stageInitialMessages2.length - 1) {
          setActiveChannel(activeChannel + 1);
        }
      }, transitionDelay);
    }
  }, [message, activeChannel, setActiveChannel, stageInitialMessages2.length]);

  const handleKeyPress = (e: React.KeyboardEvent<HTMLTextAreaElement>): void => {
    if (e.key === "Enter") handleClickSendMessage();
  };

  // Auto-scroll to the bottom when messages update.
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  return (
    <div id="chat-container" className="chat-container">
      <div className="header">
        <div className="header-top">
          <img className="header-title" src={Logo} alt="Logo" />
          <Button icon="pi pi-info-circle" className="help-button" onClick={toggleHelp} />
        </div>
        <Steps
          model={stepsItems.map((item, index) => ({
            ...item,
            className: [
              `step-${index}`,
              index < activeChannel ? "p-complete" : "",
              index === activeChannel ? "p-highlight" : ""
            ].join(" ")
          }))}
          activeIndex={activeChannel}
          className="mt-4 custom-steps"
        />
      </div>

      {isHelp && (
        <div className="help-container">
          <div className="modal">
            <Button icon="pi pi-times" className="modal-button" onClick={toggleHelp} />
            <p className="modal-text">
              Transferring you to a human assistant. Please hold on while I connect you...
            </p>
          </div>
        </div>
      )}

      <div className="chat-messages">
        {messages.map((msg) => {
          const messageClass = msg.AgentName.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-+|-+$/g, "");
          const isUserMessage = messageClass === "candidate";
          const journeyLabel = msg.AgentName.toLowerCase().includes("agent") ? "Journey" : "Candidate";

          return (
            <div key={msg.TransactionId} className={`message-wrapper ${messageClass}`}>
              <div className={`message-container ${messageClass}`}>
                <div className={`message-info ${messageClass}`}>
                  <div className="agent-name">{journeyLabel}</div>
                </div>
                <div
                  className={`message-content ${messageClass}`}
                  style={{
                    backgroundColor: isUserMessage ? colorMap[activeChannel as keyof typeof colorMap] : "white",
                    color: isUserMessage ? "" : "#000"
                  }}
                >
                  {msg.AgentName === "Agent" ? (
                    <TypingText text={msg.Content} speed={30} />
                  ) : (
                    <ReactMarkdown className="markdown-content">{msg.Content}</ReactMarkdown>
                  )}
                </div>
              </div>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      <div className="chat-input">
        <InputTextarea
          value={message || translate}
          onKeyPress={handleKeyPress}
          placeholder="Type a message..."
          autoResize
          rows={1}
          onChange={(e) => {
            const inputValue = e.target.value;
            const expectedCandidate = candidateScript[activeChannel];
            // If auto-typing is not active, and input is empty and first character matches,
            // trigger auto-typing of the expected candidate message.
            if (
              !autoTypingActive &&
              !message &&
              expectedCandidate &&
              inputValue.length === 1 &&
              expectedCandidate[0].toLowerCase() === inputValue[0].toLowerCase()
            ) {
              setAutoTypingActive(true);
              let index = 1;
              const interval = setInterval(() => {
                setMessage(expectedCandidate.substring(0, index + 1));
                index++;
                if (index >= expectedCandidate.length) {
                  clearInterval(interval);
                  setAutoTypingActive(false);
                }
              }, 50);
              return;
            }
            // Otherwise, update the message normally.
            setMessage(inputValue);
          }}
          className="chat-input-field"
        />
        <div className="input-icons">
          <Button className="send-btn" label="Send" onClick={handleClickSendMessage} />
        </div>
      </div>
    </div>
  );
};

export default ChatRoom;
