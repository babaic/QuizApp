import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { Routes } from 'react-router';
import { Route } from 'react-router';
import QuizList from "./Pages/AllQuizes/AllQuizes.tsx";
import QuizDetail from "./Pages/QuizDetail/QuizDetail.tsx";

function App() {
    return (
        <div className="App">
            <Routes>
                <Route path="/" element={<QuizList />} />
                <Route path="/quiz/:quizId" element={<QuizDetail />} />
            </Routes>
        </div>
    );
}

export default App
