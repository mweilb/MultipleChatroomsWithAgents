import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import ChatRoom from './ChatRoom';
import BeforeWeStart from './BeforeWeStart';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<BeforeWeStart />} />
        <Route path="/chatroom" element={<ChatRoom />} />
      </Routes>
    </Router>
  );
}

export default App;
