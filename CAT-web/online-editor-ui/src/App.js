import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import './App.scss';
import ContentArea from './components/ContentArea';
import Navbar from './components/Navbar';

function App() {
    return (
        <div className="app">
            <Navbar />
            <ContentArea className="ContentArea" />
        </div>
    );
}

export default App;
