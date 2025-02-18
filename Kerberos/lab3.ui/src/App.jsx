import { useState, useEffect } from 'react';
import axios from 'axios';

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [currentUser, setCurrentUser] = useState('');
    const [tgt, setTgt] = useState(''); 
    const [serviceTicket, setServiceTicket] = useState(''); 
    const [books, setBooks] = useState([]);
    const [view, setView] = useState('login'); 

    const authBaseUrl = 'https://localhost:7141/api/auth';
    const bookBaseUrl = 'https://localhost:7134/api/books';
    const serviceTicketUrl = 'https://localhost:7141/api/auth/request-service-ticket';

    const handleLogin = async (username, password) => {
        try {
            const res = await axios.post(`${authBaseUrl}/login`, { name: username, password });
            if (res.data.tgt) {
                setTgt(res.data.tgt);
                setCurrentUser(username);
                setIsLoggedIn(true);
                setView('books');
                requestServiceTicket(res.data.tgt);
            }
        } catch (error) {
            console.error(error);
            alert('Login error: check your username and password');
        }
    };

    const requestServiceTicket = async (TGT) => {
        try {
            const response = await axios.post(serviceTicketUrl, { TGT });
            if (response.data.serviceTicket) {
                setServiceTicket(response.data.serviceTicket);
            } else {
                alert('Failed to get service ticket');
            }
        } catch (error) {
            console.error(error);
            alert('Error requesting service ticket');
        }
    };

    const handleRegister = async (username, password) => {
        try {
            const res = await axios.post(`${authBaseUrl}/register`, { name: username, password });
            alert(res.data);
            handleLogin(username, password);
        } catch (error) {
            console.error(error);
            alert('Registration error');
        }
    };

    useEffect(() => {
        if (isLoggedIn && serviceTicket) {
            axios
                .get(bookBaseUrl, { headers: { Authorization: `Bearer ${serviceTicket}` } })
                .then((res) => {
                    setBooks(res.data);
                })
                .catch((err) => {
                    console.error(err);
                    alert('Failed to fetch the book list');
                });
        }
    }, [isLoggedIn, serviceTicket]);

    const handleLogout = () => {
        setIsLoggedIn(false);
        setCurrentUser('');
        setTgt('');
        setServiceTicket('');
        setBooks([]);
        setView('login');
    };

    return (
        <div className="App" style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
            {isLoggedIn && (
                <header style={{ marginBottom: '20px' }}>
                    <h2>Welcome, {currentUser}!</h2>
                    <button onClick={handleLogout}>Logout</button>
                </header>
            )}

            {!isLoggedIn && view === 'login' && (
                <LoginForm
                    onLogin={handleLogin}
                    switchToRegister={() => setView('register')}
                />
            )}

            {!isLoggedIn && view === 'register' && (
                <RegisterForm
                    onRegister={handleRegister}
                    switchToLogin={() => setView('login')}
                />
            )}

            {isLoggedIn && view === 'books' && <BookList books={books} />}
        </div>
    );
}

const LoginForm = ({ onLogin, switchToRegister }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const handleSubmit = (e) => {
        e.preventDefault();
        onLogin(username, password);
    };

    return (
        <div>
            <h3>Login</h3>
            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '10px' }}>
                    <label>Username: </label>
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div style={{ marginBottom: '10px' }}>
                    <label>Password: </label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit">Login</button>
            </form>
            <p>
                Don't have an account?{' '}
                <button onClick={switchToRegister}>Register</button>
            </p>
        </div>
    );
};

const RegisterForm = ({ onRegister, switchToLogin }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const handleSubmit = (e) => {
        e.preventDefault();
        onRegister(username, password); 
    };

    return (
        <div>
            <h3>Register</h3>
            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '10px' }}>
                    <label>Username: </label>
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div style={{ marginBottom: '10px' }}>
                    <label>Password: </label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit">Register</button>
            </form>
            <p>
                Already have an account?{' '}
                <button onClick={switchToLogin}>Login</button>
            </p>
        </div>
    );
};

const BookList = ({ books }) => {
    return (
        <div>
            <h3>Book List</h3>
            {books.length === 0 ? (
                <p>No books available</p>
            ) : (
                <ul>
                    {books.map((book) => (
                        <li key={book.id}>
                            <strong>{book.title}</strong> - {book.author}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default App;
